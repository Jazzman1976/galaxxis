using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Universes;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Application.Commands;

/// <summary>Implementation of <see cref="ICommandService"/>.</summary>
internal sealed class CommandService(GameDbContext context, TimeProvider timeProvider) : ICommandService
{
    /// <inheritdoc />
    public async Task<CommandResult> RequestBuildAsync(string accountId, int planetId, BuildingType building, CancellationToken cancellationToken = default)
    {
        Planet? planet = await LoadOwnedPlanetAsync(accountId, planetId, cancellationToken);
        if (planet is null)
        {
            return CommandResult.NotFound;
        }

        // Pre-checks for an immediate answer; the engine repeats them when it decides.
        bool constructionPending = await context.GameEvents.AnyAsync(
            e => e.PlanetId == planetId
                && e.Type == GameEventTypes.BuildBuilding
                && (e.State == GameEventState.Requested || e.State == GameEventState.Scheduled),
            cancellationToken);
        if (constructionPending)
        {
            return CommandResult.Refused(GameEventMessages.QueueBusy);
        }

        ResourceAmounts cost = BuildingCatalog.For(building).BuildCostFor(planet.Buildings.LevelOf(building) + 1);
        if (!planet.Stock.Covers(cost))
        {
            return CommandResult.Refused(GameEventMessages.NotEnoughResources(planet.Stock, cost));
        }

        return await RecordAsync(planet, GameEventTypes.BuildBuilding, new BuildBuildingPayload(building), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CommandResult> RequestProductionSpeedAsync(
        string accountId, int planetId, BuildingType building, int speedPercent, CancellationToken cancellationToken = default)
    {
        Planet? planet = await LoadOwnedPlanetAsync(accountId, planetId, cancellationToken);
        if (planet is null)
        {
            return CommandResult.NotFound;
        }

        if (!BuildingCatalog.IsResourceBuilding(building))
        {
            return CommandResult.Refused($"{building} has no adjustable production speed.");
        }

        if (speedPercent is < 0 or > 100 || speedPercent % ProductionSpeed.StepPercent != 0)
        {
            return CommandResult.Refused(GameEventMessages.InvalidProductionSpeed);
        }

        return await RecordAsync(planet, GameEventTypes.ChangeProductionSpeed, new ChangeProductionSpeedPayload(building, speedPercent), cancellationToken);
    }

    /// <inheritdoc />
    public async Task<CommandStatusView?> GetStatusAsync(string accountId, long eventId, CancellationToken cancellationToken = default)
    {
        var found = await context.GameEvents
            .AsNoTracking()
            .Where(e => e.Id == eventId && e.Player != null && e.Player.AccountId == accountId)
            .Select(e => new { Event = e, e.Universe })
            .SingleOrDefaultAsync(cancellationToken);

        if (found is null)
        {
            return null;
        }

        GameEvent gameEvent = found.Event;
        DateTimeOffset? completesAt = gameEvent.State == GameEventState.Scheduled
            ? found.Universe.Timeline.RealTimeOf(gameEvent.ProcessAtTick)
            : null;

        return new CommandStatusView(gameEvent.Id, gameEvent.Type, gameEvent.State, gameEvent.Error, completesAt);
    }

    /// <summary>Loads an own planet (untracked) with its stock settled to now, for the pre-checks.</summary>
    private async Task<Planet?> LoadOwnedPlanetAsync(string accountId, int planetId, CancellationToken cancellationToken)
    {
        Planet? planet = await context.Planets
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Universe)
            .SingleOrDefaultAsync(p => p.Id == planetId && p.Owner != null && p.Owner.AccountId == accountId, cancellationToken);

        if (planet is not null)
        {
            UniverseTimeline timeline = planet.Universe.Timeline;
            planet.SettleResourcesTo(timeline.TickAt(timeProvider.GetUtcNow()), timeline);
        }

        return planet;
    }

    /// <summary>Stores the command as a requested event at the current universe tick.</summary>
    private async Task<CommandResult> RecordAsync<TPayload>(Planet planet, string eventType, TPayload payload, CancellationToken cancellationToken)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        long tick = planet.Universe.Timeline.TickAt(now);

        GameEvent gameEvent = new()
        {
            UniverseId = planet.UniverseId,
            Type = eventType,
            State = GameEventState.Requested,
            PlayerId = planet.OwnerPlayerId,
            PlanetId = planet.Id,
            RequestedTick = tick,
            ProcessAtTick = tick,
            Payload = GameEventPayload.Serialize(payload),
            CreatedAt = now,
        };

        context.GameEvents.Add(gameEvent);
        await context.SaveChangesAsync(cancellationToken);

        return CommandResult.Recorded(gameEvent.Id);
    }
}
