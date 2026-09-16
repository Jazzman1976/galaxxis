# Galaxxis.Application

The use cases of Galaxxis: what a player or an administrator can do.

## Purpose

The web UI and the tools never talk to the database or the game rules directly. They call the
services of this module, for example "submit a build command", "show my planet", "assign a starting
planet" or "generate a universe". Checks that protect the game, above all "does this planet really
belong to the current player?", happen here, in one central place.

Commands are only *recorded* here, as game events with state `Requested`. Whether a command is
accepted is decided by the engine, which is the only component that changes game state. The services
may run a quick pre-check purely to show an immediate message such as "not enough ore".

## Allowed dependencies

`Galaxxis.Domain`, `Galaxxis.Persistence`. Enforced by `tests/Galaxxis.Architecture.Tests`.

## Registration

```csharp
services.AddGalaxxisApplication();
```

## Structure

Organised by feature (`Buildings/`, `Players/`, `Universe/`, …). Services arrive in steps 3 and 5 of
the foundation refactoring. Later features (`Quests/`, `Trading/`) get their own folders.
