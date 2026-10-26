namespace Galaxxis.Domain.Universes.Generation;

/// <summary>
/// Everything that shapes a generated universe: its size and how densely it is populated.
/// </summary>
/// <remarks>
/// The defaults reproduce the universes of the legacy generator: 10 galaxies with 10 star systems with
/// 10 planets each (1,000 planets), coordinates from -10 to 10 on each axis, a minimum distance of 2
/// between neighbours, and roughly one planet in ten with a moon.
/// </remarks>
public sealed record UniverseGenerationSettings
{
    /// <summary>Name of the universe. When empty, a random name is generated.</summary>
    public string? Name { get; init; }

    /// <summary>Number of galaxies in the universe.</summary>
    public int GalaxyCount { get; init; } = 10;

    /// <summary>Number of star systems in each galaxy.</summary>
    public int StarSystemsPerGalaxy { get; init; } = 10;

    /// <summary>Number of planets in each star system.</summary>
    public int PlanetsPerStarSystem { get; init; } = 10;

    /// <summary>
    /// Coordinates are drawn between <c>-CoordinateRange</c> and <c>+CoordinateRange</c> on every axis,
    /// at every level (galaxies in the universe, systems in a galaxy, planets in a system).
    /// </summary>
    public int CoordinateRange { get; init; } = 10;

    /// <summary>Minimum distance between two neighbours at the same level.</summary>
    public double MinimumDistance { get; init; } = 2;

    /// <summary>Probability (0–1) that a planet has exactly one moon.</summary>
    public double OneMoonProbability { get; init; } = 0.09;

    /// <summary>Probability (0–1) that a planet has two moons.</summary>
    public double TwoMoonsProbability { get; init; } = 0.01;

    /// <summary>
    /// Checks that the settings describe a universe that can actually be generated.
    /// </summary>
    /// <exception cref="ArgumentException">A setting is out of range; the message names it.</exception>
    public void Validate()
    {
        RequireAtLeastOne(GalaxyCount, nameof(GalaxyCount));
        RequireAtLeastOne(StarSystemsPerGalaxy, nameof(StarSystemsPerGalaxy));
        RequireAtLeastOne(PlanetsPerStarSystem, nameof(PlanetsPerStarSystem));
        RequireAtLeastOne(CoordinateRange, nameof(CoordinateRange));

        if (MinimumDistance < 0)
        {
            throw new ArgumentException($"{nameof(MinimumDistance)} must not be negative.", nameof(MinimumDistance));
        }

        if (OneMoonProbability is < 0 or > 1 || TwoMoonsProbability is < 0 or > 1
            || OneMoonProbability + TwoMoonsProbability > 1)
        {
            throw new ArgumentException(
                $"{nameof(OneMoonProbability)} and {nameof(TwoMoonsProbability)} must be between 0 and 1 and add up to at most 1.");
        }
    }

    private static void RequireAtLeastOne(int value, string name)
    {
        if (value < 1)
        {
            throw new ArgumentException($"{name} must be at least 1, but was {value}.", name);
        }
    }
}
