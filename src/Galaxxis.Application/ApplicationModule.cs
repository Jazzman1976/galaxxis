using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Application;

/// <summary>
/// Registers everything the Application module provides in the dependency injection container.
/// </summary>
/// <remarks>
/// The Application module holds the use cases the web UI and the tools call, such as submitting a
/// build command, showing a planet or generating a universe. Its services arrive in steps 3 and 5 of
/// the foundation refactoring; this method is the single place where they will be registered.
/// </remarks>
public static class ApplicationModule
{
    /// <summary>
    /// Adds the application services to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection of the host.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddGalaxxisApplication(this IServiceCollection services)
    {
        return services;
    }
}
