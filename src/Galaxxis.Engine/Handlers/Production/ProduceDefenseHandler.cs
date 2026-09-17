using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Combat;
using Galaxxis.Domain.Defenses;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;

namespace Galaxxis.Engine.Handlers.Production;

/// <summary>
/// Produces one defense in the defense system: pays the costs and starts the defense system when accepted, adds the
/// defense to the planet when the production time has passed.
/// </summary>
/// <remarks>
/// Works like <see cref="ProduceShipHandler"/>: the defense system is an order building that consumes supply
/// resources only while it works, one defense at a time (queue <see cref="GameEventQueues.DefenseSystem"/>).
/// </remarks>
internal sealed class ProduceDefenseHandler : IGameEventHandler
{
    /// <inheritdoc />
    public string EventType => GameEventTypes.ProduceDefense;

    /// <inheritdoc />
    public string? Queue => GameEventQueues.DefenseSystem;

    /// <inheritdoc />
    public async Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        ProduceDefensePayload payload = context.ReadPayload<ProduceDefensePayload>();

        DefenseRules? rules = DefenseCatalog.All.FirstOrDefault(defense => defense.Type == payload.Defense);
        if (rules is null)
        {
            return AcceptResult.Reject($"{payload.Defense} defenses cannot be produced yet.");
        }

        Planet? planet = await context.LoadOwnedPlanetAsync(cancellationToken);
        if (planet is null)
        {
            return AcceptResult.Reject("The planet does not exist or does not belong to you.");
        }

        if (planet.Buildings.DefenseSystem.Level < 1)
        {
            return AcceptResult.Reject("A defense system is needed to produce defenses.");
        }

        Strategy strategy = RaceProfiles.For(planet.Owner!.Race).Strategy;
        ResourceAmounts cost = rules.CostFor(strategy);

        if (!planet.Stock.Covers(cost))
        {
            return AcceptResult.Reject(GameEventContext.NotEnoughResourcesMessage(planet.Stock, cost));
        }

        planet.Stock -= cost;
        planet.Buildings = planet.Buildings.WithWorking(BuildingType.DefenseSystem, true);

        return AcceptResult.CompleteAfter(rules.ProductionDuration);
    }

    /// <inheritdoc />
    public async Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        ProduceDefensePayload payload = context.ReadPayload<ProduceDefensePayload>();

        Planet planet = await context.LoadPlanetAsync(context.Event.PlanetId!.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Planet {context.Event.PlanetId} of a defense production no longer exists.");

        planet.Buildings = planet.Buildings.WithWorking(BuildingType.DefenseSystem, false);

        int playerId = context.Event.PlayerId
            ?? throw new InvalidOperationException("A defense production event must belong to a player.");
        Player player = await context.Database.Players.FindAsync([playerId], cancellationToken)
            ?? throw new InvalidOperationException($"Player {playerId} of a defense production no longer exists.");

        DefenseRules rules = DefenseCatalog.For(payload.Defense);
        Strategy strategy = RaceProfiles.For(player.Race).Strategy;
        CombatStats combat = rules.CombatFor(strategy);

        context.Database.Defenses.Add(new Defense
        {
            PlayerId = playerId,
            PlanetId = planet.Id,
            Type = rules.Type,
            Strategy = strategy,
            WeaponSlots = combat.WeaponSlots,
            ArmorSlots = combat.ArmorSlots,
            PowerUpSlots = combat.PowerUpSlots,
            StructurePoints = combat.StructurePoints,
            CpuPower = combat.CpuPower,
            AccuracyPercent = combat.AccuracyPercent,
            ShieldPercent = combat.ShieldPercent,
            AttackPower = combat.AttackPower,
            Armor = combat.Armor,
        });
    }
}
