using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// Works through the due events of one universe, strictly in order of their tick.
/// </summary>
/// <remarks>
/// <para>
/// <b>One event at a time, always the earliest.</b> Before each event, the processor asks the database for the single
/// next due event (lowest tick, then lowest id). Accepting an event can schedule it, or a follow-up, at a tick that
/// is already due; asking again each time guarantees that it is handled in the right order. The extra query costs
/// about a millisecond and keeps the ordering rule simple and certain.
/// </para>
/// <para>
/// <b>Only due events.</b> Due means <c>ProcessAtTick</c> is at or before the universe's current tick, derived from
/// the wall clock. A paused universe has no due events. Events of empty ticks cost nothing: after an outage of
/// hours, only the events that fell due are processed, not every tick in between.
/// </para>
/// </remarks>
internal sealed class UniverseEventProcessor(
    IDbContextFactory<GameDbContext> contextFactory,
    GameEventExecutor executor,
    TimeProvider timeProvider)
{
    /// <summary>Processes due events of one universe until none is due or the limit is reached.</summary>
    /// <param name="universeId">The universe.</param>
    /// <param name="maxEvents">Upper limit for this call, so other universes get their turn.</param>
    /// <param name="cancellationToken">Signals shutdown.</param>
    /// <returns>How many events were processed, and whether due events remain.</returns>
    public async Task<UniverseProcessingResult> ProcessDueEventsAsync(int universeId, int maxEvents, CancellationToken cancellationToken)
    {
        int processed = 0;

        while (processed < maxEvents)
        {
            long? nextEventId = await NextDueEventIdAsync(universeId, cancellationToken);
            if (nextEventId is not long eventId)
            {
                return new UniverseProcessingResult(processed, MoreEventsDue: false);
            }

            await executor.ExecuteAsync(eventId, cancellationToken);
            processed++;
        }

        bool moreDue = await NextDueEventIdAsync(universeId, cancellationToken) is not null;
        return new UniverseProcessingResult(processed, moreDue);
    }

    private async Task<long?> NextDueEventIdAsync(int universeId, CancellationToken cancellationToken)
    {
        await using GameDbContext database = await contextFactory.CreateDbContextAsync(cancellationToken);

        Universe? universe = await database.Universes.AsNoTracking().SingleOrDefaultAsync(u => u.Id == universeId, cancellationToken);
        if (universe is null || universe.IsPaused)
        {
            return null;
        }

        long currentTick = universe.Timeline.TickAt(timeProvider.GetUtcNow());

        return await database.GameEvents
            .Where(e => e.UniverseId == universeId
                && (e.State == GameEventState.Requested || e.State == GameEventState.Scheduled)
                && e.ProcessAtTick <= currentTick)
            .OrderBy(e => e.ProcessAtTick)
            .ThenBy(e => e.Id)
            .Select(e => (long?)e.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}

/// <summary>The outcome of processing one universe for a while.</summary>
/// <param name="ProcessedEvents">Number of events handled.</param>
/// <param name="MoreEventsDue">Whether due events remain because the limit was reached (a backlog).</param>
internal sealed record UniverseProcessingResult(int ProcessedEvents, bool MoreEventsDue);
