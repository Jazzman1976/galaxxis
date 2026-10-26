using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Economy;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Tests.Economy;

/// <summary>
/// Daily production, consumption and shortage throttling of a planet.
/// </summary>
public class PlanetEconomyTests
{
    private const int Precision = 9;

    private static readonly RaceProfile Dwarf = RaceProfiles.For(Race.Dwarf);
    private static readonly RaceProfile Human = RaceProfiles.For(Race.Human);

    [Fact]
    public void An_empty_planet_produces_the_base_income_with_race_factors()
    {
        ResourceAmounts production = PlanetEconomy.GrossDailyProduction(PlanetBuildings.Empty, Human);

        // Human: energy and antimatter x1.5, crystals x0.75.
        Assert.Equal(new ResourceAmounts
        {
            Ore = 50, Energy = 75, Tools = 50, Mana = 30, Crystals = 22.5, Antimatter = 45, Elementium = 10,
        }, production);
        Assert.Equal(ResourceAmounts.Zero, PlanetEconomy.GrossDailyDemand(PlanetBuildings.Empty));
    }

    [Fact]
    public void Production_and_consumption_scale_with_the_speed_but_the_base_income_does_not()
    {
        var buildings = PlanetBuildings.Empty with { OreMine = new ResourceBuildingState(1, new ProductionSpeed(60)) };

        ResourceAmounts production = PlanetEconomy.GrossDailyProduction(buildings, Dwarf);
        ResourceAmounts demand = PlanetEconomy.GrossDailyDemand(buildings);

        // Dwarf ore x1.5: (base income 50 + 60 % of 200) x 1.5
        Assert.Equal((50 + 0.6 * 200) * 1.5, production.Ore, Precision);
        // 60 % of the level-1 upkeep of 5 per day
        Assert.Equal(new ResourceAmounts { Energy = 3, Mana = 3, Elementium = 3 }, demand);
    }

    [Fact]
    public void A_stopped_building_neither_produces_nor_consumes()
    {
        var buildings = PlanetBuildings.Empty with { OreMine = new ResourceBuildingState(10, ProductionSpeed.Stopped) };

        Assert.Equal(PlanetEconomy.GrossDailyProduction(PlanetBuildings.Empty, Dwarf), PlanetEconomy.GrossDailyProduction(buildings, Dwarf));
        Assert.Equal(ResourceAmounts.Zero, PlanetEconomy.GrossDailyDemand(buildings));
    }

    [Fact]
    public void Order_buildings_consume_only_while_working()
    {
        var idle = PlanetBuildings.Empty with { Shipyard = new OrderBuildingState(1, IsWorking: false) };
        var working = PlanetBuildings.Empty with { Shipyard = new OrderBuildingState(1, IsWorking: true) };

        Assert.Equal(ResourceAmounts.Zero, PlanetEconomy.GrossDailyDemand(idle));
        Assert.Equal(Shipyard.Rules.DailyUpkeepAt(1), PlanetEconomy.GrossDailyDemand(working));
    }

    [Fact]
    public void Without_empty_supply_resources_nothing_is_throttled()
    {
        var buildings = PlanetBuildings.Empty with { MageAcademy = new ResourceBuildingState(10, ProductionSpeed.Full) };

        ResourceAmounts net = PlanetEconomy.NetDailyRates(buildings, Dwarf, emptySupplyResources: []);

        ResourceAmounts expected = PlanetEconomy.GrossDailyProduction(buildings, Dwarf) - PlanetEconomy.GrossDailyDemand(buildings);
        Assert.Equal(expected, net);
    }

    [Fact]
    public void An_empty_supply_resource_throttles_its_consumers_so_demand_equals_production()
    {
        // Mage academy level 10 demands 2,445 energy per day; a dwarf planet without power supply
        // only produces the base income of 50 energy.
        var buildings = PlanetBuildings.Empty with { MageAcademy = new ResourceBuildingState(10, ProductionSpeed.Full) };
        const double energyDemand = 15 * (2 * 81 + 1);
        double ratio = 50 / energyDemand;

        ResourceAmounts net = PlanetEconomy.NetDailyRates(buildings, Dwarf, [ResourceType.Energy]);

        Assert.Equal(0, net.Energy, Precision);
        // Mana: base income 30 plus the throttled share of 175 * (81 + 1) = 14,350.
        Assert.Equal(30 + 14350 * ratio, net.Mana, Precision);
        // Consumption of other resources is throttled by the same ratio.
        Assert.Equal(22.5 - 2445 * ratio, net.Antimatter, Precision);
    }

    [Fact]
    public void An_empty_resource_whose_production_covers_the_demand_throttles_nothing()
    {
        var buildings = PlanetBuildings.Empty with
        {
            OreMine = new ResourceBuildingState(1, ProductionSpeed.Full),
            PowerSupply = new ResourceBuildingState(5, ProductionSpeed.Full),
        };

        ResourceAmounts throttled = PlanetEconomy.NetDailyRates(buildings, Dwarf, [ResourceType.Energy]);
        ResourceAmounts unthrottled = PlanetEconomy.NetDailyRates(buildings, Dwarf, []);

        Assert.Equal(unthrottled, throttled);
    }

    [Fact]
    public void Production_orders_are_served_first_and_can_stop_resource_buildings_completely()
    {
        // A producing shipyard at level 5 alone demands 495 energy per day, far more than the base income of 50.
        var buildings = PlanetBuildings.Empty with
        {
            MageAcademy = new ResourceBuildingState(10, ProductionSpeed.Full),
            Shipyard = new OrderBuildingState(5, IsWorking: true),
        };

        ResourceAmounts net = PlanetEconomy.NetDailyRates(buildings, Dwarf, [ResourceType.Energy]);

        // The mage academy stands still: mana is only the base income of 30, minus the shipyard's own
        // mana upkeep of 10 * (2 * 16 + 1) = 330.
        Assert.Equal(30 - 330, net.Mana, Precision);
        // Energy: base income 50 minus the shipyard's 495; the throttled mage academy consumes nothing.
        Assert.Equal(50 - 495, net.Energy, Precision);
    }

    [Fact]
    public void The_missing_resources_of_a_payment_are_reported_per_resource()
    {
        var stock = new ResourceAmounts { Ore = 100, Mana = 5 };
        var cost = new ResourceAmounts { Ore = 60, Mana = 20, Crystals = 3 };

        Assert.Equal(new ResourceAmounts { Mana = 15, Crystals = 3 }, stock.MissingFor(cost));
    }
}
