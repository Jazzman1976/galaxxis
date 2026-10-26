using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Engine.Processing;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Galaxxis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Galaxxis.Engine.Tests;

/// <summary>
/// A small game world for one engine test: its own universe, players and planets in the test databases, a fake
/// clock the test moves forward, and helpers to record commands and run the engine.
/// </summary>
/// <remarks>
/// Creating a harness pauses every universe an earlier test of the same class left running, so a test only ever
/// sees its own universes being processed.
/// </remarks>
internal sealed class EngineHarness : IAsyncDisposable
{
    private readonly TestDatabases _databases;

    private EngineHarness(TestDatabases databases, FakeTimeProvider time, ServiceProvider services)
    {
        _databases = databases;
        Time = time;
        Services = services;
    }

    /// <summary>The clock of this test. Move it with <see cref="Advance"/>.</summary>
    public FakeTimeProvider Time { get; }

    /// <summary>The engine's services, as a host would build them.</summary>
    public ServiceProvider Services { get; }

    public static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    /// <summary>Creates a harness with the Persistence and Engine modules and optional extra registrations.</summary>
    public static async Task<EngineHarness> CreateAsync(
        TestDatabases databases,
        Action<IServiceCollection>? configure = null,
        FakeTimeProvider? sharedTime = null)
    {
        FakeTimeProvider time = sharedTime ?? new FakeTimeProvider(new DateTimeOffset(2026, 9, 16, 12, 0, 0, TimeSpan.Zero));

        ServiceProvider services = databases.CreateServices((collection, configuration) =>
        {
            collection.AddGalaxxisEngine(configuration);
            collection.AddSingleton<TimeProvider>(time);
            configure?.Invoke(collection);
        });

        var harness = new EngineHarness(databases, time, services);

        if (sharedTime is null)
        {
            await using GameDbContext database = databases.CreateGameContext();
            await database.Universes
                .Where(universe => universe.PausedSince == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(universe => universe.PausedSince, DateTimeOffset.MinValue), Cancellation);
        }

        return harness;
    }

    /// <summary>Moves the clock forward.</summary>
    public void Advance(TimeSpan duration) => Time.Advance(duration);

    /// <summary>Creates a running universe whose tick 0 is now.</summary>
    public async Task<Universe> CreateUniverseAsync(TimeSpan? tickLength = null)
    {
        await using GameDbContext database = _databases.CreateGameContext();
        Universe universe = new()
        {
            Name = "Test universe",
            Epoch = Time.GetUtcNow(),
            TickLength = tickLength ?? TimeSpan.FromSeconds(1),
            CreatedAt = Time.GetUtcNow(),
            Galaxies = [new Galaxy { Name = "Test galaxy", StarSystems = [new StarSystem { Name = "Test system" }] }],
        };

        database.Universes.Add(universe);
        await database.SaveChangesAsync(Cancellation);
        return universe;
    }

    /// <summary>Creates a player with one planet.</summary>
    public async Task<(Player Player, Planet Planet)> CreatePlayerWithPlanetAsync(
        Universe universe, Race race, ResourceAmounts stock, PlanetBuildings? buildings = null)
    {
        await using GameDbContext database = _databases.CreateGameContext();
        Player player = new()
        {
            UniverseId = universe.Id,
            AccountId = Guid.NewGuid().ToString(),
            Race = race,
            CreatedAt = Time.GetUtcNow(),
        };

        Planet planet = await NewPlanetAsync(database, universe, stock, buildings);
        planet.Owner = player;

        database.Planets.Add(planet);
        await database.SaveChangesAsync(Cancellation);
        return (player, planet);
    }

    /// <summary>Creates a planet without owner.</summary>
    public async Task<Planet> CreateUnclaimedPlanetAsync(Universe universe, ResourceAmounts stock)
    {
        await using GameDbContext database = _databases.CreateGameContext();
        Planet planet = await NewPlanetAsync(database, universe, stock, buildings: null);
        database.Planets.Add(planet);
        await database.SaveChangesAsync(Cancellation);
        return planet;
    }

    /// <summary>Stations ships of a player at a planet.</summary>
    public async Task<List<Ship>> CreateShipsAsync(Player player, Planet planet, int count, int storageCapacity)
    {
        await using GameDbContext database = _databases.CreateGameContext();
        List<Ship> ships = Enumerable.Range(0, count).Select(_ => new Ship
        {
            PlayerId = player.Id,
            PlanetId = planet.Id,
            Type = Domain.Ships.ShipType.Explorer,
            StorageCapacity = storageCapacity,
        }).ToList();

        database.Ships.AddRange(ships);
        await database.SaveChangesAsync(Cancellation);
        return ships;
    }

    /// <summary>Records a command the way the web application will: as a requested event at the current tick.</summary>
    public async Task<long> RequestAsync<TPayload>(Universe universe, string eventType, TPayload payload, int? playerId, int? planetId)
    {
        await using GameDbContext database = _databases.CreateGameContext();
        Universe current = await database.Universes.AsNoTracking().SingleAsync(u => u.Id == universe.Id, Cancellation);
        long tick = current.Timeline.TickAt(Time.GetUtcNow());

        GameEvent gameEvent = new()
        {
            UniverseId = universe.Id,
            Type = eventType,
            State = GameEventState.Requested,
            PlayerId = playerId,
            PlanetId = planetId,
            RequestedTick = tick,
            ProcessAtTick = tick,
            Payload = payload is string raw ? raw : GameEventPayload.Serialize(payload),
            CreatedAt = Time.GetUtcNow(),
        };

        database.GameEvents.Add(gameEvent);
        await database.SaveChangesAsync(Cancellation);
        return gameEvent.Id;
    }

    /// <summary>Processes all due events of one universe, as the engine does in a cycle.</summary>
    public Task<UniverseProcessingResult> ProcessAsync(Universe universe, int maxEvents = 1000) =>
        Services.GetRequiredService<UniverseEventProcessor>().ProcessDueEventsAsync(universe.Id, maxEvents, Cancellation);

    /// <summary>Runs one complete engine cycle (all running universes, with leases).</summary>
    public Task RunCycleAsync() => Services.GetRequiredService<EngineCycle>().RunOnceAsync(Cancellation);

    /// <summary>Reads an event fresh from the database.</summary>
    public async Task<GameEvent> EventAsync(long eventId)
    {
        await using GameDbContext database = _databases.CreateGameContext();
        return await database.GameEvents.AsNoTracking().SingleAsync(e => e.Id == eventId, Cancellation);
    }

    /// <summary>Reads all events of a universe fresh from the database, in id order.</summary>
    public async Task<List<GameEvent>> EventsAsync(Universe universe)
    {
        await using GameDbContext database = _databases.CreateGameContext();
        return await database.GameEvents.AsNoTracking().Where(e => e.UniverseId == universe.Id).OrderBy(e => e.Id).ToListAsync(Cancellation);
    }

    /// <summary>Reads a planet fresh from the database.</summary>
    public async Task<Planet> PlanetAsync(int planetId)
    {
        await using GameDbContext database = _databases.CreateGameContext();
        return await database.Planets.AsNoTracking().SingleAsync(p => p.Id == planetId, Cancellation);
    }

    /// <summary>Opens a database context for custom checks.</summary>
    public GameDbContext Database() => _databases.CreateGameContext();

    public ValueTask DisposeAsync() => Services.DisposeAsync();

    private static async Task<Planet> NewPlanetAsync(GameDbContext database, Universe universe, ResourceAmounts stock, PlanetBuildings? buildings)
    {
        int starSystemId = await database.StarSystems
            .Where(system => system.Galaxy.UniverseId == universe.Id)
            .Select(system => system.Id)
            .FirstAsync(Cancellation);

        return new Planet
        {
            UniverseId = universe.Id,
            StarSystemId = starSystemId,
            Name = "Test planet",
            Fields = 150,
            Stock = stock,
            Buildings = buildings ?? PlanetBuildings.Empty,
        };
    }
}
