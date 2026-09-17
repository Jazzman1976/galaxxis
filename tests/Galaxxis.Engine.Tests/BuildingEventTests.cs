using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Engine.Processing;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Galaxxis.Testing;

namespace Galaxxis.Engine.Tests;

public class BuildingEventTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    private static readonly ResourceAmounts Plenty = ResourceAmounts.Uniform(10_000);

    [Fact]
    public async Task A_build_is_paid_on_acceptance_and_completes_after_its_duration()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, Plenty);

        long eventId = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id);
        await world.ProcessAsync(universe);

        GameEvent accepted = await world.EventAsync(eventId);
        Assert.Equal(GameEventState.Scheduled, accepted.State);
        Assert.Equal(25, accepted.ProcessAtTick); // level 1 takes 25 seconds, one tick per second
        Assert.Equal(GameEventQueues.Construction, accepted.Queue);
        Assert.Equal(Plenty - OreMine.Rules.BuildCostFor(1), (await world.PlanetAsync(planet.Id)).Stock);

        // Not due yet after 24 seconds ...
        world.Advance(TimeSpan.FromSeconds(24));
        await world.ProcessAsync(universe);
        Assert.Equal(0, (await world.PlanetAsync(planet.Id)).OreMineLevel);

        // ... but after 25.
        world.Advance(TimeSpan.FromSeconds(1));
        await world.ProcessAsync(universe);
        Assert.Equal(GameEventState.Done, (await world.EventAsync(eventId)).State);
        Assert.Equal(1, (await world.PlanetAsync(planet.Id)).OreMineLevel);
    }

    [Fact]
    public async Task A_build_without_enough_resources_is_rejected_with_the_missing_amounts()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        var stock = new ResourceAmounts { Ore = 3, Mana = 100, Crystals = 100 };
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, stock);

        long eventId = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id);
        await world.ProcessAsync(universe);

        GameEvent rejected = await world.EventAsync(eventId);
        Assert.Equal(GameEventState.Rejected, rejected.State);
        Assert.Equal("Not enough resources: 7 ore missing.", rejected.Error);
        Assert.Equal(stock, (await world.PlanetAsync(planet.Id)).Stock);
    }

    [Fact]
    public async Task Of_twenty_simultaneous_build_requests_exactly_one_is_accepted_and_paid()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Human, Plenty);

        // A double click, several tabs or a script: all requests arrive before the engine runs.
        await Task.WhenAll(Enumerable.Range(0, 20).Select(_ =>
            world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id)));

        await world.ProcessAsync(universe);

        List<GameEvent> events = await world.EventsAsync(universe);
        Assert.Single(events, e => e.State == GameEventState.Scheduled);
        Assert.Equal(19, events.Count(e => e.State == GameEventState.Rejected && e.Error == GameEventExecutor.QueueBusyMessage));
        Assert.Equal(Plenty - OreMine.Rules.BuildCostFor(1), (await world.PlanetAsync(planet.Id)).Stock);
    }

    [Fact]
    public async Task Nobody_can_build_on_a_planet_of_another_player()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (_, Planet victimPlanet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Elf, Plenty);
        (Player intruder, _) = await world.CreatePlayerWithPlanetAsync(universe, Race.Orc, Plenty);

        long eventId = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), intruder.Id, victimPlanet.Id);
        await world.ProcessAsync(universe);

        Assert.Equal(GameEventState.Rejected, (await world.EventAsync(eventId)).State);
        Assert.Equal(Plenty, (await world.PlanetAsync(victimPlanet.Id)).Stock);
    }

    [Fact]
    public async Task A_late_processed_construction_still_takes_effect_at_its_own_tick()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Dwarf, Plenty);

        long eventId = await world.RequestAsync(universe, GameEventTypes.BuildBuilding, new BuildBuildingPayload(BuildingType.OreMine), player.Id, planet.Id);
        await world.ProcessAsync(universe);

        // The server is "down" for over 16 minutes; the construction was due at tick 25.
        world.Advance(TimeSpan.FromSeconds(1000));
        await world.ProcessAsync(universe);

        Planet completed = await world.PlanetAsync(planet.Id);
        Assert.Equal(GameEventState.Done, (await world.EventAsync(eventId)).State);
        Assert.Equal(1, completed.OreMineLevel);

        // Settled exactly to the completion tick: the 25 seconds before used level 0 (only base income, dwarf ore x1.5).
        Assert.Equal(25, completed.ResourcesSettledTick);
        double expectedOre = Plenty.Ore - OreMine.Rules.BuildCostFor(1).Ore + 50 * 1.5 * 25 / 86_400;
        Assert.Equal(expectedOre, completed.Ore, 6);
    }

    [Fact]
    public async Task A_production_speed_change_takes_effect_immediately()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(
            universe, Race.Troll, Plenty, PlanetBuildings.Empty.WithLevel(BuildingType.Factory, 3));

        world.Advance(TimeSpan.FromSeconds(10));
        long valid = await world.RequestAsync(universe, GameEventTypes.ChangeProductionSpeed, new ChangeProductionSpeedPayload(BuildingType.Factory, 40), player.Id, planet.Id);
        long invalid = await world.RequestAsync(universe, GameEventTypes.ChangeProductionSpeed, new ChangeProductionSpeedPayload(BuildingType.Factory, 45), player.Id, planet.Id);
        long notAdjustable = await world.RequestAsync(universe, GameEventTypes.ChangeProductionSpeed, new ChangeProductionSpeedPayload(BuildingType.Shipyard, 50), player.Id, planet.Id);
        await world.ProcessAsync(universe);

        Planet changed = await world.PlanetAsync(planet.Id);
        Assert.Equal(GameEventState.Done, (await world.EventAsync(valid)).State);
        Assert.Equal(40, changed.FactorySpeedPercent);
        Assert.Equal(10, changed.ResourcesSettledTick); // the old speed applied until the change
        Assert.Equal(GameEventState.Rejected, (await world.EventAsync(invalid)).State);
        Assert.Equal(GameEventState.Rejected, (await world.EventAsync(notAdjustable)).State);
    }
}
