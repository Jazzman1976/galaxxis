using System.Collections.Concurrent;
using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Engine.Processing;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Galaxxis.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Galaxxis.Engine.Tests;

public class TickLogTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    [Fact]
    public async Task The_tick_log_shows_every_tick_once_with_its_events_and_settled_planets()
    {
        CapturingLoggerProvider log = new();
        await using EngineHarness world = await EngineHarness.CreateAsync(databases, services =>
            services.AddLogging(logging => logging.AddProvider(log).AddFilter(EngineTickLog.CategoryName, LogLevel.Debug)));

        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, ResourceAmounts.Uniform(10_000));

        world.Advance(TimeSpan.FromSeconds(1));
        long eventId = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id);
        await world.RunCycleAsync();

        Assert.Contains(log.Lines, line => line.Contains($"Universe {universe.Id} \"Test universe\" | tick 1 ==="));
        Assert.Contains(log.Lines, line => line.Contains($"event #{eventId} BuildBuilding") && line.Contains("Requested -> Scheduled") && line.Contains("completes at tick 26"));
        Assert.Contains(log.Lines, line => line.Contains($"planet {planet.Id}") && line.Contains("settled ticks 0-1"));

        // The same tick is not written twice; ticks passed between two wake-ups are written as a range.
        await world.RunCycleAsync();
        Assert.Single(log.Lines, line => line.Contains("| tick 1 ==="));

        world.Advance(TimeSpan.FromSeconds(3));
        await world.RunCycleAsync();
        Assert.Contains(log.Lines, line => line.Contains("| ticks 2-4 ==="));
    }

    [Fact]
    public async Task The_tick_log_is_silent_unless_its_category_is_switched_on()
    {
        CapturingLoggerProvider log = new();
        await using EngineHarness world = await EngineHarness.CreateAsync(databases, services =>
            services.AddLogging(logging => logging.AddProvider(log).SetMinimumLevel(LogLevel.Information)));

        await world.CreateUniverseAsync();
        world.Advance(TimeSpan.FromSeconds(1));
        await world.RunCycleAsync();

        Assert.DoesNotContain(log.Lines, line => line.Contains("==="));
    }

    /// <summary>Collects the formatted messages of all loggers.</summary>
    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<string> _lines = new();

        public IReadOnlyList<string> Lines => _lines.ToList();

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(_lines);

        public void Dispose()
        {
        }

        private sealed class CapturingLogger(ConcurrentQueue<string> lines) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
                lines.Enqueue(formatter(state, exception));
        }
    }
}
