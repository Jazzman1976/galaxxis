namespace Galaxxis.Persistence.Game.Events;

/// <summary>
/// The names of all game event types. The engine finds the handler of an event by this name, and the application
/// uses it when recording a command.
/// </summary>
/// <remarks>
/// Names are stored in the database, so an existing name must never be renamed. New features add new names
/// (for example quests or trading) in their own class; nothing here has to change for that.
/// </remarks>
public static class GameEventTypes
{
    /// <summary>Upgrade a building to its next level. Payload: <see cref="BuildBuildingPayload"/>. Queue: <see cref="GameEventQueues.Construction"/>.</summary>
    public const string BuildBuilding = "BuildBuilding";

    /// <summary>Change the production speed of a resource building; takes effect immediately. Payload: <see cref="ChangeProductionSpeedPayload"/>.</summary>
    public const string ChangeProductionSpeed = "ChangeProductionSpeed";

    /// <summary>Produce one ship in the shipyard. Payload: <see cref="ProduceShipPayload"/>. Queue: <see cref="GameEventQueues.Shipyard"/>.</summary>
    public const string ProduceShip = "ProduceShip";

    /// <summary>Produce one defense in the defense system. Payload: <see cref="ProduceDefensePayload"/>. Queue: <see cref="GameEventQueues.DefenseSystem"/>.</summary>
    public const string ProduceDefense = "ProduceDefense";

    /// <summary>Send ships to plunder another planet. Payload: <see cref="FleetMissionPayload"/>.</summary>
    public const string FleetAttack = "FleetAttack";

    /// <summary>Send ships to look at another planet's stock. Payload: <see cref="FleetMissionPayload"/>.</summary>
    public const string FleetScan = "FleetScan";

    /// <summary>Ships coming home from a mission, possibly with loot. Created by the engine. Payload: <see cref="FleetReturnPayload"/>.</summary>
    public const string FleetReturn = "FleetReturn";
}

/// <summary>
/// The queues of a planet. Events of the same queue on the same planet run one after another: while one is
/// scheduled, the engine rejects further requests for that queue (and the database refuses a second one).
/// </summary>
public static class GameEventQueues
{
    /// <summary>One building upgrade at a time per planet.</summary>
    public const string Construction = "Construction";

    /// <summary>One ship at a time per shipyard.</summary>
    public const string Shipyard = "Shipyard";

    /// <summary>One defense at a time per defense system.</summary>
    public const string DefenseSystem = "DefenseSystem";
}
