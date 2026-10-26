using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Economy;

/// <summary>
/// Calculates how much of each resource a planet produces and consumes per day, including the effect
/// of supply shortages.
/// </summary>
/// <remarks>
/// <para><b>The rules</b> (game design confirmed 2026-09-16):</para>
/// <list type="number">
///   <item>
///     <b>Production</b> of a resource = (base income + output of the resource building for it) × race factor.
///     The building output scales with its level and with the production speed the player set.
///     The base income never scales and is never throttled.
///   </item>
///   <item>
///     <b>Consumption</b>: every building consumes supply resources while it works. Resource buildings work
///     continuously and consume in proportion to their production speed; order buildings consume while an
///     order is in progress.
///   </item>
///   <item>
///     <b>Shortage</b>: when a supply resource is <em>empty</em> and its gross demand is higher than its gross
///     production, every resource building that consumes it is throttled proportionally: it runs at
///     <c>available / demanded</c> of its set speed, so demand no longer exceeds production. A building that
///     consumes several empty resources runs at the lowest of their ratios. Order buildings are not
///     throttled; their demand is served first, so orders keep their promised duration.
///   </item>
/// </list>
/// <para>
/// Throttle ratios are based on <em>gross</em> numbers (what the buildings would produce and demand at their
/// set speeds). That keeps the calculation a single, non-recursive step. Because the base income is part of
/// the gross production and is never throttled, consumers keep running at a positive share, and a planet
/// can always recover. The only exception is an order whose own demand exceeds the whole
/// production of a resource; it ends after its fixed duration, after which the consumers resume.
/// </para>
/// </remarks>
public static class PlanetEconomy
{
    /// <summary>
    /// Production per day at the set production speeds, without any shortage throttling.
    /// </summary>
    /// <param name="buildings">Levels and speeds of the planet's buildings.</param>
    /// <param name="race">The race of the planet owner (production factors).</param>
    /// <returns>The gross daily production of every resource.</returns>
    public static ResourceAmounts GrossDailyProduction(PlanetBuildings buildings, RaceProfile race) =>
        DailyProduction(buildings, race, throttles: null);

    /// <summary>
    /// Consumption per day at the set production speeds, without any shortage throttling.
    /// </summary>
    /// <param name="buildings">Levels, speeds and order state of the planet's buildings.</param>
    /// <returns>The gross daily demand of every resource. Only supply resources can be non-zero.</returns>
    public static ResourceAmounts GrossDailyDemand(PlanetBuildings buildings) =>
        DailyConsumption(buildings, throttles: null);

    /// <summary>
    /// Calculates the net change per day of every resource (production minus consumption), taking into
    /// account that the given supply resources are empty.
    /// </summary>
    /// <param name="buildings">Levels, speeds and order state of the planet's buildings.</param>
    /// <param name="race">The race of the planet owner.</param>
    /// <param name="emptySupplyResources">Supply resources whose stock is currently 0.</param>
    /// <returns>
    /// The net daily rate per resource. For an empty resource the rate can still be negative when other
    /// shortages reduce its own production; the settlement keeps such a stock at 0.
    /// </returns>
    public static ResourceAmounts NetDailyRates(
        PlanetBuildings buildings,
        RaceProfile race,
        IReadOnlyCollection<ResourceType> emptySupplyResources)
    {
        IReadOnlyDictionary<BuildingType, double> throttles = ThrottlesFor(buildings, race, emptySupplyResources);
        return DailyProduction(buildings, race, throttles) - DailyConsumption(buildings, throttles);
    }

    /// <summary>
    /// Determines, for every resource building, the share of its set speed it can actually run at.
    /// 1 means unthrottled.
    /// </summary>
    private static IReadOnlyDictionary<BuildingType, double> ThrottlesFor(
        PlanetBuildings buildings,
        RaceProfile race,
        IReadOnlyCollection<ResourceType> emptySupplyResources)
    {
        ResourceAmounts grossProduction = GrossDailyProduction(buildings, race);
        ResourceAmounts fixedDemand = OrderBuildingDemand(buildings);
        ResourceAmounts throttleableDemand = ResourceBuildingDemand(buildings, throttles: null);

        Dictionary<BuildingType, double> throttles = BuildingCatalog.ResourceBuildings
            .ToDictionary(rules => rules.Type, _ => 1.0);

        foreach (ResourceType resource in emptySupplyResources)
        {
            // Order buildings are served first; resource buildings share what is left.
            double availableForResourceBuildings = grossProduction[resource] - fixedDemand[resource];
            double demandedByResourceBuildings = throttleableDemand[resource];

            if (demandedByResourceBuildings <= availableForResourceBuildings)
            {
                // Enough is produced for everyone, the empty stock refills by itself: no throttling needed.
                continue;
            }

            double ratio = Math.Clamp(availableForResourceBuildings / demandedByResourceBuildings, 0, 1);

            foreach (ResourceBuildingRules rules in BuildingCatalog.ResourceBuildings)
            {
                bool consumesThisResource = rules.UpkeepBaseValues[resource] > 0;
                if (consumesThisResource)
                {
                    throttles[rules.Type] = Math.Min(throttles[rules.Type], ratio);
                }
            }
        }

        return throttles;
    }

    private static ResourceAmounts DailyProduction(
        PlanetBuildings buildings,
        RaceProfile race,
        IReadOnlyDictionary<BuildingType, double>? throttles)
    {
        // The base income is never scaled by speed or throttling (see BaseIncome).
        ResourceAmounts production = BaseIncome.PerDay;

        foreach (ResourceBuildingRules rules in BuildingCatalog.ResourceBuildings)
        {
            ResourceBuildingState state = buildings.ResourceBuilding(rules.Type);
            double effectiveSpeed = state.Speed.Factor * ThrottleOf(rules.Type, throttles);
            double output = rules.DailyProductionAt(state.Level) * effectiveSpeed;

            production = production.With(rules.Produces, production[rules.Produces] + output);
        }

        return production.MultipliedBy(race.ProductionFactors);
    }

    private static ResourceAmounts DailyConsumption(
        PlanetBuildings buildings,
        IReadOnlyDictionary<BuildingType, double>? throttles) =>
        ResourceBuildingDemand(buildings, throttles) + OrderBuildingDemand(buildings);

    private static ResourceAmounts ResourceBuildingDemand(
        PlanetBuildings buildings,
        IReadOnlyDictionary<BuildingType, double>? throttles)
    {
        ResourceAmounts demand = ResourceAmounts.Zero;

        foreach (ResourceBuildingRules rules in BuildingCatalog.ResourceBuildings)
        {
            ResourceBuildingState state = buildings.ResourceBuilding(rules.Type);
            double effectiveSpeed = state.Speed.Factor * ThrottleOf(rules.Type, throttles);
            demand += rules.DailyUpkeepAt(state.Level) * effectiveSpeed;
        }

        return demand;
    }

    private static ResourceAmounts OrderBuildingDemand(PlanetBuildings buildings)
    {
        ResourceAmounts demand = ResourceAmounts.Zero;

        foreach (OrderBuildingRules rules in BuildingCatalog.OrderBuildings)
        {
            OrderBuildingState state = buildings.OrderBuilding(rules.Type);
            if (state.IsWorking)
            {
                demand += rules.DailyUpkeepAt(state.Level);
            }
        }

        return demand;
    }

    private static double ThrottleOf(BuildingType type, IReadOnlyDictionary<BuildingType, double>? throttles) =>
        throttles is not null && throttles.TryGetValue(type, out double throttle) ? throttle : 1.0;
}
