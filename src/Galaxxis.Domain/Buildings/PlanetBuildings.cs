namespace Galaxxis.Domain.Buildings;

/// <summary>
/// The state of a resource building on a planet.
/// </summary>
/// <param name="Level">Current level; 0 means not built yet.</param>
/// <param name="Speed">Production speed chosen by the player.</param>
public readonly record struct ResourceBuildingState(int Level, ProductionSpeed Speed)
{
    /// <summary>A building that is not built yet and set to full speed, the state of a fresh planet.</summary>
    public static ResourceBuildingState NotBuilt => new(0, ProductionSpeed.Full);
}

/// <summary>
/// The state of an order building on a planet.
/// </summary>
/// <param name="Level">Current level; 0 means not built yet.</param>
/// <param name="IsWorking">
/// Whether an order (for example producing a ship) is currently in progress. The building consumes
/// supply resources only while this is <see langword="true"/>.
/// </param>
public readonly record struct OrderBuildingState(int Level, bool IsWorking)
{
    /// <summary>A building that is not built yet and idle.</summary>
    public static OrderBuildingState NotBuilt => new(0, false);
}

/// <summary>
/// The levels and settings of all buildings on one planet. The input of every economy calculation.
/// </summary>
/// <remarks>
/// A plain snapshot without behaviour. The Persistence module stores the same information in the
/// planet table; the engine builds this snapshot from it before applying game rules.
/// </remarks>
public sealed record PlanetBuildings
{
    /// <summary>The ore mine.</summary>
    public ResourceBuildingState OreMine { get; init; } = ResourceBuildingState.NotBuilt;

    /// <summary>The power supply.</summary>
    public ResourceBuildingState PowerSupply { get; init; } = ResourceBuildingState.NotBuilt;

    /// <summary>The factory.</summary>
    public ResourceBuildingState Factory { get; init; } = ResourceBuildingState.NotBuilt;

    /// <summary>The mage academy.</summary>
    public ResourceBuildingState MageAcademy { get; init; } = ResourceBuildingState.NotBuilt;

    /// <summary>The crystal mine.</summary>
    public ResourceBuildingState CrystalMine { get; init; } = ResourceBuildingState.NotBuilt;

    /// <summary>The neutron accelerator.</summary>
    public ResourceBuildingState NeutronAccelerator { get; init; } = ResourceBuildingState.NotBuilt;

    /// <summary>The elementium synthesizer.</summary>
    public ResourceBuildingState ElementiumSynthesizer { get; init; } = ResourceBuildingState.NotBuilt;

    /// <summary>The shipyard.</summary>
    public OrderBuildingState Shipyard { get; init; } = OrderBuildingState.NotBuilt;

    /// <summary>The defense system.</summary>
    public OrderBuildingState DefenseSystem { get; init; } = OrderBuildingState.NotBuilt;

    /// <summary>A planet on which nothing has been built yet.</summary>
    public static PlanetBuildings Empty { get; } = new();

    /// <summary>Gets the state of a resource building by its type.</summary>
    /// <param name="type">A resource building type.</param>
    /// <returns>The state of that building.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a resource building.</exception>
    public ResourceBuildingState ResourceBuilding(BuildingType type) => type switch
    {
        BuildingType.OreMine => OreMine,
        BuildingType.PowerSupply => PowerSupply,
        BuildingType.Factory => Factory,
        BuildingType.MageAcademy => MageAcademy,
        BuildingType.CrystalMine => CrystalMine,
        BuildingType.NeutronAccelerator => NeutronAccelerator,
        BuildingType.ElementiumSynthesizer => ElementiumSynthesizer,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Not a resource building."),
    };

    /// <summary>Gets the state of an order building by its type.</summary>
    /// <param name="type">An order building type.</param>
    /// <returns>The state of that building.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not an order building.</exception>
    public OrderBuildingState OrderBuilding(BuildingType type) => type switch
    {
        BuildingType.Shipyard => Shipyard,
        BuildingType.DefenseSystem => DefenseSystem,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Not an order building."),
    };

    /// <summary>Gets the current level of any building.</summary>
    /// <param name="type">The building.</param>
    /// <returns>Its level; 0 if not built yet.</returns>
    public int LevelOf(BuildingType type) => BuildingCatalog.IsResourceBuilding(type)
        ? ResourceBuilding(type).Level
        : OrderBuilding(type).Level;

    /// <summary>Returns a copy in which one building has a different level. Everything else stays unchanged.</summary>
    /// <param name="type">The building.</param>
    /// <param name="level">The new level.</param>
    /// <returns>The changed copy.</returns>
    public PlanetBuildings WithLevel(BuildingType type, int level) => BuildingCatalog.IsResourceBuilding(type)
        ? WithResourceBuilding(type, ResourceBuilding(type) with { Level = level })
        : WithOrderBuilding(type, OrderBuilding(type) with { Level = level });

    /// <summary>Returns a copy in which a resource building runs at a different production speed.</summary>
    /// <param name="type">A resource building.</param>
    /// <param name="speed">The new speed.</param>
    /// <returns>The changed copy.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not a resource building.</exception>
    public PlanetBuildings WithSpeed(BuildingType type, ProductionSpeed speed) =>
        WithResourceBuilding(type, ResourceBuilding(type) with { Speed = speed });

    /// <summary>Returns a copy in which an order building is working (or idle).</summary>
    /// <param name="type">An order building.</param>
    /// <param name="isWorking">Whether an order is in progress.</param>
    /// <returns>The changed copy.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="type"/> is not an order building.</exception>
    public PlanetBuildings WithWorking(BuildingType type, bool isWorking) =>
        WithOrderBuilding(type, OrderBuilding(type) with { IsWorking = isWorking });

    private PlanetBuildings WithResourceBuilding(BuildingType type, ResourceBuildingState state) => type switch
    {
        BuildingType.OreMine => this with { OreMine = state },
        BuildingType.PowerSupply => this with { PowerSupply = state },
        BuildingType.Factory => this with { Factory = state },
        BuildingType.MageAcademy => this with { MageAcademy = state },
        BuildingType.CrystalMine => this with { CrystalMine = state },
        BuildingType.NeutronAccelerator => this with { NeutronAccelerator = state },
        BuildingType.ElementiumSynthesizer => this with { ElementiumSynthesizer = state },
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Not a resource building."),
    };

    private PlanetBuildings WithOrderBuilding(BuildingType type, OrderBuildingState state) => type switch
    {
        BuildingType.Shipyard => this with { Shipyard = state },
        BuildingType.DefenseSystem => this with { DefenseSystem = state },
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Not an order building."),
    };
}
