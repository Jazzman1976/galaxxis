using Galaxxis.Domain.Planets;

namespace Galaxxis.Domain.Universes.Generation;

/// <summary>
/// Generates a complete universe (galaxies, star systems, planets and moons) as a blueprint.
/// </summary>
/// <remarks>
/// <para>
/// <b>Reproducible.</b> All randomness comes from one <see cref="Random"/> created from the seed, and the
/// universe is built in a fixed order. The same settings and seed therefore always produce exactly the same
/// universe, which makes generation testable and a universe re-creatable. (.NET keeps the algorithm of a
/// seeded <see cref="Random"/> stable across versions for exactly this purpose.)
/// </para>
/// <para>
/// <b>Pure.</b> Nothing is saved here. The Application module's universe generation service stores the
/// blueprint in the database in a single transaction.
/// </para>
/// </remarks>
public static class UniverseGenerator
{
    /// <summary>Generates a universe blueprint.</summary>
    /// <param name="settings">Size and density of the universe.</param>
    /// <param name="seed">The random seed. Use the same seed to generate the same universe again.</param>
    /// <returns>The generated universe.</returns>
    /// <exception cref="ArgumentException">The settings are invalid.</exception>
    /// <exception cref="InvalidOperationException">The objects do not fit into the coordinate range.</exception>
    public static UniverseBlueprint Generate(UniverseGenerationSettings settings, int seed)
    {
        settings.Validate();

        Random random = new(seed);
        NameGenerator names = new(random);

        // A random name is always drawn, even when a name was chosen: otherwise choosing a name would shift the
        // random sequence and the same seed would produce a different map.
        string randomName = names.Next();
        string universeName = string.IsNullOrWhiteSpace(settings.Name) ? randomName : settings.Name.Trim();
        names.Reserve(universeName);

        List<GalaxyBlueprint> galaxies = [];
        foreach (Coordinate galaxyCoordinate in Place(random, settings, settings.GalaxyCount, "galaxies"))
        {
            List<StarSystemBlueprint> starSystems = [];
            foreach (Coordinate systemCoordinate in Place(random, settings, settings.StarSystemsPerGalaxy, "star systems"))
            {
                List<PlanetBlueprint> planets = [];
                foreach (Coordinate planetCoordinate in Place(random, settings, settings.PlanetsPerStarSystem, "planets"))
                {
                    planets.Add(new PlanetBlueprint(
                        names.Next(),
                        planetCoordinate,
                        PlanetDefaults.PlanetFields,
                        CreateMoons(random, names, settings)));
                }

                starSystems.Add(new StarSystemBlueprint(names.Next(), systemCoordinate, planets));
            }

            galaxies.Add(new GalaxyBlueprint(names.Next(), galaxyCoordinate, starSystems));
        }

        return new UniverseBlueprint(universeName, seed, galaxies);
    }

    private static IReadOnlyList<Coordinate> Place(Random random, UniverseGenerationSettings settings, int count, string level) =>
        CoordinateScatter.Place(random, count, settings.CoordinateRange, settings.MinimumDistance, level);

    private static IReadOnlyList<MoonBlueprint> CreateMoons(Random random, NameGenerator names, UniverseGenerationSettings settings)
    {
        // One roll decides the number of moons: [0, two) → 2 moons, [two, two + one) → 1 moon, otherwise none.
        double roll = random.NextDouble();
        int moonCount = roll < settings.TwoMoonsProbability ? 2
            : roll < settings.TwoMoonsProbability + settings.OneMoonProbability ? 1
            : 0;

        return Enumerable.Range(0, moonCount)
            .Select(_ => new MoonBlueprint(names.Next(), PlanetDefaults.MoonFields))
            .ToList();
    }
}
