using Galaxxis.Domain.Universes;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Tools.LoadTest;

/// <summary>
/// Watches the backlog of a universe once per second: how many events are due but not yet processed, and how far the
/// oldest of them lags behind the current tick.
/// </summary>
/// <remarks>
/// <para>
/// "Due" is the engine's own definition: <c>Requested</c> or <c>Scheduled</c> with <c>ProcessAtTick</c> at or before the
/// current tick. A few due events are normal even for a healthy engine: everything recorded since its last wake-up
/// (at 50 commands per second and a poll interval of one second, up to about 50). A backlog means the count keeps
/// growing and the lag exceeds a few seconds.
/// </para>
/// <para>Reads the database with its own context per sample; it never changes anything.</para>
/// </remarks>
internal sealed class BacklogMonitor(IDbContextFactory<GameDbContext> contextFactory, int universeId)
{
    private readonly List<BacklogSample> _samples = [];
    private readonly Lock _lock = new();

    /// <summary>All samples so far.</summary>
    public IReadOnlyList<BacklogSample> Samples
    {
        get
        {
            lock (_lock)
            {
                return _samples.ToList();
            }
        }
    }

    /// <summary>The most recent sample, if any.</summary>
    public BacklogSample? Latest
    {
        get
        {
            lock (_lock)
            {
                return _samples.Count == 0 ? null : _samples[^1];
            }
        }
    }

    /// <summary>Takes a sample every second until cancelled.</summary>
    /// <param name="cancellationToken">Stops sampling.</param>
    /// <returns>A task that completes when sampling has stopped.</returns>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                BacklogSample sample = await SampleAsync(cancellationToken);
                lock (_lock)
                {
                    _samples.Add(sample);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Normal end of the run.
        }
    }

    /// <summary>Measures the backlog right now.</summary>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The sample.</returns>
    public async Task<BacklogSample> SampleAsync(CancellationToken cancellationToken)
    {
        await using GameDbContext database = await contextFactory.CreateDbContextAsync(cancellationToken);
        Universe universe = await database.Universes.AsNoTracking().SingleAsync(u => u.Id == universeId, cancellationToken);
        UniverseTimeline timeline = universe.Timeline;
        long currentTick = timeline.TickAt(DateTimeOffset.UtcNow);

        var due = await database.GameEvents
            .Where(e => e.UniverseId == universeId
                && (e.State == GameEventState.Requested || e.State == GameEventState.Scheduled)
                && e.ProcessAtTick <= currentTick)
            .GroupBy(_ => 1)
            .Select(group => new { Count = group.Count(), OldestTick = group.Min(e => e.ProcessAtTick) })
            .SingleOrDefaultAsync(cancellationToken);

        TimeSpan lag = due is null ? TimeSpan.Zero : timeline.DurationOf(currentTick - due.OldestTick);
        return new BacklogSample(DateTimeOffset.UtcNow, due?.Count ?? 0, lag);
    }
}

/// <summary>One measurement of the backlog.</summary>
/// <param name="At">When it was taken.</param>
/// <param name="DueEvents">Events due but not processed yet.</param>
/// <param name="OldestLag">How far the oldest due event lags behind the current tick (zero without due events).</param>
internal sealed record BacklogSample(DateTimeOffset At, int DueEvents, TimeSpan OldestLag);
