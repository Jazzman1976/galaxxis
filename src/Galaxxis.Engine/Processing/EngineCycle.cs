using Galaxxis.Domain.Universes;
using Galaxxis.Engine.Configuration;
using Galaxxis.Persistence.Configuration;
using Galaxxis.Persistence.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// One wake-up of the engine: for every running universe, make sure this instance holds its lease, then process its
/// due events.
/// </summary>
/// <remarks>
/// Called periodically by <see cref="UniverseProcessorService"/>. Kept separate from the background service so tests
/// can run exactly one cycle and simulate several engine instances.
/// </remarks>
internal sealed class EngineCycle(
    IDbContextFactory<GameDbContext> contextFactory,
    UniverseEventProcessor processor,
    EngineStatus status,
    IOptions<EngineOptions> engineOptions,
    IOptions<DatabaseConnectionOptions> connectionOptions,
    TimeProvider timeProvider,
    EngineTickLog tickLog,
    ILogger<EngineCycle> logger) : IAsyncDisposable
{
    // The leases this instance holds, by universe id. Only the single engine loop uses this dictionary.
    private readonly Dictionary<int, UniverseLease> _leases = [];

    // For the tick log: the last tick written per universe, so every tick line appears once. Only the engine loop uses it.
    private readonly Dictionary<int, long> _lastLoggedTicks = [];

    /// <summary>Runs one cycle over all running universes.</summary>
    /// <param name="cancellationToken">Signals shutdown.</param>
    /// <returns>A task that completes when the cycle is done.</returns>
    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        List<RunningUniverse> runningUniverses = await RunningUniversesAsync(cancellationToken);
        List<int> runningUniverseIds = runningUniverses.Select(universe => universe.Id).ToList();
        await ReleaseLeasesExceptAsync(runningUniverseIds);

        int processedEvents = 0;
        List<int> backloggedUniverses = [];

        foreach (RunningUniverse universe in runningUniverses)
        {
            int universeId = universe.Id;
            if (!await EnsureLeaseAsync(universeId, cancellationToken))
            {
                // Another instance processes this universe; this instance is its standby.
                _lastLoggedTicks.Remove(universeId);
                continue;
            }

            LogTickReached(universe);

            UniverseProcessingResult result = await processor.ProcessDueEventsAsync(
                universeId, engineOptions.Value.MaxEventsPerUniversePerCycle, cancellationToken);

            processedEvents += result.ProcessedEvents;
            if (result.MoreEventsDue)
            {
                backloggedUniverses.Add(universeId);
            }
        }

        status.RecordSuccess(timeProvider.GetUtcNow(), _leases.Keys.ToList(), processedEvents, backloggedUniverses);

        if (backloggedUniverses.Count > 0)
        {
            logger.LogWarning(
                "Events are still due in universe(s) {UniverseIds} after processing {Count} events; the backlog continues next cycle.",
                backloggedUniverses, processedEvents);
        }
    }

    /// <summary>Releases all leases, so a standby instance can take over.</summary>
    /// <returns>A task that completes when all leases are released.</returns>
    public async ValueTask DisposeAsync() => await ReleaseLeasesExceptAsync([]);

    private async Task<List<RunningUniverse>> RunningUniversesAsync(CancellationToken cancellationToken)
    {
        await using GameDbContext database = await contextFactory.CreateDbContextAsync(cancellationToken);
        List<RunningUniverse> universes = await database.Universes
            .AsNoTracking()
            .Where(universe => universe.PausedSince == null)
            .OrderBy(universe => universe.Id)
            .Select(universe => new RunningUniverse(
                universe.Id, universe.Name, universe.Epoch, universe.TickLength, universe.TotalPausedDuration))
            .ToListAsync(cancellationToken);

        // A paused or removed universe starts with a fresh tick line when it runs again.
        foreach (int universeId in _lastLoggedTicks.Keys.Where(id => universes.All(universe => universe.Id != id)).ToList())
        {
            _lastLoggedTicks.Remove(universeId);
        }

        return universes;
    }

    /// <summary>Writes the tick line of a universe once per new tick (only when the tick log is switched on).</summary>
    private void LogTickReached(RunningUniverse universe)
    {
        if (!tickLog.IsEnabled)
        {
            return;
        }

        long tick = new UniverseTimeline(universe.Epoch, universe.TickLength, universe.TotalPausedDuration, PausedSince: null)
            .TickAt(timeProvider.GetUtcNow());

        bool known = _lastLoggedTicks.TryGetValue(universe.Id, out long lastLoggedTick);
        if (known && tick <= lastLoggedTick)
        {
            // Still the same tick: the engine wakes up more often than ticks pass.
            return;
        }

        tickLog.TickReached(universe.Id, universe.Name, known ? lastLoggedTick + 1 : tick, tick);
        _lastLoggedTicks[universe.Id] = tick;
    }

    private async Task<bool> EnsureLeaseAsync(int universeId, CancellationToken cancellationToken)
    {
        if (_leases.TryGetValue(universeId, out UniverseLease? lease))
        {
            if (await lease.IsStillHeldAsync(cancellationToken))
            {
                return true;
            }

            _leases.Remove(universeId);
            await lease.DisposeAsync();
        }

        UniverseLease? acquired = await UniverseLease.TryAcquireAsync(
            connectionOptions.Value.Game, universeId, logger, cancellationToken);

        if (acquired is null)
        {
            return false;
        }

        _leases[universeId] = acquired;
        return true;
    }

    private async Task ReleaseLeasesExceptAsync(IReadOnlyCollection<int> keep)
    {
        foreach (int universeId in _leases.Keys.Where(id => !keep.Contains(id)).ToList())
        {
            UniverseLease lease = _leases[universeId];
            _leases.Remove(universeId);
            await lease.DisposeAsync();
        }
    }

    /// <summary>The columns of a running universe the cycle needs: its id, and name and clock for the tick log.</summary>
    private sealed record RunningUniverse(int Id, string Name, DateTimeOffset Epoch, TimeSpan TickLength, TimeSpan TotalPausedDuration);
}
