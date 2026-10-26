using Galaxxis.Persistence;
using Galaxxis.Persistence.Accounts;
using Galaxxis.Persistence.Game;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Testing;

/// <summary>
/// A fresh pair of Galaxxis databases (game and accounts) on SQL Server for one test class, created with the
/// real migrations and deleted afterwards.
/// </summary>
/// <remarks>
/// <para>
/// Use it as an xUnit class fixture: <c>public class MyTests(TestDatabases databases) : IClassFixture&lt;TestDatabases&gt;</c>.
/// All tests of the class share the databases, so tests must not depend on what other tests stored (for example,
/// look up the universe a test created by its id instead of counting all universes).
/// </para>
/// <para>
/// By default SQL Server LocalDB is used, which is installed with Visual Studio. The environment variable
/// <c>GALAXXIS_TEST_SQLSERVER</c> selects another server, in one of two forms:
/// </para>
/// <list type="bullet">
///   <item>a server name, used with Windows authentication: <c>localhost\SQLEXPRESS</c>;</item>
///   <item>a connection string without database, for SQL authentication, for example the SQL Server container of the
///   GitHub Actions build (<c>.github/workflows/build.yml</c>):
///   <c>Server=localhost,1433;User Id=sa;Password=…;TrustServerCertificate=True</c>.</item>
/// </list>
/// <para>
/// The login needs the right to create databases.
/// </para>
/// <para>This file is shared by several test projects through a linked compile item.</para>
/// </remarks>
public sealed class TestDatabases : IAsyncLifetime
{
    private readonly string _serverConnectionString = ServerConnectionString(Environment.GetEnvironmentVariable("GALAXXIS_TEST_SQLSERVER"));

    private readonly string _suffix = Guid.NewGuid().ToString("N")[..12];

    /// <summary>Connection string of this fixture's game database.</summary>
    public string GameConnectionString => ConnectionString($"GalaxxisTestGame_{_suffix}");

    /// <summary>Connection string of this fixture's accounts database.</summary>
    public string AccountsConnectionString => ConnectionString($"GalaxxisTestAccounts_{_suffix}");

    /// <summary>A configuration pointing all modules at this fixture's databases.</summary>
    public IConfiguration Configuration => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Game"] = GameConnectionString,
            ["ConnectionStrings:Accounts"] = AccountsConnectionString,
            ["Persistence:ApplyMigrationsOnStartup"] = "false",
        })
        .Build();

    /// <summary>Creates both databases by applying the migrations.</summary>
    /// <returns>A task that completes when both databases exist.</returns>
    public async ValueTask InitializeAsync()
    {
        await using GameDbContext game = CreateGameContext();
        await game.Database.MigrateAsync();

        await using AccountDbContext accounts = CreateAccountContext();
        await accounts.Database.MigrateAsync();
    }

    /// <summary>Deletes both databases.</summary>
    /// <returns>A task that completes when both databases are gone.</returns>
    public async ValueTask DisposeAsync()
    {
        await using GameDbContext game = CreateGameContext();
        await game.Database.EnsureDeletedAsync();

        await using AccountDbContext accounts = CreateAccountContext();
        await accounts.Database.EnsureDeletedAsync();
    }

    /// <summary>Creates a new, independent game database context, as a fresh unit of work.</summary>
    /// <returns>The context; dispose it after use.</returns>
    public GameDbContext CreateGameContext() =>
        new(new DbContextOptionsBuilder<GameDbContext>().UseSqlServer(GameConnectionString).Options);

    /// <summary>Creates a new, independent accounts database context.</summary>
    /// <returns>The context; dispose it after use.</returns>
    public AccountDbContext CreateAccountContext() =>
        new(new DbContextOptionsBuilder<AccountDbContext>().UseSqlServer(AccountsConnectionString).Options);

    /// <summary>
    /// Builds a service provider the way a host would: logging, configuration and the Persistence module,
    /// plus whatever <paramref name="configure"/> adds.
    /// </summary>
    /// <param name="configure">Registers further modules or replaces services, for example the time provider.</param>
    /// <returns>The service provider; dispose it after use.</returns>
    public ServiceProvider CreateServices(Action<IServiceCollection, IConfiguration>? configure = null)
    {
        IConfiguration configuration = Configuration;
        ServiceCollection services = new();

        services.AddLogging();
        services.AddSingleton(configuration);
        services.AddGalaxxisPersistence(configuration);
        configure?.Invoke(services, configuration);

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
    }

    /// <summary>The connection string of the test server without database, from the setting described in the remarks.</summary>
    private static string ServerConnectionString(string? setting) => setting switch
    {
        null or "" => @"Server=(localdb)\MSSQLLocalDB;Trusted_Connection=True;TrustServerCertificate=True",

        // "Key=Value" pairs: already a connection string (SQL authentication, e.g. in CI).
        _ when setting.Contains('=', StringComparison.Ordinal) => setting,

        // Only a server name: Windows authentication, as with LocalDB.
        _ => $"Server={setting};Trusted_Connection=True;TrustServerCertificate=True",
    };

    private string ConnectionString(string database) =>
        new SqlConnectionStringBuilder(_serverConnectionString) { InitialCatalog = database }.ConnectionString;
}
