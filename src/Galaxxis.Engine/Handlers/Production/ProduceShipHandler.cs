using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Combat;
using Galaxxis.Domain.Races;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Ships;
using Galaxxis.Persistence.Game.Entities;
using Galaxxis.Persistence.Game.Events;

namespace Galaxxis.Engine.Handlers.Production;

/// <summary>
/// Produces one ship in the shipyard: pays the costs and starts the shipyard when accepted, adds the ship when the
/// production time has passed.
/// </summary>
/// <remarks>
/// The shipyard is an order building: it consumes supply resources only while it works on this order, so the
/// handler switches it to working on acceptance and back to idle on completion (after settling the stock to the
/// respective tick). One ship at a time per shipyard (queue <see cref="GameEventQueues.Shipyard"/>).
/// </remarks>
internal sealed class ProduceShipHandler : IGameEventHandler
{
    /// <inheritdoc />
    public string EventType => GameEventTypes.ProduceShip;

    /// <inheritdoc />
    public string? Queue => GameEventQueues.Shipyard;

    /// <inheritdoc />
    public async Task<AcceptResult> AcceptAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        ProduceShipPayload payload = context.ReadPayload<ProduceShipPayload>();

        ShipRules? rules = ShipCatalog.All.FirstOrDefault(ship => ship.Type == payload.Ship);
        if (rules is null)
        {
            return AcceptResult.Reject($"{payload.Ship} ships cannot be produced yet.");
        }

        Planet? planet = await context.LoadOwnedPlanetAsync(cancellationToken);
        if (planet is null)
        {
            return AcceptResult.Reject(GameEventMessages.PlanetNotOwned);
        }

        if (planet.Buildings.Shipyard.Level < 1)
        {
            return AcceptResult.Reject("A shipyard is needed to produce ships.");
        }

        // The owner's race decides whether the offensive or defensive variant is built.
        Strategy strategy = RaceProfiles.For(planet.Owner!.Race).Strategy;
        ResourceAmounts cost = rules.CostFor(strategy);

        if (!planet.Stock.Covers(cost))
        {
            return AcceptResult.Reject(GameEventMessages.NotEnoughResources(planet.Stock, cost));
        }

        planet.Stock -= cost;
        planet.Buildings = planet.Buildings.WithWorking(BuildingType.Shipyard, true);

        return AcceptResult.CompleteAfter(rules.ProductionDuration);
    }

    /// <inheritdoc />
    public async Task CompleteAsync(GameEventContext context, CancellationToken cancellationToken)
    {
        ProduceShipPayload payload = context.ReadPayload<ProduceShipPayload>();

        Planet planet = await context.LoadPlanetAsync(context.Event.PlanetId!.Value, cancellationToken)
            ?? throw new InvalidOperationException($"Planet {context.Event.PlanetId} of a ship production no longer exists.");

        // Settled to this tick: the shipyard consumed until now and stops consuming from here on.
        planet.Buildings = planet.Buildings.WithWorking(BuildingType.Shipyard, false);

        int playerId = context.Event.PlayerId
            ?? throw new InvalidOperationException("A ship production event must belong to a player.");
        Player player = await context.Database.Players.FindAsync([playerId], cancellationToken)
            ?? throw new InvalidOperationException($"Player {playerId} of a ship production no longer exists.");

        ShipRules rules = ShipCatalog.For(payload.Ship);
        Strategy strategy = RaceProfiles.For(player.Race).Strategy;
        CombatStats combat = rules.CombatFor(strategy);

        context.Database.Ships.Add(new Ship
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
            DriveStrength = rules.Travel.DriveStrength,
            ElementiumPerDistanceUnit = rules.Travel.ElementiumPerDistanceUnit,
            StorageCapacity = rules.Travel.StorageCapacity,
        });
    }
}
