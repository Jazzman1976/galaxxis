using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Planets;

/// <summary>
/// The state a planet starts with when a universe is generated or a player receives a starting planet.
/// </summary>
/// <remarks>
/// Values taken over from the legacy database defaults. The starting stock is enough to build the first
/// level of all seven resource buildings.
/// </remarks>
public static class PlanetDefaults
{
    /// <summary>Resources on a fresh planet.</summary>
    public static ResourceAmounts StartingStock { get; } = new()
    {
        Ore = 200,
        Energy = 200,
        Tools = 200,
        Mana = 175,
        Crystals = 175,
        Antimatter = 175,
        Elementium = 75,
    };

    /// <summary>Buildings on a fresh planet: none built, all resource buildings set to full speed.</summary>
    public static PlanetBuildings Buildings => PlanetBuildings.Empty;

    /// <summary>Number of building plots on a planet ("Bauplätze").</summary>
    public const int PlanetFields = 150;

    /// <summary>Number of building plots on a moon.</summary>
    public const int MoonFields = 20;
}
