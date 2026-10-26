using Galaxxis.Domain.Buildings;
using Galaxxis.Persistence.Game.Entities;

namespace Galaxxis.Application.Commands;

/// <summary>
/// Records the commands a player gives and reports what became of them.
/// </summary>
/// <remarks>
/// <para>
/// <b>Recording, not deciding.</b> A command is stored as a <c>Requested</c> game event; the engine decides on it at the
/// next wake-up (usually within a second). Before recording, the service runs the same checks the engine will run
/// (ownership, running construction, resources) so the player gets an immediate answer in the common cases. The
/// engine checks again, so nothing is lost if the situation changed in between.
/// </para>
/// <para>
/// <b>Ownership.</b> Every command is tied to the signed-in account (<c>accountId</c>). Planets of other players are
/// reported as not found, never revealed.
/// </para>
/// </remarks>
public interface ICommandService
{
    /// <summary>Requests upgrading a building on an own planet by one level.</summary>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="planetId">The planet.</param>
    /// <param name="building">The building to upgrade.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>Recorded (with the event id), refused (with the reason) or not found.</returns>
    Task<CommandResult> RequestBuildAsync(string accountId, int planetId, BuildingType building, CancellationToken cancellationToken = default);

    /// <summary>Requests a new production speed for a resource building on an own planet.</summary>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="planetId">The planet.</param>
    /// <param name="building">The resource building.</param>
    /// <param name="speedPercent">The new speed: a multiple of 10 between 0 and 100.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>Recorded (with the event id), refused (with the reason) or not found.</returns>
    Task<CommandResult> RequestProductionSpeedAsync(string accountId, int planetId, BuildingType building, int speedPercent, CancellationToken cancellationToken = default);

    /// <summary>Gets the current state of a command of the signed-in account.</summary>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="eventId">The event id returned when the command was recorded.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The status, or <see langword="null"/> if the command does not exist or belongs to someone else.</returns>
    Task<CommandStatusView?> GetStatusAsync(string accountId, long eventId, CancellationToken cancellationToken = default);
}

/// <summary>The answer to a command request.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="EventId">For <see cref="CommandOutcome.Recorded"/>: the event id to follow the command.</param>
/// <param name="Message">For <see cref="CommandOutcome.Refused"/>: why, in words a player understands.</param>
public sealed record CommandResult(CommandOutcome Outcome, long? EventId, string? Message)
{
    /// <summary>The command was recorded and waits for the engine.</summary>
    /// <param name="eventId">The new event.</param>
    /// <returns>The result.</returns>
    public static CommandResult Recorded(long eventId) => new(CommandOutcome.Recorded, eventId, null);

    /// <summary>The command was refused right away.</summary>
    /// <param name="message">The reason.</param>
    /// <returns>The result.</returns>
    public static CommandResult Refused(string message) => new(CommandOutcome.Refused, null, message);

    /// <summary>The planet does not exist or belongs to someone else.</summary>
    public static CommandResult NotFound { get; } = new(CommandOutcome.NotFound, null, null);
}

/// <summary>Possible answers to a command request.</summary>
public enum CommandOutcome
{
    /// <summary>Recorded; the engine decides shortly.</summary>
    Recorded,

    /// <summary>Refused immediately; nothing was recorded.</summary>
    Refused,

    /// <summary>The planet does not exist or belongs to someone else.</summary>
    NotFound,
}

/// <summary>The current state of a command.</summary>
/// <param name="EventId">The event.</param>
/// <param name="Type">The event type.</param>
/// <param name="State">Requested, Scheduled, Done, Rejected or Failed.</param>
/// <param name="Error">Why it was rejected or failed.</param>
/// <param name="CompletesAt">For scheduled commands: the estimated completion time.</param>
public sealed record CommandStatusView(long EventId, string Type, GameEventState State, string? Error, DateTimeOffset? CompletesAt)
{
    /// <summary>Whether the engine has decided (anything other than still waiting).</summary>
    public bool IsDecided => State != GameEventState.Requested;
}
