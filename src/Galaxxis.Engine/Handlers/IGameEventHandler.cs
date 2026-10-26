namespace Galaxxis.Engine.Handlers;

/// <summary>
/// Implements one game event type: decides whether a request is accepted and applies its effect when it is due.
/// </summary>
/// <remarks>
/// <para>
/// <b>Lifecycle.</b> A player command arrives as a <c>Requested</c> event. When its tick is reached, the engine calls
/// <see cref="AcceptAsync"/>: the handler checks the rules against the current game state, pays costs, and answers
/// with an <see cref="AcceptResult"/>. If the result is "complete after a duration", the engine schedules the event,
/// and when that tick is reached it calls <see cref="CompleteAsync"/> to apply the effect.
/// </para>
/// <para>
/// <b>Transactions.</b> Each call runs inside its own database transaction together with the event's state change.
/// Everything the handler changes through <see cref="GameEventContext.Database"/>, including follow-up events it
/// schedules, is saved together or not at all. If the handler throws, nothing is saved and the event is marked
/// <c>Failed</c>; other events are not affected. A handler never calls <c>SaveChanges</c> itself.
/// </para>
/// <para>
/// <b>Adding a handler.</b> Implement this interface in the Engine assembly: handlers are found automatically at
/// startup. Handlers in other assemblies (for example tests) are registered with
/// <c>services.AddScoped&lt;IGameEventHandler, MyHandler&gt;()</c>. Each event type must have exactly one handler;
/// duplicates stop the host at startup.
/// </para>
/// </remarks>
public interface IGameEventHandler
{
    /// <summary>The event type this handler implements, for example <c>GameEventTypes.BuildBuilding</c>.</summary>
    string EventType { get; }

    /// <summary>
    /// The planet queue events of this type belong to, or <see langword="null"/> if they may run in parallel.
    /// While an event of the queue is scheduled on a planet, the engine rejects further requests for that queue
    /// before calling <see cref="AcceptAsync"/>, so handlers need not check it.
    /// </summary>
    string? Queue { get; }

    /// <summary>
    /// Decides about a requested event. Check the rules, pay costs, reserve what the event needs.
    /// </summary>
    /// <param name="context">The event, the database and helpers.</param>
    /// <param name="cancellationToken">Signals that the engine is shutting down.</param>
    /// <returns>Rejected, completed immediately, or complete after a duration.</returns>
    Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken);

    /// <summary>
    /// Applies the effect of a scheduled event whose tick has been reached.
    /// </summary>
    /// <param name="context">The event, the database and helpers.</param>
    /// <param name="cancellationToken">Signals that the engine is shutting down.</param>
    /// <returns>A task that completes when the effect has been applied to the tracked entities.</returns>
    Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken);
}

/// <summary>
/// The decision of <see cref="IGameEventHandler.AcceptAsync"/>. Create it with one of the static methods.
/// </summary>
public sealed record AcceptResult
{
    private AcceptResult(AcceptOutcome outcome, TimeSpan duration, string? reason)
    {
        Outcome = outcome;
        Duration = duration;
        Reason = reason;
    }

    /// <summary>What happens with the event.</summary>
    public AcceptOutcome Outcome { get; }

    /// <summary>For <see cref="AcceptOutcome.CompleteAfter"/>: how long until the effect happens.</summary>
    public TimeSpan Duration { get; }

    /// <summary>For <see cref="AcceptOutcome.Rejected"/>: why, in words a player understands.</summary>
    public string? Reason { get; }

    /// <summary>
    /// The command is refused. Anything the handler changed before deciding to reject is discarded by the engine,
    /// so a handler may pay costs first and still reject afterwards.
    /// </summary>
    /// <param name="reason">Why, in words a player understands, for example "Not enough ore".</param>
    /// <returns>The result.</returns>
    public static AcceptResult Reject(string reason) => new(AcceptOutcome.Rejected, TimeSpan.Zero, reason);

    /// <summary>The command took effect during acceptance, for example a production speed change.</summary>
    /// <returns>The result.</returns>
    public static AcceptResult CompletedImmediately() => new(AcceptOutcome.CompletedImmediately, TimeSpan.Zero, null);

    /// <summary>The command is accepted; <see cref="IGameEventHandler.CompleteAsync"/> runs after <paramref name="duration"/>.</summary>
    /// <param name="duration">Real time until the effect happens; converted to ticks by the engine (rounded up).</param>
    /// <returns>The result.</returns>
    public static AcceptResult CompleteAfter(TimeSpan duration) => new(AcceptOutcome.CompleteAfter, duration, null);
}

/// <summary>The possible decisions about a requested event.</summary>
public enum AcceptOutcome
{
    /// <summary>Refused for a game reason; the event ends as <c>Rejected</c>.</summary>
    Rejected,

    /// <summary>Took effect immediately; the event ends as <c>Done</c>.</summary>
    CompletedImmediately,

    /// <summary>Accepted; the event becomes <c>Scheduled</c> and completes later.</summary>
    CompleteAfter,
}
