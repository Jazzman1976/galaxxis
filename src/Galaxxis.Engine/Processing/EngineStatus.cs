namespace Galaxxis.Engine.Processing;

/// <summary>
/// What the engine of this process did most recently. Written by the engine loop, read by the health check.
/// </summary>
/// <remarks>Thread-safe: the engine loop writes while web requests for the health endpoint read.</remarks>
internal sealed class EngineStatus
{
    private readonly Lock _lock = new();
    private EngineStatusSnapshot _snapshot = new(null, null, null, [], 0, []);

    /// <summary>The current state.</summary>
    public EngineStatusSnapshot Current
    {
        get
        {
            lock (_lock)
            {
                return _snapshot;
            }
        }
    }

    /// <summary>Records a completed cycle.</summary>
    /// <param name="at">When the cycle finished.</param>
    /// <param name="heldUniverses">Universes this instance processes.</param>
    /// <param name="processedEvents">Events handled in the cycle.</param>
    /// <param name="backloggedUniverses">Universes that still had due events at the end of the cycle.</param>
    public void RecordSuccess(DateTimeOffset at, IReadOnlyList<int> heldUniverses, int processedEvents, IReadOnlyList<int> backloggedUniverses)
    {
        lock (_lock)
        {
            _snapshot = _snapshot with
            {
                LastSuccessfulCycle = at,
                HeldUniverses = heldUniverses,
                EventsInLastCycle = processedEvents,
                BackloggedUniverses = backloggedUniverses,
            };
        }
    }

    /// <summary>Records a cycle that stopped with an error.</summary>
    /// <param name="at">When the error occurred.</param>
    /// <param name="error">A short description.</param>
    public void RecordFailure(DateTimeOffset at, string error)
    {
        lock (_lock)
        {
            _snapshot = _snapshot with { LastFailedCycle = at, LastError = error };
        }
    }
}

/// <summary>An immutable picture of the engine state.</summary>
/// <param name="LastSuccessfulCycle">When a cycle last completed; <see langword="null"/> before the first.</param>
/// <param name="LastFailedCycle">When a cycle last failed; <see langword="null"/> if none did.</param>
/// <param name="LastError">The error of the last failed cycle.</param>
/// <param name="HeldUniverses">Universes this instance processes (holds the lease for).</param>
/// <param name="EventsInLastCycle">Events handled in the last completed cycle.</param>
/// <param name="BackloggedUniverses">Universes with due events left after the last cycle.</param>
internal sealed record EngineStatusSnapshot(
    DateTimeOffset? LastSuccessfulCycle,
    DateTimeOffset? LastFailedCycle,
    string? LastError,
    IReadOnlyList<int> HeldUniverses,
    int EventsInLastCycle,
    IReadOnlyList<int> BackloggedUniverses);
