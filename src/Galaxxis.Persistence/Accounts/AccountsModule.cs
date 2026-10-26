using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Persistence.Accounts;

/// <summary>
/// Registers login account management (ASP.NET Core Identity) on top of <see cref="AccountDbContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// Separate from <c>AddGalaxxisPersistence</c> because not every host needs accounts: the engine never does.
/// <c>AddGalaxxisApplication</c> calls <see cref="AddGalaxxisAccounts"/>, so hosts using the Application module get
/// account management automatically. Calling it more than once is harmless (Identity registers with TryAdd).
/// </para>
/// <para>
/// The account rules (unique e-mail, password strength, lockout) are defined once in
/// <see cref="ConfigureIdentity"/> and shared by all hosts, so the tools and the web application always
/// apply the same rules.
/// </para>
/// </remarks>
public static class AccountsModule
{
    /// <summary>
    /// Registers account management without web sign-in (cookies). For console tools such as the universe
    /// generator, which only create or look up accounts.
    /// </summary>
    /// <param name="services">The service collection of the host.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddGalaxxisAccounts(this IServiceCollection services)
    {
        services.AddIdentityCore<AppUser>(ConfigureIdentity).AddGalaxxisAccountStores();
        return services;
    }

    /// <summary>
    /// Connects an Identity setup to the Galaxxis account database and role type. The web application uses
    /// this with its own cookie-based <c>AddIdentity</c> call.
    /// </summary>
    /// <param name="builder">The Identity builder returned by <c>AddIdentity</c> or <c>AddIdentityCore</c>.</param>
    /// <returns>The same builder, to allow chaining.</returns>
    public static IdentityBuilder AddGalaxxisAccountStores(this IdentityBuilder builder) =>
        builder
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<AccountDbContext>();

    /// <summary>The account rules that apply everywhere.</summary>
    /// <param name="options">The Identity options to configure.</param>
    public static void ConfigureIdentity(IdentityOptions options)
    {
        // One account per e-mail address; e-mail addresses identify people when they sign up.
        options.User.RequireUniqueEmail = true;

        // Temporarily lock an account after repeated wrong passwords, to stop password guessing.
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.AllowedForNewUsers = true;
    }
}
