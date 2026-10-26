using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Verteidigungsanlage: produces defenses, one order at a time. Consumes supply resources only while an order is in progress.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel sheet "Kosten und Erträge", section "Sondergebäude", row "Verteidigungsanlage".
/// Game design (confirmed 2026-09-16): every building that works consumes resources; an order building
/// works while an order is in progress.
/// </remarks>
public static class DefenseSystem
{
    /// <summary>The rules of the Verteidigungsanlage.</summary>
    public static OrderBuildingRules Rules { get; } = new(
        Type: BuildingType.DefenseSystem,
        BuildCostBaseValues: new ResourceAmounts { Ore = BaseValue.Normal, Mana = BaseValue.Normal, Crystals = BaseValue.Few, Elementium = BaseValue.Few },
        UpkeepBaseValues: new ResourceAmounts { Energy = BaseValue.Normal, Mana = BaseValue.Much, Antimatter = BaseValue.Few, Elementium = BaseValue.Normal });
}
