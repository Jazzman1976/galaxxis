using Galaxxis.Domain.Buildings;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;

namespace Galaxxis.Engine.Handlers.Buildings;

/// <summary>
/// Sets the production speed of a resource building. Takes effect at once, with no costs.
/// </summary>
/// <remarks>
/// Game design (confirmed 2026-09-16): speeds are set in 10 % steps by the player, and the game never changes them
/// on its own. Production and consumption scale with the speed from the event's tick on.
/// </remarks>
internal sealed class ChangeProductionSpeedHandler : IGameEventHandler
{
    /// <inheritdoc />
    public string EventType => GameEventTypes.ChangeProductionSpeed;

    /// <inheritdoc />
    public string? Queue => null;

    /// <inheritdoc />
    public async Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        ChangeProductionSpeedPayload payload = context.ReadPayload<ChangeProductionSpeedPayload>();

        if (!BuildingCatalog.IsResourceBuilding(payload.Building))
        {
            return AcceptResult.Reject($"{payload.Building} has no adjustable production speed.");
        }

        if (payload.SpeedPercent is < 0 or > 100 || payload.SpeedPercent % ProductionSpeed.StepPercent != 0)
        {
            return AcceptResult.Reject("The production speed must be a multiple of 10 between 0 and 100 percent.");
        }

        Planet? planet = await context.LoadOwnedPlanetAsync(cancellationToken);
        if (planet is null)
        {
            return AcceptResult.Reject("The planet does not exist or does not belong to you.");
        }

        // The stock is settled to this tick (LoadOwnedPlanetAsync), so the old speed applied until now.
        planet.Buildings = planet.Buildings.WithSpeed(payload.Building, new ProductionSpeed(payload.SpeedPercent));

        return AcceptResult.CompletedImmediately();
    }

    /// <inheritdoc />
    public Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken) =>
        throw new InvalidOperationException("A production speed change takes effect when it is accepted and is never scheduled.");
}
