using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Galaxxis.Web.Views.Error;

/// <summary>
/// Error pages for unexpected errors (outside development) and for status codes such as 404 and 403.
/// </summary>
/// <remarks>
/// Reached only internally: the pipeline re-executes the request here (<c>UseExceptionHandler</c>,
/// <c>UseStatusCodePagesWithReExecute</c> in Program.cs). Details of an error are logged, never shown.
/// </remarks>
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
[Route("error")]
public sealed class ErrorController : Controller
{
    /// <summary>Shows an error page.</summary>
    /// <param name="statusCode">The status code of the failed request; empty for an unhandled exception.</param>
    /// <returns>The error page with the original status code.</returns>
    [Route("{statusCode:int?}")]
    public IActionResult Index(int? statusCode)
    {
        int code = statusCode ?? StatusCodes.Status500InternalServerError;
        Response.StatusCode = code;

        ViewBag.Heading = code switch
        {
            StatusCodes.Status404NotFound => "Nicht gefunden",
            StatusCodes.Status403Forbidden => "Kein Zugriff",
            StatusCodes.Status400BadRequest => "Ungültige Anfrage",
            _ => "Ein Fehler ist aufgetreten",
        };

        ViewBag.Text = code switch
        {
            StatusCodes.Status404NotFound => "Diese Seite gibt es nicht, oder sie gehört nicht zu deinem Konto.",
            StatusCodes.Status403Forbidden => "Für diese Seite fehlt deinem Konto die Berechtigung.",
            StatusCodes.Status400BadRequest => "Die Anfrage war ungültig. Bitte lade die Seite neu und versuche es noch einmal.",
            _ => "Bitte versuche es später noch einmal.",
        };

        return View("Error");
    }
}
