using Galaxxis.Application.Commands;
using Galaxxis.Application.Planets;
using Galaxxis.Domain.Buildings;
using Galaxxis.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Building;

/// <summary>
/// One page for every resource building (ore mine, power supply, factory, mage academy, crystal mine, neutron
/// accelerator, elementium synthesizer): upgrade it and set its production speed.
/// </summary>
/// <remarks>
/// <para>
/// All resource buildings follow the same rules, so one controller and one view serve all of them; the building is
/// chosen by <c>type</c>, for example <c>/building/index/42?type=OreMine</c>.
/// </para>
/// <para>
/// <b>How a command works.</b> The Application service <em>records</em> the command; the engine decides on it
/// within about a second. The page therefore shows "recorded" first and then asks <see cref="Status"/> until the engine
/// has decided (script <c>commands.js</c>). Without JavaScript the forms still work: the actions then redirect back
/// to the page with a message.
/// </para>
/// </remarks>
public sealed class BuildingController(IPlanetViewService planets, ICommandService commands) : Controller
{
    private const string MessageKey = "BuildingMessage";

    /// <summary>Shows a resource building of an own planet.</summary>
    /// <param name="id">The planet.</param>
    /// <param name="type">The resource building.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The building page, or 404 for foreign planets and buildings without this page.</returns>
    [HttpGet]
    public async Task<IActionResult> Index(int id, BuildingType type, CancellationToken cancellationToken)
    {
        if (!BuildingCatalog.IsResourceBuilding(type))
        {
            return NotFound();
        }

        PlanetView? planet = await planets.GetOwnPlanetAsync(User.AccountId(), id, cancellationToken);
        if (planet is null)
        {
            return NotFound();
        }

        return View("Building", new BuildingViewModel(planet, type, TempData[MessageKey] as string));
    }

    /// <summary>Orders the upgrade of the building by one level.</summary>
    /// <param name="id">The planet.</param>
    /// <param name="type">The resource building.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>For scripts: the command result as JSON. Otherwise a redirect back to the building page.</returns>
    [HttpPost]
    public async Task<IActionResult> Build(int id, BuildingType type, CancellationToken cancellationToken)
    {
        if (!BuildingCatalog.IsResourceBuilding(type))
        {
            return NotFound();
        }

        CommandResult result = await commands.RequestBuildAsync(User.AccountId(), id, type, cancellationToken);
        return CommandAnswer(result, id, type);
    }

    /// <summary>Orders a new production speed for the building.</summary>
    /// <param name="id">The planet.</param>
    /// <param name="type">The resource building.</param>
    /// <param name="speedPercent">The new speed: 0 to 100 in steps of 10.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>For scripts: the command result as JSON. Otherwise a redirect back to the building page.</returns>
    [HttpPost]
    public async Task<IActionResult> ChangeSpeed(int id, BuildingType type, int speedPercent, CancellationToken cancellationToken)
    {
        if (!BuildingCatalog.IsResourceBuilding(type))
        {
            return NotFound();
        }

        CommandResult result = await commands.RequestProductionSpeedAsync(User.AccountId(), id, type, speedPercent, cancellationToken);
        return CommandAnswer(result, id, type);
    }

    /// <summary>Reports whether the engine has decided on a command yet. Called repeatedly by <c>commands.js</c>.</summary>
    /// <param name="id">The event id returned when the command was recorded.</param>
    /// <param name="cancellationToken">Aborted when the request is aborted.</param>
    /// <returns>The status as JSON, or 404 if the command does not exist or belongs to someone else.</returns>
    [HttpGet]
    public async Task<IActionResult> Status(long id, CancellationToken cancellationToken)
    {
        CommandStatusView? status = await commands.GetStatusAsync(User.AccountId(), id, cancellationToken);
        if (status is null)
        {
            return NotFound();
        }

        // Enums as text, so the script can compare readable values.
        return Json(new
        {
            state = status.State.ToString(),
            isDecided = status.IsDecided,
            error = status.Error,
        });
    }

    /// <summary>The answer to a command, as JSON for scripts or as redirect with message for plain form posts.</summary>
    private IActionResult CommandAnswer(CommandResult result, int planetId, BuildingType type)
    {
        if (result.Outcome == CommandOutcome.NotFound)
        {
            return NotFound();
        }

        if (IsScriptRequest())
        {
            return Json(new
            {
                outcome = result.Outcome.ToString(),
                message = result.Message,
                statusUrl = result.EventId is long eventId ? Url.Action(nameof(Status), new { id = eventId }) : null,
            });
        }

        TempData[MessageKey] = result.Outcome == CommandOutcome.Recorded
            ? "Der Befehl wurde erteilt und wird in Kürze ausgeführt."
            : result.Message;

        return RedirectToAction(nameof(Index), new { id = planetId, type });
    }

    private bool IsScriptRequest() => Request.Headers.XRequestedWith == "XMLHttpRequest";
}
