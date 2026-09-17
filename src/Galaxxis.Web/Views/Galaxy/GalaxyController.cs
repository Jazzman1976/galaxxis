using Galaxxis.Application.Map;
using Galaxxis.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Galaxy;

/// <summary>
/// The galaxy map: the star systems of one galaxy.
/// </summary>
public sealed class GalaxyController(IMapService map) : Controller
{
    /// <summary>Shows the star systems of a galaxy; systems with own planets are marked.</summary>
    /// <param name="id">The galaxy.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The galaxy page, the join page if the account has not entered the universe yet, or 404.</returns>
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken)
    {
        GalaxyMapView? galaxy = await map.GetGalaxyAsync(User.AccountId(), id, cancellationToken);
        if (galaxy is null)
        {
            return NotFound();
        }

        if (!galaxy.HasJoined)
        {
            return RedirectToAction("Join", "Universe", new { id = galaxy.UniverseId });
        }

        return View("Galaxy", galaxy);
    }
}
