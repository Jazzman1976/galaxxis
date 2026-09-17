using Galaxxis.Domain.Fleets;
using Galaxxis.Domain.Resources;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;

namespace Galaxxis.Engine.Handlers.Fleets;

/// <summary>
/// Sends ships to another planet to plunder resources, then schedules their return with the loot.
/// </summary>
/// <remarks>
/// This carries over the legacy game's attack: there is no fight yet, and the flight takes a fixed time
/// (<see cref="FleetRules.TravelDuration"/>). The combat system of the concept will extend this handler.
/// The loot is taken from the target when the fleet arrives and added to the home planet when it returns.
/// Both steps are separate events, each in its own transaction, and the return is scheduled inside the arrival's
/// transaction: the loot can neither get lost nor be duplicated.
/// </remarks>
internal sealed class FleetAttackHandler : IGameEventHandler
{
    /// <inheritdoc />
    public string EventType => GameEventTypes.FleetAttack;

    /// <inheritdoc />
    public string? Queue => null;

    /// <inheritdoc />
    public async Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        FleetMissionPayload payload = context.ReadPayload<FleetMissionPayload>();

        string? rejection = await FleetMission.ValidateAndReserveShipsAsync(context, payload, cancellationToken);
        return rejection is null
            ? AcceptResult.CompleteAfter(FleetRules.TravelDuration)
            : AcceptResult.Reject(rejection);
    }

    /// <inheritdoc />
    public async Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        FleetMissionPayload payload = context.ReadPayload<FleetMissionPayload>();

        List<Ship> ships = await FleetMission.LoadShipsAsync(context, payload.ShipIds, cancellationToken);
        double storageCapacity = ships.Sum(ship => ship.StorageCapacity);

        // The target is settled to the arrival tick, so the fleet finds what the planet has at that moment.
        Planet? target = await context.LoadPlanetAsync(payload.TargetPlanetId, cancellationToken);
        ResourceAmounts loot = ResourceAmounts.Zero;

        if (target is not null)
        {
            loot = FleetRules.Plunder(target.Stock, storageCapacity);
            target.Stock -= loot;
        }

        // Keep the result in the finished event: it is the attack report.
        context.WritePayload(payload with { Result = loot });

        context.ScheduleFollowUp(
            GameEventTypes.FleetReturn,
            new FleetReturnPayload(context.Event.Id, ships.Select(ship => ship.Id).ToList(), loot),
            FleetRules.TravelDuration,
            context.Event.PlayerId,
            context.Event.PlanetId);
    }
}
