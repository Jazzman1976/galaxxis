using Galaxxis.Domain.Races;

namespace Galaxxis.Application.Players;

/// <summary>
/// Everything about a login account taking part in universes: which universes exist for it and joining one.
/// </summary>
/// <remarks>
/// Every method takes the <c>accountId</c> of the signed-in login account (the Identity user id). The web application
/// reads it from the authentication cookie; it is never taken from a form or URL, so a player can only act as
/// themselves.
/// </remarks>
public interface IPlayerService
{
    /// <summary>Lists all universes with the account's participation in each.</summary>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="cancellationToken">Cancels the query.</param>
    /// <returns>One entry per universe, oldest first.</returns>
    Task<IReadOnlyList<UniverseEntry>> ListUniversesAsync(string accountId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lets the account enter a universe: creates its player with the chosen race and assigns exactly one random
    /// unclaimed planet as home planet. Entering a universe the account already plays in changes nothing.
    /// </summary>
    /// <remarks>
    /// Game design (confirmed 2026-09-13): a planet is assigned when the player enters the universe, not at registration.
    /// The claimed planet starts fresh with the default starting stock and no buildings, whatever it accumulated while
    /// it was unclaimed, so late joiners get no advantage.
    /// </remarks>
    /// <param name="accountId">The signed-in account.</param>
    /// <param name="universeId">The universe to enter.</param>
    /// <param name="race">The race to play in this universe.</param>
    /// <param name="cancellationToken">Cancels the operation.</param>
    /// <returns>What happened and the home planet.</returns>
    Task<JoinUniverseResult> JoinUniverseAsync(string accountId, int universeId, Race race, CancellationToken cancellationToken = default);
}

/// <summary>A universe from the point of view of one account.</summary>
/// <param name="UniverseId">The universe.</param>
/// <param name="Name">Its name.</param>
/// <param name="IsPaused">Whether universe time is stopped.</param>
/// <param name="HasJoined">Whether the account plays in this universe.</param>
/// <param name="Race">The account's race there, if joined.</param>
/// <param name="PlanetCount">How many planets the account owns there.</param>
public sealed record UniverseEntry(int UniverseId, string Name, bool IsPaused, bool HasJoined, Race? Race, int PlanetCount);

/// <summary>The outcome of <see cref="IPlayerService.JoinUniverseAsync"/>.</summary>
/// <param name="Outcome">What happened.</param>
/// <param name="HomePlanetId">The player's home planet when joined; otherwise <see langword="null"/>.</param>
public sealed record JoinUniverseResult(JoinUniverseOutcome Outcome, int? HomePlanetId);

/// <summary>Possible outcomes of entering a universe.</summary>
public enum JoinUniverseOutcome
{
    /// <summary>A new player was created and received a home planet.</summary>
    Joined,

    /// <summary>The account already plays in this universe; nothing changed.</summary>
    AlreadyJoined,

    /// <summary>There is no universe with this id.</summary>
    UniverseNotFound,

    /// <summary>Every planet of the universe is taken.</summary>
    NoFreePlanet,
}
