using Galaxxis.Domain.Combat;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Defenses;

/// <summary>
/// The rules of one defense class: costs, combat attributes and production duration.
/// </summary>
/// <remarks>
/// Like ships, each defense class has an offensive and a defensive variant; a race always builds the variant
/// matching its <see cref="Strategy"/>. Defenses cannot move and have no travel attributes.
/// </remarks>
/// <param name="Type">The defense class.</param>
/// <param name="OffensiveCost">Production costs of the offensive variant.</param>
/// <param name="DefensiveCost">Production costs of the defensive variant.</param>
/// <param name="OffensiveCombat">Combat attributes of the offensive variant.</param>
/// <param name="DefensiveCombat">Combat attributes of the defensive variant.</param>
/// <param name="ProductionDuration">How long the defense system needs to produce one defense.</param>
public sealed record DefenseRules(
    DefenseType Type,
    ResourceAmounts OffensiveCost,
    ResourceAmounts DefensiveCost,
    CombatStats OffensiveCombat,
    CombatStats DefensiveCombat,
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
