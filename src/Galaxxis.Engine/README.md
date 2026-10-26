# Galaxxis.Engine

The heart of the game: universe time and event processing.

## Purpose

Every change to the game world (a building finishing, a ship being produced, a fleet arriving) is a game event
stored in the database. The engine is the **only** component that processes these events and therefore the only
component that changes game state. This single-writer design keeps the game consistent without complicated locking:
the web application only *records* commands, the engine decides and applies them.

## How it works

```
UniverseProcessorService (BackgroundService, wakes up every Engine:PollInterval)
 └─ EngineCycle.RunOnceAsync
     ├─ for each running (not paused) universe:
     │    └─ UniverseLease: SQL Server application lock "galaxxis-universe-{id}"
     │         held by this instance → process; held by another instance → stand by
     └─ UniverseEventProcessor: repeat "take the single next due event" (lowest tick, then lowest id)
          └─ GameEventExecutor: one event, own DI scope, own DbContext, own transaction
               ├─ Requested → queue busy? → handler.AcceptAsync → Rejected | Done | Scheduled (+duration)
               ├─ Scheduled → handler.CompleteAsync → Done (follow-up events saved in the same transaction)
               └─ any exception → rollback, event marked Failed (with the error), next event continues
```

| Rule | Why |
| --- | --- |
| **Universe time comes from the wall clock** (`Domain/Universes/UniverseTimeline`): tick = (now − epoch − pauses) / tick length | No drift under load, time runs while the server is down, only an explicit pause stops it |
| **Due** = `ProcessAtTick` ≤ current tick; empty ticks cost nothing | After an outage only the events that fell due are processed, not every tick |
| **Strict order**: always the earliest due event next | An accepted event can become due immediately; asking again each time keeps the order exact |
| **Planets are settled to the event's tick**, never to "now" | An event processed late still affects production from its own tick |
| **One transaction per event**; a rejection discards the handler's changes | Nothing half-done is ever saved; costs cannot be lost or duplicated |
| **Failure isolation**: a throwing handler or unreadable payload marks only that event `Failed` | One broken event can never stop a universe (the legacy engine retried it forever) |
| **Queues**: one scheduled event per planet and queue (construction, shipyard, defense system), checked by the engine and enforced by a unique index | Double clicks and parallel requests cannot start two orders |
| **Single instance per universe** via `sp_getapplock`, re-checked every cycle | Two servers never process the same universe; a second instance is a hot standby |
| **Backlog limit**: at most `Engine:MaxEventsPerUniversePerCycle` events per universe per wake-up | One busy universe cannot starve the others |

## Event types

| Type (`Persistence/Game/Events/GameEventTypes`) | Handler | Queue | Accept | Complete |
| --- | --- | --- | --- | --- |
| `BuildBuilding` | `Handlers/Buildings/BuildBuildingHandler` | Construction | pay costs, duration from rules | level + 1 |
| `ChangeProductionSpeed` | `Handlers/Buildings/ChangeProductionSpeedHandler` | – | set speed immediately | – |
| `ProduceShip` | `Handlers/Production/ProduceShipHandler` | Shipyard | shipyard ≥ 1, pay costs, shipyard works | add ship, shipyard idle |
| `ProduceDefense` | `Handlers/Production/ProduceDefenseHandler` | DefenseSystem | defense system ≥ 1, pay costs, works | add defense, idle |
| `FleetAttack` | `Handlers/Fleets/FleetAttackHandler` | – | validate and reserve ships | plunder target, schedule return |
| `FleetScan` | `Handlers/Fleets/FleetScanHandler` | – | validate and reserve ships | record target stock, schedule return |
| `FleetReturn` | `Handlers/Fleets/FleetReturnHandler` | – | never requested | add loot, release ships |

Payloads are JSON records in `Persistence/Game/Events/GameEventPayloads.cs`, shared with the Application module.
Fleet missions still use the legacy placeholders (fixed travel time, plunder without combat); see `docs/TODO.md`.

## Adding an event type (for example for quests or trading)

1. Add a type name and a payload record in `Galaxxis.Persistence/Game/Events` (or in the feature's own folder there).
2. Implement `IGameEventHandler` in `Galaxxis.Engine/Handlers/<Feature>/`. It is found automatically at startup.
   Use `GameEventContext` to read the payload, load and settle planets, and schedule follow-up events.
   Never call `SaveChanges`; the engine saves the event and everything the handler changed in one transaction.
3. System-triggered, recurring events (no player, no planet) schedule their next occurrence in `CompleteAsync`.
4. No change to the processor, the executor or the database schema is needed.
   `tests/Galaxxis.Engine.Tests/ExtensibilityTests.cs` proves this with a recurring event and a player-to-player transfer.

## Allowed dependencies

`Galaxxis.Domain`, `Galaxxis.Persistence`. Enforced by `tests/Galaxxis.Architecture.Tests`.

## Registration

```csharp
services.AddGalaxxisPersistence(configuration);
services.AddGalaxxisEngine(configuration);
```

Registers the handlers, the processing classes, the background service (starts with the host) and the health
check `engine`. The engine currently runs inside the web application. To run it as its own service (for example on
Azure), a new host only needs these two calls. A host that must not process events either does not call
`AddGalaxxisEngine` or sets `Engine:Enabled` to `false` (the web application keeps its health endpoint that way).

## Configuration

| Key | Meaning |
| --- | --- |
| `Engine:Enabled` | Whether this host processes events (default `true`). `false` for additional web instances that only serve players, and for web tests. Several enabled instances are safe: only one processes a universe at a time. |
| `Engine:PollInterval` | How often the engine looks for due events (100 ms to 1 min, default 1 s). Does not affect game time. |
| `Engine:MaxEventsPerUniversePerCycle` | Events per universe per wake-up before moving on to the next universe (default 1000). |
| `Logging:LogLevel:Galaxxis.Engine.TickLog` | `Debug` switches the tick log on (see below). The web application does this in `appsettings.Development.json`. |

## Tick log (watching the engine work)

`Processing/EngineTickLog` writes a running protocol at `Debug` level under its own logging category
`Galaxxis.Engine.TickLog`, so it costs nothing when switched off (production):

```
12:00:01.004 dbug: Galaxxis.Engine.TickLog[0] === Universe 1 "Andromeda" | tick 1234 ===
12:00:01.012 dbug: Galaxxis.Engine.TickLog[0]       planet 12 "Kepler" settled ticks 1100-1234 | stock Ore=5230, Energy=812, ...
12:00:01.020 dbug: Galaxxis.Engine.TickLog[0]     tick 1234 | event #17 BuildBuilding | player 3, planet 12 | Requested -> Scheduled | completes at tick 1259 | {"building":"OreMine"}
```

- **Tick line**: once per new tick of every running universe this instance processes. When the engine wakes up less
  often than ticks pass, the line shows a range (`ticks 1231-1234`); the events of those ticks are still processed,
  each at its own tick.
- **Event line**: every handled event with its state change and the completion tick, rejection reason or error.
  Written after the commit, so it never shows a rolled-back outcome.
- **Settled line**: every planet a handler loads, with its stock at the event's tick. It appears before the line of the
  event that loaded it. There is no production step per tick: resources are settled only when an event needs the
  planet, so production becomes visible here.

## Health check `engine`

Healthy when the engine is disabled in this host (`Engine:Enabled = false`); otherwise:
Healthy when a cycle completed recently and no universe has a backlog; Degraded while starting or when due events
pile up; Unhealthy when no cycle completed for ten poll intervals (at least one minute). The data includes the
processed universes, events in the last cycle and the last error.
