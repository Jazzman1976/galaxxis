using Galaxxis.Domain.Races;
using Galaxxis.Domain.Ships;

namespace Galaxxis.Persistence.Game.Entities;

/// <summary>
/// A single ship owned by a player.
/// </summary>
/// <remarks>
/// Attributes are copied from the ship rules when the ship is produced and stored per ship, because they
/// will change individually later (equipment sockets, damage).
/// </remarks>
public sealed class Ship
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The owning player.</summary>
    public int PlayerId { get; set; }

    /// <summary>Navigation to the owning player.</summary>
    public Player Player { get; set; } = null!;

    /// <summary>The planet the ship is currently stationed at.</summary>
    public int PlanetId { get; set; }

    /// <summary>Navigation to the planet the ship is stationed at.</summary>
    public Planet Planet { get; set; } = null!;

    /// <summary>The ship class.</summary>
    public ShipType Type { get; set; }

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

    /// <summary>Speed in distance units per hour.</summary>
    public int DriveStrength { get; set; }

    /// <summary>Elementium consumed per distance unit travelled.</summary>
    public int ElementiumPerDistanceUnit { get; set; }

    /// <summary>Resources the ship can carry.</summary>
    public int StorageCapacity { get; set; }
}
