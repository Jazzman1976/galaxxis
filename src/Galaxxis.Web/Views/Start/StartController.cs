using Galaxxis.Application.Players;
using Galaxxis.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Start;

/// <summary>
/// The start page: the game introduction for visitors, the list of universes for signed-in players.
/// </summary>
[AllowAnonymous]
public sealed class StartController(IPlayerService players) : Controller
{
    /// <summary>Shows the introduction with login and registration, or the universes a signed-in player can play in.</summary>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The start page.</returns>
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return View("Start");
        }

        IReadOnlyList<UniverseEntry> universes = await players.ListUniversesAsync(User.AccountId(), cancellationToken);
        return View("Universes", universes);
    }
}
