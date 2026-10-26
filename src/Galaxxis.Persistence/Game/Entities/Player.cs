using Galaxxis.Domain.Races;

namespace Galaxxis.Persistence.Game.Entities;

/// <summary>
/// A player in one universe.
/// </summary>
/// <remarks>
/// A login account (accounts database) can play in several universes and has one player per universe,
/// each with its own race. The link is <see cref="AccountId"/>, the id of the Identity user. The legacy
/// code linked by e-mail address, which breaks as soon as someone changes their address.
/// Because the two databases are separate, this link is not a database foreign key.
/// </remarks>
public sealed class Player
{
    /// <summary>Primary key.</summary>
    public int Id { get; set; }

    /// <summary>The universe the player plays in.</summary>
    public int UniverseId { get; set; }

    /// <summary>Navigation to the universe.</summary>
    public Universe Universe { get; set; } = null!;

    /// <summary>Id of the login account (Identity user id) this player belongs to. Unique per universe.</summary>
    public string AccountId { get; set; } = string.Empty;

    /// <summary>The race the player chose for this universe.</summary>
    public Race Race { get; set; }

    /// <summary>When the player joined the universe.</summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The planets the player owns.</summary>
    public List<Planet> Planets { get; set; } = [];
}
