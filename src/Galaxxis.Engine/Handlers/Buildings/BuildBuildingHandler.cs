using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Resources;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;

namespace Galaxxis.Engine.Handlers.Buildings;

/// <summary>
/// Upgrades a building by one level: pays the construction costs when the request is accepted, raises the level
/// when the construction time has passed.
/// </summary>
/// <remarks>
/// Only one construction runs per planet at a time (queue <see cref="GameEventQueues.Construction"/>), as in the
/// legacy game. Costs and durations come from the building rules in the Domain.
/// </remarks>
internal sealed class BuildBuildingHandler : IGameEventHandler
{
    /// <inheritdoc />
    public string EventType => GameEventTypes.BuildBuilding;

    /// <inheritdoc />
    public string? Queue => GameEventQueues.Construction;

    /// <inheritdoc />
    public async Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        BuildBuildingPayload payload = context.ReadPayload<BuildBuildingPayload>();

        Planet? planet = await context.LoadOwnedPlanetAsync(cancellationToken);
        if (planet is null)
        {
            return AcceptResult.Reject(GameEventMessages.PlanetNotOwned);
        }

        BuildingRules rules = BuildingCatalog.For(payload.Building);
        int targetLevel = planet.Buildings.LevelOf(payload.Building) + 1;
        ResourceAmounts cost = rules.BuildCostFor(targetLevel);

        // The stock is settled to the event's tick, so resources produced until the request count.
        if (!planet.Stock.Covers(cost))
        {
            return AcceptResult.Reject(GameEventMessages.NotEnoughResources(planet.Stock, cost));
        }

        // Costs are paid when construction starts (Concept and legacy game alike).
        planet.Stock -= cost;

        return AcceptResult.CompleteAfter(rules.BuildDurationFor(targetLevel));
    }

    /// <inheritdoc />
    public async Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        BuildBuildingPayload payload = context.ReadPayload<BuildBuildingPayload>();

        // No ownership check: a construction that was accepted completes on its planet.
        Planet planet = await context.LoadPlanetAsync(context.Event.PlanetId!.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Planet {context.Event.PlanetId} of a scheduled construction no longer exists.");

        // Settled to the completion tick above, so production up to now used the old level;
        // from this tick on, the new level produces and consumes.
        int newLevel = planet.Buildings.LevelOf(payload.Building) + 1;
        planet.Buildings = planet.Buildings.WithLevel(payload.Building, newLevel);
    }
}
