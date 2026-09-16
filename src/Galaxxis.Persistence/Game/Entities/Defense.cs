using Galaxxis.Domain.Defenses;
using Galaxxis.Domain.Races;

namespace Galaxxis.Persistence.Game.Entities;

/// <summary>
/// A single defense installation on a planet.
/// </summary>
/// <remarks>Attributes are copied from the defense rules when it is produced, like for ships.</remarks>
public sealed class Defense
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The owning player.</summary>
    public int PlayerId { get; set; }

    /// <summary>Navigation to the owning player.</summary>
    public Player Player { get; set; } = null!;

    /// <summary>The planet the defense protects.</summary>
    public int PlanetId { get; set; }

    /// <summary>Navigation to the planet.</summary>
    public Planet Planet { get; set; } = null!;

    /// <summary>The defense class.</summary>
    public DefenseType Type { get; set; }

    /// <summary>Whether this is the offensive or defensive variant.</summary>
    public Strategy Strategy { get; set; }

    /// <summary>Sockets for weapon goods.</summary>
    public int WeaponSlots { get; set; }

    /// <summary>Sockets for armor goods.</summary>
    public int ArmorSlots { get; set; }

    /// <summary>Sockets for power-up goods.</summary>
    public int PowerUpSlots { get; set; }

    /// <summary>Hit points.</summary>
    public int StructurePoints { get; set; }

    /// <summary>Initiative and capture chance.</summary>
    public int CpuPower { get; set; }

    /// <summary>Chance to hit in percent.</summary>
    public int AccuracyPercent { get; set; }

    /// <summary>Chance to absorb a hit in percent.</summary>
    public int ShieldPercent { get; set; }

    /// <summary>Damage points of a hit.</summary>
    public int AttackPower { get; set; }

    /// <summary>Points subtracted from each hit.</summary>
    public int Armor { get; set; }
}
