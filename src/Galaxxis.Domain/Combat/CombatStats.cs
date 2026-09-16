namespace Galaxxis.Domain.Combat;

/// <summary>
/// The combat attributes shared by ships and defenses.
/// </summary>
/// <remarks>
/// Concept: KonzeptWord §7.1 (meaning of each attribute) and ConzeptExcel ships and defenses tables (values).
/// The combat system that uses them is not implemented yet.
/// </remarks>
/// <param name="WeaponSlots">Sockets for weapon goods ("Waffenslots").</param>
/// <param name="ArmorSlots">Sockets for armor goods ("Rüstungsslots").</param>
/// <param name="PowerUpSlots">Sockets for power-up goods ("PowerUp Slots").</param>
/// <param name="StructurePoints">Hit points; the unit is destroyed or captured at 0 (§7.1.7).</param>
/// <param name="CpuPower">Decides initiative and capture chance (§7.1.8).</param>
/// <param name="AccuracyPercent">Chance to hit, capped at 95 % (§7.1.1).</param>
/// <param name="ShieldPercent">Chance to absorb a hit completely, capped at 65 % (§7.1.2).</param>
/// <param name="AttackPower">Damage points of a hit before armor (§7.1.3).</param>
/// <param name="Armor">Points subtracted from every hit (§7.1.4).</param>
public sealed record CombatStats(
    int WeaponSlots,
    int ArmorSlots,
    int PowerUpSlots,
    int StructurePoints,
    int CpuPower,
    int AccuracyPercent,
    int ShieldPercent,
    int AttackPower,
    int Armor);
