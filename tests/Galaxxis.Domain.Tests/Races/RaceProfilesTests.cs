using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Tests.Races;

/// <summary>
/// Race profiles must match KonzeptWord §5.1–5.6.
/// </summary>
public class RaceProfilesTests
{
    [Theory]
    [InlineData(Race.Human, ResourceType.Energy, ResourceType.Antimatter, ResourceType.Crystals, Strategy.Defensive, WeaponTechnology.Kinetic, ArmorType.Structural)]
    [InlineData(Race.Dwarf, ResourceType.Ore, ResourceType.Crystals, ResourceType.Antimatter, Strategy.Offensive, WeaponTechnology.Kinetic, ArmorType.Structural)]
    [InlineData(Race.Elf, ResourceType.Energy, ResourceType.Mana, ResourceType.Crystals, Strategy.Defensive, WeaponTechnology.Magic, ArmorType.AntiMagic)]
    [InlineData(Race.Goblin, ResourceType.Ore, ResourceType.Mana, ResourceType.Antimatter, Strategy.Offensive, WeaponTechnology.Magic, ArmorType.AntiMagic)]
    [InlineData(Race.Troll, ResourceType.Tools, ResourceType.Antimatter, ResourceType.Mana, Strategy.Defensive, WeaponTechnology.Energetic, ArmorType.Compensating)]
    [InlineData(Race.Orc, ResourceType.Tools, ResourceType.Crystals, ResourceType.Mana, Strategy.Offensive, WeaponTechnology.Energetic, ArmorType.Compensating)]
    public void Profile_matches_the_concept(
        Race race,
        ResourceType firstBonus,
        ResourceType secondBonus,
        ResourceType penalty,
        Strategy strategy,
        WeaponTechnology weapons,
        ArmorType armor)
    {
        RaceProfile profile = RaceProfiles.For(race);

        Assert.Equal(race, profile.Race);
        Assert.Equal(strategy, profile.Strategy);
        Assert.Equal(weapons, profile.Weapons);
        Assert.Equal(armor, profile.Armor);

        foreach (ResourceType resource in ResourceTypes.All)
        {
            double expected = resource == firstBonus || resource == secondBonus ? 1.5
                : resource == penalty ? 0.75
                : 1.0;
            Assert.Equal(expected, profile.ProductionFactors[resource]);
        }
    }
}
