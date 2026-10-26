using System.Security.Claims;

namespace Galaxxis.Web.Infrastructure;

/// <summary>
/// Reads the signed-in account from the request's user.
/// </summary>
/// <remarks>
/// Every Application service takes the account id of the signed-in user and checks ownership itself. Controllers
/// therefore pass <see cref="AccountId"/> along and never an account or player id that came with the request.
/// </remarks>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// The id of the signed-in account (the Identity user id, stored as <c>Player.AccountId</c> in the game database).
    /// </summary>
    /// <param name="user">The user of the current request; must be signed in.</param>
    /// <returns>The account id.</returns>
    /// <exception cref="InvalidOperationException">The user is not signed in. Pages that call this require sign-in.</exception>
    public static string AccountId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("The request has no signed-in account; the page must require sign-in.");
}
