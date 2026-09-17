using Galaxxis.Application.Planets;
using Galaxxis.Domain.Buildings;

namespace Galaxxis.Web.Views.Building;

/// <summary>What the building page shows: the planet, the chosen resource building and a message.</summary>
/// <param name="Planet">The own planet, as of now.</param>
/// <param name="Type">The resource building shown.</param>
/// <param name="Message">The answer to the last command, if the page was reached by a plain form post.</param>
public sealed record BuildingViewModel(PlanetView Planet, BuildingType Type, string? Message)
{
    /// <summary>The state of the building.</summary>
    public ResourceBuildingView Building => Planet.ResourceBuilding(Type);

    /// <summary>The running or requested upgrade of this building, if any.</summary>
    public PendingOrderView? Construction => Planet.PendingOrders.FirstOrDefault(order => order.Building == Type);

    /// <summary>Time until the running upgrade completes, if it is scheduled.</summary>
    public TimeSpan? ConstructionRemaining => Construction?.CompletesAt - Planet.CalculatedAt;

    /// <summary>Whether another building on this planet is being upgraded (only one construction at a time).</summary>
    public bool IsOtherConstructionBusy => Construction is null && Planet.IsConstructionBusy;
}
