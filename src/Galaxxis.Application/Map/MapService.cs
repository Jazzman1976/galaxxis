using Galaxxis.Persistence.Game;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Application.Map;

/// <summary>Implementation of <see cref="IMapService"/>. Read-only queries; nothing is tracked or changed.</summary>
internal sealed class MapService(GameDbContext context) : IMapService
{
    /// <inheritdoc />
    public async Task<UniverseMapView?> GetUniverseAsync(string accountId, int universeId, CancellationToken cancellationToken = default)
    {
        var universe = await context.Universes
            .AsNoTracking()
            .Where(u => u.Id == universeId)
            .Select(u => new { u.Id, u.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (universe is null)
        {
            return null;
        }

        List<MapEntry> galaxies = await context.Galaxies
            .AsNoTracking()
            .Where(galaxy => galaxy.UniverseId == universeId)
            .OrderBy(galaxy => galaxy.Id)
            .Select(galaxy => new MapEntry(
                galaxy.Id,
                galaxy.Name,
                galaxy.StarSystems.Any(system => system.Planets.Any(planet => planet.Owner != null && planet.Owner.AccountId == accountId))))
            .ToListAsync(cancellationToken);

        return new UniverseMapView(universe.Id, universe.Name, await HasJoinedAsync(accountId, universeId, cancellationToken), galaxies);
    }

    /// <inheritdoc />
    public async Task<GalaxyMapView?> GetGalaxyAsync(string accountId, int galaxyId, CancellationToken cancellationToken = default)
    {
        var galaxy = await context.Galaxies
            .AsNoTracking()
            .Where(g => g.Id == galaxyId)
            .Select(g => new { g.Id, g.Name, g.UniverseId, UniverseName = g.Universe.Name })
            .SingleOrDefaultAsync(cancellationToken);

        if (galaxy is null)
        {
            return null;
        }

        List<MapEntry> systems = await context.StarSystems
            .AsNoTracking()
            .Where(system => system.GalaxyId == galaxyId)
            .OrderBy(system => system.Id)
            .Select(system => new MapEntry(
                system.Id,
                system.Name,
                system.Planets.Any(planet => planet.Owner != null && planet.Owner.AccountId == accountId)))
            .ToListAsync(cancellationToken);

        return new GalaxyMapView(
            galaxy.Id, galaxy.Name, galaxy.UniverseId, galaxy.UniverseName,
            await HasJoinedAsync(accountId, galaxy.UniverseId, cancellationToken), systems);
    }

    /// <inheritdoc />
    public async Task<StarSystemMapView?> GetStarSystemAsync(string accountId, int starSystemId, CancellationToken cancellationToken = default)
    {
        var system = await context.StarSystems
            .AsNoTracking()
            .Where(s => s.Id == starSystemId)
            .Select(s => new { s.Id, s.Name, s.GalaxyId, GalaxyName = s.Galaxy.Name, s.Galaxy.UniverseId })
            .SingleOrDefaultAsync(cancellationToken);

        if (system is null)
        {
            return null;
        }

        List<MapEntry> planets = await context.Planets
            .AsNoTracking()
            .Where(planet => planet.StarSystemId == starSystemId)
            .OrderBy(planet => planet.Id)
            .Select(planet => new MapEntry(planet.Id, planet.Name, planet.Owner != null && planet.Owner.AccountId == accountId))
            .ToListAsync(cancellationToken);

        return new StarSystemMapView(
            system.Id, system.Name, system.GalaxyId, system.GalaxyName, system.UniverseId,
            await HasJoinedAsync(accountId, system.UniverseId, cancellationToken), planets);
    }

    private Task<bool> HasJoinedAsync(string accountId, int universeId, CancellationToken cancellationToken) =>
        context.Players.AnyAsync(player => player.UniverseId == universeId && player.AccountId == accountId, cancellationToken);
}
