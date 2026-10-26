using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Universes;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// The correctness checks after a load test: the game state must be consistent however many commands arrived at once.
/// </summary>
/// <remarks>
/// Every check reads the database and changes nothing. They are only meaningful for universes built by
/// <see cref="LoadTestWorld"/>, because the level check relies on its known starting level.
/// </remarks>
internal static class InvariantChecks
{
    /// <summary>Runs all checks for one universe.</summary>
    /// <param name="database">A fresh database context.</param>
    /// <param name="universeId">The load test universe.</param>
    /// <param name="cancellationToken">Cancels the checks.</param>
    /// <returns>One result per check.</returns>
    public static async Task<IReadOnlyList<CheckResult>> RunAsync(GameDbContext database, int universeId, CancellationToken cancellationToken)
    {
        Universe universe = await database.Universes.AsNoTracking().SingleAsync(u => u.Id == universeId, cancellationToken);
        long currentTick = universe.Timeline.TickAt(DateTimeOffset.UtcNow);

        List<Planet> planets = await database.Planets
            .AsNoTracking()
            .Include(planet => planet.Owner)
            .Where(planet => planet.UniverseId == universeId && planet.OwnerPlayerId != null)
            .ToListAsync(cancellationToken);

        List<GameEvent> events = await database.GameEvents
            .AsNoTracking()
            .Where(e => e.UniverseId == universeId)
            .ToListAsync(cancellationToken);

        return
        [
            NoBuildingUpgradedTwice(planets, events),
            NoNegativeStock(planets, universe.Timeline, currentTick),
            AtMostOneScheduledPerQueue(events),
            NoFailedEvents(events),
            NoForgottenRequests(events, currentTick),
        ];
    }

    /// <summary>
    /// Every building's level is its starting level plus its completed constructions: no construction took effect twice
    /// or was lost.
    /// </summary>
    private static CheckResult NoBuildingUpgradedTwice(List<Planet> planets, List<GameEvent> events)
    {
        Dictionary<(int PlanetId, BuildingType Building), int> completed = events
            .Where(e => e.Type == GameEventTypes.BuildBuilding && e.State == GameEventState.Done && e.PlanetId is not null)
            .GroupBy(e => (e.PlanetId!.Value, GameEventPayload.Deserialize<BuildBuildingPayload>(e.Payload).Building))
            .ToDictionary(group => group.Key, group => group.Count());

        List<string> wrong = [];
        foreach (Planet planet in planets)
        {
            foreach (ResourceBuildingRules rules in BuildingCatalog.ResourceBuildings)
            {
                int expected = LoadTestWorld.StartingLevel + completed.GetValueOrDefault((planet.Id, rules.Type));
                int actual = planet.Buildings.LevelOf(rules.Type);
                if (actual != expected)
                {
                    wrong.Add($"planet {planet.Id} {rules.Type}: level {actual}, expected {expected}");
                }
            }
        }

        return CheckResult.From(
            "No building upgraded twice (level = start + completed constructions)",
            $"{completed.Values.Sum()} completed constructions on {planets.Count} planets",
            wrong);
    }

    /// <summary>No stock is negative, neither as stored nor when settled to now.</summary>
    private static CheckResult NoNegativeStock(List<Planet> planets, UniverseTimeline timeline, long currentTick)
    {
        List<string> wrong = [];
        foreach (Planet planet in planets)
        {
            if (HasNegative(planet.Stock))
            {
                wrong.Add($"planet {planet.Id} stored: {planet.Stock}");
            }

            // Settling an untracked copy changes nothing in the database.
            planet.SettleResourcesTo(currentTick, timeline);
            if (HasNegative(planet.Stock))
            {
                wrong.Add($"planet {planet.Id} settled to now: {planet.Stock}");
            }
        }

        return CheckResult.From("No negative stock (stored and settled to now)", $"{planets.Count} planets", wrong);
    }

    private static bool HasNegative(ResourceAmounts stock) => stock != stock.ClampedToZero();

    /// <summary>The engine never ran two orders of the same queue on one planet at the same time.</summary>
    private static CheckResult AtMostOneScheduledPerQueue(List<GameEvent> events)
    {
        List<string> wrong = events
            .Where(e => e.State == GameEventState.Scheduled && e.Queue is not null)
            .GroupBy(e => (e.PlanetId, e.Queue))
            .Where(group => group.Count() > 1)
            .Select(group => $"planet {group.Key.PlanetId} queue {group.Key.Queue}: {group.Count()} scheduled")
            .ToList();

        return CheckResult.From("At most one scheduled order per planet and queue", $"{events.Count(e => e.State == GameEventState.Scheduled)} scheduled", wrong);
    }

    /// <summary>No event failed for a technical reason.</summary>
    private static CheckResult NoFailedEvents(List<GameEvent> events)
    {
        List<string> wrong = events
            .Where(e => e.State == GameEventState.Failed)
            .GroupBy(e => e.Error ?? "(no error text)")
            .Select(group => $"{group.Count()}x {group.Key}")
            .ToList();

        return CheckResult.From("No failed events", $"{events.Count} events in total", wrong);
    }

    /// <summary>
    /// No command is left waiting: every requested event that is due has been decided. Run after the engine caught up,
    /// so a few just-recorded commands do not count.
    /// </summary>
    private static CheckResult NoForgottenRequests(List<GameEvent> events, long currentTick)
    {
        List<string> wrong = events
            .Where(e => e.State == GameEventState.Requested && e.ProcessAtTick < currentTick - 5)
            .Select(e => $"event {e.Id} ({e.Type}) requested at tick {e.RequestedTick}")
            .Take(20)
            .ToList();

        return CheckResult.From(
            "No command left undecided (requested more than 5 ticks ago)",
            $"{events.Count(e => e.State == GameEventState.Requested)} still requested",
            wrong);
    }
}

/// <summary>The outcome of one check.</summary>
/// <param name="Name">What was checked.</param>
/// <param name="Passed">Whether the check found no problem.</param>
/// <param name="Scope">What was looked at, e.g. "1000 planets".</param>
/// <param name="Problems">The first problems found (at most ten), empty when passed.</param>
internal sealed record CheckResult(string Name, bool Passed, string Scope, IReadOnlyList<string> Problems)
{
    /// <summary>Creates a result from the list of problems found.</summary>
    /// <param name="name">What was checked.</param>
    /// <param name="scope">What was looked at.</param>
    /// <param name="problems">All problems found.</param>
    /// <returns>Passed if there are no problems; otherwise failed with the first ten.</returns>
    public static CheckResult From(string name, string scope, IReadOnlyList<string> problems) =>
        new(name, problems.Count == 0, scope, problems.Take(10).ToList());
}
