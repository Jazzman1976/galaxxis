using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Fleets;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Ships;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Galaxxis.Testing;
using Microsoft.EntityFrameworkCore;

namespace Galaxxis.Engine.Tests;

public class ProductionAndFleetTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    private static readonly ResourceAmounts Plenty = ResourceAmounts.Uniform(10_000);

    [Fact]
    public async Task A_ship_is_produced_and_the_shipyard_works_only_meanwhile()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(
            universe, Race.Dwarf, Plenty, PlanetBuildings.Empty.WithLevel(BuildingType.Shipyard, 1));

        long eventId = await world.RequestAsync(universe, GameEventTypes.ProduceShip, new ProduceShipPayload(ShipType.Explorer), player.Id, planet.Id);
        await world.ProcessAsync(universe);

        Planet producing = await world.PlanetAsync(planet.Id);
        Assert.True(producing.ShipyardIsWorking);
        // Dwarves are offensive: they build the offensive explorer variant.
        Assert.Equal(Plenty - ShipCatalog.Explorer.CostFor(Strategy.Offensive), producing.Stock);

        world.Advance(ShipCatalog.Explorer.ProductionDuration);
        await world.ProcessAsync(universe);

        Assert.Equal(GameEventState.Done, (await world.EventAsync(eventId)).State);
        Assert.False((await world.PlanetAsync(planet.Id)).ShipyardIsWorking);

        await using GameDbContext database = world.Database();
        Ship ship = await database.Ships.SingleAsync(s => s.PlanetId == planet.Id, EngineHarness.Cancellation);
        Assert.Equal(ShipType.Explorer, ship.Type);
        Assert.Equal(Strategy.Offensive, ship.Strategy);
        Assert.Equal(50, ship.StorageCapacity);
    }

    [Fact]
    public async Task Ships_need_a_shipyard()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player player, Planet planet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Dwarf, Plenty);

        long eventId = await world.RequestAsync(universe, GameEventTypes.ProduceShip, new ProduceShipPayload(ShipType.Explorer), player.Id, planet.Id);
        await world.ProcessAsync(universe);

        GameEvent rejected = await world.EventAsync(eventId);
        Assert.Equal(GameEventState.Rejected, rejected.State);
        Assert.Contains("shipyard", rejected.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task An_attack_plunders_the_target_and_the_loot_arrives_with_the_returning_ships()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player attacker, Planet home) = await world.CreatePlayerWithPlanetAsync(universe, Race.Orc, ResourceAmounts.Uniform(100));
        Planet target = await world.CreateUnclaimedPlanetAsync(universe, ResourceAmounts.Uniform(1000));
        List<Ship> ships = await world.CreateShipsAsync(attacker, home, count: 2, storageCapacity: 350);

        long attackId = await world.RequestAsync(universe, GameEventTypes.FleetAttack,
            new FleetMissionPayload(target.Id, ships.Select(s => s.Id).ToList()), attacker.Id, home.Id);
        await world.ProcessAsync(universe);
        Assert.Equal(GameEventState.Scheduled, (await world.EventAsync(attackId)).State);

        // Arrival: 700 storage, split evenly over seven resources.
        world.Advance(FleetRules.TravelDuration);
        await world.ProcessAsync(universe);

        GameEvent attack = await world.EventAsync(attackId);
        Assert.Equal(GameEventState.Done, attack.State);
        Assert.Equal(ResourceAmounts.Uniform(100), GameEventPayload.Deserialize<FleetMissionPayload>(attack.Payload).Result);

        // The unclaimed target produced its base income during the 100-second flight, then lost the loot.
        ResourceAmounts incomeDuringFlight = BaseIncome.PerDay * (FleetRules.TravelDuration.TotalSeconds / 86_400);
        AssertClose(ResourceAmounts.Uniform(900) + incomeDuringFlight, (await world.PlanetAsync(target.Id)).Stock);

        // Return: the loot is added at home and the ships are free again.
        world.Advance(FleetRules.TravelDuration);
        await world.ProcessAsync(universe);

        GameEvent returned = (await world.EventsAsync(universe)).Single(e => e.Type == GameEventTypes.FleetReturn);
        Assert.Equal(GameEventState.Done, returned.State);

        Planet homeAfter = await world.PlanetAsync(home.Id);
        Assert.True(homeAfter.Ore > 200 - 0.01, $"Loot of 100 ore expected on top of 100: {homeAfter.Stock}");

        await using GameDbContext database = world.Database();
        Assert.All(await database.Ships.Where(s => s.PlanetId == home.Id).ToListAsync(EngineHarness.Cancellation), ship => Assert.Null(ship.MissionEventId));
    }

    [Fact]
    public async Task Ships_on_a_mission_cannot_be_sent_on_a_second_one()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player attacker, Planet home) = await world.CreatePlayerWithPlanetAsync(universe, Race.Orc, Plenty);
        Planet target = await world.CreateUnclaimedPlanetAsync(universe, Plenty);
        List<int> shipIds = (await world.CreateShipsAsync(attacker, home, count: 1, storageCapacity: 50)).Select(s => s.Id).ToList();

        long first = await world.RequestAsync(universe, GameEventTypes.FleetAttack, new FleetMissionPayload(target.Id, shipIds), attacker.Id, home.Id);
        long second = await world.RequestAsync(universe, GameEventTypes.FleetScan, new FleetMissionPayload(target.Id, shipIds), attacker.Id, home.Id);
        await world.ProcessAsync(universe);

        Assert.Equal(GameEventState.Scheduled, (await world.EventAsync(first)).State);
        Assert.Equal(GameEventState.Rejected, (await world.EventAsync(second)).State);
    }

    [Fact]
    public async Task A_scan_reports_the_target_stock()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player scout, Planet home) = await world.CreatePlayerWithPlanetAsync(universe, Race.Elf, Plenty);
        var targetStock = new ResourceAmounts { Ore = 123, Crystals = 45 };
        Planet target = await world.CreateUnclaimedPlanetAsync(universe, targetStock);
        List<int> shipIds = (await world.CreateShipsAsync(scout, home, count: 1, storageCapacity: 50)).Select(s => s.Id).ToList();

        long scanId = await world.RequestAsync(universe, GameEventTypes.FleetScan, new FleetMissionPayload(target.Id, shipIds), scout.Id, home.Id);
        await world.ProcessAsync(universe);
        world.Advance(FleetRules.TravelDuration);
        await world.ProcessAsync(universe);

        GameEvent scan = await world.EventAsync(scanId);
        Assert.Equal(GameEventState.Done, scan.State);

        // The report shows the stock at arrival: the initial stock plus the base income of the flight time.
        ResourceAmounts expected = targetStock + BaseIncome.PerDay * (FleetRules.TravelDuration.TotalSeconds / 86_400);
        AssertClose(expected, GameEventPayload.Deserialize<FleetMissionPayload>(scan.Payload).Result!.Value);
        AssertClose(expected, (await world.PlanetAsync(target.Id)).Stock); // scanning takes nothing
    }

    [Fact]
    public async Task A_plundered_unclaimed_planet_fills_up_again_with_its_base_income()
    {
        await using EngineHarness world = await EngineHarness.CreateAsync(databases);
        Universe universe = await world.CreateUniverseAsync();
        (Player attacker, Planet home) = await world.CreatePlayerWithPlanetAsync(universe, Race.Orc, Plenty);
        Planet target = await world.CreateUnclaimedPlanetAsync(universe, ResourceAmounts.Uniform(1000));
        List<int> shipIds = (await world.CreateShipsAsync(attacker, home, count: 1, storageCapacity: 1_000_000)).Select(s => s.Id).ToList();

        // Plunder everything (arrival at tick 100), wait for the return (tick 200).
        await world.RequestAsync(universe, GameEventTypes.FleetAttack, new FleetMissionPayload(target.Id, shipIds), attacker.Id, home.Id);
        await world.ProcessAsync(universe);
        world.Advance(FleetRules.TravelDuration);
        await world.ProcessAsync(universe);
        AssertClose(ResourceAmounts.Zero, (await world.PlanetAsync(target.Id)).Stock);
        world.Advance(FleetRules.TravelDuration);
        await world.ProcessAsync(universe);

        // A day later, a scan arrives at tick 86,700: the planet has been refilling since the plunder at tick 100.
        world.Advance(TimeSpan.FromDays(1));
        long scanId = await world.RequestAsync(universe, GameEventTypes.FleetScan, new FleetMissionPayload(target.Id, shipIds), attacker.Id, home.Id);
        await world.ProcessAsync(universe);
        world.Advance(FleetRules.TravelDuration);
        await world.ProcessAsync(universe);

        ResourceAmounts report = GameEventPayload.Deserialize<FleetMissionPayload>((await world.EventAsync(scanId)).Payload).Result!.Value;
        AssertClose(BaseIncome.PerDay * (86_600 / 86_400.0), report);
    }

    private static void AssertClose(ResourceAmounts expected, ResourceAmounts actual)
    {
        foreach (ResourceType resource in ResourceTypes.All)
        {
            Assert.True(Math.Abs(expected[resource] - actual[resource]) < 1e-6, $"{resource}: expected {expected[resource]}, actual {actual[resource]}");
        }
    }
}
