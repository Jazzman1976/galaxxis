using Galaxxis.Domain.Universes.Generation;

namespace Galaxxis.Application.Universes;

/// <summary>
/// Administrative operations on universes: generating new ones, listing them, pausing and resuming.
/// </summary>
/// <remarks>
/// Used by the universe generator tool and the admin page of the web application. Callers must make sure only
/// administrators reach these operations; the service itself does not check permissions.
/// </remarks>
public interface IUniverseAdminService
{
    /// <summary>
    /// Generates a complete universe (galaxies, star systems, planets, moons) and stores it in a single
    /// transaction: either the whole universe is created or nothing.
    /// </summary>
    /// <param name="request">What to generate.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The created universe. It starts <b>paused</b>, so it can be checked before players enter.</returns>
    /// <exception cref="ArgumentException">The settings are invalid.</exception>
    /// <exception cref="InvalidOperationException">The map does not fit into the coordinate range.</exception>
    Task<UniverseSummary> GenerateAsync(GenerateUniverseRequest request, CancellationToken cancellationToken = default);

    /// <summary>Lists all universes, oldest first.</summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>One summary per universe.</returns>
    Task<IReadOnlyList<UniverseSummary>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts or continues universe time. Resuming a universe that is already running changes nothing.
    /// </summary>
    /// <param name="universeId">The universe to resume.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The updated universe, or <see langword="null"/> if no universe has this id.</returns>
    Task<UniverseSummary?> ResumeAsync(int universeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops universe time, for example for maintenance: no production, no construction progress, no due
    /// events until it is resumed. Pausing a paused universe changes nothing.
    /// </summary>
    /// <param name="universeId">The universe to pause.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>The updated universe, or <see langword="null"/> if no universe has this id.</returns>
    Task<UniverseSummary?> PauseAsync(int universeId, CancellationToken cancellationToken = default);
}

/// <summary>What to generate.</summary>
/// <param name="Map">Size and density of the map.</param>
/// <param name="Seed">Random seed; <see langword="null"/> picks a random one. The seed is stored with the universe.</param>
/// <param name="TickLength">Real time per tick; <see langword="null"/> uses the configured default.</param>
public sealed record GenerateUniverseRequest(
    UniverseGenerationSettings Map,
    int? Seed = null,
    TimeSpan? TickLength = null);

/// <summary>An overview of one universe.</summary>
/// <param name="Id">Database id, used to address the universe in commands.</param>
/// <param name="Name">Display name.</param>
/// <param name="Seed">The seed the map was generated with.</param>
/// <param name="TickLength">Real time per tick.</param>
/// <param name="IsPaused">Whether universe time is currently stopped.</param>
/// <param name="GalaxyCount">Number of galaxies.</param>
/// <param name="PlanetCount">Number of planets.</param>
/// <param name="ClaimedPlanetCount">Number of planets owned by a player.</param>
/// <param name="CreatedAt">When the universe was generated.</param>
public sealed record UniverseSummary(
    int Id,
    string Name,
    int Seed,
    TimeSpan TickLength,
    bool IsPaused,
    int GalaxyCount,
    int PlanetCount,
    int ClaimedPlanetCount,
    DateTimeOffset CreatedAt);
