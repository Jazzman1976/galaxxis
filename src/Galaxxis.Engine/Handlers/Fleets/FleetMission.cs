using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Engine.Handlers.Fleets;

/// <summary>
/// The steps attack and scan missions share: validating the request and reserving the ships.
/// </summary>
internal static class FleetMission
{
    /// <summary>
    /// Checks a mission request and, if it is valid, marks the ships as being on this mission.
    /// </summary>
    /// <param name="context">The context of the requested mission event.</param>
    /// <param name="payload">The mission payload.</param>
    /// <param name="cancellationToken">Cancels the queries.</param>
    /// <returns><see langword="null"/> if the mission may start; otherwise the reason for rejecting it.</returns>
    public static async Task<string?> ValidateAndReserveShipsAsync(
        GameEventContext context, FleetMissionPayload payload, CancellationToken cancellationToken)
    {
        Planet? origin = await context.LoadOwnedPlanetAsync(cancellationToken);
        if (origin is null)
        {
            return "The planet does not exist or does not belong to you.";
        }

        Planet? target = await context.LoadPlanetAsync(payload.TargetPlanetId, cancellationToken);
        if (target is null || target.Id == origin.Id)
        {
            return "The target planet does not exist in this universe.";
        }

        if (target.OwnerPlayerId == context.Event.PlayerId)
        {
            return "You cannot send a mission against your own planet.";
        }

        List<int> requestedIds = payload.ShipIds.Distinct().ToList();
        if (requestedIds.Count == 0)
        {
            return "Choose at least one ship.";
        }

        List<Ship> ships = await context.Database.Ships
            .Where(ship => requestedIds.Contains(ship.Id))
            .ToListAsync(cancellationToken);

        // Every ship must exist, belong to the player, be stationed at the origin and not already be on a mission.
        bool allAvailable = ships.Count == requestedIds.Count && ships.All(ship =>
            ship.PlayerId == context.Event.PlayerId
            && ship.PlanetId == origin.Id
            && ship.MissionEventId is null);

        if (!allAvailable)
        {
            return "Some of the chosen ships are not available at this planet.";
        }

        // Reserve the ships, so they cannot be sent on a second mission at the same time.
        foreach (Ship ship in ships)
        {
            ship.MissionEventId = context.Event.Id;
        }

        return null;
    }

    /// <summary>Loads the ships of a mission.</summary>
    /// <param name="context">The context of a mission or return event.</param>
    /// <param name="shipIds">The ship ids from the payload.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The ships that still exist.</returns>
    public static Task<List<Ship>> LoadShipsAsync(GameEventContext context, IReadOnlyList<int> shipIds, CancellationToken cancellationToken) =>
        context.Database.Ships.Where(ship => shipIds.Contains(ship.Id)).ToListAsync(cancellationToken);
}
