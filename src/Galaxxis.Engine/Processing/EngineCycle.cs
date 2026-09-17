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
    ILogger<EngineCycle> logger) : IAsyncDisposable
{
    // The leases this instance holds, by universe id. Only the single engine loop uses this dictionary.
    private readonly Dictionary<int, UniverseLease> _leases = [];

    /// <summary>Runs one cycle over all running universes.</summary>
    /// <param name="cancellationToken">Signals shutdown.</param>
    /// <returns>A task that completes when the cycle is done.</returns>
    public async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        List<int> runningUniverseIds = await RunningUniverseIdsAsync(cancellationToken);
        await ReleaseLeasesExceptAsync(runningUniverseIds);

        int processedEvents = 0;
        List<int> backloggedUniverses = [];

        foreach (int universeId in runningUniverseIds)
        {
            if (!await EnsureLeaseAsync(universeId, cancellationToken))
            {
                // Another instance processes this universe; this instance is its standby.
                continue;
            }

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

    private async Task<List<int>> RunningUniverseIdsAsync(CancellationToken cancellationToken)
    {
        await using GameDbContext database = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await database.Universes
            .Where(universe => universe.PausedSince == null)
            .OrderBy(universe => universe.Id)
            .Select(universe => universe.Id)
            .ToListAsync(cancellationToken);
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
}
