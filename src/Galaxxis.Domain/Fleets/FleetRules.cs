using Galaxxis.Domain.Resources;

namespace Galaxxis.Domain.Fleets;

/// <summary>
/// The rules of fleet missions (attack, scan, return) as far as they exist today.
/// </summary>
/// <remarks>
/// This is the state of the legacy game, carried over unchanged: missions take a fixed time and an attack plunders
/// resources without a fight. The concept's combat system (KonzeptWord §7) and travel times based on distance and
/// drive strength are open work (docs/TODO.md) and will replace these placeholders.
/// </remarks>
public static class FleetRules
{
    /// <summary>
    /// How long a fleet needs to reach its target, and again to return. Placeholder taken over from the legacy game
    /// (100 ticks of one second); does not depend on distance yet.
    /// </summary>
    public static TimeSpan TravelDuration { get; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// Calculates what an attacking fleet takes from a planet.
    /// </summary>
    /// <remarks>
    /// The fleet fills its storage as evenly as possible across all resources: it takes the same amount of every
    /// resource, except resources the planet has less of, which it takes completely. The free storage of those is
    /// shared among the others. (The legacy game approximated this with a loop; this is the exact result.)
    /// </remarks>
    /// <param name="planetStock">The planet's stock at the moment of the attack.</param>
    /// <param name="storageCapacity">The total storage capacity of the attacking ships.</param>
    /// <returns>The loot per resource; never more than the stock, and in total never more than the capacity.</returns>
    public static ResourceAmounts Plunder(ResourceAmounts planetStock, double storageCapacity)
    {
        ResourceAmounts available = planetStock.ClampedToZero();
        double totalAvailable = ResourceTypes.All.Sum(resource => available[resource]);

        if (storageCapacity <= 0)
        {
            return ResourceAmounts.Zero;
        }

        if (totalAvailable <= storageCapacity)
        {
            return available;
        }

        // Find the share per resource ("water level"): go through the resources from smallest to largest stock.
        // Each resource smaller than the remaining equal share is taken completely; the rest is split equally.
        List<double> ascending = ResourceTypes.All.Select(resource => available[resource]).Order().ToList();
        double remainingCapacity = storageCapacity;
        double share = 0;

        for (int index = 0; index < ascending.Count; index++)
        {
            int resourcesLeft = ascending.Count - index;
            share = remainingCapacity / resourcesLeft;

            if (ascending[index] >= share)
            {
                break;
            }

            remainingCapacity -= ascending[index];
        }

        double level = share;
        return ResourceTypes.All.Aggregate(
            ResourceAmounts.Zero,
            (loot, resource) => loot.With(resource, Math.Min(available[resource], level)));
    }
}
