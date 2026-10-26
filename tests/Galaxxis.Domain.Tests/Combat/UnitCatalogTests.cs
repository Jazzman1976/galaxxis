using Galaxxis.Domain.Defenses;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Ships;

namespace Galaxxis.Domain.Tests.Combat;

/// <summary>
/// Ship and defense values must match the concept spreadsheet (ConzeptExcel ships and defenses tables).
/// </summary>
public class UnitCatalogTests
{
    [Fact]
    public void Explorer_costs_depend_on_the_race_strategy()
    {
        Assert.Equal(
            new ResourceAmounts { Tools = 225, Crystals = 225, Antimatter = 450, Elementium = 75 },
            ShipCatalog.Explorer.CostFor(Strategy.Offensive));
        Assert.Equal(
            new ResourceAmounts { Tools = 225, Crystals = 450, Antimatter = 225, Elementium = 75 },
            ShipCatalog.Explorer.CostFor(Strategy.Defensive));
    }

    [Fact]
    public void Carrier_has_the_spreadsheet_travel_and_combat_values()
    {
        Assert.Equal(new ShipTravelStats(DriveStrength: 800, ElementiumPerDistanceUnit: 2, StorageCapacity: 500), ShipCatalog.Carrier.Travel);
        Assert.Equal(56, ShipCatalog.Carrier.CombatFor(Strategy.Defensive).AttackPower);
        Assert.Equal(24, ShipCatalog.Carrier.CombatFor(Strategy.Defensive).Armor);
    }

    [Fact]
    public void Light_manual_cannon_uses_the_corrected_defensive_values()
    {
        // The spreadsheet shows "5000 %", "3500 %" and "1500 %" for the defensive variant: meant are 50, 35 and 15.
        var defensive = DefenseCatalog.LightManualCannon.CombatFor(Strategy.Defensive);

        Assert.Equal(50, defensive.StructurePoints);
        Assert.Equal(35, defensive.AttackPower);
        Assert.Equal(15, defensive.Armor);
    }

    [Fact]
    public void Classes_without_rules_cannot_be_produced_yet()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ShipCatalog.For(ShipType.Battlestation));
        Assert.Throws<ArgumentOutOfRangeException>(() => DefenseCatalog.For(DefenseType.HeavyTurret));
    }
}
