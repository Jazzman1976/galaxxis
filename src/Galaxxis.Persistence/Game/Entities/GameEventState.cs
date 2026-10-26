namespace Galaxxis.Persistence.Game.Entities;

/// <summary>
/// The lifecycle states of a <see cref="GameEvent"/>. Stored as text, so the table stays readable in SQL.
/// </summary>
public enum GameEventState
{
    /// <summary>Recorded by the web application; the engine has not decided about it yet.</summary>
    Requested,

    /// <summary>Accepted by the engine (costs paid); its effect happens at <see cref="GameEvent.ProcessAtTick"/>.</summary>
    Scheduled,

    /// <summary>The effect has been applied. Final state.</summary>
    Done,

    /// <summary>The engine refused the command for a game reason, for example not enough resources. Final state.</summary>
    Rejected,

    /// <summary>A technical error occurred while handling the event; see <see cref="GameEvent.Error"/>. Final state.</summary>
    Failed,
}
