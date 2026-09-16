using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Universes;
using Galaxxis.Persistence.Accounts;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Persistence.Tests;

public class MigrationTests
{
    [Fact]
    public void The_game_migrations_match_the_entity_model()
    {
        // Fails when an entity or configuration was changed without adding a migration.
        using GameDbContext context = new(new DbContextOptionsBuilder<GameDbContext>().UseSqlServer("Server=unused").Options);

        Assert.False(context.Database.HasPendingModelChanges(), "Run: dotnet ef migrations add <Name> --context GameDbContext (see Galaxxis.Persistence/README.md)");
    }

    [Fact]
    public void The_account_migrations_match_the_entity_model()
    {
        using AccountDbContext context = new(new DbContextOptionsBuilder<AccountDbContext>().UseSqlServer("Server=unused").Options);

        Assert.False(context.Database.HasPendingModelChanges(), "Run: dotnet ef migrations add <Name> --context AccountDbContext (see Galaxxis.Persistence/README.md)");
    }
}

public class SchemaTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    [Fact]
    public async Task A_planet_keeps_its_stock_buildings_and_coordinate()
    {
        var stock = new ResourceAmounts { Ore = 1.5, Energy = 2, Tools = 3, Mana = 4, Crystals = 5, Antimatter = 6, Elementium = 7.25 };
        var buildings = PlanetBuildings.Empty with
        {
            OreMine = new ResourceBuildingState(3, new ProductionSpeed(70)),
            ElementiumSynthesizer = new ResourceBuildingState(1, ProductionSpeed.Stopped),
            Shipyard = new OrderBuildingState(2, IsWorking: true),
        };

        int planetId;
        await using (GameDbContext context = databases.CreateGameContext())
        {
            Planet planet = NewPlanet(NewUniverse());
            planet.Stock = stock;
            planet.Buildings = buildings;
            planet.Coordinate = new Coordinate(-3, 4, 5);
            context.Planets.Add(planet);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
            planetId = planet.Id;
        }

        await using (GameDbContext context = databases.CreateGameContext())
        {
            Planet loaded = await context.Planets.SingleAsync(p => p.Id == planetId, TestContext.Current.CancellationToken);

            Assert.Equal(stock, loaded.Stock);
            Assert.Equal(buildings, loaded.Buildings);
            Assert.Equal(new Coordinate(-3, 4, 5), loaded.Coordinate);
            Assert.NotEmpty(loaded.RowVersion);
        }
    }

    [Fact]
    public async Task Only_one_event_per_planet_and_queue_can_be_scheduled()
    {
        await using GameDbContext context = databases.CreateGameContext();
        Planet planet = NewPlanet(NewUniverse());
        context.Planets.Add(planet);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Any number of requests may wait for the engine's decision ...
        context.GameEvents.AddRange(
            NewEvent(planet, GameEventState.Requested, "Construction"),
            NewEvent(planet, GameEventState.Requested, "Construction"),
            // ... and different queues may run in parallel ...
            NewEvent(planet, GameEventState.Scheduled, "Construction"),
            NewEvent(planet, GameEventState.Scheduled, "Shipyard"));
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        // ... but a second scheduled construction on the same planet is refused by the database.
        context.GameEvents.Add(NewEvent(planet, GameEventState.Scheduled, "Construction"));
        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync(TestContext.Current.CancellationToken));

        Assert.Contains("UX_GameEvents_OneScheduledPerPlanetQueue", error.InnerException?.Message);
    }

    [Fact]
    public async Task Event_states_are_stored_as_readable_text()
    {
        await using GameDbContext context = databases.CreateGameContext();
        Universe universe = NewUniverse();
        GameEvent gameEvent = new()
        {
            Universe = universe,
            Type = "Test",
            State = GameEventState.Rejected,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        context.GameEvents.Add(gameEvent);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);

        string stored = await context.Database
            .SqlQuery<string>($"SELECT [State] AS [Value] FROM [GameEvents] WHERE [Id] = {gameEvent.Id}")
            .SingleAsync(TestContext.Current.CancellationToken);

        Assert.Equal("Rejected", stored);
    }

    [Fact]
    public async Task Hosts_get_a_scoped_context_and_a_context_factory()
    {
        await using ServiceProvider services = databases.CreateServices();
        await using AsyncServiceScope scope = services.CreateAsyncScope();

        GameDbContext scoped = scope.ServiceProvider.GetRequiredService<GameDbContext>();
        await using GameDbContext fromFactory = await services
            .GetRequiredService<IDbContextFactory<GameDbContext>>()
            .CreateDbContextAsync(TestContext.Current.CancellationToken);

        Assert.NotSame(scoped, fromFactory);
        Assert.True(await fromFactory.Database.CanConnectAsync(TestContext.Current.CancellationToken));
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<AccountDbContext>());
    }

    private static Universe NewUniverse() => new()
    {
        Name = "Test",
        Epoch = DateTimeOffset.UtcNow,
        TickLength = TimeSpan.FromSeconds(1),
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private static Planet NewPlanet(Universe universe) => new()
    {
        Universe = universe,
        Name = "Testplanet",
        Fields = 150,
        StarSystem = new StarSystem
        {
            Name = "Testsystem",
            Galaxy = new Galaxy { Name = "Testgalaxy", Universe = universe },
        },
    };

    private static GameEvent NewEvent(Planet planet, GameEventState state, string queue) => new()
    {
        UniverseId = planet.UniverseId,
        PlanetId = planet.Id,
        Type = "BuildBuilding",
        State = state,
        Queue = queue,
        CreatedAt = DateTimeOffset.UtcNow,
    };
}
