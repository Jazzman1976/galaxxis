using Galaxxis.Persistence.Accounts;
using Galaxxis.Persistence.Configuration;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Galaxxis.Persistence;

/// <summary>
/// Registers everything the Persistence module provides in the dependency injection container.
/// </summary>
/// <remarks>
/// Every host (web application, engine, tools) calls <see cref="AddGalaxxisPersistence"/> once in its
/// <c>Program.cs</c>. This is the only place where the database contexts and settings are wired up,
/// so a host never needs to know the module's internals. Hosts that manage login accounts additionally call
/// <see cref="AccountsModule.AddGalaxxisAccounts"/>.
/// </remarks>
public static class PersistenceModule
{
    /// <summary>
    /// Adds the database settings, both database contexts and the optional startup migration.
    /// </summary>
    /// <param name="services">The service collection of the host.</param>
    /// <param name="configuration">The host configuration the settings are read from.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    /// <remarks>
    /// Registered services:
    /// <list type="bullet">
    ///   <item><see cref="GameDbContext"/> (scoped) and <c>IDbContextFactory&lt;GameDbContext&gt;</c> (singleton).</item>
    ///   <item><see cref="AccountDbContext"/> (scoped).</item>
    ///   <item>A hosted service that applies migrations at startup if <c>Persistence:ApplyMigrationsOnStartup</c> is true.</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddGalaxxisPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Settings are validated when the host starts (ValidateOnStart), so a missing connection
        // string is reported immediately instead of on the first database access.
        services.AddOptions<DatabaseConnectionOptions>()
            .Bind(configuration.GetSection(DatabaseConnectionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<PersistenceOptions>()
            .Bind(configuration.GetSection(PersistenceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // The factory also registers GameDbContext itself as a scoped service.
        services.AddDbContextFactory<GameDbContext>((provider, options) =>
            UseSqlServer(options, provider.GetRequiredService<IOptions<DatabaseConnectionOptions>>().Value.Game));

        services.AddDbContext<AccountDbContext>((provider, options) =>
            UseSqlServer(options, provider.GetRequiredService<IOptions<DatabaseConnectionOptions>>().Value.Accounts));

        services.AddHostedService<DatabaseMigrationService>();

        return services;
    }

    private static void UseSqlServer(DbContextOptionsBuilder options, string connectionString) =>
        options.UseSqlServer(connectionString, sql =>
        {
            // Retry automatically on transient connection errors, which are normal on Azure SQL.
            // Consequence for code that opens an explicit transaction: wrap it in
            // context.Database.CreateExecutionStrategy().ExecuteAsync(...), so the whole unit can be retried.
            sql.EnableRetryOnFailure();
        });
}
