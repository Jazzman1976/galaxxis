using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using Galaxxis.Engine.Processing;

namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// Collects the engine's own measurements (<see cref="EngineMetrics"/>) while the engine runs inside this process:
/// the processing time of every event and of every cycle.
/// </summary>
/// <remarks>
/// Listens with a <see cref="MeterListener"/> to the meter <see cref="EngineMetrics.MeterName"/>, the same way
/// <c>dotnet-counters</c> or Application Insights would. When the engine runs in another process
/// (<c>Engine:Enabled = false</c> here), nothing is collected and the report says so.
/// </remarks>
internal sealed class EngineMeasurements : IDisposable
{
    private readonly MeterListener _listener = new();
    private readonly ConcurrentQueue<double> _eventDurations = new();
    private readonly ConcurrentQueue<double> _cycleDurations = new();

    /// <summary>Starts listening.</summary>
    public EngineMeasurements()
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == EngineMetrics.MeterName
                && instrument.Name is EngineMetrics.EventDurationName or EngineMetrics.CycleDurationName)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };

        _listener.SetMeasurementEventCallback<double>((instrument, value, _, _) =>
        {
            ConcurrentQueue<double> target = instrument.Name == EngineMetrics.EventDurationName ? _eventDurations : _cycleDurations;
            target.Enqueue(value);
        });

        _listener.Start();
    }

    /// <summary>Processing times of all events so far, in milliseconds.</summary>
    public IReadOnlyCollection<double> EventDurations => _eventDurations;

    /// <summary>Durations of all cycles so far, in milliseconds.</summary>
    public IReadOnlyCollection<double> CycleDurations => _cycleDurations;

    /// <summary>Stops listening.</summary>
    public void Dispose() => _listener.Dispose();
}
