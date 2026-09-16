using Galaxxis.Persistence.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Persistence;

/// <summary>
/// Registers everything the Persistence module provides in the dependency injection container.
/// </summary>
/// <remarks>
/// Every host (web application, engine, tools) calls <see cref="AddGalaxxisPersistence"/> once in its
/// <c>Program.cs</c>. This is the only place where the module's services and settings are wired up,
/// so a host never needs to know the module's internals.
/// </remarks>
public static class PersistenceModule
{
    /// <summary>
    /// Adds the database settings and (from step 3 of the foundation refactoring on) the EF Core
    /// database contexts to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection of the host.</param>
    /// <param name="configuration">The host configuration the settings are read from.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
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

        return services;
    }
}
