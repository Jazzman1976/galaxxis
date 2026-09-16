namespace Galaxxis.Domain.Universes;

/// <summary>
/// A position in three-dimensional space.
/// </summary>
/// <remarks>
/// Coordinates are relative to the parent: a galaxy's coordinate is its position in the universe,
/// a star system's coordinate its position within the galaxy, a planet's coordinate its position within
/// the star system.
/// </remarks>
/// <param name="X">Position on the X axis.</param>
/// <param name="Y">Position on the Y axis.</param>
/// <param name="Z">Position on the Z axis.</param>
public readonly record struct Coordinate(int X, int Y, int Z)
{
    /// <summary>Calculates the straight-line (Euclidean) distance to another coordinate.</summary>
    /// <param name="other">The other coordinate, in the same parent space.</param>
    /// <returns>The distance in distance units.</returns>
    public double DistanceTo(Coordinate other)
    {
        double deltaX = other.X - X;
        double deltaY = other.Y - Y;
        double deltaZ = other.Z - Z;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
    }

    /// <summary>Formats the coordinate as "(x|y|z)".</summary>
    /// <returns>The formatted coordinate.</returns>
    public override string ToString() => $"({X}|{Y}|{Z})";
}
