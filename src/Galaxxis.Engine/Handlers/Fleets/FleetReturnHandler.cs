using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;

namespace Galaxxis.Engine.Handlers.Fleets;

/// <summary>
/// Brings the ships of a mission home: adds the loot to the home planet and makes the ships available again.
/// </summary>
/// <remarks>
/// Return events are created by the engine itself (<see cref="FleetAttackHandler"/>, <see cref="FleetScanHandler"/>)
/// directly as scheduled events, so they are never requested and never accepted.
/// </remarks>
internal sealed class FleetReturnHandler : IGameEventHandler
{
    /// <inheritdoc />
    public string EventType => GameEventTypes.FleetReturn;

    /// <inheritdoc />
    public string? Queue => null;

    /// <inheritdoc />
    public Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken) =>
        Task.FromResult(AcceptResult.Reject("Fleet returns are created by the game and cannot be requested."));

    /// <inheritdoc />
    public async Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        FleetReturnPayload payload = context.ReadPayload<FleetReturnPayload>();

        // Settled to the return tick before adding the loot, so production up to now is accounted first.
        Planet? home = context.Event.PlanetId is int planetId
            ? await context.LoadPlanetAsync(planetId, cancellationToken)
            : null;

        if (home is not null)
        {
            home.Stock += payload.Loot;
        }

        // Release only the ships that are still bound to this mission.
        foreach (Ship ship in await FleetMission.LoadShipsAsync(context, payload.ShipIds, cancellationToken))
        {
            if (ship.MissionEventId == payload.MissionEventId)
            {
                ship.MissionEventId = null;
            }
        }
    }
}
