using Galaxxis.Application.Planets;
using Galaxxis.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Planet;

/// <summary>
/// The planetary control center: resources and buildings of an own planet.
/// </summary>
public sealed class PlanetController(IPlanetViewService planets) : Controller
{
    /// <summary>Shows an own planet.</summary>
    /// <param name="id">The planet.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The planet page, or 404 if the planet does not exist or belongs to someone else.</returns>
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken)
    {
        // Planets of other players are reported as not found, never revealed.
        PlanetView? planet = await planets.GetOwnPlanetAsync(User.AccountId(), id, cancellationToken);
        return planet is null ? NotFound() : View("Planet", planet);
    }
}
