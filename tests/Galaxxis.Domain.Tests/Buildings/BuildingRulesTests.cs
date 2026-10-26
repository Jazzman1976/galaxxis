using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Planets;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Tests.Buildings;

/// <summary>
/// Building numbers must match the concept spreadsheet (ConzeptExcel, sheet "Kosten und Erträge").
/// The sheet lists all values at "Stufe X" = level 20.
/// </summary>
public class BuildingRulesTests
{
    private const int SpreadsheetLevel = 20;

    public static TheoryData<BuildingType, ResourceAmounts> BuildCostsAtLevel20 => new()
    {
        { BuildingType.OreMine, new ResourceAmounts { Ore = 14450, Mana = 14450, Crystals = 7225 } },
        { BuildingType.PowerSupply, new ResourceAmounts { Ore = 21675, Mana = 7225, Crystals = 7225 } },
        { BuildingType.Factory, new ResourceAmounts { Ore = 14450, Mana = 7225, Crystals = 14450 } },
        { BuildingType.MageAcademy, new ResourceAmounts { Ore = 21675, Mana = 14450, Crystals = 21675, Elementium = 7225 } },
        { BuildingType.CrystalMine, new ResourceAmounts { Ore = 21675, Mana = 21675, Crystals = 14450, Elementium = 7225 } },
        { BuildingType.NeutronAccelerator, new ResourceAmounts { Ore = 28900, Mana = 14450, Crystals = 14450, Elementium = 7225 } },
        { BuildingType.ElementiumSynthesizer, new ResourceAmounts { Ore = 36125, Mana = 28900, Crystals = 28900, Elementium = 14450 } },
        { BuildingType.Shipyard, new ResourceAmounts { Ore = 21675, Mana = 14450, Crystals = 21675, Elementium = 14450 } },
        { BuildingType.DefenseSystem, new ResourceAmounts { Ore = 21675, Mana = 21675, Crystals = 14450, Elementium = 14450 } },
    };

    public static TheoryData<BuildingType, ResourceAmounts> UpkeepAtLevel20 => new()
    {
        { BuildingType.OreMine, new ResourceAmounts { Energy = 3615, Mana = 3615, Elementium = 3615 } },
        { BuildingType.PowerSupply, new ResourceAmounts { Mana = 3615, Antimatter = 3615, Elementium = 3615 } },
        { BuildingType.Factory, new ResourceAmounts { Energy = 3615, Mana = 3615, Antimatter = 3615, Elementium = 3615 } },
        { BuildingType.MageAcademy, new ResourceAmounts { Energy = 10845, Antimatter = 10845, Elementium = 3615 } },
        { BuildingType.CrystalMine, new ResourceAmounts { Energy = 3615, Mana = 7230, Antimatter = 10845, Elementium = 3615 } },
        { BuildingType.NeutronAccelerator, new ResourceAmounts { Energy = 7230, Mana = 10845, Elementium = 3615 } },
        { BuildingType.ElementiumSynthesizer, new ResourceAmounts { Energy = 10845, Mana = 10845, Antimatter = 10845 } },
        { BuildingType.Shipyard, new ResourceAmounts { Energy = 10845, Mana = 7230, Antimatter = 14460, Elementium = 10845 } },
        { BuildingType.DefenseSystem, new ResourceAmounts { Energy = 10845, Mana = 14460, Antimatter = 7230, Elementium = 10845 } },
    };

    [Theory]
    [MemberData(nameof(BuildCostsAtLevel20))]
    public void Build_costs_match_the_concept_spreadsheet(BuildingType type, ResourceAmounts expected)
    {
        Assert.Equal(expected, BuildingCatalog.For(type).BuildCostFor(SpreadsheetLevel));
    }

    [Theory]
    [MemberData(nameof(UpkeepAtLevel20))]
    public void Upkeep_matches_the_concept_spreadsheet(BuildingType type, ResourceAmounts expected)
    {
        Assert.Equal(expected, BuildingCatalog.For(type).DailyUpkeepAt(SpreadsheetLevel));
    }

    [Theory]
    // The spreadsheet's "auf Stufe X" column includes the base income (50 / 30 / 10), the rules exclude it.
    [InlineData(BuildingType.OreMine, ResourceType.Ore, 72450 - 50)]
    [InlineData(BuildingType.PowerSupply, ResourceType.Energy, 72450 - 50)]
    [InlineData(BuildingType.Factory, ResourceType.Tools, 72450 - 50)]
    [InlineData(BuildingType.MageAcademy, ResourceType.Mana, 63380 - 30)]
    [InlineData(BuildingType.CrystalMine, ResourceType.Crystals, 63380 - 30)]
    [InlineData(BuildingType.NeutronAccelerator, ResourceType.Antimatter, 63380 - 30)]
    [InlineData(BuildingType.ElementiumSynthesizer, ResourceType.Elementium, 27160 - 10)]
    public void Production_matches_the_concept_spreadsheet(BuildingType type, ResourceType produces, double expected)
    {
        var rules = (ResourceBuildingRules)BuildingCatalog.For(type);

        Assert.Equal(produces, rules.Produces);
        Assert.Equal(expected, rules.DailyProductionAt(SpreadsheetLevel));
    }

    [Fact]
    public void Total_supply_upkeep_of_all_resource_buildings_matches_the_spreadsheet_balance()
    {
        // ConzeptExcel "Billanz Rohstoffe", row "Gesamt Verbrauch bei Stufe X".
        ResourceAmounts total = BuildingCatalog.ResourceBuildings
            .Aggregate(ResourceAmounts.Zero, (sum, rules) => sum + rules.DailyUpkeepAt(SpreadsheetLevel));

        Assert.Equal(new ResourceAmounts { Energy = 39765, Mana = 39765, Antimatter = 39765, Elementium = 21690 }, total);
    }

    [Theory]
    [InlineData(1, 25)]
    [InlineData(2, 150)]
    [InlineData(20, 45150)]
    public void Build_duration_grows_from_25_seconds_at_level_1(int targetLevel, double expectedSeconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), OreMine.Rules.BuildDurationFor(targetLevel));
    }

    [Fact]
    public void Every_building_consumes_supply_resources_while_it_works()
    {
        // Game design principle: whenever a building does something (produce, research, process), it consumes
        // resources. A building added without consumption values would silently break the economy balance.
        foreach (BuildingType type in Enum.GetValues<BuildingType>())
        {
            ResourceAmounts upkeep = BuildingCatalog.For(type).UpkeepBaseValues;

            Assert.True(
                ResourceTypes.Supply.Any(resource => upkeep[resource] > 0),
                $"{type} defines no consumption. Every building that works must consume supply resources.");
            Assert.All(
                ResourceTypes.All.Except(ResourceTypes.Supply),
                resource => Assert.True(upkeep[resource] == 0, $"{type} consumes {resource}, which is not a supply resource."));
        }
    }

    [Fact]
    public void Every_building_type_has_rules()
    {
        foreach (BuildingType type in Enum.GetValues<BuildingType>())
        {
            Assert.Equal(type, BuildingCatalog.For(type).Type);
        }
    }

    [Fact]
    public void A_fresh_planet_can_afford_the_first_level_of_every_resource_building()
    {
        ResourceAmounts allFirstLevels = BuildingCatalog.ResourceBuildings
            .Aggregate(ResourceAmounts.Zero, (sum, rules) => sum + rules.BuildCostFor(1));

        Assert.True(PlanetDefaults.StartingStock.Covers(allFirstLevels));
    }
}
