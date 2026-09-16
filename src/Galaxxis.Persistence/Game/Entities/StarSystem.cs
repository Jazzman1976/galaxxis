using Galaxxis.Domain.Universes;

namespace Galaxxis.Persistence.Game.Entities;

/// <summary>
/// A star system within a galaxy. Named <c>StarSystem</c> rather than <c>System</c> to avoid clashing
/// with the .NET <c>System</c> namespace (the legacy code suffered from that).
/// </summary>
public sealed class StarSystem
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The galaxy this star system belongs to.</summary>
    public int GalaxyId { get; set; }

    /// <summary>Navigation to the galaxy.</summary>
    public Galaxy Galaxy { get; set; } = null!;

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>X position within the galaxy.</summary>
    public int X { get; set; }

    /// <summary>Y position within the galaxy.</summary>
    public int Y { get; set; }

    /// <summary>Z position within the galaxy.</summary>
    public int Z { get; set; }

    /// <summary>The planets of the star system.</summary>
    public List<Planet> Planets { get; set; } = [];

    /// <summary>The position as a Domain coordinate. Not stored separately; reads and writes <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/>.</summary>
    public Coordinate Coordinate
    {
        get => new(X, Y, Z);
        set => (X, Y, Z) = (value.X, value.Y, value.Z);
    }
}
