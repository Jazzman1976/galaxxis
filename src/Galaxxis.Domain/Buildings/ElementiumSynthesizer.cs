using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Elementium Synthetisierer: produces elementium continuously. Elementium is needed for everything.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", row "Elementium Synthetisierer". Race production factors are applied
/// by <see cref="Economy.PlanetEconomy"/>, not here.
/// </remarks>
public static class ElementiumSynthesizer
{
    /// <summary>The rules of the Elementium Synthetisierer.</summary>
    public static ResourceBuildingRules Rules { get; } = new(
        Type: BuildingType.ElementiumSynthesizer,
        Produces: ResourceType.Elementium,

        // 75 per day at level 1 (category III production base value; not one of the BaseValue guide values).
        ProductionBaseValue: 75,

        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.VeryMuch, Mana = BaseValue.Much, Crystals = BaseValue.Much, Elementium = BaseValue.Few },
        UpkeepBaseValues: new ResourceAmounts { Energy = BaseValue.Normal, Mana = BaseValue.Normal, Antimatter = BaseValue.Normal });
}
