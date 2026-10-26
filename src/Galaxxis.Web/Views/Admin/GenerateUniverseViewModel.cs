using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Galaxxis.Application.Universes;

namespace Galaxxis.Web.Views.Admin;

/// <summary>The input of the "generate universe" form on the admin page.</summary>
/// <remarks>
/// Covers the same options as <c>UniverseGenerator -- generate</c>. The form starts with the configured defaults
/// (<see cref="UniverseGenerationOptions"/>); the finer map settings (coordinate range, distances, moon probabilities)
/// are not offered here and always come from the configuration. Only form checks live here; the Application service
/// validates the complete settings again when it generates the map.
/// </remarks>
public sealed class GenerateUniverseViewModel : IValidatableObject
{
    /// <summary>
    /// Largest map the web page generates. The whole universe is stored inside one web request; bigger maps belong
    /// into the generator tool, which has no request time limit.
    /// </summary>
    public const int MaximumPlanets = 20_000;

    /// <summary>Name of the universe; empty picks a random name.</summary>
    // The name column of the Universes table holds at most 50 characters.
    [StringLength(50, ErrorMessage = "Der Name darf höchstens 50 Zeichen lang sein.")]
    [Display(Name = "Name")]
    public string? Name { get; set; }

    /// <summary>Random seed; empty picks a random one. The same seed generates the same map.</summary>
    [Display(Name = "Seed")]
    public int? Seed { get; set; }

    /// <summary>Number of galaxies.</summary>
    [Required(ErrorMessage = "Bitte gib die Anzahl der Galaxien an.")]
    [Range(1, 100, ErrorMessage = "Galaxien: bitte eine Zahl von 1 bis 100.")]
    [Display(Name = "Galaxien")]
    public int? GalaxyCount { get; set; }

    /// <summary>Number of star systems in each galaxy.</summary>
    [Required(ErrorMessage = "Bitte gib die Anzahl der Sternensysteme pro Galaxie an.")]
    [Range(1, 100, ErrorMessage = "Sternensysteme pro Galaxie: bitte eine Zahl von 1 bis 100.")]
    [Display(Name = "Sternensysteme pro Galaxie")]
    public int? StarSystemsPerGalaxy { get; set; }

    /// <summary>Number of planets in each star system.</summary>
    [Required(ErrorMessage = "Bitte gib die Anzahl der Planeten pro Sternensystem an.")]
    [Range(1, 100, ErrorMessage = "Planeten pro Sternensystem: bitte eine Zahl von 1 bis 100.")]
    [Display(Name = "Planeten pro Sternensystem")]
    public int? PlanetsPerStarSystem { get; set; }

    /// <summary>
    /// Real time per tick in the format <c>hh:mm:ss</c> or <c>hh:mm:ss.fff</c>, the same format as in the configuration.
    /// </summary>
    /// <remarks>
    /// Kept as text and parsed with the invariant culture: the page runs with German culture, where the model binder
    /// would expect a comma before the milliseconds.
    /// </remarks>
    [Required(ErrorMessage = "Bitte gib die Ticklänge an, zum Beispiel 00:00:01.")]
    [Display(Name = "Ticklänge")]
    public string? TickLength { get; set; }

    /// <summary>Whether to start universe time right after generation instead of leaving the universe paused.</summary>
    [Display(Name = "Sofort starten")]
    public bool StartImmediately { get; set; }

    /// <summary>Creates a form filled with the configured defaults.</summary>
    /// <param name="defaults">The <c>UniverseGeneration</c> settings of this host.</param>
    /// <returns>The pre-filled form.</returns>
    public static GenerateUniverseViewModel FromDefaults(UniverseGenerationOptions defaults) => new()
    {
        Name = defaults.Map.Name,
        GalaxyCount = defaults.Map.GalaxyCount,
        StarSystemsPerGalaxy = defaults.Map.StarSystemsPerGalaxy,
        PlanetsPerStarSystem = defaults.Map.PlanetsPerStarSystem,
        TickLength = defaults.TickLength.ToString("c", CultureInfo.InvariantCulture),
    };

    /// <summary>The number of planets the map will have.</summary>
    public long PlanetCount => (long)(GalaxyCount ?? 0) * (StarSystemsPerGalaxy ?? 0) * (PlanetsPerStarSystem ?? 0);

    /// <summary>The parsed tick length, or <see langword="null"/> if the text is not a valid time span.</summary>
    public TimeSpan? ParsedTickLength =>
        TimeSpan.TryParse(TickLength, CultureInfo.InvariantCulture, out TimeSpan tickLength) ? tickLength : null;

    /// <summary>
    /// Builds the request for <see cref="IUniverseAdminService.GenerateAsync"/>: the configured map settings with the
    /// values of this form. Call only after the form is valid.
    /// </summary>
    /// <param name="defaults">The <c>UniverseGeneration</c> settings of this host.</param>
    /// <returns>The request.</returns>
    public GenerateUniverseRequest ToRequest(UniverseGenerationOptions defaults) => new(
        defaults.Map with
        {
            Name = string.IsNullOrWhiteSpace(Name) ? null : Name.Trim(),
            GalaxyCount = GalaxyCount!.Value,
            StarSystemsPerGalaxy = StarSystemsPerGalaxy!.Value,
            PlanetsPerStarSystem = PlanetsPerStarSystem!.Value,
        },
        Seed,
        ParsedTickLength);

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(TickLength))
        {
            if (ParsedTickLength is not TimeSpan tickLength)
            {
                yield return new ValidationResult(
                    "Die Ticklänge hat nicht das Format hh:mm:ss, zum Beispiel 00:00:01 oder 00:00:00.500.", [nameof(TickLength)]);
            }
            else if (tickLength < UniverseGenerationOptions.MinimumTickLength || tickLength > UniverseGenerationOptions.MaximumTickLength)
            {
                yield return new ValidationResult(
                    "Die Ticklänge muss zwischen 00:00:00.100 und 1 Tag liegen.", [nameof(TickLength)]);
            }
        }

        if (PlanetCount > MaximumPlanets)
        {
            yield return new ValidationResult(
                $"Die Karte hätte {PlanetCount:N0} Planeten; hier sind höchstens {MaximumPlanets:N0} möglich. " +
                "Größere Universen mit dem Generator-Tool erzeugen.");
        }
    }
}
