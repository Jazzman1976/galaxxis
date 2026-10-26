using Galaxxis.Domain.Universes;

namespace Galaxxis.Persistence.Game.Entities;

/// <summary>A galaxy within a universe.</summary>
public sealed class Galaxy
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The universe this galaxy belongs to.</summary>
    public int UniverseId { get; set; }

    /// <summary>Navigation to the universe.</summary>
    public Universe Universe { get; set; } = null!;

    /// <summary>Display name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>X position within the universe.</summary>
    public int X { get; set; }

    /// <summary>Y position within the universe.</summary>
    public int Y { get; set; }

    /// <summary>Z position within the universe.</summary>
    public int Z { get; set; }

    /// <summary>The star systems of the galaxy.</summary>
    public List<StarSystem> StarSystems { get; set; } = [];

    /// <summary>The position as a Domain coordinate. Not stored separately; reads and writes <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/>.</summary>
    public Coordinate Coordinate
    {
        get => new(X, Y, Z);
        set => (X, Y, Z) = (value.X, value.Y, value.Z);
    }
}
