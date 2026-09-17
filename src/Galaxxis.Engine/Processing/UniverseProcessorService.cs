using Galaxxis.Engine.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// The engine's background loop: wakes up every <see cref="EngineOptions.PollInterval"/> and runs one
/// <see cref="EngineCycle"/>.
/// </summary>
/// <remarks>
/// <para>
/// Starts automatically with the host (the legacy ticker had to be started by an administrator after every restart).
/// Whether a universe runs is stored in the database (pause/resume), not in this process.
/// </para>
/// <para>
/// A failing cycle (for example the database is unreachable) is logged and retried on the next wake-up; it never
/// stops the loop or the host. <see cref="PeriodicTimer"/> never overlaps cycles: a slow cycle simply delays the next.
/// </para>
/// </remarks>
internal sealed class UniverseProcessorService(
    EngineCycle cycle,
    EngineStatus status,
    IOptions<EngineOptions> options,
    TimeProvider timeProvider,
    ILogger<UniverseProcessorService> logger) : BackgroundService
{
    /// <summary>Runs cycles until the host stops.</summary>
    /// <param name="stoppingToken">Signals that the host is stopping.</param>
    /// <returns>A task that completes when the loop has stopped and all leases are released.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("The engine is starting; it checks for due events every {Interval}.", options.Value.PollInterval);
        using PeriodicTimer timer = new(options.Value.PollInterval, timeProvider);

        try
        {
            do
            {
                try
                {
                    await cycle.RunOnceAsync(stoppingToken);
                }
                catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
                {
                    logger.LogError(exception, "An engine cycle failed; it is retried on the next wake-up.");
                    status.RecordFailure(timeProvider.GetUtcNow(), exception.Message);
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal shutdown.
        }
        finally
        {
            // Hand the universes over to a standby instance right away instead of waiting for the connection timeout.
            await cycle.DisposeAsync();
            logger.LogInformation("The engine has stopped.");
        }
    }
}
