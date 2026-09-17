using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Engine.Handlers;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Galaxxis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Engine.Tests;

public class TimeAndInstanceTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    private static readonly ResourceAmounts Plenty = ResourceAmounts.Uniform(10_000);

    [Fact]
    public async Task After_an_outage_all_due_events_run_in_tick_order()
    {
        TickRecorder recorder = new();
        await using EngineHarness world = await EngineHarness.CreateAsync(databases, services =>
        {
            services.AddSingleton(recorder);
            services.AddScoped<IGameEventHandler, RecordingHandler>();
        });
        Universe universe = await world.CreateUniverseAsync();

        // Requested at ticks 0, 10 and 20; each completes after a different delay.
        await world.RequestAsync(universe, RecordingHandler.Type, new RecordingPayload("slow", 100), null, null);
        world.Advance(TimeSpan.FromSeconds(10));
        await world.RequestAsync(universe, RecordingHandler.Type, new RecordingPayload("fast", 5), null, null);
        world.Advance(TimeSpan.FromSeconds(10));
        await world.RequestAsync(universe, RecordingHandler.Type, new RecordingPayload("medium", 40), null, null);

        // The engine was down all this time and now runs once.
        world.Advance(TimeSpan.FromHours(2));
        await world.ProcessAsync(universe);

        Assert.Equal(["fast@15", "medium@60", "slow@100"], recorder.Completions);
    }

    [Fact]
    public async Task A_paused_universe_processes_nothing_and_its_time_stands_still()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, Plenty);
        long eventId = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id);
        await world.ProcessAsync(universe);

        // Pause for a day right after the construction started.
        await SetPausedAsync(world, universe, paused: true);
        world.Advance(TimeSpan.FromDays(1));
        await world.ProcessAsync(universe);
        Assert.Equal(GameEventState.Scheduled, (await world.EventAsync(eventId)).State);

        // Resume: the construction still needs its full 25 seconds of universe time.
        await SetPausedAsync(world, universe, paused: false);
        world.Advance(TimeSpan.FromSeconds(24));
        await world.ProcessAsync(universe);
        Assert.Equal(GameEventState.Scheduled, (await world.EventAsync(eventId)).State);

        world.Advance(TimeSpan.FromSeconds(1));
        await world.ProcessAsync(universe);
        Assert.Equal(GameEventState.Done, (await world.EventAsync(eventId)).State);
    }

    [Fact]
    public async Task Only_one_engine_instance_processes_a_universe_and_a_standby_takes_over()
    {
        await using EngineHarness first = await EngineHarness.CreateAsync(databases);
        Universe universe = await first.CreateUniverseAsync();
        (Player player, Planet planet) = await first.CreatePlayerWithPlanetAsync(universe, Race.Human, Plenty);

        // A second engine instance with its own services (like a second server), sharing the clock.
        EngineHarness second = await EngineHarness.CreateAsync(databases, sharedTime: first.Time);
        try
        {
            await first.RunCycleAsync(); // takes the lease

            long eventId = await first.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id);
            await second.RunCycleAsync();
            Assert.Equal(GameEventState.Requested, (await second.EventAsync(eventId)).State);

            // The first instance shuts down and releases its lease; the standby continues.
            await first.Services.DisposeAsync();
            await second.RunCycleAsync();
            Assert.Equal(GameEventState.Scheduled, (await second.EventAsync(eventId)).State);
        }
        finally
        {
            await second.DisposeAsync();
        }
    }

    [Fact]
    public async Task Universes_are_processed_independently()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe running = await world.CreateUniverseAsync();
        Universe paused = await world.CreateUniverseAsync();
        (Player runningPlayer, Planet runningPlanet) = await world.CreatePlayerWithPlanetAsync(running, Race.Human, Plenty);
        (Player pausedPlayer, Planet pausedPlanet) = await world.CreatePlayerWithPlanetAsync(paused, Race.Human, Plenty);
        await SetPausedAsync(world, paused, paused: true);

        long inRunning = await world.RequestAsync(running, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), runningPlayer.Id, runningPlanet.Id);
        long inPaused = await world.RequestAsync(paused, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), pausedPlayer.Id, pausedPlanet.Id);

        await world.RunCycleAsync();

        Assert.Equal(GameEventState.Scheduled, (await world.EventAsync(inRunning)).State);
        Assert.Equal(GameEventState.Requested, (await world.EventAsync(inPaused)).State);
    }

    private static async Task SetPausedAsync(EngineHarness world, Universe universe, bool paused)
    {
        await using GameDbContext database = world.Database();
        Universe stored = await database.Universes.SingleAsync(u => u.Id == universe.Id, EngineHarness.Cancellation);
        DateTimeOffset now = world.Time.GetUtcNow();

        if (paused)
        {
            stored.PausedSince = now;
        }
        else if (stored.PausedSince is DateTimeOffset since)
        {
            stored.TotalPausedDuration += now - since;
            stored.PausedSince = null;
        }

        await database.SaveChangesAsync(EngineHarness.Cancellation);
    }

    private sealed record RecordingPayload(string Name, int DelaySeconds);

    private sealed class TickRecorder
    {
        public List<string> Completions { get; } = [];
    }

    /// <summary>Schedules itself with the given delay and records the tick when it completes.</summary>
    private sealed class RecordingHandler(TickRecorder recorder) : IGameEventHandler
    {
        public const string Type = "TestRecording";

        public string EventType => Type;

        public string? Queue => null;

        public Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken) =>
            Task.FromResult(AcceptResult.CompleteAfter(TimeSpan.FromSeconds(context.ReadPayload<RecordingPayload>().DelaySeconds)));

        public Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken)
        {
            recorder.Completions.Add($"{context.ReadPayload<RecordingPayload>().Name}@{context.Tick}");
            return Task.CompletedTask;
        }
    }
}
