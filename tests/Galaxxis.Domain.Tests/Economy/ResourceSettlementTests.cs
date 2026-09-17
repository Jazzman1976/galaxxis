using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Economy;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Tests.Economy;

/// <summary>
/// Settling a planet's stock over a period of time.
/// </summary>
public class ResourceSettlementTests
{
    private const int Precision = 6;

    private static readonly RaceProfile Dwarf = RaceProfiles.For(Race.Dwarf);
    private static readonly RaceProfile Elf = RaceProfiles.For(Race.Elf);

    private static readonly ResourceAmounts LargeStock = ResourceAmounts.Uniform(1_000_000);

    [Fact]
    public void Nothing_changes_when_no_time_passed()
    {
        ResourceAmounts stock = new() { Ore = 10, Energy = 20 };

        Assert.Equal(stock, ResourceSettlement.Settle(stock, PlanetBuildings.Empty, Dwarf, TimeSpan.Zero));
    }

    [Fact]
    public void A_negative_period_is_rejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ResourceSettlement.Settle(ResourceAmounts.Zero, PlanetBuildings.Empty, Dwarf, TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void Without_shortage_the_stock_changes_linearly()
    {
        var buildings = PlanetBuildings.Empty with { OreMine = new ResourceBuildingState(5, ProductionSpeed.Full) };
        ResourceAmounts dailyRate = PlanetEconomy.NetDailyRates(buildings, Dwarf, []);

        ResourceAmounts settled = ResourceSettlement.Settle(LargeStock, buildings, Dwarf, TimeSpan.FromHours(6));

        AssertEqual(LargeStock + dailyRate * 0.25, settled);
    }

    [Fact]
    public void Matches_the_legacy_per_tick_loop_while_no_stock_runs_empty()
    {
        // A mixed planet where some supply resources decline, but the stock is large enough never to run empty.
        var buildings = new PlanetBuildings
        {
            OreMine = new ResourceBuildingState(5, ProductionSpeed.Full),
            PowerSupply = new ResourceBuildingState(5, ProductionSpeed.Full),
            Factory = new ResourceBuildingState(2, ProductionSpeed.Full),
            MageAcademy = new ResourceBuildingState(3, ProductionSpeed.Full),
            CrystalMine = new ResourceBuildingState(2, ProductionSpeed.Full),
            NeutronAccelerator = new ResourceBuildingState(3, ProductionSpeed.Full),
            ElementiumSynthesizer = new ResourceBuildingState(2, ProductionSpeed.Full),
        };
        const int seconds = 3600;

        // Smaller than LargeStock on purpose: the legacy loop adds 3,600 tiny amounts, and floating point
        // rounding error grows with the magnitude of the stock.
        ResourceAmounts stock = ResourceAmounts.Uniform(100_000);

        ResourceAmounts legacy = LegacyTickLoop(stock, buildings, Elf, ticks: seconds);
        ResourceAmounts settled = ResourceSettlement.Settle(stock, buildings, Elf, TimeSpan.FromSeconds(seconds));

        AssertEqual(legacy, settled);
    }

    [Fact]
    public void When_a_supply_resource_runs_empty_its_consumers_are_throttled_from_that_moment_on()
    {
        // Dwarf planet, mage academy level 10, no power supply: energy demand 2,445/day vs. base income 50/day.
        var buildings = PlanetBuildings.Empty with { MageAcademy = new ResourceBuildingState(10, ProductionSpeed.Full) };
        ResourceAmounts stock = LargeStock with { Energy = 100 };

        const double energyDemand = 2445;
        const double manaOutput = 14350;
        double daysUntilEmpty = 100 / (energyDemand - 50);
        double ratio = 50 / energyDemand;

        ResourceAmounts settled = ResourceSettlement.Settle(stock, buildings, Dwarf, TimeSpan.FromDays(1));

        Assert.Equal(0, settled.Energy, Precision);

        // Full mana production until energy ran out, throttled production afterwards.
        double expectedMana = 1_000_000
            + (30 + manaOutput) * daysUntilEmpty
            + (30 + manaOutput * ratio) * (1 - daysUntilEmpty);
        Assert.Equal(expectedMana, settled.Mana, Precision);

        // Elementium consumption (815/day) is throttled the same way; base income 10/day.
        double expectedElementium = 1_000_000
            + (10 - 815) * daysUntilEmpty
            + (10 - 815 * ratio) * (1 - daysUntilEmpty);
        Assert.Equal(expectedElementium, settled.Elementium, Precision);
    }

    [Fact]
    public void Stocks_never_become_negative()
    {
        var buildings = new PlanetBuildings
        {
            OreMine = new ResourceBuildingState(15, ProductionSpeed.Full),
            Factory = new ResourceBuildingState(15, ProductionSpeed.Full),
            MageAcademy = new ResourceBuildingState(15, ProductionSpeed.Full),
            CrystalMine = new ResourceBuildingState(15, ProductionSpeed.Full),
            NeutronAccelerator = new ResourceBuildingState(15, ProductionSpeed.Full),
            ElementiumSynthesizer = new ResourceBuildingState(15, ProductionSpeed.Full),
            Shipyard = new OrderBuildingState(15, IsWorking: true),
            DefenseSystem = new OrderBuildingState(15, IsWorking: true),
        };

        foreach (TimeSpan period in new[] { TimeSpan.FromSeconds(1), TimeSpan.FromHours(1), TimeSpan.FromDays(30) })
        {
            ResourceAmounts settled = ResourceSettlement.Settle(ResourceAmounts.Uniform(500), buildings, Dwarf, period);

            Assert.All(ResourceTypes.All, resource => Assert.True(settled[resource] >= 0, $"{resource} is negative after {period}: {settled}"));
        }
    }

    [Fact]
    public void A_planet_with_empty_stocks_and_overbuilt_consumers_still_produces_something()
    {
        // The worst economic decision possible: everything empty, every resource building at full speed,
        // no power supply. The player must never be locked out.
        var buildings = new PlanetBuildings
        {
            OreMine = new ResourceBuildingState(20, ProductionSpeed.Full),
            Factory = new ResourceBuildingState(20, ProductionSpeed.Full),
            MageAcademy = new ResourceBuildingState(20, ProductionSpeed.Full),
            CrystalMine = new ResourceBuildingState(20, ProductionSpeed.Full),
            NeutronAccelerator = new ResourceBuildingState(20, ProductionSpeed.Full),
            ElementiumSynthesizer = new ResourceBuildingState(20, ProductionSpeed.Full),
        };

        ResourceAmounts settled = ResourceSettlement.Settle(ResourceAmounts.Zero, buildings, Dwarf, TimeSpan.FromDays(1));

        // More than the base income alone: the throttled mines still contribute.
        Assert.True(settled.Ore > BaseIncome.PerDay.Ore * Dwarf.ProductionFactors.Ore, $"The ore mine must keep producing: {settled}");
        Assert.True(settled.Tools > BaseIncome.PerDay.Tools * Dwarf.ProductionFactors.Tools, $"The factory must keep producing: {settled}");
        Assert.All(ResourceTypes.All, resource => Assert.True(settled[resource] >= 0));
    }

    [Fact]
    public void Stopping_all_buildings_lets_every_stock_recover_through_the_base_income()
    {
        var stopped = new PlanetBuildings
        {
            OreMine = new ResourceBuildingState(20, ProductionSpeed.Stopped),
            MageAcademy = new ResourceBuildingState(20, ProductionSpeed.Stopped),
            ElementiumSynthesizer = new ResourceBuildingState(20, ProductionSpeed.Stopped),
        };

        ResourceAmounts settled = ResourceSettlement.Settle(ResourceAmounts.Zero, stopped, Dwarf, TimeSpan.FromDays(1));

        AssertEqual(BaseIncome.PerDay.MultipliedBy(Dwarf.ProductionFactors), settled);
    }

    [Fact]
    public void An_unclaimed_planet_produces_exactly_its_base_income_without_race_factors()
    {
        var plundered = new ResourceAmounts { Ore = 3 };

        ResourceAmounts settled = ResourceSettlement.SettleUnclaimed(plundered, TimeSpan.FromDays(2));

        // Concept "Grundertrag (pro 24h)": 50 / 30 / 10 per day.
        AssertEqual(new ResourceAmounts { Ore = 103, Energy = 100, Tools = 100, Mana = 60, Crystals = 60, Antimatter = 60, Elementium = 20 }, settled);
        Assert.Throws<ArgumentOutOfRangeException>(() => ResourceSettlement.SettleUnclaimed(plundered, TimeSpan.FromSeconds(-1)));
    }

    [Fact]
    public void Settling_in_one_step_or_in_many_small_steps_gives_the_same_result_without_shortage()
    {
        var buildings = PlanetBuildings.Empty with
        {
            OreMine = new ResourceBuildingState(8, ProductionSpeed.Full),
            PowerSupply = new ResourceBuildingState(6, new ProductionSpeed(70)),
        };

        ResourceAmounts oneStep = ResourceSettlement.Settle(LargeStock, buildings, Dwarf, TimeSpan.FromHours(10));

        ResourceAmounts manySteps = LargeStock;
        for (int hour = 0; hour < 10; hour++)
        {
            manySteps = ResourceSettlement.Settle(manySteps, buildings, Dwarf, TimeSpan.FromHours(1));
        }

        AssertEqual(oneStep, manySteps);
    }

    /// <summary>
    /// Re-implementation of the legacy <c>PlanetCache.UpdateRessources</c> loop (one iteration per second),
    /// kept here as a reference. Legacy formula per tick: (race factor × (production + base income) × usage
    /// − Σ upkeep × usage) / 86,400, applied only if the stock stays non-negative.
    /// </summary>
    private static ResourceAmounts LegacyTickLoop(ResourceAmounts stock, PlanetBuildings buildings, RaceProfile race, int ticks)
    {
        ResourceAmounts perTick = ResourceAmounts.Zero;

        foreach (ResourceBuildingRules rules in BuildingCatalog.ResourceBuildings)
        {
            ResourceBuildingState state = buildings.ResourceBuilding(rules.Type);
            double usage = state.Speed.Factor;
            double income = race.ProductionFactors[rules.Produces]
                * (rules.DailyProductionAt(state.Level) + BaseIncome.PerDay[rules.Produces]) * usage;

            perTick = perTick.With(rules.Produces, perTick[rules.Produces] + income / 86_400);
            perTick -= rules.DailyUpkeepAt(state.Level) * (usage / 86_400);
        }

        for (int tick = 0; tick < ticks; tick++)
        {
            foreach (ResourceType resource in ResourceTypes.All)
            {
                double next = stock[resource] + perTick[resource];
                if (next >= 0)
                {
                    stock = stock.With(resource, next);
                }
            }
        }

        return stock;
    }

    private static void AssertEqual(ResourceAmounts expected, ResourceAmounts actual)
    {
        foreach (ResourceType resource in ResourceTypes.All)
        {
            Assert.True(
                Math.Abs(expected[resource] - actual[resource]) < 1e-6,
                $"{resource}: expected {expected[resource]}, actual {actual[resource]}");
        }
    }
}
