using Galaxxis.Domain.Fleets;
using Galaxxis.Domain.Resources;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;

namespace Galaxxis.Engine.Handlers.Fleets;

/// <summary>
/// Sends ships to another planet to see its resource stock, then schedules their return.
/// </summary>
/// <remarks>
/// Carries over the legacy scan: the report is the target's stock at the arrival tick, stored in the finished event.
/// Flight time is the fixed <see cref="FleetRules.TravelDuration"/>.
/// </remarks>
internal sealed class FleetScanHandler : IGameEventHandler
{
    /// <inheritdoc />
    public string EventType => GameEventTypes.FleetScan;

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

        Planet? target = await context.LoadPlanetAsync(payload.TargetPlanetId, cancellationToken);
        context.WritePayload(payload with { Result = target?.Stock ?? ResourceAmounts.Zero });

        context.ScheduleFollowUp(
            GameEventTypes.FleetReturn,
            new FleetReturnPayload(context.Event.Id, payload.ShipIds, ResourceAmounts.Zero),
            FleetRules.TravelDuration,
            context.Event.PlayerId,
            context.Event.PlanetId);
    }
}
