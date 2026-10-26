using Galaxxis.Domain.Races;

namespace Galaxxis.Web.Views.Universe;

/// <summary>What the join page shows: the universe and the race selection.</summary>
public sealed class JoinViewModel
{
    /// <summary>The universe to enter.</summary>
    public int UniverseId { get; init; }

    /// <summary>The name of the universe.</summary>
    public string UniverseName { get; init; } = string.Empty;

    /// <summary>The preselected race.</summary>
    public Race Race { get; init; } = Race.Human;

    /// <summary>A message about a failed attempt, if any.</summary>
    public string? Message { get; init; }
}
