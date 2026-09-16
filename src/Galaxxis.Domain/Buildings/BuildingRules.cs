using Galaxxis.Domain.Growth;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Buildings;

/// <summary>
/// The rules every building shares: what the next level costs, how long it takes to build, and what
/// the building consumes per day while it works.
/// </summary>
/// <remarks>
/// <para>
/// Every building that works consumes supply resources, so every building defines upkeep base values.
/// Resource buildings work continuously, order buildings while an order is in progress
/// (see <see cref="BuildingType"/>).
/// </para>
/// <para>
/// The concrete numbers of each building are defined in its own class (for example <see cref="OreMine"/>)
/// as base values at level 1; this type turns them into values for any level with the growth formula.
/// </para>
/// </remarks>
/// <param name="Type">Which building these rules belong to.</param>
/// <param name="BuildCostBaseValues">Construction costs of level 1. Grow with <see cref="GrowthFactor.Construction"/>.</param>
/// <param name="UpkeepBaseValues">
/// Supply consumption per day at level 1 while the building works (resource buildings: at full speed).
/// Grows with <see cref="GrowthFactor.Supply"/>.
/// </param>
public abstract record BuildingRules(
    BuildingType Type,
    ResourceAmounts BuildCostBaseValues,
    ResourceAmounts UpkeepBaseValues)
{
    /// <summary>
    /// Base value of the construction duration in seconds. The same for all buildings.
    /// Concept: ConzeptExcel "Wachstums Raten", column "Bauzeit" (base value 25, factor 5).
    /// </summary>
    public const double BuildDurationBaseSeconds = BaseValue.VeryMuch;

    /// <summary>Calculates what it costs to build <paramref name="targetLevel"/>.</summary>
    /// <param name="targetLevel">The level after construction, i.e. current level + 1.</param>
    /// <returns>The construction costs.</returns>
    public ResourceAmounts BuildCostFor(int targetLevel) =>
        BuildCostBaseValues.GrownTo(GrowthFactor.Construction, targetLevel);

    /// <summary>Calculates how long it takes to build <paramref name="targetLevel"/>.</summary>
    /// <param name="targetLevel">The level after construction, i.e. current level + 1.</param>
    /// <returns>The construction duration. Level 1 takes 25 seconds, level 20 about 12.5 hours.</returns>
    public TimeSpan BuildDurationFor(int targetLevel) =>
        TimeSpan.FromSeconds(new GrowthRate(BuildDurationBaseSeconds, GrowthFactor.BuildTime).ValueAt(targetLevel));

    /// <summary>
    /// Calculates the supply consumption per day at <paramref name="level"/> while the building works
    /// (for resource buildings: at full speed).
    /// </summary>
    /// <param name="level">The current level of the building.</param>
    /// <returns>The daily consumption. Only supply resources can be non-zero.</returns>
    public ResourceAmounts DailyUpkeepAt(int level) =>
        UpkeepBaseValues.GrownTo(GrowthFactor.Supply, level);
}

/// <summary>
/// Rules of a resource building: a building that continuously produces one resource.
/// </summary>
/// <param name="Type">Which building these rules belong to.</param>
/// <param name="Produces">The resource the building produces.</param>
/// <param name="ProductionBaseValue">
/// Production per day at level 1 and full speed, without base income. Grows with <see cref="GrowthFactor.Resources"/>.
/// </param>
/// <param name="BuildCostBaseValues">Construction costs of level 1.</param>
/// <param name="UpkeepBaseValues">Supply consumption per day at level 1 and full speed.</param>
public sealed record ResourceBuildingRules(
    BuildingType Type,
    ResourceType Produces,
    double ProductionBaseValue,
    ResourceAmounts BuildCostBaseValues,
    ResourceAmounts UpkeepBaseValues)
    : BuildingRules(Type, BuildCostBaseValues, UpkeepBaseValues)
{
    /// <summary>
    /// Calculates the production per day at <paramref name="level"/> and full speed, before race factors
    /// and without the planet's base income.
    /// </summary>
    /// <param name="level">The current level of the building. Level 0 produces nothing.</param>
    /// <returns>The daily production of <see cref="Produces"/>.</returns>
    public double DailyProductionAt(int level) =>
        new GrowthRate(ProductionBaseValue, GrowthFactor.Resources).ValueAt(level);
}

/// <summary>
/// Rules of an order building: a building that works on one order at a time, such as producing a ship
/// or a defense (later also research or processing). It consumes supply resources only while an order
/// is in progress.
/// </summary>
/// <param name="Type">Which building these rules belong to.</param>
/// <param name="BuildCostBaseValues">Construction costs of level 1.</param>
/// <param name="UpkeepBaseValues">Supply consumption per day at level 1 while an order is in progress.</param>
public sealed record OrderBuildingRules(
    BuildingType Type,
    ResourceAmounts BuildCostBaseValues,
    ResourceAmounts UpkeepBaseValues)
    : BuildingRules(Type, BuildCostBaseValues, UpkeepBaseValues);
