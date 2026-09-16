using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Races;

/// <summary>
/// Everything that distinguishes one race from the others.
/// </summary>
/// <param name="Race">The race this profile describes.</param>
/// <param name="Strategy">Whether the race builds offensive or defensive ship and defense variants.</param>
/// <param name="Weapons">The damage type of the race's weapons.</param>
/// <param name="Armor">The armor type of the race's ships and defenses.</param>
/// <param name="ProductionFactors">
/// Multiplier per resource applied to everything a planet of this race produces, including the base
/// income. 1.5 is a bonus, 0.75 a penalty, 1 neutral.
/// </param>
public sealed record RaceProfile(
    Race Race,
    Strategy Strategy,
    WeaponTechnology Weapons,
    ArmorType Armor,
    ResourceAmounts ProductionFactors);
