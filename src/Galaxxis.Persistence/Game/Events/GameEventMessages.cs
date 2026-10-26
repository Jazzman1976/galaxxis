using Galaxxis.Domain.Resources;

namespace Galaxxis.Persistence.Game.Events;

/// <summary>
/// The texts players see when a command is refused, shared by the engine (which decides) and the application
/// (which pre-checks a command to answer immediately). Keeping them in one place guarantees that the immediate
/// answer and the engine's final decision use the same wording.
/// </summary>
/// <remarks>
/// Messages are stored with rejected events (<c>GameEvent.Error</c>) and shown as they are. Translating them into
/// the players' language is open work (see docs/plans/TODO.md).
/// </remarks>
public static class GameEventMessages
{
    /// <summary>The planet does not exist or belongs to someone else.</summary>
    public const string PlanetNotOwned = "The planet does not exist or does not belong to you.";

    /// <summary>Another order of the same queue (for example a construction) is already running on the planet.</summary>
    public const string QueueBusy = "Another order of this kind is already in progress on this planet.";

    /// <summary>The production speed is not a valid 10 % step.</summary>
    public const string InvalidProductionSpeed = "The production speed must be a multiple of 10 between 0 and 100 percent.";

    /// <summary>Builds the message for a payment that cannot be made, naming only what is missing.</summary>
    /// <param name="stock">What is available.</param>
    /// <param name="cost">What is needed.</param>
    /// <returns>A message such as "Not enough resources: 12 ore, 3 mana missing."</returns>
    public static string NotEnoughResources(ResourceAmounts stock, ResourceAmounts cost)
    {
        ResourceAmounts missing = stock.MissingFor(cost);
        IEnumerable<string> parts = ResourceTypes.All
            .Where(resource => missing[resource] > 0)
            .Select(resource => $"{Math.Ceiling(missing[resource]):0} {resource.ToString().ToLowerInvariant()}");

        return $"Not enough resources: {string.Join(", ", parts)} missing.";
    }
}
