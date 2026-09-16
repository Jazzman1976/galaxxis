using System.ComponentModel.DataAnnotations;
using Galaxxis.Domain.Universes.Generation;

namespace Galaxxis.Application.Universes;

/// <summary>
/// Default values for generating a universe, bound from the configuration section <c>UniverseGeneration</c>.
/// </summary>
/// <remarks>
/// The universe generator tool (and later an admin page) starts from these defaults and lets the
/// administrator override single values.
/// </remarks>
public sealed class UniverseGenerationOptions
{
    /// <summary>Name of the configuration section the options are bound from.</summary>
    public const string SectionName = "UniverseGeneration";

    /// <summary>Shortest allowed tick length. Shorter ticks only add load without making the game more precise.</summary>
    public static readonly TimeSpan MinimumTickLength = TimeSpan.FromMilliseconds(100);

    /// <summary>Longest allowed tick length.</summary>
    public static readonly TimeSpan MaximumTickLength = TimeSpan.FromDays(1);

    /// <summary>
    /// Real time one tick of the new universe represents. Durations in the game are defined in real time,
    /// so the tick length only sets how finely universe time is resolved; one second is a good default.
    /// </summary>
    [Range(typeof(TimeSpan), "00:00:00.100", "1.00:00:00",
        ErrorMessage = "UniverseGeneration:TickLength must be between 100 milliseconds and 1 day.")]
    public TimeSpan TickLength { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>Size and density of the map (configuration section <c>UniverseGeneration:Map</c>).</summary>
    public UniverseGenerationSettings Map { get; set; } = new();
}
