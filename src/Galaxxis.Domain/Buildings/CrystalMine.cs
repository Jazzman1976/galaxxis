using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Kristallmine: produces crystals continuously. Crystals are used for construction and goods production.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", row "Kristallmine". Race production factors are applied
/// by <see cref="Economy.PlanetEconomy"/>, not here.
/// </remarks>
public static class CrystalMine
{
    /// <summary>The rules of the Kristallmine.</summary>
    public static ResourceBuildingRules Rules { get; } = new(
        Type: BuildingType.CrystalMine,
        Produces: ResourceType.Crystals,

        // 175 per day at level 1 (category II production base value; not one of the BaseValue guide values).
        ProductionBaseValue: 175,

        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.Normal, Mana = BaseValue.Normal, Crystals = BaseValue.Few, Elementium = BaseValue.VeryFew },
        UpkeepBaseValues: new ResourceAmounts { Energy = BaseValue.VeryFew, Mana = BaseValue.Few, Antimatter = BaseValue.Normal, Elementium = BaseValue.VeryFew });
}
