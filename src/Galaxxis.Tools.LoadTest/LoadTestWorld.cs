using Galaxxis.Application.Players;
using Galaxxis.Application.Universes;
using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Universes;
using Galaxxis.Domain.Universes.Generation;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// Builds the game world a load test runs in: a fresh universe that has already been running for some days, with one
/// synthetic player per simulated account.
/// </summary>
/// <remarks>
/// <para>
/// <b>The real use cases where they exist.</b> The universe is generated and resumed through
/// <see cref="IUniverseAdminService"/> and every player joins through <see cref="IPlayerService"/>, exactly as an
/// administrator and the web application do. This also puts load on joining.
/// </para>
/// <para>
/// <b>Direct writes for test preparation only.</b> Three things have no use case, because they never happen in the
/// game: moving the universe's start into the past, giving planets a large stock and starting buildings, and marking
/// some planets as not settled for days. They are written directly to the database, like the setup of an automated
/// test. Everything that happens afterwards goes through commands and the engine only.
/// </para>
/// </remarks>
internal static class LoadTestWorld
{
    /// <summary>
    /// The level every resource building of a load test planet starts with. Level 1 keeps the next constructions short
    /// (level 2 takes 150 s, level 3 about 9 minutes), so a 30-minute run completes many of them. <see cref="InvariantChecks"/> relies on it:
    /// a building's level must be this value plus the number of its completed constructions.
    /// </summary>
    public const int StartingLevel = 1;

    /// <summary>Prefix of the account ids of synthetic players, so they can never be mistaken for real accounts.</summary>
    public const string AccountPrefix = "loadtest-";

    /// <summary>
    /// The stock of every load test planet: large enough that constructions are limited by the construction queue,
    /// not by resources. The load test measures the engine, not the economy.
    /// </summary>
    private static readonly ResourceAmounts RichStock = new()
    {
        Ore = 10_000_000,
        Energy = 10_000_000,
        Tools = 10_000_000,
        Mana = 10_000_000,
        Crystals = 10_000_000,
        Antimatter = 10_000_000,
        Elementium = 10_000_000,
    };

    /// <summary>Creates the universe and the players.</summary>
    /// <param name="services">The host's service provider.</param>
    /// <param name="settings">What to simulate.</param>
    /// <param name="cancellationToken">Cancels the setup.</param>
    /// <returns>The universe and the players with their home planets.</returns>
    public static async Task<LoadTestUniverse> CreateAsync(IServiceProvider services, LoadTestSettings settings, CancellationToken cancellationToken)
    {
        int universeId = await GenerateUniverseAsync(services, settings, cancellationToken);
        Console.WriteLine($"Generated universe {universeId}; it has been running for {settings.StaleDays} day(s).");

        IReadOnlyList<SimulatedPlayer> players = await JoinPlayersAsync(services, universeId, settings.Players, cancellationToken);
        Console.WriteLine($"{players.Count} players joined.");

        int stalePlanets = await PreparePlanetsAsync(services, universeId, players, settings, cancellationToken);
        Console.WriteLine(
            $"Planets prepared: resource buildings at level {StartingLevel}, large stock, {stalePlanets} planet(s) not settled for {settings.StaleDays} day(s).");

        return new LoadTestUniverse(universeId, players);
    }

    /// <summary>Generates the universe, moves its start into the past and resumes it.</summary>
    private static async Task<int> GenerateUniverseAsync(IServiceProvider services, LoadTestSettings settings, CancellationToken cancellationToken)
    {
        // A quarter more planets than players, ten systems of ten planets per galaxy.
        const int PlanetsPerGalaxy = 100;
        int galaxies = Math.Max(1, (int)Math.Ceiling(settings.Players * 1.25 / PlanetsPerGalaxy));

        using IServiceScope scope = services.CreateScope();
        IUniverseAdminService admin = scope.ServiceProvider.GetRequiredService<IUniverseAdminService>();

        UniverseSummary universe = await admin.GenerateAsync(
            new GenerateUniverseRequest(
                new UniverseGenerationSettings
                {
                    Name = $"Load test {DateTimeOffset.Now:yyyy-MM-dd HH:mm}",
                    GalaxyCount = galaxies,
                    StarSystemsPerGalaxy = 10,
                    PlanetsPerStarSystem = 10,
                },
                TickLength: settings.TickLength),
            cancellationToken);

        // Test preparation: a generated universe starts at tick 0. Moving its epoch back lets it start at the tick of
        // "StaleDays days later", so planets can lag behind by that much without negative ticks.
        GameDbContext database = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        Universe stored = await database.Universes.SingleAsync(u => u.Id == universe.Id, cancellationToken);
        stored.Epoch -= TimeSpan.FromDays(settings.StaleDays);
        await database.SaveChangesAsync(cancellationToken);

        await admin.ResumeAsync(universe.Id, cancellationToken);
        return universe.Id;
    }

    /// <summary>Lets every synthetic account enter the universe, several at a time, through the real use case.</summary>
    private static async Task<IReadOnlyList<SimulatedPlayer>> JoinPlayersAsync(
        IServiceProvider services, int universeId, int count, CancellationToken cancellationToken)
    {
        Race[] races = Enum.GetValues<Race>();
        SimulatedPlayer[] players = new SimulatedPlayer[count];

        await Parallel.ForAsync(0, count, new ParallelOptions { MaxDegreeOfParallelism = 8, CancellationToken = cancellationToken }, async (index, token) =>
        {
            string accountId = $"{AccountPrefix}{universeId}-{index:D5}";

            using IServiceScope scope = services.CreateScope();
            JoinUniverseResult result = await scope.ServiceProvider.GetRequiredService<IPlayerService>()
                .JoinUniverseAsync(accountId, universeId, races[index % races.Length], token);

            if (result.HomePlanetId is not int planetId)
            {
                throw new InvalidOperationException($"Account {accountId} could not join universe {universeId}: {result.Outcome}.");
            }

            players[index] = new SimulatedPlayer(accountId, planetId);
        });

        return players;
    }

    /// <summary>Test preparation: stock, starting buildings and stale planets (see the class remarks).</summary>
    /// <returns>The number of stale planets.</returns>
    private static async Task<int> PreparePlanetsAsync(
        IServiceProvider services, int universeId, IReadOnlyList<SimulatedPlayer> players, LoadTestSettings settings, CancellationToken cancellationToken)
    {
        using IServiceScope scope = services.CreateScope();
        GameDbContext database = scope.ServiceProvider.GetRequiredService<GameDbContext>();

        Universe universe = await database.Universes.AsNoTracking().SingleAsync(u => u.Id == universeId, cancellationToken);
        UniverseTimeline timeline = universe.Timeline;

        // Every planet: the same starting buildings and a large stock, valid as of now.
        long now = timeline.TickAt(DateTimeOffset.UtcNow);
        PlanetBuildings buildings = BuildingCatalog.ResourceBuildings.Aggregate(
            PlanetBuildings.Empty, (current, rules) => current.WithLevel(rules.Type, StartingLevel));

        List<Planet> planets = await database.Planets
            .Where(planet => planet.UniverseId == universeId && planet.OwnerPlayerId != null)
            .ToListAsync(cancellationToken);

        foreach (Planet planet in planets)
        {
            planet.Buildings = buildings;
            planet.Stock = RichStock;
            planet.ResourcesSettledTick = now;
        }

        // Stale planets: their stock is valid only as of StaleDays days ago. Chosen evenly over all players.
        int staleCount = players.Count * settings.StalePercent / 100;
        long staleTick = Math.Max(0, now - timeline.TicksFor(TimeSpan.FromDays(settings.StaleDays)));
        HashSet<int> stalePlanetIds = players
            .Where((_, index) => staleCount > 0 && index % Math.Max(1, players.Count / staleCount) == 0)
            .Take(staleCount)
            .Select(player => player.PlanetId)
            .ToHashSet();

        foreach (Planet planet in planets.Where(planet => stalePlanetIds.Contains(planet.Id)))
        {
            planet.ResourcesSettledTick = staleTick;
        }

        await database.SaveChangesAsync(cancellationToken);
        return stalePlanetIds.Count;
    }
}

/// <summary>The universe of a load test run and its players.</summary>
/// <param name="UniverseId">The universe.</param>
/// <param name="Players">One entry per synthetic player.</param>
internal sealed record LoadTestUniverse(int UniverseId, IReadOnlyList<SimulatedPlayer> Players);

/// <summary>A synthetic player: the account it acts as and its home planet.</summary>
/// <param name="AccountId">The account id passed to the Application services (no real login account exists).</param>
/// <param name="PlanetId">The home planet.</param>
internal sealed record SimulatedPlayer(string AccountId, int PlanetId);
