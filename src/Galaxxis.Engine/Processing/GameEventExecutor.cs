using Galaxxis.Engine.Handlers;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// Processes exactly one game event: accepts or completes it with its handler, in its own database transaction.
/// </summary>
/// <remarks>
/// <para>
/// <b>Failure isolation</b> is the main job of this class. Whatever goes wrong while handling one event (a bug in a
/// handler, an unreadable payload, a missing handler), the transaction is rolled back and the event is marked
/// <c>Failed</c> with the error, in a separate, minimal transaction. The engine then continues with the next event.
/// A single broken event can never stop a universe (the legacy engine retried such an event forever).
/// </para>
/// <para>
/// <b>Fresh state per event.</b> Every event gets a new DI scope and a new <see cref="GameDbContext"/> from the
/// factory, so nothing an earlier event loaded or changed can leak into the next one.
/// </para>
/// <para>
/// <b>Transient database errors</b> (normal on Azure SQL) are retried by EF Core's execution strategy; the whole
/// event is re-run from a clean state, which is safe because nothing was committed.
/// </para>
/// </remarks>
internal sealed class GameEventExecutor(
    IDbContextFactory<GameDbContext> contextFactory,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    EngineTickLog tickLog,
    ILogger<GameEventExecutor> logger)
{
    /// <summary>Processes one event.</summary>
    /// <param name="eventId">The event to process. Must be due; events in a final state are skipped.</param>
    /// <param name="cancellationToken">Signals shutdown. A cancelled event stays unchanged and is processed later.</param>
    /// <returns>A task that completes when the event's new state is saved.</returns>
    public async Task ExecuteAsync(long eventId, CancellationToken cancellationToken)
    {
        try
        {
            await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
            await using GameDbContext database = await contextFactory.CreateDbContextAsync(cancellationToken);

            IExecutionStrategy strategy = database.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(
                token => HandleInTransactionAsync(database, scope.ServiceProvider, eventId, token),
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (DbUpdateException exception) when (IsQueueBusyViolation(exception))
        {
            // The database's safety net caught a second scheduled order in the same queue.
            await FinishWithoutHandlerAsync(eventId, GameEventState.Rejected, GameEventMessages.QueueBusy, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Game event {EventId} failed and is marked as Failed.", eventId);
            await FinishWithoutHandlerAsync(eventId, GameEventState.Failed, Describe(exception), cancellationToken);
        }
    }

    private async Task HandleInTransactionAsync(
        GameDbContext database, IServiceProvider services, long eventId, CancellationToken cancellationToken)
    {
        // A retry of the execution strategy starts over: forget everything the failed attempt loaded.
        database.ChangeTracker.Clear();
        await using IDbContextTransaction transaction = await database.Database.BeginTransactionAsync(cancellationToken);

        GameEvent? gameEvent = await database.GameEvents.SingleOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        if (gameEvent is null || gameEvent.State is not (GameEventState.Requested or GameEventState.Scheduled))
        {
            return;
        }

        Universe universe = await database.Universes.SingleAsync(u => u.Id == gameEvent.UniverseId, cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();

        // Remembered for the tick log: accepting moves a scheduled event to its completion tick.
        GameEventState previousState = gameEvent.State;
        long handledAtTick = gameEvent.ProcessAtTick;

        IGameEventHandler? handler = GameEventHandlers.Find(services.GetServices<IGameEventHandler>(), gameEvent.Type);
        if (handler is null)
        {
            Finish(gameEvent, GameEventState.Failed, $"No handler is registered for the event type \"{gameEvent.Type}\".", now);
        }
        else if (gameEvent.State == GameEventState.Requested)
        {
            await AcceptAsync(database, handler, gameEvent, universe, now, cancellationToken);
        }
        else
        {
            await handler.CompleteAsync(new GameEventContext(database, gameEvent, universe, now, tickLog), cancellationToken);
            Finish(gameEvent, GameEventState.Done, error: null, now);
        }

        await database.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        // Written only after the commit, so the log never shows an outcome that was rolled back.
        tickLog.EventHandled(gameEvent, previousState, handledAtTick);
    }

    private async Task AcceptAsync(
        GameDbContext database,
        IGameEventHandler handler,
        GameEvent gameEvent,
        Universe universe,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        // The handler decides the queue, not the requester, so a request cannot bypass it.
        gameEvent.Queue = handler.Queue;

        if (gameEvent.Queue is not null && gameEvent.PlanetId is not null)
        {
            bool queueBusy = await database.GameEvents.AnyAsync(
                other => other.PlanetId == gameEvent.PlanetId
                    && other.Queue == gameEvent.Queue
                    && other.State == GameEventState.Scheduled
                    && other.Id != gameEvent.Id,
                cancellationToken);

            if (queueBusy)
            {
                Finish(gameEvent, GameEventState.Rejected, GameEventMessages.QueueBusy, now);
                return;
            }
        }

        AcceptResult result = await handler.AcceptAsync(new GameEventContext(database, gameEvent, universe, now, tickLog), cancellationToken);

        switch (result.Outcome)
        {
            case AcceptOutcome.Rejected:
                // Discard whatever the handler changed before rejecting (for example costs already deducted),
                // then record only the rejection.
                database.ChangeTracker.Clear();
                database.Attach(gameEvent);
                Finish(gameEvent, GameEventState.Rejected, result.Reason ?? "The request was rejected.", now);
                break;

            case AcceptOutcome.CompletedImmediately:
                Finish(gameEvent, GameEventState.Done, error: null, now);
                break;

            case AcceptOutcome.CompleteAfter:
                gameEvent.State = GameEventState.Scheduled;
                gameEvent.ProcessAtTick += universe.Timeline.TicksFor(result.Duration);
                break;
        }
    }

    /// <summary>Records a final state for an event in a separate transaction, without running any handler.</summary>
    private async Task FinishWithoutHandlerAsync(long eventId, GameEventState state, string error, CancellationToken cancellationToken)
    {
        await using GameDbContext database = await contextFactory.CreateDbContextAsync(cancellationToken);
        await database.Database.CreateExecutionStrategy().ExecuteAsync(async token =>
        {
            database.ChangeTracker.Clear();
            GameEvent? gameEvent = await database.GameEvents.SingleOrDefaultAsync(e => e.Id == eventId, token);
            if (gameEvent is not null)
            {
                GameEventState previousState = gameEvent.State;
                Finish(gameEvent, state, error, timeProvider.GetUtcNow());
                await database.SaveChangesAsync(token);
                tickLog.EventHandled(gameEvent, previousState, gameEvent.ProcessAtTick);
            }
        }, cancellationToken);
    }

    private static void Finish(GameEvent gameEvent, GameEventState state, string? error, DateTimeOffset now)
    {
        gameEvent.State = state;
        gameEvent.Error = error;
        gameEvent.CompletedAt = now;
    }

    private static bool IsQueueBusyViolation(DbUpdateException exception) =>
        exception.InnerException?.Message.Contains(GameEvent.OneScheduledPerQueueIndexName, StringComparison.Ordinal) == true;

    private static string Describe(Exception exception)
    {
        // Keep the stored error short and readable; the full exception is in the log.
        string message = $"{exception.GetType().Name}: {exception.Message}";
        return message.Length <= 2000 ? message : message[..2000];
    }
}
