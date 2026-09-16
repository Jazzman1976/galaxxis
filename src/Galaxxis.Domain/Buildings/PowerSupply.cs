using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Kraftwerk: produces energy continuously. Energy is the most widely consumed supply resource.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", row "Kraftwerk". Race production factors are applied
/// by <see cref="Economy.PlanetEconomy"/>, not here.
/// </remarks>
public static class PowerSupply
{
    /// <summary>The rules of the Kraftwerk.</summary>
    public static ResourceBuildingRules Rules { get; } = new(
        Type: BuildingType.PowerSupply,
        Produces: ResourceType.Energy,

        // 200 per day at level 1 (category I production base value; not one of the BaseValue guide values).
        ProductionBaseValue: 200,

        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.Normal, Mana = BaseValue.VeryFew, Crystals = BaseValue.VeryFew },
        UpkeepBaseValues: new ResourceAmounts { Mana = BaseValue.VeryFew, Antimatter = BaseValue.VeryFew, Elementium = BaseValue.VeryFew });
}
