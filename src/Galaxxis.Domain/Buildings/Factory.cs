using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Fabrik: produces tools continuously. Tools are needed to produce ships and defenses.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", row "Fabrik". Race production factors are applied
/// by <see cref="Economy.PlanetEconomy"/>, not here.
/// </remarks>
public static class Factory
{
    /// <summary>The rules of the Fabrik.</summary>
    public static ResourceBuildingRules Rules { get; } = new(
        Type: BuildingType.Factory,
        Produces: ResourceType.Tools,

        // 200 per day at level 1 (category I production base value; not one of the BaseValue guide values).
        ProductionBaseValue: 200,

        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.Few, Mana = BaseValue.VeryFew, Crystals = BaseValue.Few },
        UpkeepBaseValues: new ResourceAmounts { Energy = BaseValue.VeryFew, Mana = BaseValue.VeryFew, Antimatter = BaseValue.VeryFew, Elementium = BaseValue.VeryFew });
}
