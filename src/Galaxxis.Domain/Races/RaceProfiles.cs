using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Races;

/// <summary>
/// The profiles of all six races, exactly as defined in the concept.
/// </summary>
/// <remarks>
/// <para>
/// Concept: KonzeptWord §5.1–5.6. Production bonuses and penalties follow the specialisation scheme
/// of §4.1: each race specialises in two kinds of use (primary and secondary), which gives two resources
/// a 1.5x bonus and one resource a 0.75x penalty.
/// </para>
/// <para>
/// The legacy code implemented the same factors inside each building class; they now live in one place.
/// </para>
/// </remarks>
public static class RaceProfiles
{
    private static readonly IReadOnlyDictionary<Race, RaceProfile> ProfilesByRace = new Dictionary<Race, RaceProfile>
    {
        // §5.1 Menschen: primary supply, secondary goods production.
        [Race.Human] = new(
            Race.Human,
            Strategy.Defensive,
            WeaponTechnology.Kinetic,
            ArmorType.Structural,
            ResourceAmounts.One with { Energy = 1.5, Antimatter = 1.5, Crystals = 0.75 }),

        // §5.2 Zwerge: primary construction, secondary goods production.
        [Race.Dwarf] = new(
            Race.Dwarf,
            Strategy.Offensive,
            WeaponTechnology.Kinetic,
            ArmorType.Structural,
            ResourceAmounts.One with { Ore = 1.5, Crystals = 1.5, Antimatter = 0.75 }),

        // §5.3 Elfen: primary supply, secondary construction.
        [Race.Elf] = new(
            Race.Elf,
            Strategy.Defensive,
            WeaponTechnology.Magic,
            ArmorType.AntiMagic,
            ResourceAmounts.One with { Energy = 1.5, Mana = 1.5, Crystals = 0.75 }),

        // §5.4 Goblins: primary construction, secondary supply.
        [Race.Goblin] = new(
            Race.Goblin,
            Strategy.Offensive,
            WeaponTechnology.Magic,
            ArmorType.AntiMagic,
            ResourceAmounts.One with { Ore = 1.5, Mana = 1.5, Antimatter = 0.75 }),

        // §5.5 Trolle: primary goods production, secondary supply.
        [Race.Troll] = new(
            Race.Troll,
            Strategy.Defensive,
            WeaponTechnology.Energetic,
            ArmorType.Compensating,
            ResourceAmounts.One with { Tools = 1.5, Antimatter = 1.5, Mana = 0.75 }),

        // §5.6 Orks: primary goods production, secondary construction.
        [Race.Orc] = new(
            Race.Orc,
            Strategy.Offensive,
            WeaponTechnology.Energetic,
            ArmorType.Compensating,
            ResourceAmounts.One with { Tools = 1.5, Crystals = 1.5, Mana = 0.75 }),
    };

    /// <summary>Gets the profile of a race.</summary>
    /// <param name="race">The race.</param>
    /// <returns>The profile of that race.</returns>
    public static RaceProfile For(Race race) =>
        ProfilesByRace.TryGetValue(race, out RaceProfile? profile)
            ? profile
            : throw new ArgumentOutOfRangeException(nameof(race), race, "Unknown race.");
}
