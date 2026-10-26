namespace Galaxxis.Domain.Universes.Generation;

/// <summary>
/// Places a number of objects at random coordinates inside a cube, keeping a minimum distance between them.
/// </summary>
/// <remarks>
/// Random positions are tried one after another; a position too close to an already placed object is
/// rejected. This is the approach of the legacy universe creator, with two fixes: the distance check had a
/// typo that compared the Y axis with the Z axis, and a failed placement is now reported as an error instead
/// of silently producing fewer objects.
/// </remarks>
internal static class CoordinateScatter
{
    /// <summary>Upper bound of rejected attempts per placed object before giving up.</summary>
    private const int MaxAttemptsPerObject = 10_000;

    /// <summary>Places <paramref name="count"/> objects.</summary>
    /// <param name="random">The seeded random source of the universe being generated.</param>
    /// <param name="count">How many coordinates to create.</param>
    /// <param name="range">Coordinates lie between <c>-range</c> and <c>+range</c> on every axis.</param>
    /// <param name="minimumDistance">Minimum distance between any two created coordinates.</param>
    /// <param name="level">What is being placed, for the error message (for example "planets").</param>
    /// <returns>The created coordinates, in creation order.</returns>
    /// <exception cref="InvalidOperationException">The objects do not fit into the cube with the required distance.</exception>
    public static IReadOnlyList<Coordinate> Place(Random random, int count, int range, double minimumDistance, string level)
    {
        List<Coordinate> placed = new(count);
        int attempts = 0;

        while (placed.Count < count)
        {
            if (++attempts > MaxAttemptsPerObject * count)
            {
                throw new InvalidOperationException(
                    $"Could not place {count} {level} within a coordinate range of ±{range} while keeping a " +
                    $"minimum distance of {minimumDistance}. Increase the coordinate range, or reduce the " +
                    "count or the minimum distance.");
            }

            // Random.Next's upper bound is exclusive, so range + 1 makes +range reachable.
            Coordinate candidate = new(
                random.Next(-range, range + 1),
                random.Next(-range, range + 1),
                random.Next(-range, range + 1));

            if (placed.All(existing => existing.DistanceTo(candidate) >= minimumDistance))
            {
                placed.Add(candidate);
            }
        }

        return placed;
    }
}
