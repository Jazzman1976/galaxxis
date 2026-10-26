using Galaxxis.Persistence.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Galaxxis.Web.Views.Account;

/// <summary>
/// Sign-in, registration and sign-out of login accounts (ASP.NET Core Identity).
/// </summary>
/// <remarks>
/// <para>
/// Registration only creates the login account. A player comes into existence per universe when the account enters a
/// universe for the first time (see <c>UniverseController.Join</c>); that is also when the home planet is assigned.
/// </para>
/// <para>
/// Security: sign-in and registration are POST requests with antiforgery token (global filter). Repeated wrong
/// passwords temporarily lock the account (rules in <see cref="AccountsModule.ConfigureIdentity"/>). Error messages do
/// not reveal whether an account name exists.
/// </para>
/// </remarks>
[AllowAnonymous]
public sealed class AccountController(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    ILogger<AccountController> logger) : Controller
{
    /// <summary>Shows the login form.</summary>
    /// <param name="returnUrl">The page to continue with after signing in, if the player was sent here from it.</param>
    /// <returns>The login page.</returns>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) =>
        View("Login", new LoginViewModel { ReturnUrl = returnUrl });

    /// <summary>Signs in with account name (or e-mail address) and password.</summary>
    /// <param name="model">The form input.</param>
    /// <returns>A redirect to the return page or the start page, or the login page with a message.</returns>
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Login", model);
        }

        // Players may type their account name or their e-mail address.
        AppUser? user = await userManager.FindByNameAsync(model.UserName!)
            ?? await userManager.FindByEmailAsync(model.UserName!);

        // lockoutOnFailure: count wrong passwords and lock the account after too many (password guessing protection).
        SignInResult result = user is null
            ? SignInResult.Failed
            : await signInManager.PasswordSignInAsync(user, model.Password!, isPersistent: model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Only local return URLs: never redirect to another site (open redirect protection).
            return Url.IsLocalUrl(model.ReturnUrl)
                ? LocalRedirect(model.ReturnUrl)
                : RedirectToAction("Index", "Start");
        }

        if (result.IsLockedOut)
        {
            logger.LogWarning("Sign-in refused for locked account {AccountId}.", user?.Id);
            ModelState.AddModelError(string.Empty, "Zu viele Fehlversuche: Das Konto ist vorübergehend gesperrt. Bitte versuche es später erneut.");
        }
        else
        {
            // The same message whether the account exists or not.
            ModelState.AddModelError(string.Empty, "Benutzername oder Passwort ist falsch.");
        }

        return View("Login", model);
    }

    /// <summary>Shows the registration form.</summary>
    /// <returns>The registration page.</returns>
    [HttpGet]
    public IActionResult Register() => View("Register", new RegisterViewModel());

    /// <summary>Creates a login account and signs it in.</summary>
    /// <param name="model">The form input.</param>
    /// <returns>A redirect to the start page (universe list), or the registration page with the problems.</returns>
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Register", model);
        }

        AppUser user = new()
        {
            UserName = model.UserName,
            Email = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
        };

        // Identity checks password rules and that account name and e-mail address are not taken yet.
        IdentityResult result = await userManager.CreateAsync(user, model.Password!);
        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Register", model);
        }

        logger.LogInformation("Account {AccountId} registered.", user.Id);
        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Start");
    }

    /// <summary>Signs out.</summary>
    /// <returns>A redirect to the start page.</returns>
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Start");
    }

    /// <summary>Shown when a signed-in account opens a page it has no permission for, for example the admin page.</summary>
    /// <returns>The access denied page with status 403.</returns>
    [HttpGet]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View("AccessDenied");
    }
}
