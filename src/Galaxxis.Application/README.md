# Galaxxis.Application

The use cases of Galaxxis: what a player or an administrator can do.

## Purpose

The web UI and the tools never talk to the database or the game rules directly. They call the
services of this module, for example "generate a universe", "submit a build command", "show my planet" or
"assign a starting planet". Checks that protect the game, above all "does this planet really belong to the
current player?", happen here, in one central place.

Commands are only *recorded* here, as game events with state `Requested`. Whether a command is
accepted is decided by the engine, which is the only component that changes game state. The services
may run a quick pre-check purely to show an immediate message such as "not enough ore".

## Allowed dependencies

`Galaxxis.Domain`, `Galaxxis.Persistence`. Enforced by `tests/Galaxxis.Architecture.Tests`.

## Registration

```csharp
services.AddGalaxxisPersistence(configuration);
services.AddGalaxxisApplication(configuration);   // also registers account management
```

## Services

| Service | What it does |
| --- | --- |
| `Universes/IUniverseAdminService` | Generate a universe (one transaction, starts paused), list, pause, resume |
| `Accounts/IAdminAccountBootstrapper` | Create the first admin account and role from configuration; safe to repeat |
| `Players/IPlayerService` | List universes for an account; enter a universe (creates the player, assigns a random free home planet exactly once) |
| `Map/IMapService` | Universe, galaxy and star system maps, marking places with own planets |
| `Planets/IPlanetViewService` | An own planet as of now: stock, net rates, resource buildings, pending orders (settled for display only, never saved) |
| `Commands/ICommandService` | Record build and production speed commands as `Requested` events after quick pre-checks; report their status |

The administration services do not check permissions: callers (admin page, generator tool) must allow only
administrators. All player services take the signed-in `accountId` and check ownership themselves; planets and
commands of other players are reported as not found.

## Configuration

| Key | Meaning |
| --- | --- |
| `UniverseGeneration:TickLength` | Default real time per tick for new universes (100 ms to 1 day, default 1 s) |
| `UniverseGeneration:Map:*` | Default map size and density (`GalaxyCount`, `StarSystemsPerGalaxy`, `PlanetsPerStarSystem`, `CoordinateRange`, `MinimumDistance`, `OneMoonProbability`, `TwoMoonsProbability`) |
| `BootstrapAdmin:Email`, `UserName`, `Password`, `FirstName`, `LastName` | The admin account; validated only when the bootstrap runs |

## Structure

Organised by feature (`Universes/`, `Accounts/`, `Players/`, `Map/`, `Planets/`, `Commands/`; later `Quests/`, `Trading/`).
The system clock is injected as `TimeProvider`, so tests can control time.
