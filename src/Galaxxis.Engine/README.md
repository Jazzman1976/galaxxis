# Galaxxis.Engine

The heart of the game: universe time and event processing.

## Purpose

Every change to the game world (a building finishing, a fleet arriving, a trade completing) is a
game event stored in the database. The engine is the **only** component that processes these events
and therefore the only component that changes game state. This single-writer design keeps the game
consistent without complicated locking.

How it works (implemented in step 4 of the foundation refactoring):

- **Universe time** is derived from the wall clock (`UniverseClock`), so it never drifts and keeps
  running while the server is down. Universes can be paused explicitly for maintenance.
- A background service wakes up every `Engine:PollInterval`, loads all due events in a fixed order
  and processes **each event in its own transaction**. A failing event is marked `Failed` and logged;
  it can never block the rest of the universe.
- Each event type has exactly one handler class. Handlers are found through dependency injection,
  so a new feature adds handlers without touching the engine.
- A database lock guarantees that only one engine instance processes a universe. A second instance
  waits as a hot standby.

## Allowed dependencies

`Galaxxis.Domain`, `Galaxxis.Persistence`. Enforced by `tests/Galaxxis.Architecture.Tests`.

## Registration

```csharp
services.AddGalaxxisEngine(configuration);
```

The engine currently runs inside the web application. To run it as its own service (for example on
Azure), a new host only needs `AddGalaxxisPersistence` and `AddGalaxxisEngine`.

## Configuration

| Key | Meaning |
| --- | --- |
| `Engine:PollInterval` | How often the engine looks for due events (100 ms to 1 min, default 1 s). Does not affect game time. |
