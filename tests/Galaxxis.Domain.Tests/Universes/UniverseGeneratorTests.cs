using Galaxxis.Domain.Planets;
using Galaxxis.Domain.Universes;
using Galaxxis.Domain.Universes.Generation;

namespace Galaxxis.Domain.Tests.Universes;

public class UniverseGeneratorTests
{
    private static readonly UniverseGenerationSettings DefaultSettings = new();

    [Fact]
    public void The_same_seed_generates_the_same_universe()
    {
        UniverseBlueprint first = UniverseGenerator.Generate(DefaultSettings, seed: 42);
        UniverseBlueprint second = UniverseGenerator.Generate(DefaultSettings, seed: 42);

        Assert.Equal(Describe(first), Describe(second));
    }

    [Fact]
    public void A_different_seed_generates_a_different_universe()
    {
        Assert.NotEqual(
            Describe(UniverseGenerator.Generate(DefaultSettings, seed: 1)),
            Describe(UniverseGenerator.Generate(DefaultSettings, seed: 2)));
    }

    [Fact]
    public void The_default_universe_has_ten_galaxies_with_ten_systems_with_ten_planets()
    {
        UniverseBlueprint universe = UniverseGenerator.Generate(DefaultSettings, seed: 7);

        Assert.Equal(10, universe.Galaxies.Count);
        Assert.All(universe.Galaxies, galaxy => Assert.Equal(10, galaxy.StarSystems.Count));
        Assert.Equal(1000, universe.PlanetCount);
        Assert.All(AllPlanets(universe), planet => Assert.Equal(PlanetDefaults.PlanetFields, planet.Fields));
    }

    [Fact]
    public void All_names_in_a_universe_are_unique()
    {
        UniverseBlueprint universe = UniverseGenerator.Generate(DefaultSettings, seed: 3);

        List<string> names = [universe.Name];
        foreach (GalaxyBlueprint galaxy in universe.Galaxies)
        {
            names.Add(galaxy.Name);
            foreach (StarSystemBlueprint system in galaxy.StarSystems)
            {
                names.Add(system.Name);
                foreach (PlanetBlueprint planet in system.Planets)
                {
                    names.Add(planet.Name);
                    names.AddRange(planet.Moons.Select(moon => moon.Name));
                }
            }
        }

        Assert.Equal(names.Count, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Neighbours_keep_the_minimum_distance_and_stay_within_the_range()
    {
        UniverseBlueprint universe = UniverseGenerator.Generate(DefaultSettings, seed: 11);

        AssertPlacement(universe.Galaxies.Select(galaxy => galaxy.Coordinate).ToList());
        foreach (GalaxyBlueprint galaxy in universe.Galaxies)
        {
            AssertPlacement(galaxy.StarSystems.Select(system => system.Coordinate).ToList());
            foreach (StarSystemBlueprint system in galaxy.StarSystems)
            {
                AssertPlacement(system.Planets.Select(planet => planet.Coordinate).ToList());
            }
        }
    }

    [Fact]
    public void About_one_planet_in_ten_has_a_moon()
    {
        UniverseBlueprint universe = UniverseGenerator.Generate(DefaultSettings, seed: 5);
        List<PlanetBlueprint> planets = AllPlanets(universe).ToList();

        double withMoons = planets.Count(planet => planet.Moons.Count > 0) / (double)planets.Count;
        double withTwoMoons = planets.Count(planet => planet.Moons.Count == 2) / (double)planets.Count;

        Assert.InRange(withMoons, 0.05, 0.15);
        Assert.InRange(withTwoMoons, 0.0, 0.04);
        Assert.All(planets.SelectMany(planet => planet.Moons), moon => Assert.Equal(PlanetDefaults.MoonFields, moon.Fields));
    }

    [Fact]
    public void Choosing_a_name_does_not_change_the_map_of_a_seed()
    {
        UniverseBlueprint random = UniverseGenerator.Generate(DefaultSettings, seed: 9);
        UniverseBlueprint named = UniverseGenerator.Generate(DefaultSettings with { Name = "Andromeda" }, seed: 9);

        Assert.Equal(
            AllPlanets(random).Select(planet => planet.Coordinate),
            AllPlanets(named).Select(planet => planet.Coordinate));
    }

    [Fact]
    public void A_chosen_name_is_used_for_the_universe()
    {
        UniverseBlueprint universe = UniverseGenerator.Generate(DefaultSettings with { Name = "  Andromeda  " }, seed: 1);

        Assert.Equal("Andromeda", universe.Name);
        Assert.Equal(1, universe.Seed);
    }

    [Fact]
    public void Invalid_settings_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => UniverseGenerator.Generate(DefaultSettings with { GalaxyCount = 0 }, seed: 1));
        Assert.Throws<ArgumentException>(() => UniverseGenerator.Generate(DefaultSettings with { OneMoonProbability = 0.8, TwoMoonsProbability = 0.5 }, seed: 1));
    }

    [Fact]
    public void Settings_that_cannot_fit_report_a_clear_error_instead_of_a_smaller_universe()
    {
        // At most 27 positions exist within a range of ±1, and a distance of 2 leaves far fewer usable.
        var impossible = DefaultSettings with { GalaxyCount = 30, CoordinateRange = 1, MinimumDistance = 2 };

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => UniverseGenerator.Generate(impossible, seed: 1));
        Assert.Contains("galaxies", error.Message);
    }

    private static void AssertPlacement(IReadOnlyList<Coordinate> coordinates)
    {
        int range = DefaultSettings.CoordinateRange;
        Assert.All(coordinates, coordinate =>
        {
            Assert.InRange(coordinate.X, -range, range);
            Assert.InRange(coordinate.Y, -range, range);
            Assert.InRange(coordinate.Z, -range, range);
        });

        for (int i = 0; i < coordinates.Count; i++)
        {
            for (int j = i + 1; j < coordinates.Count; j++)
            {
                Assert.True(coordinates[i].DistanceTo(coordinates[j]) >= DefaultSettings.MinimumDistance);
            }
        }
    }

    private static IEnumerable<PlanetBlueprint> AllPlanets(UniverseBlueprint universe) =>
        universe.Galaxies.SelectMany(galaxy => galaxy.StarSystems).SelectMany(system => system.Planets);

    /// <summary>Flattens a blueprint into text, because records holding lists compare lists by reference.</summary>
    private static string Describe(UniverseBlueprint universe) =>
        string.Join('\n', universe.Galaxies.SelectMany(galaxy =>
            galaxy.StarSystems.SelectMany(system =>
                system.Planets.Select(planet =>
                    $"{galaxy.Name}{galaxy.Coordinate}/{system.Name}{system.Coordinate}/{planet.Name}{planet.Coordinate}" +
                    $"[{string.Join(',', planet.Moons.Select(moon => moon.Name))}]"))));
}
