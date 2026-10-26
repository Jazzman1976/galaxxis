using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Magier Akademie: produces mana continuously. Mana is used for construction and consumed as supply.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", row "Magier Akademie". Race production factors are applied
/// by <see cref="Economy.PlanetEconomy"/>, not here.
/// </remarks>
public static class MageAcademy
{
    /// <summary>The rules of the Magier Akademie.</summary>
    public static ResourceBuildingRules Rules { get; } = new(
        Type: BuildingType.MageAcademy,
        Produces: ResourceType.Mana,

        // 175 per day at level 1 (category II production base value; not one of the BaseValue guide values).
        ProductionBaseValue: 175,

        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.Normal, Mana = BaseValue.Few, Crystals = BaseValue.Normal, Elementium = BaseValue.VeryFew },
        UpkeepBaseValues: new ResourceAmounts { Energy = BaseValue.Normal, Antimatter = BaseValue.Normal, Elementium = BaseValue.VeryFew });
}
