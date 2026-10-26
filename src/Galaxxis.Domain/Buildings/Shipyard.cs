using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Schiffswerft: produces ships, one order at a time. Consumes supply resources only while an order is in progress.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", section "Sondergebäude", row "Schiffswerft".
/// Game design (confirmed 2026-09-16): every building that works consumes resources; an order building
/// works while an order is in progress.
/// </remarks>
public static class Shipyard
{
    /// <summary>The rules of the Schiffswerft.</summary>
    public static OrderBuildingRules Rules { get; } = new(
        Type: BuildingType.Shipyard,
        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.Normal, Mana = BaseValue.Few, Crystals = BaseValue.Normal, Elementium = BaseValue.Few },
        UpkeepBaseValues: new ResourceAmounts { Energy = BaseValue.Normal, Mana = BaseValue.Few, Antimatter = BaseValue.Much, Elementium = BaseValue.Normal });
}
