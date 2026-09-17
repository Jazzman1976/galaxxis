using Galaxxis.Engine.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// Reports whether the engine of this process is working, for monitoring (for example Azure App Service health checks).
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><b>Healthy</b>: a cycle completed recently and no universe has a backlog.</item>
///   <item><b>Degraded</b>: still starting, or due events are piling up faster than they are processed.</item>
///   <item><b>Unhealthy</b>: no cycle has completed for a long time (the engine is stuck or keeps failing).</item>
/// </list>
/// "Recently" means within ten poll intervals, but at least one minute.
/// </remarks>
internal sealed class EngineHealthCheck(EngineStatus status, IOptions<EngineOptions> options, TimeProvider timeProvider) : IHealthCheck
{
    /// <inheritdoc />
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        EngineStatusSnapshot snapshot = status.Current;
        TimeSpan allowedSilence = TimeSpan.FromTicks(Math.Max(options.Value.PollInterval.Ticks * 10, TimeSpan.FromMinutes(1).Ticks));

        Dictionary<string, object> data = new()
        {
            ["lastSuccessfulCycle"] = snapshot.LastSuccessfulCycle?.ToString("O") ?? "never",
            ["processedUniverses"] = snapshot.HeldUniverses,
            ["eventsInLastCycle"] = snapshot.EventsInLastCycle,
            ["backloggedUniverses"] = snapshot.BackloggedUniverses,
            ["lastError"] = snapshot.LastError ?? "none",
        };

        HealthCheckResult result = snapshot.LastSuccessfulCycle switch
        {
            null => HealthCheckResult.Degraded("The engine has not completed a cycle yet.", data: data),
            DateTimeOffset last when timeProvider.GetUtcNow() - last > allowedSilence =>
                HealthCheckResult.Unhealthy($"The engine has not completed a cycle since {last:O}.", data: data),
            _ when snapshot.BackloggedUniverses.Count > 0 =>
                HealthCheckResult.Degraded("Due events are piling up; the engine is behind.", data: data),
            _ => HealthCheckResult.Healthy("The engine is processing events.", data: data),
        };

        return Task.FromResult(result);
    }
}
