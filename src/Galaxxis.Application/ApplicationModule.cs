using Galaxxis.Application.Accounts;
using Galaxxis.Application.Universes;
using Galaxxis.Persistence.Accounts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Galaxxis.Application;

/// <summary>
/// Registers everything the Application module provides in the dependency injection container.
/// </summary>
/// <remarks>
/// The Application module holds the use cases the web UI and the tools call, such as generating a universe
/// or (from step 5 of the foundation refactoring on) submitting a build command and showing a planet.
/// Requires <c>AddGalaxxisPersistence</c>. Account management is registered here as well, because some use cases
/// (creating the admin, later registration) need it; the web application's cookie-based Identity setup can be
/// added on top without conflicts.
/// </remarks>
public static class ApplicationModule
{
    /// <summary>
    /// Adds the application services to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection of the host.</param>
    /// <param name="configuration">The host configuration the settings are read from.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddGalaxxisApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // The real clock. Tests replace it with a fake time provider to control time.
        services.TryAddSingleton(TimeProvider.System);

        // Use cases about login accounts need Identity's user and role managers.
        services.AddGalaxxisAccounts();

        services.AddOptions<UniverseGenerationOptions>()
            .Bind(configuration.GetSection(UniverseGenerationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Not validated on start: only needed when an admin account is actually created.
        services.AddOptions<BootstrapAdminOptions>()
            .Bind(configuration.GetSection(BootstrapAdminOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddScoped<IUniverseAdminService, UniverseAdminService>();
        services.AddScoped<IAdminAccountBootstrapper, AdminAccountBootstrapper>();

        return services;
    }
}
