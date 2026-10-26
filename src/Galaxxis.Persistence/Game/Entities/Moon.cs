namespace Galaxxis.Persistence.Game.Entities;

/// <summary>A moon orbiting a planet. Moons have no game function yet besides existing.</summary>
public sealed class Moon
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The planet the moon orbits.</summary>
    public int PlanetId { get; set; }

    /// <summary>Navigation to the planet.</summary>
    public Planet Planet { get; set; } = null!;

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Number of building plots.</summary>
    public int Fields { get; set; }
}
