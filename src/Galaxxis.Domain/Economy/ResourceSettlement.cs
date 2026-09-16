using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Economy;

/// <summary>
/// Brings a planet's resource stock up to date: calculates what the stock is after a period of time in
/// which the planet's buildings did not change.
/// </summary>
/// <remarks>
/// <para>
/// <b>When it is used.</b> Stocks are not updated continuously. Each planet stores its stock and the moment
/// it was last settled. Whenever something changes the planet (a building finishes, the player changes a
/// production speed, resources are spent) or when a player looks at it, the stock is settled up to that
/// moment first. Between two such moments levels and speeds are constant, which is what makes an exact
/// calculation possible.
/// </para>
/// <para>
/// <b>How it works.</b> With constant buildings every resource changes at a constant rate, so the stock after
/// a period is simply <c>stock + rate × time</c>. That is one calculation, no matter whether the period is
/// one second or one month (the legacy code looped once per elapsed tick).
/// The only thing that can change a rate within the period is a supply resource running empty, which starts
/// throttling its consumers (see <see cref="PlanetEconomy"/>). The period is therefore split at the
/// moments a supply resource runs empty:
/// </para>
/// <list type="number">
///   <item>Calculate the rates, treating the supply resources that are already empty as empty.</item>
///   <item>Find the first moment a not-yet-empty supply resource would reach 0.</item>
///   <item>Advance all stocks to that moment (or to the end of the period), then repeat.</item>
/// </list>
/// <para>
/// A supply resource that ran empty stays "empty" for the rest of the period: its consumers stay throttled,
/// and its stock can only grow if the rate is positive. Because there are only four supply resources, the
/// period is split at most four times. Stocks never become negative.
/// </para>
/// </remarks>
public static class ResourceSettlement
{
    /// <summary>
    /// Calculates the resource stock after <paramref name="elapsed"/> time.
    /// </summary>
    /// <param name="stock">The stock at the start of the period. Negative values are treated as 0.</param>
    /// <param name="buildings">Levels, speeds and production state, constant during the period.</param>
    /// <param name="race">The race of the planet owner.</param>
    /// <param name="elapsed">Length of the period. Must not be negative.</param>
    /// <returns>The stock at the end of the period. Never negative.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="elapsed"/> is negative.</exception>
    public static ResourceAmounts Settle(
        ResourceAmounts stock,
        PlanetBuildings buildings,
        RaceProfile race,
        TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed), elapsed, "The settlement period must not be negative.");
        }

        ResourceAmounts current = stock.ClampedToZero();
        double remainingDays = elapsed.TotalDays;

        // Supply resources that are empty at the start of the period.
        HashSet<ResourceType> emptySupply = ResourceTypes.Supply
            .Where(resource => current[resource] <= 0)
            .ToHashSet();

        while (true)
        {
            ResourceAmounts ratesPerDay = PlanetEconomy.NetDailyRates(buildings, race, emptySupply);

            // Find the supply resource that runs empty first within the remaining period, if any.
            double segmentDays = remainingDays;
            ResourceType? runsEmptyNext = null;

            foreach (ResourceType resource in ResourceTypes.Supply.Where(resource => !emptySupply.Contains(resource)))
            {
                if (ratesPerDay[resource] >= 0)
                {
                    continue;
                }

                double daysUntilEmpty = current[resource] / -ratesPerDay[resource];
                if (daysUntilEmpty < segmentDays)
                {
                    segmentDays = daysUntilEmpty;
                    runsEmptyNext = resource;
                }
            }

            // Advance all stocks through this segment. Empty supply resources with a negative rate stay at 0
            // (ClampedToZero), other resources change linearly.
            current = (current + ratesPerDay * segmentDays).ClampedToZero();
            remainingDays -= segmentDays;

            if (runsEmptyNext is not ResourceType emptied)
            {
                return current;
            }

            // Set exactly 0 to avoid a tiny floating point remainder, then continue with this resource empty.
            current = current.With(emptied, 0);
            emptySupply.Add(emptied);
        }
    }
}
