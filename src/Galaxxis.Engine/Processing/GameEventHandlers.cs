using Galaxxis.Engine.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Galaxxis.Engine.Processing;

/// <summary>
/// Finds the handler of an event type among the registered <see cref="IGameEventHandler"/> services.
/// </summary>
internal static class GameEventHandlers
{
    /// <summary>Finds the handler for <paramref name="eventType"/>.</summary>
    /// <param name="handlers">All registered handlers.</param>
    /// <param name="eventType">The event type of the event to handle.</param>
    /// <returns>The handler, or <see langword="null"/> if none is registered for this type.</returns>
    /// <exception cref="InvalidOperationException">More than one handler is registered for the type.</exception>
    public static IGameEventHandler? Find(IEnumerable<IGameEventHandler> handlers, string eventType)
    {
        List<IGameEventHandler> matches = handlers.Where(handler => handler.EventType == eventType).ToList();
        return matches.Count switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new InvalidOperationException(
                $"{matches.Count} handlers are registered for the event type \"{eventType}\": " +
                $"{string.Join(", ", matches.Select(handler => handler.GetType().Name))}. There must be exactly one."),
        };
    }

    /// <summary>Checks that every handler has an event type and no event type has two handlers.</summary>
    /// <param name="handlers">All registered handlers.</param>
    /// <exception cref="InvalidOperationException">The registration is invalid; the message names the problem.</exception>
    public static void EnsureValid(IEnumerable<IGameEventHandler> handlers)
    {
        List<IGameEventHandler> all = handlers.ToList();

        IGameEventHandler? unnamed = all.FirstOrDefault(handler => string.IsNullOrWhiteSpace(handler.EventType));
        if (unnamed is not null)
        {
            throw new InvalidOperationException($"The event handler {unnamed.GetType().Name} has no event type.");
        }

        foreach (IGrouping<string, IGameEventHandler> duplicate in all.GroupBy(handler => handler.EventType).Where(group => group.Count() > 1))
        {
            throw new InvalidOperationException(
                $"Several handlers are registered for the event type \"{duplicate.Key}\": " +
                $"{string.Join(", ", duplicate.Select(handler => handler.GetType().Name))}. There must be exactly one.");
        }
    }
}

/// <summary>
/// Checks the handler registration when the host starts, so a duplicate handler stops the application immediately
/// instead of failing events at runtime.
/// </summary>
internal sealed class GameEventHandlerRegistrationCheck(IServiceScopeFactory scopeFactory) : IHostedService
{
    /// <summary>Validates the registered handlers.</summary>
    /// <param name="cancellationToken">Unused.</param>
    /// <returns>A completed task.</returns>
    /// <exception cref="InvalidOperationException">The registration is invalid.</exception>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        GameEventHandlers.EnsureValid(scope.ServiceProvider.GetServices<IGameEventHandler>());
        return Task.CompletedTask;
    }

    /// <summary>Nothing to do on shutdown.</summary>
    /// <param name="cancellationToken">Unused.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
