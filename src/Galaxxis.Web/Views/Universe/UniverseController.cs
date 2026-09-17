using Galaxxis.Application.Map;
using Galaxxis.Application.Players;
using Galaxxis.Domain.Races;
using Galaxxis.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Universe;

/// <summary>
/// The universe map (list of galaxies) and entering a universe for the first time.
/// </summary>
public sealed class UniverseController(IMapService map, IPlayerService players) : Controller
{
    /// <summary>Shows the galaxies of a universe; galaxies with own planets are marked.</summary>
    /// <param name="id">The universe.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The universe page, the join page if the account has not entered the universe yet, or 404.</returns>
    public async Task<IActionResult> Index(int id, CancellationToken cancellationToken)
    {
        UniverseMapView? universe = await map.GetUniverseAsync(User.AccountId(), id, cancellationToken);
        if (universe is null)
        {
            return NotFound();
        }

        if (!universe.HasJoined)
        {
            return RedirectToAction(nameof(Join), new { id });
        }

        return View("Universe", universe);
    }

    /// <summary>Shows the race selection for entering a universe.</summary>
    /// <param name="id">The universe.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The join page, the universe page if already joined, or 404.</returns>
    [HttpGet]
    public async Task<IActionResult> Join(int id, CancellationToken cancellationToken)
    {
        UniverseMapView? universe = await map.GetUniverseAsync(User.AccountId(), id, cancellationToken);
        if (universe is null)
        {
            return NotFound();
        }

        if (universe.HasJoined)
        {
            return RedirectToAction(nameof(Index), new { id });
        }

        return View("Join", new JoinViewModel { UniverseId = id, UniverseName = universe.Name });
    }

    /// <summary>
    /// Enters the universe with the chosen race: creates the player and assigns a random free home planet.
    /// </summary>
    /// <param name="id">The universe.</param>
    /// <param name="race">The chosen race.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>A redirect to the home planet, the join page with a message, or 404.</returns>
    [HttpPost]
    public async Task<IActionResult> Join(int id, Race race, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(race))
        {
            return BadRequest();
        }

        JoinUniverseResult result = await players.JoinUniverseAsync(User.AccountId(), id, race, cancellationToken);

        switch (result.Outcome)
        {
            case JoinUniverseOutcome.Joined:
            case JoinUniverseOutcome.AlreadyJoined:
                return RedirectToAction("Index", "Planet", new { id = result.HomePlanetId });

            case JoinUniverseOutcome.NoFreePlanet:
                UniverseMapView? universe = await map.GetUniverseAsync(User.AccountId(), id, cancellationToken);
                return View("Join", new JoinViewModel
                {
                    UniverseId = id,
                    UniverseName = universe?.Name ?? string.Empty,
                    Race = race,
                    Message = "In diesem Universum ist kein Planet mehr frei. Bitte wähle ein anderes Universum.",
                });

            default:
                return NotFound();
        }
    }
}
