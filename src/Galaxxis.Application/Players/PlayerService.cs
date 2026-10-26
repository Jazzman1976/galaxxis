using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Planets;
using Galaxxis.Domain.Races;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Galaxxis.Application.Players;

/// <summary>Implementation of <see cref="IPlayerService"/>.</summary>
/// <remarks>
/// <para>
/// Joining is one of the few writes outside the engine. It is safe because it only touches an <em>unclaimed</em>
/// planet and a new player row, and it is protected against races in two ways:
/// </para>
/// <list type="bullet">
///   <item>Two accounts picking the same free planet: the planet's row version makes the second save fail; that account
///   simply tries another planet.</item>
///   <item>The same account joining twice at once (double click): the unique index on universe and account refuses the
///   second player; the account is then reported as already joined.</item>
/// </list>
/// </remarks>
internal sealed class PlayerService(
    GameDbContext context,
    TimeProvider timeProvider,
    ILogger<PlayerService> logger) : IPlayerService
{
    private const int MaxClaimAttempts = 10;

    /// <inheritdoc />
    public async Task<IReadOnlyList<UniverseEntry>> ListUniversesAsync(string accountId, CancellationToken cancellationToken = default) =>
        await context.Universes
            .AsNoTracking()
            .OrderBy(universe => universe.Id)
            .Select(universe => new UniverseEntry(
                universe.Id,
                universe.Name,
                universe.PausedSince != null,
                context.Players.Any(player => player.UniverseId == universe.Id && player.AccountId == accountId),
                context.Players.Where(player => player.UniverseId == universe.Id && player.AccountId == accountId).Select(player => (Race?)player.Race).FirstOrDefault(),
                context.Planets.Count(planet => planet.UniverseId == universe.Id && planet.Owner != null && planet.Owner.AccountId == accountId)))
            .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<JoinUniverseResult> JoinUniverseAsync(string accountId, int universeId, Race race, CancellationToken cancellationToken = default)
    {
        Universe? universe = await context.Universes.AsNoTracking().SingleOrDefaultAsync(u => u.Id == universeId, cancellationToken);
        if (universe is null)
        {
            return new JoinUniverseResult(JoinUniverseOutcome.UniverseNotFound, null);
        }

        if (await HomePlanetIdAsync(accountId, universeId, cancellationToken) is int existingHome)
        {
            return new JoinUniverseResult(JoinUniverseOutcome.AlreadyJoined, existingHome);
        }

        for (int attempt = 1; attempt <= MaxClaimAttempts; attempt++)
        {
            context.ChangeTracker.Clear();

            IQueryable<Planet> unclaimed = context.Planets.Where(p => p.UniverseId == universeId && p.OwnerPlayerId == null);
            int unclaimedCount = await unclaimed.CountAsync(cancellationToken);
            if (unclaimedCount == 0)
            {
                return new JoinUniverseResult(JoinUniverseOutcome.NoFreePlanet, null);
            }

            // Pick a random one of the planets counted. Others may claim planets between counting and picking, so the
            // pick can come back empty even though planets are left: that is simply another attempt.
            Planet? planet = await unclaimed
                .OrderBy(p => p.Id)
                .Skip(Random.Shared.Next(unclaimedCount))
                .FirstOrDefaultAsync(cancellationToken);
            if (planet is null)
            {
                continue;
            }

            DateTimeOffset now = timeProvider.GetUtcNow();
            Player player = new() { UniverseId = universeId, AccountId = accountId, Race = race, CreatedAt = now };

            // The new home planet starts fresh, from the current tick on.
            planet.Owner = player;
            planet.Stock = PlanetDefaults.StartingStock;
            planet.Buildings = PlanetBuildings.Empty;
            planet.ResourcesSettledTick = universe.Timeline.TickAt(now);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                logger.LogInformation(
                    "Account {AccountId} joined universe {UniverseId} as {Race} with home planet {PlanetId}.",
                    accountId, universeId, race, planet.Id);
                return new JoinUniverseResult(JoinUniverseOutcome.Joined, planet.Id);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Someone else claimed or changed this planet in the meantime: try another one.
            }
            catch (DbUpdateException)
            {
                // Most likely the same account joined in parallel (unique index). If so, report that player.
                context.ChangeTracker.Clear();
                if (await HomePlanetIdAsync(accountId, universeId, cancellationToken) is int parallelHome)
                {
                    return new JoinUniverseResult(JoinUniverseOutcome.AlreadyJoined, parallelHome);
                }

                throw;
            }
        }

        throw new InvalidOperationException(
            $"Could not claim a planet in universe {universeId} after {MaxClaimAttempts} attempts; too many players are joining at the same moment.");
    }

    private Task<int?> HomePlanetIdAsync(string accountId, int universeId, CancellationToken cancellationToken) =>
        context.Planets
            .AsNoTracking()
            .Where(planet => planet.UniverseId == universeId && planet.Owner != null && planet.Owner.AccountId == accountId)
            .OrderBy(planet => planet.Id)
            .Select(planet => (int?)planet.Id)
            .FirstOrDefaultAsync(cancellationToken);

}
