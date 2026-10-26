namespace Galaxxis.Domain.Buildings;

/// <summary>
/// Looks up the rules of a building by its type.
/// </summary>
/// <remarks>
/// The rules themselves are defined in one class per building (<see cref="OreMine"/>,
/// <see cref="Shipyard"/>, …). This catalog only lists them, so code that works with any building,
/// such as the economy calculation or the build command handler, can find the right rules.
/// A new building needs its rule class, a <see cref="BuildingType"/> member and an entry here.
/// </remarks>
public static class BuildingCatalog
{
    /// <summary>All resource buildings.</summary>
    public static IReadOnlyList<ResourceBuildingRules> ResourceBuildings { get; } =
    [
        OreMine.Rules,
        PowerSupply.Rules,
        Factory.Rules,
        MageAcademy.Rules,
        CrystalMine.Rules,
        NeutronAccelerator.Rules,
        ElementiumSynthesizer.Rules,
    ];

    /// <summary>All order buildings.</summary>
    public static IReadOnlyList<OrderBuildingRules> OrderBuildings { get; } =
    [
        Shipyard.Rules,
        DefenseSystem.Rules,
    ];

    /// <summary>Checks whether a building type is a resource building.</summary>
    /// <param name="type">The building type.</param>
    /// <returns><see langword="true"/> for resource buildings, <see langword="false"/> for order buildings.</returns>
    public static bool IsResourceBuilding(BuildingType type) => ResourceBuildings.Any(rules => rules.Type == type);

    /// <summary>Gets the rules of any building.</summary>
    /// <param name="type">The building type.</param>
    /// <returns>The rules of that building.</returns>
    public static BuildingRules For(BuildingType type) =>
        (BuildingRules?)ResourceBuildings.FirstOrDefault(rules => rules.Type == type)
        ?? OrderBuildings.FirstOrDefault(rules => rules.Type == type)
        ?? throw new ArgumentOutOfRangeException(nameof(type), type, "No rules defined for this building type.");
}
