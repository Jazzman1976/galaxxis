using Galaxxis.Domain.Combat;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Ships;

/// <summary>
/// The rules of all ship classes that can be produced.
/// </summary>
/// <remarks>
/// Concept: ConzeptExcel ships table ("Schiffe (Schiffswerft)"). Only the classes the legacy game already
/// offered are included; the table defines six more, which are added together with their feature.
/// The production duration of 100 seconds is taken over from the legacy game; the concept does not define one.
/// </remarks>
public static class ShipCatalog
{
    /// <summary>Aufklärer: small, fast scout.</summary>
    public static ShipRules Explorer { get; } = new(
        ShipType.Explorer,
        OffensiveCost: new ResourceAmounts { Tools = 225, Crystals = 225, Antimatter = 450, Elementium = 75 },
        DefensiveCost: new ResourceAmounts { Tools = 225, Crystals = 450, Antimatter = 225, Elementium = 75 },
        OffensiveCombat: new CombatStats(WeaponSlots: 2, ArmorSlots: 1, PowerUpSlots: 2, StructurePoints: 50, CpuPower: 9, AccuracyPercent: 45, ShieldPercent: 20, AttackPower: 40, Armor: 10),
        DefensiveCombat: new CombatStats(WeaponSlots: 1, ArmorSlots: 2, PowerUpSlots: 2, StructurePoints: 50, CpuPower: 9, AccuracyPercent: 40, ShieldPercent: 25, AttackPower: 35, Armor: 15),
        Travel: new ShipTravelStats(DriveStrength: 1000, ElementiumPerDistanceUnit: 1, StorageCapacity: 50),
        ProductionDuration: TimeSpan.FromSeconds(100));

    /// <summary>Frachter: slower transport ship with large storage.</summary>
    public static ShipRules Carrier { get; } = new(
        ShipType.Carrier,
        OffensiveCost: new ResourceAmounts { Tools = 450, Crystals = 450, Antimatter = 900, Elementium = 150 },
        DefensiveCost: new ResourceAmounts { Tools = 450, Crystals = 900, Antimatter = 450, Elementium = 150 },
        OffensiveCombat: new CombatStats(WeaponSlots: 2, ArmorSlots: 2, PowerUpSlots: 3, StructurePoints: 80, CpuPower: 8, AccuracyPercent: 40, ShieldPercent: 20, AttackPower: 64, Armor: 16),
        DefensiveCombat: new CombatStats(WeaponSlots: 1, ArmorSlots: 3, PowerUpSlots: 3, StructurePoints: 80, CpuPower: 8, AccuracyPercent: 35, ShieldPercent: 25, AttackPower: 56, Armor: 24),
        Travel: new ShipTravelStats(DriveStrength: 800, ElementiumPerDistanceUnit: 2, StorageCapacity: 500),
        ProductionDuration: TimeSpan.FromSeconds(100));

    /// <summary>All ship classes that can currently be produced.</summary>
    public static IReadOnlyList<ShipRules> All { get; } = [Explorer, Carrier];

    /// <summary>Gets the rules of a ship class.</summary>
    /// <param name="type">The ship class.</param>
    /// <returns>The rules of that class.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The class cannot be produced yet.</exception>
    public static ShipRules For(ShipType type) =>
        All.FirstOrDefault(rules => rules.Type == type)
        ?? throw new ArgumentOutOfRangeException(nameof(type), type, "This ship class cannot be produced yet.");
}
