namespace Galaxxis.Domain.Universes.Generation;

/// <summary>
/// A generated universe that has not been saved yet: the complete tree of galaxies, star systems,
/// planets and moons with names and coordinates.
/// </summary>
/// <remarks>
/// The Application module turns a blueprint into database rows. Keeping generation and saving apart means
/// the generation can be tested without a database and a universe can be inspected before it is created.
/// </remarks>
/// <param name="Name">Name of the universe.</param>
/// <param name="Seed">The random seed that produced this blueprint; the same seed produces the same universe.</param>
/// <param name="Galaxies">The galaxies of the universe.</param>
public sealed record UniverseBlueprint(string Name, int Seed, IReadOnlyList<GalaxyBlueprint> Galaxies)
{
    /// <summary>Total number of planets in the universe.</summary>
    public int PlanetCount => Galaxies.Sum(galaxy => galaxy.StarSystems.Sum(system => system.Planets.Count));
}

/// <summary>A generated galaxy.</summary>
/// <param name="Name">Name of the galaxy.</param>
/// <param name="Coordinate">Position in the universe.</param>
/// <param name="StarSystems">The star systems of the galaxy.</param>
public sealed record GalaxyBlueprint(string Name, Coordinate Coordinate, IReadOnlyList<StarSystemBlueprint> StarSystems);

/// <summary>A generated star system.</summary>
/// <param name="Name">Name of the star system.</param>
/// <param name="Coordinate">Position in the galaxy.</param>
/// <param name="Planets">The planets of the star system.</param>
public sealed record StarSystemBlueprint(string Name, Coordinate Coordinate, IReadOnlyList<PlanetBlueprint> Planets);

/// <summary>A generated planet. It starts without owner and with the default stock and buildings.</summary>
/// <param name="Name">Name of the planet.</param>
/// <param name="Coordinate">Position in the star system.</param>
/// <param name="Fields">Number of building plots.</param>
/// <param name="Moons">The moons of the planet (0–2).</param>
public sealed record PlanetBlueprint(string Name, Coordinate Coordinate, int Fields, IReadOnlyList<MoonBlueprint> Moons);

/// <summary>A generated moon.</summary>
/// <param name="Name">Name of the moon.</param>
/// <param name="Fields">Number of building plots.</param>
public sealed record MoonBlueprint(string Name, int Fields);
