using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Neutronenbeschleuniger: produces antimatter continuously. Antimatter is consumed as supply and used for goods production.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", row "Neutronenbeschleuniger". Race production factors are applied
/// by <see cref="Economy.PlanetEconomy"/>, not here.
/// </remarks>
public static class NeutronAccelerator
{
    /// <summary>The rules of the Neutronenbeschleuniger.</summary>
    public static ResourceBuildingRules Rules { get; } = new(
        Type: BuildingType.NeutronAccelerator,
        Produces: ResourceType.Antimatter,

        // 175 per day at level 1 (category II production base value; not one of the BaseValue guide values).
        ProductionBaseValue: 175,

        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.Much, Mana = BaseValue.Few, Crystals = BaseValue.Few, Elementium = BaseValue.VeryFew },
        UpkeepBaseValues: new ResourceAmounts { Energy = BaseValue.Few, Mana = BaseValue.Normal, Elementium = BaseValue.VeryFew });
}
