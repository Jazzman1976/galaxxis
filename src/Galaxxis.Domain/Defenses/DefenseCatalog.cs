using Galaxxis.Domain.Combat;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Defenses;

/// <summary>
/// The rules of all defense classes that can be produced.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel defenses table ("Verteidigung (Verteidigungsanlage)"). Some cells of the defensive
/// variants are mis-formatted as percentages in the spreadsheet ("5000 %" means 50); the values below are the
/// corrected numbers, which the legacy game used as well. The production duration of 100 seconds is taken
/// over from the legacy game; the concept does not define one.
/// </remarks>
public static class DefenseCatalog
{
    /// <summary>Kleines Manuelles Geschütz: the smallest defense.</summary>
    public static DefenseRules LightManualCannon { get; } = new(
        DefenseType.LightManualCannon,
        OffensiveCost: new ResourceAmounts { Tools = 100, Crystals = 100, Antimatter = 200, Elementium = 50 },
        DefensiveCost: new ResourceAmounts { Tools = 100, Crystals = 200, Antimatter = 100, Elementium = 50 },
        OffensiveCombat: new CombatStats(WeaponSlots: 2, ArmorSlots: 1, PowerUpSlots: 2, StructurePoints: 50, CpuPower: 9, AccuracyPercent: 45, ShieldPercent: 20, AttackPower: 40, Armor: 10),
        DefensiveCombat: new CombatStats(WeaponSlots: 1, ArmorSlots: 2, PowerUpSlots: 2, StructurePoints: 50, CpuPower: 9, AccuracyPercent: 40, ShieldPercent: 25, AttackPower: 35, Armor: 15),
        ProductionDuration: TimeSpan.FromSeconds(100));

    /// <summary>All defense classes that can currently be produced.</summary>
    public static IReadOnlyList<DefenseRules> All { get; } = [LightManualCannon];

    /// <summary>Gets the rules of a defense class.</summary>
    /// <param name="type">The defense class.</param>
    /// <returns>The rules of that class.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The class cannot be produced yet.</exception>
    public static DefenseRules For(DefenseType type) =>
        All.FirstOrDefault(rules => rules.Type == type)
        ?? throw new ArgumentOutOfRangeException(nameof(type), type, "This defense class cannot be produced yet.");
}
