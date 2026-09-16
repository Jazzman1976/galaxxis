namespace Galaxxis.Persistence.Game.Entities;

/// <summary>
/// Something that happens in a universe at a certain tick: a player command (build, change speed, attack)
/// or a system action (for example spawning quests later).
/// </summary>
/// <remarks>
/// <para>
/// <b>Lifecycle</b> (see <see cref="GameEventState"/>): the web application records a command as
/// <see cref="GameEventState.Requested"/>. The engine then decides: it either accepts it
/// (<see cref="GameEventState.Scheduled"/>, costs paid, <see cref="ProcessAtTick"/> moved to when the effect
/// happens) or rejects it (<see cref="GameEventState.Rejected"/> with a reason in <see cref="Error"/>). When the
/// scheduled tick is reached, the engine applies the effect (<see cref="GameEventState.Done"/>). A technical
/// failure marks the event <see cref="GameEventState.Failed"/> without affecting any other event.
/// </para>
/// <para>
/// <b>Deliberately general.</b> An event belongs to a universe; the player and the planet are optional, and
/// everything else is in the JSON <see cref="Payload"/>. New features (quests, trading) add new event types
/// and payloads without changing this table.
/// </para>
/// <para>
/// Events are never deleted by the engine. Finished events are the history players can look at later.
/// </para>
/// </remarks>
public sealed class GameEvent
{
    /// <summary>Primary key. Also the tie-breaker for events due at the same tick: lower ids run first.</summary>
    public long Id { get; set; }

    /// <summary>The universe the event happens in.</summary>
    public int UniverseId { get; set; }

    /// <summary>Navigation to the universe.</summary>
    public Universe Universe { get; set; } = null!;

    /// <summary>
    /// What kind of event this is, for example <c>"BuildBuilding"</c>. The engine finds the handler by this name.
    /// Stored as text on purpose, so new features can add types without a database change.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Where the event is in its lifecycle.</summary>
    public GameEventState State { get; set; }

    /// <summary>The player who issued the command; <see langword="null"/> for events triggered by the game itself.</summary>
    public int? PlayerId { get; set; }

    /// <summary>Navigation to the player, if any.</summary>
    public Player? Player { get; set; }

    /// <summary>The planet the event concerns; <see langword="null"/> for events not bound to a planet.</summary>
    public int? PlanetId { get; set; }

    /// <summary>Navigation to the planet, if any.</summary>
    public Planet? Planet { get; set; }

    /// <summary>
    /// Events of the same queue on the same planet run strictly one after another, for example the
    /// construction queue or the shipyard. The database allows only one <see cref="GameEventState.Scheduled"/>
    /// event per planet and queue. <see langword="null"/> for events that may run in parallel.
    /// </summary>
    public string? Queue { get; set; }

    /// <summary>The tick at which the command was issued.</summary>
    public long RequestedTick { get; set; }

    /// <summary>
    /// The tick at which the engine handles the event next: for a requested event the tick it was issued,
    /// for a scheduled event the tick its effect happens.
    /// </summary>
    public long ProcessAtTick { get; set; }

    /// <summary>Event-type specific data as JSON, for example which building to upgrade.</summary>
    public string Payload { get; set; } = "{}";

    /// <summary>Why the event was rejected or failed; <see langword="null"/> otherwise.</summary>
    public string? Error { get; set; }

    /// <summary>When the event was recorded (real time).</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>When the event reached <see cref="GameEventState.Done"/>, <see cref="GameEventState.Rejected"/> or <see cref="GameEventState.Failed"/> (real time).</summary>
    public DateTimeOffset? CompletedAt { get; set; }
}
