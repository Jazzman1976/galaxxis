using System.ComponentModel.DataAnnotations;

namespace Galaxxis.Web.Views.Account;

/// <summary>The input of the registration form.</summary>
/// <remarks>
/// Only the form checks live here (required fields, e-mail format, matching passwords). Password strength and unique
/// account names and e-mail addresses are checked by Identity when the account is created.
/// </remarks>
public sealed class RegisterViewModel
{
    /// <summary>The account name used to sign in.</summary>
    [Required(ErrorMessage = "Bitte gib einen Benutzernamen ein.")]
    [StringLength(50, ErrorMessage = "Der Benutzername darf höchstens 50 Zeichen lang sein.")]
    [Display(Name = "Benutzername")]
    public string? UserName { get; set; }

    /// <summary>The e-mail address; unique among all accounts.</summary>
    [Required(ErrorMessage = "Bitte gib deine E-Mail-Adresse ein.")]
    [EmailAddress(ErrorMessage = "Bitte gib eine gültige E-Mail-Adresse ein.")]
    [Display(Name = "E-Mail")]
    public string? Email { get; set; }

    /// <summary>Optional first name.</summary>
    [StringLength(100)]
    [Display(Name = "Vorname")]
    public string? FirstName { get; set; }

    /// <summary>Optional last name.</summary>
    [StringLength(100)]
    [Display(Name = "Nachname")]
    public string? LastName { get; set; }

    /// <summary>The password.</summary>
    [Required(ErrorMessage = "Bitte gib ein Passwort ein.")]
    [DataType(DataType.Password)]
    [Display(Name = "Passwort")]
    public string? Password { get; set; }

    /// <summary>The password again, to catch typing mistakes.</summary>
    [Required(ErrorMessage = "Bitte wiederhole das Passwort.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Die beiden Passwörter stimmen nicht überein.")]
    [Display(Name = "Passwort wiederholen")]
    public string? ConfirmPassword { get; set; }
}
