namespace Galaxxis.Domain.Races;

/// <summary>
/// The six playable races. Each race has its own production strengths and weaknesses,
/// weapon and armor technology and strategy; see <see cref="RaceProfiles"/>.
/// </summary>
/// <remarks>Concept: KonzeptWord §4 and §5.</remarks>
public enum Race
{
    /// <summary>Menschen.</summary>
    Human,

    /// <summary>Zwerge.</summary>
    Dwarf,

    /// <summary>Elfen.</summary>
    Elf,

    /// <summary>Goblins.</summary>
    Goblin,

    /// <summary>Trolle.</summary>
    Troll,

    /// <summary>Orks. (The legacy code spelled this "Ork"; the new code uses the English "Orc".)</summary>
    Orc,
}
