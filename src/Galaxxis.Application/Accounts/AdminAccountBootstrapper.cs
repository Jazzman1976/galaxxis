using System.ComponentModel.DataAnnotations;
using Galaxxis.Persistence.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Galaxxis.Application.Accounts;

/// <summary>
/// The administrator account to create on a fresh installation, bound from the configuration section
/// <c>BootstrapAdmin</c>.
/// </summary>
/// <remarks>
/// The password belongs into <c>appsettings.Local.json</c> (git-ignored) or an environment variable, never into a
/// checked-in file. The settings are only validated when the bootstrap actually runs, so hosts that never
/// create an admin do not need them.
/// </remarks>
public sealed class BootstrapAdminOptions
{
    /// <summary>Name of the configuration section the options are bound from.</summary>
    public const string SectionName = "BootstrapAdmin";

    /// <summary>E-mail address of the admin account. Identifies the account: an existing account with this address is reused.</summary>
    [Required(ErrorMessage = "BootstrapAdmin:Email is missing. Set it in appsettings.Local.json.")]
    [EmailAddress(ErrorMessage = "BootstrapAdmin:Email is not a valid e-mail address.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>User name for signing in.</summary>
    [Required(ErrorMessage = "BootstrapAdmin:UserName is missing. Set it in appsettings.Local.json.")]
    public string UserName { get; set; } = string.Empty;

    /// <summary>Initial password. Must satisfy the Identity password rules (upper and lower case, digit, symbol, 6+ characters).</summary>
    [Required(ErrorMessage = "BootstrapAdmin:Password is missing. Set it in appsettings.Local.json, never in a checked-in file.")]
    public string Password { get; set; } = string.Empty;

    /// <summary>Optional first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Optional last name.</summary>
    public string? LastName { get; set; }
}

/// <summary>What <see cref="IAdminAccountBootstrapper.EnsureAdminAsync"/> did.</summary>
public enum AdminBootstrapResult
{
    /// <summary>A new admin account was created.</summary>
    Created,

    /// <summary>An account with the configured e-mail address already existed; it was made sure it has the admin role.</summary>
    AlreadyExisted,
}

/// <summary>
/// Creates the first administrator account of a fresh installation.
/// </summary>
/// <remarks>
/// Replaces the legacy <c>/account/addadmin</c> web page, which anybody could open and which showed the
/// password on screen. Now only someone with access to the server configuration can create the admin.
/// </remarks>
public interface IAdminAccountBootstrapper
{
    /// <summary>
    /// Makes sure the admin role exists and an account with the configured e-mail address exists and has it.
    /// Safe to run repeatedly.
    /// </summary>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>Whether the account was created or already existed.</returns>
    /// <exception cref="OptionsValidationException">The <c>BootstrapAdmin</c> settings are incomplete.</exception>
    /// <exception cref="InvalidOperationException">Identity refused the account, for example because the password is too weak; the message says why.</exception>
    Task<AdminBootstrapResult> EnsureAdminAsync(CancellationToken cancellationToken = default);
}

/// <summary>Implementation of <see cref="IAdminAccountBootstrapper"/> on top of ASP.NET Core Identity.</summary>
internal sealed class AdminAccountBootstrapper(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IOptions<BootstrapAdminOptions> options,
    ILogger<AdminAccountBootstrapper> logger) : IAdminAccountBootstrapper
{
    /// <inheritdoc />
    public async Task<AdminBootstrapResult> EnsureAdminAsync(CancellationToken cancellationToken = default)
    {
        // Reading Value runs the validation of the BootstrapAdmin settings.
        BootstrapAdminOptions admin = options.Value;

        if (!await roleManager.RoleExistsAsync(AccountRoles.Admin))
        {
            EnsureSucceeded(await roleManager.CreateAsync(new AppRole(AccountRoles.Admin)), "create the admin role");
        }

        AdminBootstrapResult result = AdminBootstrapResult.AlreadyExisted;
        AppUser? user = await userManager.FindByEmailAsync(admin.Email);

        if (user is null)
        {
            user = new AppUser
            {
                Email = admin.Email,
                UserName = admin.UserName,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                // The address is trusted: it comes from the server configuration, not from a sign-up form.
                EmailConfirmed = true,
            };

            EnsureSucceeded(await userManager.CreateAsync(user, admin.Password), "create the admin account");
            result = AdminBootstrapResult.Created;
        }

        if (!await userManager.IsInRoleAsync(user, AccountRoles.Admin))
        {
            EnsureSucceeded(await userManager.AddToRoleAsync(user, AccountRoles.Admin), "assign the admin role");
        }

        // Never log the password.
        logger.LogInformation("Admin account {Email}: {Result}.", admin.Email, result);
        return result;
    }

    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            string reasons = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not {action}: {reasons}");
        }
    }
}
