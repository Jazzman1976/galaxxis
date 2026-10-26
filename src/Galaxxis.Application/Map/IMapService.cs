namespace Galaxxis.Application.Map;

/// <summary>
/// The star map: universes, galaxies, star systems and their planets, with the account's own places marked.
/// </summary>
/// <remarks>
/// The map is public knowledge inside a universe, so every signed-in account may look at it. Whether the account
/// plays in the universe is part of each view, so the web application can send newcomers to the join page first.
/// </remarks>
public interface IMapService
{
    /// <summary>Gets a universe with its galaxies.</summary>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="universeId">The universe.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The view, or <see langword="null"/> if the universe does not exist.</returns>
    Task<UniverseMapView?> GetUniverseAsync(string accountId, int universeId, CancellationToken cancellationToken = default);

    /// <summary>Gets a galaxy with its star systems.</summary>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="galaxyId">The galaxy.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The view, or <see langword="null"/> if the galaxy does not exist.</returns>
    Task<GalaxyMapView?> GetGalaxyAsync(string accountId, int galaxyId, CancellationToken cancellationToken = default);

    /// <summary>Gets a star system with its planets.</summary>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="starSystemId">The star system.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The view, or <see langword="null"/> if the star system does not exist.</returns>
    Task<StarSystemMapView?> GetStarSystemAsync(string accountId, int starSystemId, CancellationToken cancellationToken = default);
}

/// <summary>A place on the map in a selection list.</summary>
/// <param name="Id">Id of the galaxy, star system or planet.</param>
/// <param name="Name">Its name.</param>
/// <param name="ContainsOwnPlanet">Whether the account owns this planet, or a planet inside this galaxy or star system.</param>
public sealed record MapEntry(int Id, string Name, bool ContainsOwnPlanet);

/// <summary>A universe and its galaxies.</summary>
/// <param name="UniverseId">The universe.</param>
/// <param name="Name">Its name.</param>
/// <param name="HasJoined">Whether the account plays in it.</param>
/// <param name="Galaxies">The galaxies, in id order.</param>
public sealed record UniverseMapView(int UniverseId, string Name, bool HasJoined, IReadOnlyList<MapEntry> Galaxies);

/// <summary>A galaxy and its star systems.</summary>
/// <param name="GalaxyId">The galaxy.</param>
/// <param name="Name">Its name.</param>
/// <param name="UniverseId">The universe it belongs to.</param>
/// <param name="UniverseName">That universe's name.</param>
/// <param name="HasJoined">Whether the account plays in the universe.</param>
/// <param name="StarSystems">The star systems, in id order.</param>
public sealed record GalaxyMapView(int GalaxyId, string Name, int UniverseId, string UniverseName, bool HasJoined, IReadOnlyList<MapEntry> StarSystems);

/// <summary>A star system and its planets.</summary>
/// <param name="StarSystemId">The star system.</param>
/// <param name="Name">Its name.</param>
/// <param name="GalaxyId">The galaxy it belongs to.</param>
/// <param name="GalaxyName">That galaxy's name.</param>
/// <param name="UniverseId">The universe it belongs to.</param>
/// <param name="HasJoined">Whether the account plays in the universe.</param>
/// <param name="Planets">The planets, in id order.</param>
public sealed record StarSystemMapView(int StarSystemId, string Name, int GalaxyId, string GalaxyName, int UniverseId, bool HasJoined, IReadOnlyList<MapEntry> Planets);
