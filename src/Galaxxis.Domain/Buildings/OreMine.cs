using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Erzmine: produces ore continuously. Ore is the main construction resource.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", row "Erzmine". Race production factors are applied
/// by <see cref="Economy.PlanetEconomy"/>, not here.
/// </remarks>
public static class OreMine
{
    /// <summary>The rules of the Erzmine.</summary>
    public static ResourceBuildingRules Rules { get; } = new(
        Type: BuildingType.OreMine,
        Produces: ResourceType.Ore,

        // 200 per day at level 1 (category I production base value; not one of the BaseValue guide values).
        ProductionBaseValue: 200,

        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.Few, Mana = BaseValue.Few, Crystals = BaseValue.VeryFew },
        UpkeepBaseValues: new ResourceAmounts { Energy = BaseValue.VeryFew, Mana = BaseValue.VeryFew, Elementium = BaseValue.VeryFew });
}
