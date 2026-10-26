using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// The engine's measurements, published with <see cref="System.Diagnostics.Metrics"/>: how long one event and one
/// cycle take. Any metrics consumer can read them without the engine knowing about it, for example
/// <c>dotnet-counters monitor --counters Galaxxis.Engine</c>, the load test tool (<c>Galaxxis.Tools.LoadTest</c>) or,
/// later, Application Insights on Azure.
/// </summary>
/// <remarks>
/// <para>
/// Created through <see cref="IMeterFactory"/> (registered by <c>AddGalaxxisEngine</c>), so there is no static meter
/// and every host or test gets its own instance. Recording costs next to nothing while nobody listens.
/// </para>
/// <para>
/// Used by <see cref="UniverseEventProcessor"/> (per event) and <see cref="EngineCycle"/> (per cycle). Part of the
/// Engine module; the names are public so consumers can subscribe without repeating strings.
/// </para>
/// </remarks>
public sealed class EngineMetrics
{
    /// <summary>Name of the meter all engine instruments belong to.</summary>
    public const string MeterName = "Galaxxis.Engine";

    /// <summary>
    /// Histogram, milliseconds: time to find the next due event and process it in its own transaction. This is the
    /// full cost of one event for the engine; its throughput limit is roughly 1000 divided by this value per second.
    /// </summary>
    public const string EventDurationName = "galaxxis.engine.event.duration";

    /// <summary>Histogram, milliseconds: time of one engine cycle over all running universes.</summary>
    public const string CycleDurationName = "galaxxis.engine.cycle.duration";

    private readonly Histogram<double> _eventDuration;
    private readonly Histogram<double> _cycleDuration;

    /// <summary>Creates the instruments.</summary>
    /// <param name="meterFactory">The host's meter factory; it owns and disposes the meter.</param>
    public EngineMetrics(IMeterFactory meterFactory)
    {
        Meter meter = meterFactory.Create(MeterName);

        _eventDuration = meter.CreateHistogram<double>(
            EventDurationName, unit: "ms", description: "Time to find and process one due game event, including its transaction.");
        _cycleDuration = meter.CreateHistogram<double>(
            CycleDurationName, unit: "ms", description: "Time of one engine cycle over all running universes.");
    }

    /// <summary>Records the processing time of one event.</summary>
    /// <param name="startTimestamp">The <see cref="Stopwatch.GetTimestamp"/> taken before the event was looked up.</param>
    internal void EventProcessed(long startTimestamp) =>
        _eventDuration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);

    /// <summary>Records the duration of one cycle.</summary>
    /// <param name="startTimestamp">The <see cref="Stopwatch.GetTimestamp"/> taken when the cycle started.</param>
    internal void CycleCompleted(long startTimestamp) =>
        _cycleDuration.Record(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
}
