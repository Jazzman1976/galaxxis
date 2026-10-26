using System.Globalization;
using Galaxxis.Persistence.Game.Entities;
using Microsoft.Extensions.Logging;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// The tick log: a readable, line-by-line account of what the engine does in every tick of every running universe
/// (tick reached, each event with its state change, each planet settled with its stock). Meant for watching the game
/// run during development, for example in the VS Code Debug Console.
/// </summary>
/// <remarks>
/// <para>
/// All lines are written at <see cref="LogLevel.Debug"/> under their own logging category <see cref="CategoryName"/>,
/// so the tick log is switched on and off by configuration alone:
/// <c>"Logging": { "LogLevel": { "Galaxxis.Engine.TickLog": "Debug" } }</c>. The web application enables it in
/// <c>appsettings.Development.json</c>; production stays quiet. When the category is off, nothing is formatted.
/// </para>
/// <para>
/// Universe time is derived from the wall clock and there is no loop over ticks (see README): a tick in which nothing
/// is due costs nothing and produces only its tick line. Resources are not produced tick by tick but settled when an
/// event needs a planet, so production shows up in the "settled" lines of the planets an event touches.
/// </para>
/// <para>
/// Used by <see cref="EngineCycle"/>, <see cref="GameEventExecutor"/> and <c>GameEventContext</c>; registered as a
/// singleton by <c>AddGalaxxisEngine</c>.
/// </para>
/// </remarks>
internal sealed class EngineTickLog
{
    /// <summary>The logging category of the tick log; set its level to <c>Debug</c> to see it.</summary>
    public const string CategoryName = "Galaxxis.Engine.TickLog";

    // Long payloads (for example mission reports) are cut, so one event stays one readable line.
    private const int MaxPayloadLength = 300;

    private readonly ILogger _logger;

    /// <summary>Creates the tick log with its own logging category.</summary>
    /// <param name="loggerFactory">The host's logger factory.</param>
    public EngineTickLog(ILoggerFactory loggerFactory) => _logger = loggerFactory.CreateLogger(CategoryName);

    /// <summary>Whether the tick log is switched on; callers can skip work that only serves the log.</summary>
    public bool IsEnabled => _logger.IsEnabled(LogLevel.Debug);

    /// <summary>Writes the line that opens a tick of a universe; the lines of its events follow.</summary>
    /// <param name="universeId">The universe.</param>
    /// <param name="universeName">Its name.</param>
    /// <param name="firstTick">The first tick reached since the previous line (equal to <paramref name="tick"/> normally).</param>
    /// <param name="tick">The current tick.</param>
    public void TickReached(int universeId, string universeName, long firstTick, long tick)
    {
        if (!IsEnabled)
        {
            return;
        }

        // When the engine wakes up less often than ticks pass (short tick length, slow cycle), several ticks were
        // reached at once. Nothing is lost: events of those ticks are processed now, each with its own tick.
        if (firstTick < tick)
        {
            _logger.LogDebug("=== Universe {UniverseId} \"{UniverseName}\" | ticks {FirstTick}-{Tick} ===", universeId, universeName, firstTick, tick);
        }
        else
        {
            _logger.LogDebug("=== Universe {UniverseId} \"{UniverseName}\" | tick {Tick} ===", universeId, universeName, tick);
        }
    }

    /// <summary>Writes what happened to one event: its state change and, depending on the new state, when or why.</summary>
    /// <param name="gameEvent">The event after handling.</param>
    /// <param name="previousState">The state before handling.</param>
    /// <param name="handledAtTick">The tick at which it was handled (before a scheduled event moved to its completion tick).</param>
    public void EventHandled(GameEvent gameEvent, GameEventState previousState, long handledAtTick)
    {
        if (!IsEnabled)
        {
            return;
        }

        string outcome = gameEvent.State switch
        {
            GameEventState.Scheduled => $"completes at tick {gameEvent.ProcessAtTick}",
            GameEventState.Rejected => $"reason: {gameEvent.Error}",
            GameEventState.Failed => $"error: {gameEvent.Error}",
            _ => "done",
        };

        string payload = gameEvent.Payload.Length <= MaxPayloadLength ? gameEvent.Payload : gameEvent.Payload[..MaxPayloadLength] + " ...";

        _logger.LogDebug(
            "    tick {Tick} | event #{EventId} {Type} | player {PlayerId}, planet {PlanetId} | {PreviousState} -> {State} | {Outcome} | {Payload}",
            handledAtTick, gameEvent.Id, gameEvent.Type, Describe(gameEvent.PlayerId), Describe(gameEvent.PlanetId),
            previousState, gameEvent.State, outcome, payload);
    }

    /// <summary>Writes the stock of a planet after it was settled to an event's tick.</summary>
    /// <param name="planet">The planet, already settled.</param>
    /// <param name="previousSettledTick">The tick its stock was correct for before settling.</param>
    public void PlanetSettled(Planet planet, long previousSettledTick)
    {
        if (!IsEnabled)
        {
            return;
        }

        _logger.LogDebug(
            "      planet {PlanetId} \"{PlanetName}\" settled ticks {FromTick}-{ToTick} | stock {Stock}",
            planet.Id, planet.Name, previousSettledTick, planet.ResourcesSettledTick, planet.Stock);
    }

    private static string Describe(int? id) => id?.ToString(CultureInfo.InvariantCulture) ?? "-";
}
