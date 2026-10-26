using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Economy;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Universes;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Application.Planets;

/// <summary>Implementation of <see cref="IPlanetViewService"/>. Read-only: entities are loaded untracked and never saved.</summary>
internal sealed class PlanetViewService(GameDbContext context, TimeProvider timeProvider) : IPlanetViewService
{
    /// <inheritdoc />
    public async Task<PlanetView?> GetOwnPlanetAsync(string accountId, int planetId, CancellationToken cancellationToken = default)
    {
        Planet? planet = await context.Planets
            .AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Universe)
            .Include(p => p.StarSystem)
            .SingleOrDefaultAsync(p => p.Id == planetId && p.Owner != null && p.Owner.AccountId == accountId, cancellationToken);

        if (planet is null)
        {
            return null;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        UniverseTimeline timeline = planet.Universe.Timeline;

        // Settle for display only: the entity is not tracked, so this is never saved.
        planet.SettleResourcesTo(timeline.TickAt(now), timeline);

        RaceProfile race = RaceProfiles.For(planet.Owner!.Race);
        PlanetBuildings buildings = planet.Buildings;
        ResourceAmounts stock = planet.Stock;

        List<ResourceType> emptySupply = ResourceTypes.Supply.Where(resource => stock[resource] <= 0).ToList();
        ResourceAmounts netRatePerDay = PlanetEconomy.NetDailyRates(buildings, race, emptySupply);

        List<PendingOrderView> pendingOrders = await PendingOrdersAsync(planet.Id, timeline, cancellationToken);

        List<ResourceBuildingView> resourceBuildings = BuildingCatalog.ResourceBuildings
            .Select(rules => ResourceBuildingViewFor(rules, buildings, race, stock, pendingOrders))
            .ToList();

        return new PlanetView(
            planet.Id,
            planet.Name,
            planet.UniverseId,
            planet.Universe.Name,
            planet.Universe.IsPaused,
            planet.StarSystem.GalaxyId,
            planet.StarSystemId,
            planet.StarSystem.Name,
            planet.Owner.Race,
            stock,
            netRatePerDay,
            now,
            resourceBuildings,
            buildings.Shipyard,
            buildings.DefenseSystem,
            pendingOrders);
    }

    private async Task<List<PendingOrderView>> PendingOrdersAsync(int planetId, UniverseTimeline timeline, CancellationToken cancellationToken)
    {
        List<GameEvent> pending = await context.GameEvents
            .AsNoTracking()
            .Where(e => e.PlanetId == planetId && (e.State == GameEventState.Requested || e.State == GameEventState.Scheduled))
            .OrderBy(e => e.ProcessAtTick)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        return pending.Select(e => new PendingOrderView(
            e.Id,
            e.Type,
            e.State,
            e.Type == GameEventTypes.BuildBuilding ? GameEventPayload.Deserialize<BuildBuildingPayload>(e.Payload).Building : null,
            e.State == GameEventState.Scheduled ? timeline.RealTimeOf(e.ProcessAtTick) : null))
            .ToList();
    }

    private static ResourceBuildingView ResourceBuildingViewFor(
        ResourceBuildingRules rules,
        PlanetBuildings buildings,
        RaceProfile race,
        ResourceAmounts stock,
        IReadOnlyList<PendingOrderView> pendingOrders)
    {
        ResourceBuildingState state = buildings.ResourceBuilding(rules.Type);
        double speed = state.Speed.Factor;
        ResourceAmounts nextLevelCost = rules.BuildCostFor(state.Level + 1);

        return new ResourceBuildingView(
            rules.Type,
            state.Level,
            state.Speed.Percent,
            rules.Produces,
            ProductionPerDay: rules.DailyProductionAt(state.Level) * speed * race.ProductionFactors[rules.Produces],
            UpkeepPerDay: rules.DailyUpkeepAt(state.Level) * speed,
            NextLevelCost: nextLevelCost,
            NextLevelDuration: rules.BuildDurationFor(state.Level + 1),
            CanAffordNextLevel: stock.Covers(nextLevelCost),
            IsUnderConstruction: pendingOrders.Any(order => order.Building == rules.Type));
    }
}
