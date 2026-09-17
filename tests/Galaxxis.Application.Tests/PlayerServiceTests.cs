using Galaxxis.Application.Map;
using Galaxxis.Application.Players;
using Galaxxis.Application.Universes;
using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Planets;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Universes.Generation;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Galaxxis.Application.Tests;

public class PlayerServiceTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    private static readonly UniverseGenerationSettings TinyMap = new() { GalaxyCount = 1, StarSystemsPerGalaxy = 2, PlanetsPerStarSystem = 3 };

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 9, 17, 12, 0, 0, TimeSpan.Zero));

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Entering_a_universe_creates_the_player_and_assigns_one_fresh_random_planet()
    {
        await using ServiceProvider services = CreateServices();
        int universeId = await CreateRunningUniverseAsync(services);
        string account = NewAccount();

        JoinUniverseResult result = await Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(account, universeId, Race.Goblin, Cancellation));

        Assert.Equal(JoinUniverseOutcome.Joined, result.Outcome);
        await using GameDbContext database = databases.CreateGameContext();
        Planet home = await database.Planets.Include(p => p.Owner).SingleAsync(p => p.Id == result.HomePlanetId, Cancellation);
        Assert.Equal(account, home.Owner!.AccountId);
        Assert.Equal(Race.Goblin, home.Owner.Race);
        Assert.Equal(PlanetDefaults.StartingStock, home.Stock);
        Assert.Equal(PlanetBuildings.Empty, home.Buildings);
        Assert.Equal(1, await database.Planets.CountAsync(p => p.OwnerPlayerId == home.OwnerPlayerId, Cancellation));
    }

    [Fact]
    public async Task Entering_twice_keeps_the_first_player_and_planet()
    {
        await using ServiceProvider services = CreateServices();
        int universeId = await CreateRunningUniverseAsync(services);
        string account = NewAccount();

        JoinUniverseResult first = await Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(account, universeId, Race.Human, Cancellation));
        JoinUniverseResult second = await Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(account, universeId, Race.Orc, Cancellation));

        Assert.Equal(JoinUniverseOutcome.AlreadyJoined, second.Outcome);
        Assert.Equal(first.HomePlanetId, second.HomePlanetId);
    }

    [Fact]
    public async Task Parallel_entering_by_the_same_account_creates_only_one_player()
    {
        await using ServiceProvider services = CreateServices();
        int universeId = await CreateRunningUniverseAsync(services);
        string account = NewAccount();

        JoinUniverseResult[] results = await Task.WhenAll(Enumerable.Range(0, 5).Select(_ =>
            Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(account, universeId, Race.Elf, Cancellation))));

        Assert.All(results, result => Assert.NotNull(result.HomePlanetId));
        Assert.Single(results.Select(r => r.HomePlanetId).Distinct());
        await using GameDbContext database = databases.CreateGameContext();
        Assert.Equal(1, await database.Players.CountAsync(p => p.UniverseId == universeId && p.AccountId == account, Cancellation));
    }

    [Fact]
    public async Task Many_accounts_entering_at_once_each_get_their_own_planet_until_all_are_taken()
    {
        await using ServiceProvider services = CreateServices();
        int universeId = await CreateRunningUniverseAsync(services); // exactly 6 planets

        JoinUniverseResult[] results = await Task.WhenAll(Enumerable.Range(0, 6).Select(_ =>
            Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(NewAccount(), universeId, Race.Human, Cancellation))));

        Assert.All(results, result => Assert.Equal(JoinUniverseOutcome.Joined, result.Outcome));
        Assert.Equal(6, results.Select(r => r.HomePlanetId).Distinct().Count());
    }

    [Fact]
    public async Task A_full_universe_is_reported()
    {
        await using ServiceProvider services = CreateServices();
        int universeId = await CreateRunningUniverseAsync(services, new UniverseGenerationSettings { GalaxyCount = 1, StarSystemsPerGalaxy = 1, PlanetsPerStarSystem = 1 });

        await Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(NewAccount(), universeId, Race.Troll, Cancellation));
        JoinUniverseResult late = await Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(NewAccount(), universeId, Race.Troll, Cancellation));

        Assert.Equal(JoinUniverseOutcome.NoFreePlanet, late.Outcome);
    }

    [Fact]
    public async Task The_map_marks_only_the_accounts_own_places()
    {
        await using ServiceProvider services = CreateServices();
        int universeId = await CreateRunningUniverseAsync(services);
        string me = NewAccount();
        string stranger = NewAccount();
        JoinUniverseResult mine = await Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(me, universeId, Race.Dwarf, Cancellation));

        UniverseMapView? myView = await Use<IMapService, UniverseMapView?>(services, map => map.GetUniverseAsync(me, universeId, Cancellation));
        UniverseMapView? strangerView = await Use<IMapService, UniverseMapView?>(services, map => map.GetUniverseAsync(stranger, universeId, Cancellation));

        Assert.True(myView!.HasJoined);
        Assert.Contains(myView.Galaxies, galaxy => galaxy.ContainsOwnPlanet);
        Assert.False(strangerView!.HasJoined);
        Assert.DoesNotContain(strangerView.Galaxies, galaxy => galaxy.ContainsOwnPlanet);

        await using GameDbContext database = databases.CreateGameContext();
        int systemId = await database.Planets.Where(p => p.Id == mine.HomePlanetId).Select(p => p.StarSystemId).SingleAsync(Cancellation);
        StarSystemMapView? system = await Use<IMapService, StarSystemMapView?>(services, map => map.GetStarSystemAsync(me, systemId, Cancellation));
        Assert.Single(system!.Planets, planet => planet.ContainsOwnPlanet);
        Assert.Equal(mine.HomePlanetId, system.Planets.Single(planet => planet.ContainsOwnPlanet).Id);
    }

    [Fact]
    public async Task The_universe_list_shows_where_the_account_plays()
    {
        await using ServiceProvider services = CreateServices();
        int joined = await CreateRunningUniverseAsync(services);
        int other = await CreateRunningUniverseAsync(services);
        string account = NewAccount();
        await Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(account, joined, Race.Human, Cancellation));

        IReadOnlyList<UniverseEntry> entries = await Use<IPlayerService, IReadOnlyList<UniverseEntry>>(services, players => players.ListUniversesAsync(account, Cancellation));

        UniverseEntry joinedEntry = entries.Single(e => e.UniverseId == joined);
        Assert.True(joinedEntry.HasJoined);
        Assert.Equal(Race.Human, joinedEntry.Race);
        Assert.Equal(1, joinedEntry.PlanetCount);
        Assert.False(entries.Single(e => e.UniverseId == other).HasJoined);
    }

    private ServiceProvider CreateServices() => databases.CreateServices((services, configuration) =>
    {
        services.AddGalaxxisApplication(configuration);
        services.AddSingleton<TimeProvider>(_time);
    });

    private static async Task<int> CreateRunningUniverseAsync(ServiceProvider services, UniverseGenerationSettings? map = null)
    {
        UniverseSummary universe = await Use<IUniverseAdminService, UniverseSummary>(services, admin => admin.GenerateAsync(new GenerateUniverseRequest(map ?? TinyMap), Cancellation));
        await Use<IUniverseAdminService, UniverseSummary?>(services, admin => admin.ResumeAsync(universe.Id, Cancellation));
        return universe.Id;
    }

    private static string NewAccount() => Guid.NewGuid().ToString();

    private static async Task<TResult> Use<TService, TResult>(ServiceProvider services, Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }
}
