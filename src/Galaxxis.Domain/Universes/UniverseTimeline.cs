namespace Galaxxis.Domain.Universes;

/// <summary>
/// The clock of one universe: converts real time into universe ticks and back.
/// </summary>
/// <remarks>
/// <para>
/// <b>Universe time is derived from the wall clock</b> (design decision 2026-09-16). The current tick is
/// <c>(now - Epoch - paused time) / TickLength</c>, rounded down. Nothing needs to count ticks, the clock cannot
/// drift under server load, and time keeps running while the server is down, like in similar browser games.
/// </para>
/// <para>
/// <b>Pauses</b> are the only way to stop time: <see cref="TotalPausedDuration"/> holds all finished pauses,
/// <see cref="PausedSince"/> the one in progress. While paused, the current tick does not advance.
/// </para>
/// <para>
/// Game rules express durations as <see cref="TimeSpan"/> (a building takes 25 seconds); this type turns them
/// into ticks, so a universe with a different tick length still has the same real durations.
/// </para>
/// </remarks>
/// <param name="Epoch">The moment tick 0 started.</param>
/// <param name="TickLength">Real time one tick represents. Must be positive.</param>
/// <param name="TotalPausedDuration">Sum of all finished pauses.</param>
/// <param name="PausedSince">Start of the current pause, or <see langword="null"/> while running.</param>
public readonly record struct UniverseTimeline(
    DateTimeOffset Epoch,
    TimeSpan TickLength,
    TimeSpan TotalPausedDuration,
    DateTimeOffset? PausedSince)
{
    /// <summary>Whether time is currently stopped.</summary>
    public bool IsPaused => PausedSince is not null;

    /// <summary>Calculates the current tick.</summary>
    /// <param name="now">The current real time.</param>
    /// <returns>The tick; 0 before the epoch.</returns>
    public long TickAt(DateTimeOffset now)
    {
        // While paused, time stands still at the moment the pause started.
        DateTimeOffset effectiveNow = PausedSince is DateTimeOffset pausedSince && pausedSince < now ? pausedSince : now;
        TimeSpan universeTime = effectiveNow - Epoch - TotalPausedDuration;

        return universeTime <= TimeSpan.Zero ? 0 : universeTime.Ticks / TickLength.Ticks;
    }

    /// <summary>
    /// Converts a duration into a number of ticks, rounded up, so something never finishes earlier than its duration.
    /// </summary>
    /// <param name="duration">The duration, for example a construction time.</param>
    /// <returns>The number of ticks; 0 for a zero or negative duration.</returns>
    public long TicksFor(TimeSpan duration) =>
        duration <= TimeSpan.Zero ? 0 : (duration.Ticks + TickLength.Ticks - 1) / TickLength.Ticks;

    /// <summary>Converts a number of ticks into the real time they represent.</summary>
    /// <param name="ticks">The number of ticks.</param>
    /// <returns>The duration; zero for zero or negative tick counts.</returns>
    public TimeSpan DurationOf(long ticks) => ticks <= 0 ? TimeSpan.Zero : TimeSpan.FromTicks(ticks * TickLength.Ticks);
}
