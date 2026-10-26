using System.ComponentModel.DataAnnotations;

namespace Galaxxis.Web.Views.Account;

/// <summary>The input of the login form.</summary>
public sealed class LoginViewModel
{
    /// <summary>The page to return to after signing in. Only local addresses are followed.</summary>
    public string? ReturnUrl { get; set; }

    /// <summary>Account name or e-mail address.</summary>
    [Required(ErrorMessage = "Bitte gib deinen Benutzernamen ein.")]
    [Display(Name = "Benutzername")]
    public string? UserName { get; set; }

    /// <summary>The password.</summary>
    [Required(ErrorMessage = "Bitte gib dein Passwort ein.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string? Password { get; set; }

    /// <summary>Whether the sign-in survives closing the browser.</summary>
    [Display(Name = "Angemeldet bleiben")]
    public bool RememberMe { get; set; }
}
