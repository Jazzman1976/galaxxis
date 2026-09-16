using Galaxxis.Persistence.Accounts;
using Galaxxis.Persistence.Configuration;
using Galaxxis.Persistence.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Galaxxis.Persistence.Migrations;

/// <summary>
/// Brings both databases up to the current schema while a host starts, if
/// <see cref="PersistenceOptions.ApplyMigrationsOnStartup"/> is enabled.
/// </summary>
/// <remarks>
/// <para>
/// Registered by <c>AddGalaxxisPersistence</c> as the first hosted service, so the schema is ready before
/// any other startup work (for example the engine) touches the database. Missing databases are created.
/// </para>
/// <para>
/// Intended for developer machines. On Azure, migrations are deployed as an idempotent SQL script, and the
/// option stays disabled, so several instances starting at once never race to migrate.
/// </para>
/// </remarks>
internal sealed class DatabaseMigrationService(
    IServiceScopeFactory scopeFactory,
    IOptions<PersistenceOptions> options,
    ILogger<DatabaseMigrationService> logger) : IHostedService
{
    /// <summary>Applies pending migrations to the game and accounts database when enabled.</summary>
    /// <param name="cancellationToken">Signals that the host start was aborted.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.ApplyMigrationsOnStartup)
        {
            return;
        }

        using IServiceScope scope = scopeFactory.CreateScope();

        await MigrateAsync(scope.ServiceProvider.GetRequiredService<GameDbContext>(), "game", cancellationToken);
        await MigrateAsync(scope.ServiceProvider.GetRequiredService<AccountDbContext>(), "accounts", cancellationToken);
    }

    /// <summary>Nothing to do when the host stops.</summary>
    /// <param name="cancellationToken">Unused.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task MigrateAsync(DbContext context, string databaseName, CancellationToken cancellationToken)
    {
        List<string> pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            logger.LogDebug("The {Database} database is up to date.", databaseName);
            return;
        }

        logger.LogInformation(
            "Applying {Count} migration(s) to the {Database} database: {Migrations}",
            pending.Count, databaseName, string.Join(", ", pending));

        await context.Database.MigrateAsync(cancellationToken);
    }
}
