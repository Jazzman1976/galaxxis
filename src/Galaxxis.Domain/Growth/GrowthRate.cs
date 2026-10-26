namespace Galaxxis.Domain.Growth;

/// <summary>
/// The central growth formula of Galaxxis: how a cost, a production amount or a duration develops
/// with the level of a building or technology.
/// </summary>
/// <remarks>
/// <para>
/// Formula (Concept: KonzeptWord §6):
/// <c>value = baseValue * (factor * (level - 1)^2 + 1)</c>.
/// Level 1 therefore always equals the base value, and every further level adds quadratically more.
/// </para>
/// <para>
/// Example with base value 15 and factor 4 (construction, "normal"): level 1 = 15, level 2 = 75,
/// level 20 = 21,675. These numbers match the ConzeptExcel sheet "Wachstums Raten".
/// </para>
/// <para>
/// Which level to pass depends on the question: construction costs and durations are asked for the
/// <em>target</em> level (current level + 1), production and upkeep for the <em>current</em> level.
/// </para>
/// </remarks>
/// <param name="BaseValue">Value at level 1. A base value of 0 or less means "nothing" at every level.</param>
/// <param name="Factor">How steeply the value grows per level.</param>
public readonly record struct GrowthRate(double BaseValue, GrowthFactor Factor)
{
    /// <summary>
    /// Calculates the value at the given level.
    /// </summary>
    /// <param name="level">The level; 0 or less means the building or technology does not exist yet.</param>
    /// <returns>The grown value, or 0 for levels below 1 and for non-positive base values.</returns>
    public double ValueAt(int level)
    {
        if (level < 1 || BaseValue <= 0)
        {
            return 0;
        }

        double levelsAboveFirst = level - 1;
        return BaseValue * ((int)Factor * levelsAboveFirst * levelsAboveFirst + 1);
    }
}
