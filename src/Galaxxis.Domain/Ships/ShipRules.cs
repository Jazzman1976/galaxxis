using Galaxxis.Domain.Combat;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Ships;

/// <summary>
/// Travel attributes that only ships have.
/// </summary>
/// <remarks>Concept: KonzeptWord §7.2.</remarks>
/// <param name="DriveStrength">Speed in distance units per hour ("Antriebsstärke").</param>
/// <param name="ElementiumPerDistanceUnit">Fuel consumption ("Energieeffizienz").</param>
/// <param name="StorageCapacity">How many resources the ship can carry ("Lagerkapazität").</param>
public sealed record ShipTravelStats(int DriveStrength, int ElementiumPerDistanceUnit, int StorageCapacity);

/// <summary>
/// The rules of one ship class: costs, attributes and production duration.
/// </summary>
/// <remarks>
/// Each ship class has an offensive and a defensive variant; a race always builds the variant matching its
/// <see cref="Strategy"/>. The variants differ in costs and combat attributes, not in travel attributes.
/// </remarks>
/// <param name="Type">The ship class.</param>
/// <param name="OffensiveCost">Production costs of the offensive variant.</param>
/// <param name="DefensiveCost">Production costs of the defensive variant.</param>
/// <param name="OffensiveCombat">Combat attributes of the offensive variant.</param>
/// <param name="DefensiveCombat">Combat attributes of the defensive variant.</param>
/// <param name="Travel">Travel attributes of both variants.</param>
/// <param name="ProductionDuration">How long the shipyard needs to produce one ship.</param>
public sealed record ShipRules(
    ShipType Type,
    ResourceAmounts OffensiveCost,
    ResourceAmounts DefensiveCost,
    CombatStats OffensiveCombat,
    CombatStats DefensiveCombat,
    ShipTravelStats Travel,
    TimeSpan ProductionDuration)
{
    /// <summary>Production costs of the variant a race with <paramref name="strategy"/> builds.</summary>
    /// <param name="strategy">The strategy of the producing race.</param>
    /// <returns>The production costs.</returns>
    public ResourceAmounts CostFor(Strategy strategy) =>
        strategy == Strategy.Offensive ? OffensiveCost : DefensiveCost;

    /// <summary>Combat attributes of the variant a race with <paramref name="strategy"/> builds.</summary>
    /// <param name="strategy">The strategy of the producing race.</param>
    /// <returns>The combat attributes.</returns>
    public CombatStats CombatFor(Strategy strategy) =>
        strategy == Strategy.Offensive ? OffensiveCombat : DefensiveCombat;
}
