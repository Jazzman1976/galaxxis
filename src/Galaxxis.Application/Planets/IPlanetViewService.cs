using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Persistence.Game.Entities;

namespace Galaxxis.Application.Planets;

/// <summary>
/// Shows a player their own planet: resources as of right now, buildings, costs of the next levels and running orders.
/// </summary>
/// <remarks>
/// The resources are brought up to date (settled) for display only; nothing is saved. Changing the planet is always a
/// command recorded through <c>ICommandService</c> and decided by the engine.
/// </remarks>
public interface IPlanetViewService
{
    /// <summary>Gets a planet that belongs to the signed-in account.</summary>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="planetId">The planet.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>The view, or <see langword="null"/> if the planet does not exist or belongs to someone else.</returns>
    Task<PlanetView?> GetOwnPlanetAsync(string accountId, int planetId, CancellationToken cancellationToken = default);
}

/// <summary>A player's planet as of <see cref="CalculatedAt"/>.</summary>
/// <param name="PlanetId">The planet.</param>
/// <param name="Name">Its name.</param>
/// <param name="UniverseId">Its universe.</param>
/// <param name="UniverseName">The universe's name.</param>
/// <param name="IsUniversePaused">Whether universe time is stopped (nothing is produced meanwhile).</param>
/// <param name="GalaxyId">Its galaxy.</param>
/// <param name="StarSystemId">Its star system.</param>
/// <param name="StarSystemName">The star system's name.</param>
/// <param name="Race">The owner's race.</param>
/// <param name="Stock">The resources at <see cref="CalculatedAt"/>.</param>
/// <param name="NetRatePerDay">How much each resource grows (or shrinks) per day right now; used to count up live in the browser.</param>
/// <param name="CalculatedAt">The real time the stock was calculated for.</param>
/// <param name="ResourceBuildings">The seven resource buildings.</param>
/// <param name="Shipyard">The shipyard.</param>
/// <param name="DefenseSystem">The defense system.</param>
/// <param name="PendingOrders">Commands waiting for the engine or in progress, earliest first.</param>
public sealed record PlanetView(
    int PlanetId,
    string Name,
    int UniverseId,
    string UniverseName,
    bool IsUniversePaused,
    int GalaxyId,
    int StarSystemId,
    string StarSystemName,
    Race Race,
    ResourceAmounts Stock,
    ResourceAmounts NetRatePerDay,
    DateTimeOffset CalculatedAt,
    IReadOnlyList<ResourceBuildingView> ResourceBuildings,
    OrderBuildingState Shipyard,
    OrderBuildingState DefenseSystem,
    IReadOnlyList<PendingOrderView> PendingOrders)
{
    /// <summary>Whether a construction (of any building) is requested or in progress; only one runs at a time.</summary>
    public bool IsConstructionBusy => PendingOrders.Any(order => order.Building is not null);

    /// <summary>Gets the view of one resource building.</summary>
    /// <param name="type">A resource building type.</param>
    /// <returns>The building view.</returns>
    public ResourceBuildingView ResourceBuilding(BuildingType type) => ResourceBuildings.Single(building => building.Type == type);
}

/// <summary>One resource building of a planet.</summary>
/// <param name="Type">The building.</param>
/// <param name="Level">Current level; 0 = not built.</param>
/// <param name="SpeedPercent">Production speed chosen by the player.</param>
/// <param name="Produces">The resource it produces.</param>
/// <param name="ProductionPerDay">What it produces per day at its level and speed, with the race factor, without base income.</param>
/// <param name="UpkeepPerDay">What it consumes per day at its level and speed.</param>
/// <param name="NextLevelCost">Costs of the next level.</param>
/// <param name="NextLevelDuration">Construction time of the next level.</param>
/// <param name="CanAffordNextLevel">Whether the current stock covers <paramref name="NextLevelCost"/>.</param>
/// <param name="IsUnderConstruction">Whether an upgrade of this building is requested or in progress.</param>
public sealed record ResourceBuildingView(
    BuildingType Type,
    int Level,
    int SpeedPercent,
    ResourceType Produces,
    double ProductionPerDay,
    ResourceAmounts UpkeepPerDay,
    ResourceAmounts NextLevelCost,
    TimeSpan NextLevelDuration,
    bool CanAffordNextLevel,
    bool IsUnderConstruction);

/// <summary>A command that is not finished yet.</summary>
/// <param name="EventId">The event id, to ask for its status.</param>
/// <param name="Type">The event type.</param>
/// <param name="State">Requested (waiting for the engine) or Scheduled (accepted, in progress).</param>
/// <param name="Building">For constructions: the building being upgraded.</param>
/// <param name="CompletesAt">For scheduled orders: the estimated completion time.</param>
public sealed record PendingOrderView(long EventId, string Type, GameEventState State, BuildingType? Building, DateTimeOffset? CompletesAt);
