using Galaxxis.Engine.Configuration;
using Galaxxis.Engine.Handlers;
using Galaxxis.Engine.Processing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
    /// Adds the engine: settings, event handlers, event processing, the background service and its health check.
    /// </summary>
    /// <param name="services">The service collection of the host. <c>AddGalaxxisPersistence</c> must be called too.</param>
    /// <param name="configuration">The host configuration the settings are read from.</param>
    /// <returns>The same service collection, to allow chaining.</returns>
    /// <remarks>
    /// The background service starts processing as soon as the host runs. A host that must not process events
    /// (for example a load test client) should not call this method.
    /// </remarks>
    public static IServiceCollection AddGalaxxisEngine(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<EngineOptions>()
            .Bind(configuration.GetSection(EngineOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // The real clock. Tests replace it with a fake time provider to control universe time.
        services.TryAddSingleton(TimeProvider.System);

        AddBuiltInHandlers(services);
        services.AddHostedService<GameEventHandlerRegistrationCheck>();

        services.AddSingleton<EngineTickLog>();
        services.AddSingleton<GameEventExecutor>();
        services.AddSingleton<UniverseEventProcessor>();
        services.AddSingleton<EngineStatus>();
        services.AddSingleton<EngineCycle>();
        services.AddHostedService<UniverseProcessorService>();

        services.AddHealthChecks().AddCheck<EngineHealthCheck>("engine");

        return services;
    }

    /// <summary>
    /// Registers every <see cref="IGameEventHandler"/> implemented in the Engine assembly, so a new handler class is
    /// picked up without touching this file. Handlers are scoped: each event gets fresh instances.
    /// </summary>
    private static void AddBuiltInHandlers(IServiceCollection services)
    {
        IEnumerable<Type> handlerTypes = typeof(EngineModule).Assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IGameEventHandler).IsAssignableFrom(type))
            .OrderBy(type => type.FullName, StringComparer.Ordinal);

        foreach (Type handlerType in handlerTypes)
        {
            services.AddScoped(typeof(IGameEventHandler), handlerType);
        }
    }
}
