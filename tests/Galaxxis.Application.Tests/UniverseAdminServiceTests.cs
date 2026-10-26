using Galaxxis.Application.Universes;
using Galaxxis.Domain.Planets;
using Galaxxis.Domain.Universes.Generation;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Galaxxis.Application.Tests;

public class UniverseAdminServiceTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    private static readonly UniverseGenerationSettings SmallMap = new()
    {
        GalaxyCount = 2,
        StarSystemsPerGalaxy = 3,
        PlanetsPerStarSystem = 4,
    };

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 9, 16, 12, 0, 0, TimeSpan.Zero));

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Generating_stores_the_complete_map_as_a_paused_universe()
    {
        await using ServiceProvider services = CreateServices();
        UniverseBlueprint expected = UniverseGenerator.Generate(SmallMap, seed: 42);
        List<PlanetBlueprint> expectedPlanets = expected.Galaxies.SelectMany(g => g.StarSystems).SelectMany(s => s.Planets).ToList();

        UniverseSummary summary = await Use(services, admin => admin.GenerateAsync(new GenerateUniverseRequest(SmallMap, Seed: 42), Cancellation));

        Assert.True(summary.IsPaused);
        Assert.Equal(42, summary.Seed);
        Assert.Equal(TimeSpan.FromSeconds(1), summary.TickLength);
        Assert.Equal(2, summary.GalaxyCount);
        Assert.Equal(24, summary.PlanetCount);
        Assert.Equal(0, summary.ClaimedPlanetCount);

        await using GameDbContext context = databases.CreateGameContext();
        List<Planet> planets = await context.Planets.Include(p => p.Moons)
            .Where(p => p.UniverseId == summary.Id)
            .ToListAsync(Cancellation);

        Assert.Equal(expectedPlanets.Select(p => p.Name).Order(), planets.Select(p => p.Name).Order());
        Assert.Equal(expectedPlanets.Sum(p => p.Moons.Count), planets.Sum(p => p.Moons.Count));
        Assert.All(planets, planet =>
        {
            Assert.Null(planet.OwnerPlayerId);
            Assert.Equal(PlanetDefaults.StartingStock, planet.Stock);
            Assert.Equal(PlanetDefaults.Buildings, planet.Buildings);
        });
    }

    [Fact]
    public async Task Resuming_and_pausing_accumulate_the_paused_time()
    {
        await using ServiceProvider services = CreateServices();
        UniverseSummary created = await Use(services, admin => admin.GenerateAsync(new GenerateUniverseRequest(SmallMap), Cancellation));

        // Paused for 10 minutes after generation, then running for 5, paused for 3, running again.
        _time.Advance(TimeSpan.FromMinutes(10));
        UniverseSummary? resumed = await Use(services, admin => admin.ResumeAsync(created.Id, Cancellation));
        Assert.False(resumed!.IsPaused);

        _time.Advance(TimeSpan.FromMinutes(5));
        UniverseSummary? paused = await Use(services, admin => admin.PauseAsync(created.Id, Cancellation));
        Assert.True(paused!.IsPaused);

        _time.Advance(TimeSpan.FromMinutes(3));
        await Use(services, admin => admin.ResumeAsync(created.Id, Cancellation));

        // Resuming a running universe changes nothing.
        _time.Advance(TimeSpan.FromMinutes(1));
        await Use(services, admin => admin.ResumeAsync(created.Id, Cancellation));

        await using GameDbContext context = databases.CreateGameContext();
        Universe universe = await context.Universes.SingleAsync(u => u.Id == created.Id, Cancellation);

        Assert.Null(universe.PausedSince);
        Assert.Equal(TimeSpan.FromMinutes(13), universe.TotalPausedDuration);
    }

    [Fact]
    public async Task Unknown_universes_are_reported_as_missing()
    {
        await using ServiceProvider services = CreateServices();

        Assert.Null(await Use(services, admin => admin.ResumeAsync(int.MaxValue, Cancellation)));
        Assert.Null(await Use(services, admin => admin.PauseAsync(int.MaxValue, Cancellation)));
    }

    [Fact]
    public async Task Invalid_requests_store_nothing()
    {
        await using ServiceProvider services = CreateServices();
        int before = (await Use(services, admin => admin.ListAsync(Cancellation))).Count;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            Use(services, admin => admin.GenerateAsync(new GenerateUniverseRequest(SmallMap with { GalaxyCount = 0 }), Cancellation)));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            Use(services, admin => admin.GenerateAsync(new GenerateUniverseRequest(SmallMap, TickLength: TimeSpan.FromMilliseconds(10)), Cancellation)));

        Assert.Equal(before, (await Use(services, admin => admin.ListAsync(Cancellation))).Count);
    }

    [Fact]
    public async Task The_list_contains_generated_universes_in_creation_order()
    {
        await using ServiceProvider services = CreateServices();

        UniverseSummary first = await Use(services, admin => admin.GenerateAsync(new GenerateUniverseRequest(SmallMap with { Name = "First" }), Cancellation));
        UniverseSummary second = await Use(services, admin => admin.GenerateAsync(new GenerateUniverseRequest(SmallMap with { Name = "Second" }), Cancellation));

        List<int> ids = (await Use(services, admin => admin.ListAsync(Cancellation))).Select(u => u.Id).ToList();

        Assert.True(ids.IndexOf(first.Id) < ids.IndexOf(second.Id));
    }

    private ServiceProvider CreateServices() => databases.CreateServices((services, configuration) =>
    {
        services.AddGalaxxisApplication(configuration);
        services.AddSingleton<TimeProvider>(_time);
    });

    /// <summary>Runs <paramref name="action"/> with the service resolved from a new scope, like a separate request.</summary>
    private static async Task<T> Use<T>(ServiceProvider services, Func<IUniverseAdminService, Task<T>> action)
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<IUniverseAdminService>());
    }
}
