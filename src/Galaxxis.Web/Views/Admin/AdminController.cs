using Galaxxis.Application.Universes;
using Galaxxis.Persistence.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Admin;

/// <summary>
/// Administration: the universes with their state; pause and resume them.
/// </summary>
/// <remarks>
/// Only accounts with the <see cref="AccountRoles.Admin"/> role get here; the first admin is created with
/// <c>UniverseGenerator -- bootstrap-admin</c>. Generating universes is still done with the generator tool
/// (see docs/TODO.md). Universes keep running on their own; pausing is meant for maintenance.
/// </remarks>
[Authorize(Roles = AccountRoles.Admin)]
public sealed class AdminController(IUniverseAdminService universes, ILogger<AdminController> logger) : Controller
{
    /// <summary>Shows all universes.</summary>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The admin page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View("Admin", await universes.ListAsync(cancellationToken));

    /// <summary>Pauses a universe: its time stands still, nothing is produced and no order progresses.</summary>
    /// <param name="id">The universe.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>A redirect back to the admin page, or 404.</returns>
    [HttpPost]
    public async Task<IActionResult> Pause(int id, CancellationToken cancellationToken)
    {
        UniverseSummary? universe = await universes.PauseAsync(id, cancellationToken);
        if (universe is null)
        {
            return NotFound();
        }

        logger.LogWarning("Universe {UniverseId} paused by {Admin}.", id, User.Identity?.Name);
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Resumes a paused universe; the paused time is not counted as game time.</summary>
    /// <param name="id">The universe.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>A redirect back to the admin page, or 404.</returns>
    [HttpPost]
    public async Task<IActionResult> Resume(int id, CancellationToken cancellationToken)
    {
        UniverseSummary? universe = await universes.ResumeAsync(id, cancellationToken);
        if (universe is null)
        {
            return NotFound();
        }

        logger.LogWarning("Universe {UniverseId} resumed by {Admin}.", id, User.Identity?.Name);
        return RedirectToAction(nameof(Index));
    }
}
