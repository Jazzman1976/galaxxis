using Galaxxis.Persistence.Accounts;
using Galaxxis.Persistence.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Galaxxis.Persistence.Migrations;

/// <summary>
/// Lets the <c>dotnet ef</c> tool create a <see cref="GameDbContext"/> when adding migrations or generating
/// SQL scripts. Not used by the running application.
/// </summary>
/// <remarks>
/// Creating a migration does not connect to a database, so a local default is enough. To run
/// <c>dotnet ef database update</c> against another server, set the environment variable
/// <c>ConnectionStrings__Game</c>.
/// </remarks>
internal sealed class GameDbContextDesignTimeFactory : IDesignTimeDbContextFactory<GameDbContext>
{
    /// <summary>Creates the context for the EF tooling.</summary>
    /// <param name="args">Arguments passed by the tooling; unused.</param>
    /// <returns>A context configured for SQL Server.</returns>
    public GameDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<GameDbContext>()
            .UseSqlServer(DesignTimeConnection.For("Game", "GalaxxisGame"))
            .Options);
}

/// <summary>
/// Lets the <c>dotnet ef</c> tool create an <see cref="AccountDbContext"/>. See <see cref="GameDbContextDesignTimeFactory"/>.
/// </summary>
internal sealed class AccountDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AccountDbContext>
{
    /// <summary>Creates the context for the EF tooling.</summary>
    /// <param name="args">Arguments passed by the tooling; unused.</param>
    /// <returns>A context configured for SQL Server.</returns>
    public AccountDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AccountDbContext>()
            .UseSqlServer(DesignTimeConnection.For("Accounts", "GalaxxisAccounts"))
            .Options);
}

/// <summary>Resolves the connection string used by the EF tooling.</summary>
internal static class DesignTimeConnection
{
    /// <summary>Returns the connection string from the environment, or a LocalDB default.</summary>
    /// <param name="name">Connection string name, for example "Game".</param>
    /// <param name="defaultDatabase">Database name used for the LocalDB default.</param>
    /// <returns>The connection string.</returns>
    public static string For(string name, string defaultDatabase) =>
        Environment.GetEnvironmentVariable($"ConnectionStrings__{name}")
        ?? $@"Server=(localdb)\MSSQLLocalDB;Database={defaultDatabase};Trusted_Connection=True;TrustServerCertificate=True";
}
