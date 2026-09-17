using Galaxxis.Domain.Resources;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Engine.Handlers;

/// <summary>
/// Everything a handler needs while it handles one event: the event, its universe, the database context of the
/// event's transaction, the tick, and helpers for the common steps.
/// </summary>
/// <remarks>
/// Created by the engine for every call of <see cref="IGameEventHandler.AcceptAsync"/> or
/// <see cref="IGameEventHandler.CompleteAsync"/> and valid only during that call.
/// </remarks>
public sealed class GameEventContext
{
    /// <summary>Creates the context. Only the engine does this.</summary>
    /// <param name="database">The database context of the event's transaction.</param>
    /// <param name="gameEvent">The event being handled (tracked).</param>
    /// <param name="universe">The universe of the event (tracked).</param>
    /// <param name="now">The real time the engine uses for this event.</param>
    internal GameEventContext(GameDbContext database, GameEvent gameEvent, Universe universe, DateTimeOffset now)
    {
        Database = database;
        Event = gameEvent;
        Universe = universe;
        Now = now;
        Tick = gameEvent.ProcessAtTick;
    }

    /// <summary>
    /// The database context of this event's transaction. Load and change entities through it; the engine saves.
    /// </summary>
    public GameDbContext Database { get; }

    /// <summary>The event being handled.</summary>
    public GameEvent Event { get; }

    /// <summary>The universe the event belongs to.</summary>
    public Universe Universe { get; }

    /// <summary>
    /// The universe tick at which the event happens. All game state is settled to this tick, not to the current
    /// real time, so an event processed late still happens "at its time".
    /// </summary>
    public long Tick { get; }

    /// <summary>The real time used for timestamps such as <c>CreatedAt</c>.</summary>
    public DateTimeOffset Now { get; }

    /// <summary>Reads the event payload.</summary>
    /// <typeparam name="TPayload">The payload record type of this event type.</typeparam>
    /// <returns>The payload.</returns>
    /// <exception cref="System.Text.Json.JsonException">The stored payload does not match the type; the event fails.</exception>
    public TPayload ReadPayload<TPayload>() => GameEventPayload.Deserialize<TPayload>(Event.Payload);

    /// <summary>Replaces the event payload, for example to store a mission report in the finished event.</summary>
    /// <typeparam name="TPayload">The payload record type of this event type.</typeparam>
    /// <param name="payload">The new payload.</param>
    public void WritePayload<TPayload>(TPayload payload) => Event.Payload = GameEventPayload.Serialize(payload);

    /// <summary>
    /// Loads the planet the event concerns and checks that it belongs to the player who issued the event.
    /// The planet's stock is settled to <see cref="Tick"/>.
    /// </summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The planet, or <see langword="null"/> if the event has no planet, it does not exist, or it belongs to someone else.</returns>
    public async Task<Planet?> LoadOwnedPlanetAsync(CancellationToken cancellationToken)
    {
        if (Event.PlanetId is not int planetId || Event.PlayerId is not int playerId)
        {
            return null;
        }

        Planet? planet = await LoadPlanetAsync(planetId, cancellationToken);
        return planet is not null && planet.OwnerPlayerId == playerId && planet.UniverseId == Universe.Id ? planet : null;
    }

    /// <summary>Loads any planet of this universe with its owner, with the stock settled to <see cref="Tick"/>.</summary>
    /// <param name="planetId">The planet.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The planet, or <see langword="null"/> if it does not exist in this universe.</returns>
    public async Task<Planet?> LoadPlanetAsync(int planetId, CancellationToken cancellationToken)
    {
        Planet? planet = await Database.Planets
            .Include(p => p.Owner)
            .SingleOrDefaultAsync(p => p.Id == planetId && p.UniverseId == Universe.Id, cancellationToken);

        planet?.SettleResourcesTo(Tick, Universe.Timeline);
        return planet;
    }

    /// <summary>
    /// Schedules a follow-up event, for example the return flight after an attack. It is saved in the same
    /// transaction as the current event, so it exists if and only if the current event succeeds.
    /// </summary>
    /// <typeparam name="TPayload">The payload record type of the follow-up event.</typeparam>
    /// <param name="eventType">The follow-up event type.</param>
    /// <param name="payload">Its payload.</param>
    /// <param name="delay">Real time from <see cref="Tick"/> until the follow-up completes.</param>
    /// <param name="playerId">The player it belongs to; <see langword="null"/> for system events.</param>
    /// <param name="planetId">The planet it concerns; <see langword="null"/> if none.</param>
    /// <returns>The new event entity (not yet saved, so its id is not known yet).</returns>
    public GameEvent ScheduleFollowUp<TPayload>(string eventType, TPayload payload, TimeSpan delay, int? playerId, int? planetId)
    {
        long processAtTick = Tick + Universe.Timeline.TicksFor(delay);

        GameEvent followUp = new()
        {
            UniverseId = Universe.Id,
            Type = eventType,
            State = GameEventState.Scheduled,
            PlayerId = playerId,
            PlanetId = planetId,
            RequestedTick = Tick,
            ProcessAtTick = processAtTick,
            Payload = GameEventPayload.Serialize(payload),
            CreatedAt = Now,
        };

        Database.GameEvents.Add(followUp);
        return followUp;
    }

    /// <summary>
    /// Builds the standard rejection message for a payment that cannot be made, naming only what is missing.
    /// </summary>
    /// <param name="stock">What is available.</param>
    /// <param name="cost">What is needed.</param>
    /// <returns>A message such as "Not enough resources: 12 ore, 3 mana missing."</returns>
    public static string NotEnoughResourcesMessage(ResourceAmounts stock, ResourceAmounts cost)
    {
        ResourceAmounts missing = stock.MissingFor(cost);
        IEnumerable<string> parts = ResourceTypes.All
            .Where(resource => missing[resource] > 0)
            .Select(resource => $"{Math.Ceiling(missing[resource]):0} {resource.ToString().ToLowerInvariant()}");

        return $"Not enough resources: {string.Join(", ", parts)} missing.";
    }
}
