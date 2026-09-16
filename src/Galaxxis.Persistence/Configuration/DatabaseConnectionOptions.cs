using System.ComponentModel.DataAnnotations;

namespace Galaxxis.Persistence.Configuration;

/// <summary>
/// The connection strings of the two Galaxxis databases, bound from the standard configuration
/// section <c>ConnectionStrings</c>.
/// </summary>
/// <remarks>
/// <para>
/// Galaxxis deliberately uses two separate databases: <see cref="Game"/> holds the game world
/// (universes, planets, players, events) and <see cref="Accounts"/> holds the login accounts
/// (ASP.NET Core Identity). Keeping them apart means the game never depends on how people log in.
/// </para>
/// <para>
/// Values normally come from <c>appsettings.Local.json</c> on a developer machine and from the
/// App Service connection string settings on Azure. Both are validated when a host starts, so a
/// missing connection string stops the application immediately with a clear message instead of
/// failing later on the first database access.
/// </para>
/// </remarks>
public sealed class DatabaseConnectionOptions
{
    /// <summary>Name of the configuration section the options are bound from.</summary>
    public const string SectionName = "ConnectionStrings";

    /// <summary>Connection string of the game database (configuration key <c>ConnectionStrings:Game</c>).</summary>
    [Required(ErrorMessage =
        "ConnectionStrings:Game is missing. Copy appsettings.Local.Template.json in the repository root " +
        "to appsettings.Local.json and fill it in, or set the value via environment variable.")]
    public string Game { get; set; } = string.Empty;

    /// <summary>Connection string of the accounts database (configuration key <c>ConnectionStrings:Accounts</c>).</summary>
    [Required(ErrorMessage =
        "ConnectionStrings:Accounts is missing. Copy appsettings.Local.Template.json in the repository root " +
        "to appsettings.Local.json and fill it in, or set the value via environment variable.")]
    public string Accounts { get; set; } = string.Empty;
}
