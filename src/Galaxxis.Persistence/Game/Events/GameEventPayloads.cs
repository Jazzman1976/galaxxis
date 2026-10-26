using System.Text.Json;
using System.Text.Json.Serialization;
using Galaxxis.Domain.Buildings;
using Galaxxis.Domain.Defenses;
using Galaxxis.Domain.Resources;
using Galaxxis.Domain.Ships;

namespace Galaxxis.Persistence.Game.Events;

/// <summary>Payload of <see cref="GameEventTypes.BuildBuilding"/>.</summary>
/// <param name="Building">The building to upgrade by one level.</param>
public sealed record BuildBuildingPayload(BuildingType Building);

/// <summary>Payload of <see cref="GameEventTypes.ChangeProductionSpeed"/>.</summary>
/// <param name="Building">The resource building.</param>
/// <param name="SpeedPercent">The new speed: a multiple of 10 between 0 and 100.</param>
public sealed record ChangeProductionSpeedPayload(BuildingType Building, int SpeedPercent);

/// <summary>Payload of <see cref="GameEventTypes.ProduceShip"/>.</summary>
/// <param name="Ship">The ship class to produce.</param>
public sealed record ProduceShipPayload(ShipType Ship);

/// <summary>Payload of <see cref="GameEventTypes.ProduceDefense"/>.</summary>
/// <param name="Defense">The defense class to produce.</param>
public sealed record ProduceDefensePayload(DefenseType Defense);

/// <summary>
/// Payload of <see cref="GameEventTypes.FleetAttack"/> and <see cref="GameEventTypes.FleetScan"/>.
/// The event's planet is the planet the fleet starts from.
/// </summary>
/// <param name="TargetPlanetId">The planet to attack or scan.</param>
/// <param name="ShipIds">The ships sent on the mission; they must be stationed at the start planet.</param>
/// <param name="Result">
/// Filled by the engine when the fleet arrives: the loot of an attack, or the stock seen by a scan.
/// Kept in the finished event as the mission report.
/// </param>
public sealed record FleetMissionPayload(int TargetPlanetId, IReadOnlyList<int> ShipIds, ResourceAmounts? Result = null);

/// <summary>Payload of <see cref="GameEventTypes.FleetReturn"/>. The event's planet is the planet the fleet returns to.</summary>
/// <param name="MissionEventId">The attack or scan event the ships were sent on.</param>
/// <param name="ShipIds">The returning ships.</param>
/// <param name="Loot">Resources the fleet brings home.</param>
public sealed record FleetReturnPayload(long MissionEventId, IReadOnlyList<int> ShipIds, ResourceAmounts Loot);

/// <summary>
/// Converts event payloads to and from the JSON text stored in <c>GameEvent.Payload</c>.
/// </summary>
/// <remarks>
/// Enums are written as text (<c>"OreMine"</c>), so stored events stay readable and do not break when enum members
/// are reordered. Unknown properties are ignored when reading, so older events remain readable after a payload
/// gains an optional property.
/// </remarks>
public static class GameEventPayload
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Converts a payload to JSON.</summary>
    /// <typeparam name="TPayload">The payload record type.</typeparam>
    /// <param name="payload">The payload.</param>
    /// <returns>The JSON text.</returns>
    public static string Serialize<TPayload>(TPayload payload) => JsonSerializer.Serialize(payload, Options);

    /// <summary>Reads a payload from JSON.</summary>
    /// <typeparam name="TPayload">The payload record type.</typeparam>
    /// <param name="json">The JSON text.</param>
    /// <returns>The payload.</returns>
    /// <exception cref="JsonException">The text is not valid JSON for this payload type.</exception>
    public static TPayload Deserialize<TPayload>(string json) =>
        JsonSerializer.Deserialize<TPayload>(json, Options)
        ?? throw new JsonException($"The event payload is empty; expected {typeof(TPayload).Name}.");
}
