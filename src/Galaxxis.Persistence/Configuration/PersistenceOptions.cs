namespace Galaxxis.Persistence.Configuration;

/// <summary>
/// Behaviour settings of the database layer, bound from the configuration section <c>Persistence</c>.
/// </summary>
public sealed class PersistenceOptions
{
    /// <summary>Name of the configuration section the options are bound from.</summary>
    public const string SectionName = "Persistence";

    /// <summary>
    /// When <see langword="true"/>, pending EF Core migrations are applied to both databases while the
    /// host starts. Convenient on a developer machine; keep it <see langword="false"/> on Azure, where
    /// migrations are deployed as an idempotent SQL script instead.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; }
}
