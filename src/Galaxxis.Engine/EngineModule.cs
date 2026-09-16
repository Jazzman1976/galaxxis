using Galaxxis.Engine.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Engine;

/// <summary>
/// Registers everything the Engine module provides in the dependency injection container.
/// </summary>
/// <remarks>
/// Today the engine runs inside the web application. Because all of its wiring lives here, it can
/// later move into its own host (for example an Azure Container App) whose <c>Program.cs</c> only calls
/// <c>AddGalaxxisPersistence</c> and <see cref="AddGalaxxisEngine"/>.
/// </remarks>
public static class EngineModule
{
    /// <summary>
    /// Adds the engine settings and (from step 4 of the foundation refactoring on) the universe clock,
    /// the event handlers and the background processing service to <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection of the host.</param>
    /// <param name="configuration">The host configuration the settings are read from.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    public static IServiceCollection AddGalaxxisEngine(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EngineOptions>()
            .Bind(configuration.GetSection(EngineOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }
}
