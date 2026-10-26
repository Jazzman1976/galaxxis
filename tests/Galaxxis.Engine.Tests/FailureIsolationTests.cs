using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Engine.Handlers;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Galaxxis.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Engine.Tests;

/// <summary>
/// A broken event must never stop the universe or corrupt other events (the legacy engine retried it forever).
/// </summary>
public class FailureIsolationTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    private static readonly ResourceAmounts Plenty = ResourceAmounts.Uniform(10_000);

    [Fact]
    public async Task An_event_without_handler_fails_and_the_next_event_still_runs()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, Plenty);

        long unknown = await world.RequestAsync(universe, "DoesNotExist", new { }, player.Id, planet.Id);
        long valid = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id);
        await world.ProcessAsync(universe);

        GameEvent failed = await world.EventAsync(unknown);
        Assert.Equal(GameEventState.Failed, failed.State);
        Assert.Contains("DoesNotExist", failed.Error);
        Assert.Equal(GameEventState.Scheduled, (await world.EventAsync(valid)).State);
    }

    [Fact]
    public async Task An_unreadable_payload_fails_only_its_own_event()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, Plenty);

        long broken = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, "{ this is not json", player.Id, planet.Id);
        long valid = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id);
        UniverseProcessingResultAssert.NothingLeft(await world.ProcessAsync(universe));

        Assert.Equal(GameEventState.Failed, (await world.EventAsync(broken)).State);
        Assert.Equal(GameEventState.Scheduled, (await world.EventAsync(valid)).State);
    }

    [Fact]
    public async Task A_failing_handler_leaves_no_partial_changes_and_no_follow_up_events()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(
            databases, services => services.AddScoped<IGameEventHandler, ExplodingHandler>());
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, Plenty);

        long eventId = await world.RequestAsync(universe, ExplodingHandler.Type, new { }, player.Id, planet.Id);
        await world.ProcessAsync(universe);

        Assert.Equal(GameEventState.Failed, (await world.EventAsync(eventId)).State);
        Assert.Equal(Plenty, (await world.PlanetAsync(planet.Id)).Stock);
        Assert.Single(await world.EventsAsync(universe)); // the follow-up event was rolled back
    }

    [Fact]
    public async Task A_rejection_discards_what_the_handler_changed_before_rejecting()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(
            databases, services => services.AddScoped<IGameEventHandler, PayThenRejectHandler>());
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, Plenty);

        long eventId = await world.RequestAsync(universe, PayThenRejectHandler.Type, new { }, player.Id, planet.Id);
        await world.ProcessAsync(universe);

        GameEvent rejected = await world.EventAsync(eventId);
        Assert.Equal(GameEventState.Rejected, rejected.State);
        Assert.Equal("Changed my mind.", rejected.Error);
        Assert.Equal(Plenty, (await world.PlanetAsync(planet.Id)).Stock);
    }

    /// <summary>Changes the planet, schedules a follow-up, then throws.</summary>
    private sealed class ExplodingHandler : IGameEventHandler
    {
        public const string Type = "TestExploding";

        public string EventType => Type;

        public string? Queue => null;

        public async Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken)
        {
            Planet planet = (await context.LoadOwnedPlanetAsync(cancellationToken))!;
            planet.Stock = ResourceAmounts.Zero;
            context.ScheduleFollowUp("TestFollowUp", new { }, TimeSpan.FromSeconds(5), null, null);
            throw new InvalidOperationException("Simulated bug in a handler.");
        }

        public Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    /// <summary>Deducts costs, then decides to reject.</summary>
    private sealed class PayThenRejectHandler : IGameEventHandler
    {
        public const string Type = "TestPayThenReject";

        public string EventType => Type;

        public string? Queue => null;

        public async Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken)
        {
            Planet planet = (await context.LoadOwnedPlanetAsync(cancellationToken))!;
            planet.Stock -= ResourceAmounts.Uniform(500);
            return AcceptResult.Reject("Changed my mind.");
        }

        public Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

internal static class UniverseProcessingResultAssert
{
    public static void NothingLeft(Processing.UniverseProcessingResult result) => Assert.False(result.MoreEventsDue);
}
