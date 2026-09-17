using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Engine.Handlers;
using Galaxxis.Engine.Processing;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;
using Galaxxis.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Galaxxis.Engine.Tests;

/// <summary>
/// Proves that the planned features fit into the engine without changing it: the handlers below exist only in this
/// test project, and neither the engine nor the database schema knows about them.
/// </summary>
public class ExtensibilityTests(TestDatabases databases) : IClassFixture<TestDatabases>
{
    [Fact]
    public async Task A_recurring_system_event_without_player_or_planet_reschedules_itself()
    {
        // Like spawning quests later: the game itself triggers the event every 10 minutes.
        await using EngineHarness world = await EngineHarness.CreateAsync(
            databases, services => services.AddScoped<IGameEventHandler, RecurringPulseHandler>());
        Universe universe = await world.CreateUniverseAsync();

        await world.RequestAsync(universe, RecurringPulseHandler.Type, new PulsePayload(Round: 1), playerId: null, planetId: null);
        world.Advance(TimeSpan.FromMinutes(35));
        await world.ProcessAsync(universe);

        List<GameEvent> pulses = await world.EventsAsync(universe);
        Assert.Equal(4, pulses.Count);                                              // the request and three follow-ups
        Assert.Equal([0L, 600L, 1200L, 1800L], pulses.Select(p => p.RequestedTick));
        Assert.Equal(3, pulses.Count(p => p.State == GameEventState.Done));
        Assert.Equal(GameEventState.Scheduled, pulses[^1].State);                    // the next pulse is waiting
        Assert.All(pulses, pulse => Assert.Null(pulse.PlayerId));
    }

    [Fact]
    public async Task A_transfer_between_two_players_reserves_goods_first_and_delivers_them_later()
    {
        // Like player trading later: goods are taken from the seller on acceptance and delivered on completion.
        await using EngineHarness world = await EngineHarness.CreateAsync(
            databases, services => services.AddScoped<IGameEventHandler, TransferHandler>());
        Universe universe = await world.CreateUniverseAsync();
        (Player seller, Planet sellerPlanet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Dwarf, new ResourceAmounts { Ore = 1000 });
        (_, Planet buyerPlanet) = await world.CreatePlayerWithPlanetAsync(universe, Race.Elf, ResourceAmounts.Zero);

        long transferId = await world.RequestAsync(universe, TransferHandler.Type, new TransferPayload(buyerPlanet.Id, 400), seller.Id, sellerPlanet.Id);
        await world.ProcessAsync(universe);

        Assert.Equal(GameEventState.Scheduled, (await world.EventAsync(transferId)).State);
        Assert.True((await world.PlanetAsync(sellerPlanet.Id)).Ore < 700, "The ore is reserved: gone from the seller while in transit.");
        Assert.True((await world.PlanetAsync(buyerPlanet.Id)).Ore < 1, "Nothing delivered yet.");

        world.Advance(TimeSpan.FromMinutes(5));
        await world.ProcessAsync(universe);

        Assert.Equal(GameEventState.Done, (await world.EventAsync(transferId)).State);
        Assert.True((await world.PlanetAsync(buyerPlanet.Id)).Ore >= 400, "The ore has been delivered.");
    }

    [Fact]
    public void Built_in_handlers_cover_every_event_type_exactly_once()
    {
        using ServiceProvider services = databases.CreateServices((collection, configuration) => collection.AddGalaxxisEngine(configuration));
        using IServiceScope scope = services.CreateScope();
        List<IGameEventHandler> handlers = scope.ServiceProvider.GetServices<IGameEventHandler>().ToList();

        GameEventHandlers.EnsureValid(handlers);

        IEnumerable<string> declaredTypes = typeof(GameEventTypes).GetFields()
            .Where(field => field.IsLiteral)
            .Select(field => (string)field.GetRawConstantValue()!);
        Assert.Equal(declaredTypes.Order(), handlers.Select(handler => handler.EventType).Order());
    }

    [Fact]
    public void A_second_handler_for_the_same_event_type_is_refused()
    {
        using ServiceProvider services = databases.CreateServices((collection, configuration) =>
        {
            collection.AddGalaxxisEngine(configuration);
            collection.AddScoped<IGameEventHandler, DuplicateBuildHandler>();
        });
        using IServiceScope scope = services.CreateScope();

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() =>
            GameEventHandlers.EnsureValid(scope.ServiceProvider.GetServices<IGameEventHandler>()));
        Assert.Contains(GameEventTypes.BuildBuilding, error.Message);
    }

    private sealed record PulsePayload(int Round);

    private sealed class RecurringPulseHandler : IGameEventHandler
    {
        public const string Type = "TestRecurringPulse";

        public string EventType => Type;

        public string? Queue => null;

        public Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken) =>
            Task.FromResult(AcceptResult.CompleteAfter(TimeSpan.FromMinutes(10)));

        public Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken)
        {
            // The next occurrence is saved in the same transaction as this one.
            int round = context.ReadPayload<PulsePayload>().Round;
            context.ScheduleFollowUp(Type, new PulsePayload(round + 1), TimeSpan.FromMinutes(10), playerId: null, planetId: null);
            return Task.CompletedTask;
        }
    }

    private sealed record TransferPayload(int ToPlanetId, double Ore);

    private sealed class TransferHandler : IGameEventHandler
    {
        public const string Type = "TestTransfer";

        public string EventType => Type;

        public string? Queue => null;

        public async Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken)
        {
            TransferPayload payload = context.ReadPayload<TransferPayload>();
            Planet? seller = await context.LoadOwnedPlanetAsync(cancellationToken);
            ResourceAmounts goods = new() { Ore = payload.Ore };

            if (seller is null || !seller.Stock.Covers(goods))
            {
                return AcceptResult.Reject("Cannot sell.");
            }

            seller.Stock -= goods;
            return AcceptResult.CompleteAfter(TimeSpan.FromMinutes(5));
        }

        public async Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken)
        {
            TransferPayload payload = context.ReadPayload<TransferPayload>();
            Planet buyer = (await context.LoadPlanetAsync(payload.ToPlanetId, cancellationToken))!;
            buyer.Stock += new ResourceAmounts { Ore = payload.Ore };
        }
    }

    private sealed class DuplicateBuildHandler : IGameEventHandler
    {
        public string EventType => GameEventTypes.BuildBuilding;

        public string? Queue => null;

        public Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken) =>
            Task.FromResult(AcceptResult.CompletedImmediately());

        public Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
