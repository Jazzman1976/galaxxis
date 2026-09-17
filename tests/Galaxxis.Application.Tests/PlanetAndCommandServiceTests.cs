using Galaxxis.Application.Commands;
using Galaxxis.Application.Planets;
using Galaxxis.Application.Players;
using Galaxxis.Application.Universes;
using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Planets;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Universes.Generation;
using Galaxxis.Persistence.Game;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Galaxxis.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;

namespace Galaxxis.Application.Tests;

public class PlanetAndCommandServiceTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    private static readonly UniverseGenerationSettings TinyMap = new() { GalaxyCount = 1, StarSystemsPerGalaxy = 1, PlanetsPerStarSystem = 4 };

    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 9, 17, 12, 0, 0, TimeSpan.Zero));

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task A_player_sees_their_planet_with_resources_as_of_now()
    {
        await using ServiceProvider services = CreateServices();
        (string account, int planetId) = await JoinNewPlayerAsync(services, Race.Dwarf);

        _time.Advance(TimeSpan.FromHours(12));
        PlanetView? view = await Use<IPlanetViewService, PlanetView?>(services, planets => planets.GetOwnPlanetAsync(account, planetId, Cancellation));

        Assert.NotNull(view);
        // Half a day of base income on top of the starting stock (no buildings yet; dwarf ore x1.5).
        Assert.Equal(PlanetDefaults.StartingStock.Ore + 50 * 1.5 / 2, view.Stock.Ore, 6);
        Assert.Equal(7, view.ResourceBuildings.Count);
        Assert.Equal(OreMine.Rules.BuildCostFor(1), view.ResourceBuilding(BuildingType.OreMine).NextLevelCost);
        Assert.True(view.ResourceBuilding(BuildingType.OreMine).CanAffordNextLevel);
    }

    [Fact]
    public async Task Planets_and_commands_of_other_players_stay_invisible()
    {
        await using ServiceProvider services = CreateServices();
        (string owner, int planetId) = await JoinNewPlayerAsync(services, Race.Human);
        (string intruder, _) = await JoinNewPlayerAsync(services, Race.Orc, universeOf: planetId);

        CommandResult ownCommand = await Use<ICommandService, CommandResult>(services, commands => commands.RequestBuildAsync(owner, planetId, BuildingType.OreMine, Cancellation));

        Assert.Null(await Use<IPlanetViewService, PlanetView?>(services, planets => planets.GetOwnPlanetAsync(intruder, planetId, Cancellation)));
        Assert.Equal(CommandOutcome.NotFound, (await Use<ICommandService, CommandResult>(services, commands => commands.RequestBuildAsync(intruder, planetId, BuildingType.OreMine, Cancellation))).Outcome);
        Assert.Equal(CommandOutcome.NotFound, (await Use<ICommandService, CommandResult>(services, commands => commands.RequestProductionSpeedAsync(intruder, planetId, BuildingType.OreMine, 50, Cancellation))).Outcome);
        Assert.Null(await Use<ICommandService, CommandStatusView?>(services, commands => commands.GetStatusAsync(intruder, ownCommand.EventId!.Value, Cancellation)));
    }

    [Fact]
    public async Task A_build_command_is_recorded_for_the_engine_with_player_planet_and_tick()
    {
        await using ServiceProvider services = CreateServices();
        (string account, int planetId) = await JoinNewPlayerAsync(services, Race.Elf);
        _time.Advance(TimeSpan.FromSeconds(30));

        CommandResult result = await Use<ICommandService, CommandResult>(services, commands => commands.RequestBuildAsync(account, planetId, BuildingType.CrystalMine, Cancellation));

        Assert.Equal(CommandOutcome.Recorded, result.Outcome);
        await using GameDbContext database = databases.CreateGameContext();
        GameEvent recorded = await database.GameEvents.Include(e => e.Player).SingleAsync(e => e.Id == result.EventId, Cancellation);
        Assert.Equal(GameEventState.Requested, recorded.State);
        Assert.Equal(GameEventTypes.BuildBuilding, recorded.Type);
        Assert.Equal(planetId, recorded.PlanetId);
        Assert.Equal(account, recorded.Player!.AccountId);
        Assert.Equal(30, recorded.RequestedTick);
        Assert.Equal(BuildingType.CrystalMine, GameEventPayload.Deserialize<BuildBuildingPayload>(recorded.Payload).Building);

        CommandStatusView? status = await Use<ICommandService, CommandStatusView?>(services, commands => commands.GetStatusAsync(account, result.EventId!.Value, Cancellation));
        Assert.False(status!.IsDecided);
    }

    [Fact]
    public async Task Obvious_problems_are_refused_immediately_without_recording()
    {
        await using ServiceProvider services = CreateServices();
        (string account, int planetId) = await JoinNewPlayerAsync(services, Race.Troll);

        int foreignPlanetId = await OtherPlanetOfNewPlayerAsync(services);

        CommandResult first = await Use<ICommandService, CommandResult>(services, commands => commands.RequestBuildAsync(account, planetId, BuildingType.OreMine, Cancellation));
        CommandResult secondConstruction = await Use<ICommandService, CommandResult>(services, commands => commands.RequestBuildAsync(account, planetId, BuildingType.Factory, Cancellation));
        CommandResult onForeignPlanet = await Use<ICommandService, CommandResult>(services, commands => commands.RequestBuildAsync(account, foreignPlanetId, BuildingType.OreMine, Cancellation));
        CommandResult badSpeed = await Use<ICommandService, CommandResult>(services, commands => commands.RequestProductionSpeedAsync(account, planetId, BuildingType.OreMine, 55, Cancellation));

        Assert.Equal(CommandOutcome.Recorded, first.Outcome);
        Assert.Equal(CommandOutcome.Refused, secondConstruction.Outcome);
        Assert.Equal(GameEventMessages.QueueBusy, secondConstruction.Message);
        Assert.Equal(CommandOutcome.NotFound, onForeignPlanet.Outcome);
        Assert.Equal(CommandOutcome.Refused, badSpeed.Outcome);

        await using GameDbContext database = databases.CreateGameContext();
        Assert.Equal(1, await database.GameEvents.CountAsync(e => e.PlanetId == planetId, Cancellation));
    }

    [Fact]
    public async Task A_build_the_planet_cannot_afford_is_refused_with_the_missing_amounts()
    {
        await using ServiceProvider services = CreateServices();
        (string account, int planetId) = await JoinNewPlayerAsync(services, Race.Human);

        await using (GameDbContext database = databases.CreateGameContext())
        {
            await database.Planets.Where(p => p.Id == planetId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(p => p.Ore, 1.0), Cancellation);
        }

        CommandResult result = await Use<ICommandService, CommandResult>(services, commands => commands.RequestBuildAsync(account, planetId, BuildingType.OreMine, Cancellation));

        Assert.Equal(CommandOutcome.Refused, result.Outcome);
        Assert.Equal("Not enough resources: 9 ore missing.", result.Message);
    }

    private ServiceProvider CreateServices() => databases.CreateServices((services, configuration) =>
    {
        services.AddGalaxxisApplication(configuration);
        services.AddSingleton<TimeProvider>(_time);
    });

    /// <summary>Creates a running universe (or uses the universe of <paramref name="universeOf"/>) and lets a new account join.</summary>
    private async Task<(string Account, int PlanetId)> JoinNewPlayerAsync(ServiceProvider services, Race race, int? universeOf = null)
    {
        int universeId;
        if (universeOf is int planetId)
        {
            await using GameDbContext database = databases.CreateGameContext();
            universeId = await database.Planets.Where(p => p.Id == planetId).Select(p => p.UniverseId).SingleAsync(Cancellation);
        }
        else
        {
            UniverseSummary universe = await Use<IUniverseAdminService, UniverseSummary>(services, admin => admin.GenerateAsync(new GenerateUniverseRequest(TinyMap), Cancellation));
            await Use<IUniverseAdminService, UniverseSummary?>(services, admin => admin.ResumeAsync(universe.Id, Cancellation));
            universeId = universe.Id;
        }

        string account = Guid.NewGuid().ToString();
        JoinUniverseResult joined = await Use<IPlayerService, JoinUniverseResult>(services, players => players.JoinUniverseAsync(account, universeId, race, Cancellation));
        return (account, joined.HomePlanetId!.Value);
    }

    private async Task<int> OtherPlanetOfNewPlayerAsync(ServiceProvider services) => (await JoinNewPlayerAsync(services, Race.Human)).PlanetId;

    private static async Task<TResult> Use<TService, TResult>(ServiceProvider services, Func<TService, Task<TResult>> action)
        where TService : notnull
    {
        await using AsyncServiceScope scope = services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<TService>());
    }
}
