using System.ComponentModel.DataAnnotations;

namespace Galaxxis.Engine.Configuration;

/// <summary>
/// Settings of the event processing engine, bound from the configuration section <c>Engine</c>.
/// </summary>
public sealed class EngineOptions
{
    /// <summary>Name of the configuration section the options are bound from.</summary>
    public const string SectionName = "Engine";

    /// <summary>
    /// How often the engine wakes up to look for due events.
    /// </summary>
    /// <remarks>
    /// This is not the length of a game tick. Universe time is derived from the wall clock, so a
    /// longer interval only delays when due events are processed; it never changes game time or
    /// build durations. One second keeps player feedback snappy at negligible database cost.
    /// </remarks>
    [Range(typeof(TimeSpan), "00:00:00.100", "00:01:00",
        ErrorMessage = "Engine:PollInterval must be between 100 milliseconds and 1 minute.")]
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The maximum number of events processed for one universe in one wake-up. When more events are due (for example
    /// after a long outage), the rest follows in the next wake-ups, so no single universe can block the others.
    /// </summary>
    [Range(1, 100_000, ErrorMessage = "Engine:MaxEventsPerUniversePerCycle must be between 1 and 100000.")]
    public int MaxEventsPerUniversePerCycle { get; set; } = 1000;
}
