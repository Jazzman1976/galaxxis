using Galaxxis.Application.Map;
using Galaxxis.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.GalaxxisSystem;

/// <summary>
/// The star system map: the planets of one star system.
/// </summary>
/// <remarks>Named "GalaxxisSystem" because "System" would clash with the .NET namespace of the same name.</remarks>
public sealed class GalaxxisSystemController(IMapService map) : Controller
{
    /// <summary>Shows the planets of a star system; own planets are marked and can be opened.</summary>
    /// <param name="id">The star system.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The star system page, the join page if the account has not entered the universe yet, or 404.</returns>
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken)
    {
        StarSystemMapView? system = await map.GetStarSystemAsync(User.AccountId(), id, cancellationToken);
        if (system is null)
        {
            return NotFound();
        }

        if (!system.HasJoined)
        {
            return RedirectToAction("Join", "Universe", new { id = system.UniverseId });
        }

        return View("GalaxxisSystem", system);
    }
}
