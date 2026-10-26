using Galaxxis.Application.Accounts;
using Galaxxis.Persistence.Accounts;
using Galaxxis.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Galaxxis.Application.Tests;

public class AdminAccountBootstrapperTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Creates_the_admin_once_and_keeps_it_afterwards()
    {
        await using ServiceProvider services = CreateServices(new()
        {
            ["BootstrapAdmin:Email"] = "admin@galaxxis.test",
            ["BootstrapAdmin:UserName"] = "Administrator",
            ["BootstrapAdmin:Password"] = "Test-Password-1!",
        });

        Assert.Equal(AdminBootstrapResult.Created, await Bootstrap(services));
        Assert.Equal(AdminBootstrapResult.AlreadyExisted, await Bootstrap(services));

        using IServiceScope scope = services.CreateScope();
        UserManager<AppUser> users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        AppUser? admin = await users.FindByEmailAsync("admin@galaxxis.test");

        Assert.NotNull(admin);
        Assert.True(await users.IsInRoleAsync(admin, AccountRoles.Admin));
        Assert.True(await users.CheckPasswordAsync(admin, "Test-Password-1!"));
    }

    [Fact]
    public async Task Missing_settings_are_reported_without_creating_anything()
    {
        await using ServiceProvider services = CreateServices(new()
        {
            ["BootstrapAdmin:Email"] = "incomplete@galaxxis.test",
        });

        OptionsValidationException error = await Assert.ThrowsAsync<OptionsValidationException>(() => Bootstrap(services));

        Assert.Contains(error.Failures, failure => failure.Contains("BootstrapAdmin:Password", StringComparison.Ordinal));
    }

    [Fact]
    public async Task A_weak_password_is_refused_with_the_reason()
    {
        await using ServiceProvider services = CreateServices(new()
        {
            ["BootstrapAdmin:Email"] = "weak@galaxxis.test",
            ["BootstrapAdmin:UserName"] = "WeakAdmin",
            ["BootstrapAdmin:Password"] = "short",
        });

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => Bootstrap(services));
        Assert.Contains("password", error.Message, StringComparison.OrdinalIgnoreCase);

        using IServiceScope scope = services.CreateScope();
        Assert.Null(await scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>().FindByEmailAsync("weak@galaxxis.test"));
    }

    private ServiceProvider CreateServices(Dictionary<string, string?> adminSettings) => databases.CreateServices((services, configuration) =>
    {
        IConfiguration withAdmin = new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(adminSettings)
            .Build();

        services.AddGalaxxisApplication(withAdmin);
    });

    private static async Task<AdminBootstrapResult> Bootstrap(ServiceProvider services)
    {
        using IServiceScope scope = services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<IAdminAccountBootstrapper>().EnsureAdminAsync(Cancellation);
    }
}
