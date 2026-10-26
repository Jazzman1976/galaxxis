using Galaxxis.Application.Universes;
using Galaxxis.Domain.Universes.Generation;
using Galaxxis.Persistence.Accounts;
using Galaxxis.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Web.Tests;

/// <summary>
/// The real web application (Program.cs) on an in-memory test server, with its own fresh databases.
/// </summary>
/// <remarks>
/// Used as a class fixture. The engine is disabled (<c>Engine:Enabled = false</c>), so recorded commands stay
/// <c>Requested</c> and tests can check exactly what the web application stored. Engine behaviour is tested in
/// Galaxxis.Engine.Tests.
/// </remarks>
public sealed class WebTestServer : IAsyncLifetime
{
    private TestDatabases? _databases;
    private TestApplication? _application;

    /// <summary>The databases of this server.</summary>
    public TestDatabases Databases => _databases ?? throw new InvalidOperationException("Not initialized.");

    /// <summary>The running web application.</summary>
    public WebApplicationFactory<Program> Application => _application ?? throw new InvalidOperationException("Not initialized.");

    /// <inheritdoc />
    public async ValueTask InitializeAsync()
    {
        _databases = new TestDatabases();
        await _databases.InitializeAsync();
        _application = new TestApplication(_databases);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_application is not null)
        {
            await _application.DisposeAsync();
        }

        if (_databases is not null)
        {
            await _databases.DisposeAsync();
        }
    }

    /// <summary>Opens a new browser session (own cookies) against the server.</summary>
    /// <returns>The session.</returns>
    public Browser OpenBrowser() => new(Application.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        HandleCookies = true,
    }));

    /// <summary>Generates a small running universe (1 galaxy, 1 star system, 4 planets).</summary>
    /// <returns>The universe.</returns>
    public async Task<UniverseSummary> CreateRunningUniverseAsync()
    {
        await using AsyncServiceScope scope = Application.Services.CreateAsyncScope();
        IUniverseAdminService admin = scope.ServiceProvider.GetRequiredService<IUniverseAdminService>();

        UniverseSummary universe = await admin.GenerateAsync(new GenerateUniverseRequest(
            new UniverseGenerationSettings { GalaxyCount = 1, StarSystemsPerGalaxy = 1, PlanetsPerStarSystem = 4 }));
        return (await admin.ResumeAsync(universe.Id))!;
    }

    /// <summary>Creates an account with the admin role directly in the account database.</summary>
    /// <param name="userName">The account name.</param>
    /// <param name="password">The password.</param>
    /// <returns>A task that completes when the account exists.</returns>
    public async Task CreateAdminAsync(string userName, string password)
    {
        await using AsyncServiceScope scope = Application.Services.CreateAsyncScope();
        UserManager<AppUser> users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        RoleManager<AppRole> roles = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();

        if (!await roles.RoleExistsAsync(AccountRoles.Admin))
        {
            Assert.True((await roles.CreateAsync(new AppRole { Name = AccountRoles.Admin })).Succeeded);
        }

        AppUser admin = new() { UserName = userName, Email = $"{userName}@example.com" };
        Assert.True((await users.CreateAsync(admin, password)).Succeeded);
        Assert.True((await users.AddToRoleAsync(admin, AccountRoles.Admin)).Succeeded);
    }

    /// <summary>The web application configured for tests.</summary>
    private sealed class TestApplication(TestDatabases databases) : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Production pipeline: real error pages instead of the developer exception page.
            builder.UseEnvironment("Production");

            // Added after the application's own configuration sources, so these values win, also over a local
            // appsettings.Local.json that the build may have copied next to the test assembly.
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Game"] = databases.GameConnectionString,
                ["ConnectionStrings:Accounts"] = databases.AccountsConnectionString,
                ["Persistence:ApplyMigrationsOnStartup"] = "false",
                ["Engine:Enabled"] = "false",
            }));
        }
    }
}
