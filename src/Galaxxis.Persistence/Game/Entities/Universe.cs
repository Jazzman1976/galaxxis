using Galaxxis.Domain.Universes;

namespace Galaxxis.Persistence.Game.Entities;

/// <summary>
/// A universe: an independent game world with its own galaxies, players, events and clock.
/// </summary>
/// <remarks>
/// <para>
/// <b>Universe time.</b> The current tick is derived from the wall clock (by the Engine):
/// <c>(now - Epoch - TotalPausedDuration) / TickLength</c>. Time therefore keeps running while the server
/// is down, and only an explicit pause stops it.
/// </para>
/// <para>
/// <b>Pausing.</b> While <see cref="PausedSince"/> is set, the universe is paused. Resuming adds the pause
/// length to <see cref="TotalPausedDuration"/>, so the clock continues exactly where it stopped.
/// A freshly generated universe starts paused, so an administrator can check it before opening it.
/// </para>
/// </remarks>
public sealed class Universe
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>Display name of the universe.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The random seed the universe was generated with; the same seed recreates the same map.</summary>
    public int GenerationSeed { get; set; }

    /// <summary>The moment tick 0 started.</summary>
    public DateTimeOffset Epoch { get; set; }

    /// <summary>Real time that one tick represents.</summary>
    public TimeSpan TickLength { get; set; }

    /// <summary>When the current pause started; <see langword="null"/> while the universe is running.</summary>
    public DateTimeOffset? PausedSince { get; set; }

    /// <summary>Sum of all completed pauses. Subtracted from the elapsed real time when calculating the tick.</summary>
    public TimeSpan TotalPausedDuration { get; set; }

    /// <summary>When the universe was generated.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>Whether the universe is currently paused. Not stored; derived from <see cref="PausedSince"/>.</summary>
    public bool IsPaused => PausedSince is not null;

    /// <summary>
    /// The universe clock (Domain) built from the time columns: current tick, tick/duration conversion.
    /// Not stored itself.
    /// </summary>
    public UniverseTimeline Timeline => new(Epoch, TickLength, TotalPausedDuration, PausedSince);

    /// <summary>The galaxies of the universe.</summary>
    public List<Galaxy> Galaxies { get; set; } = [];
}
