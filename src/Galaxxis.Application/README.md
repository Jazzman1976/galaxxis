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

Callers are responsible for permissions: only administrators may reach these services.
Player-facing services (commands, planet views, onboarding) arrive in step 5 of the foundation refactoring.

## Configuration

| Key | Meaning |
| --- | --- |
| `UniverseGeneration:TickLength` | Default real time per tick for new universes (100 ms to 1 day, default 1 s) |
| `UniverseGeneration:Map:*` | Default map size and density (`GalaxyCount`, `StarSystemsPerGalaxy`, `PlanetsPerStarSystem`, `CoordinateRange`, `MinimumDistance`, `OneMoonProbability`, `TwoMoonsProbability`) |
| `BootstrapAdmin:Email`, `UserName`, `Password`, `FirstName`, `LastName` | The admin account; validated only when the bootstrap runs |

## Structure

Organised by feature (`Universes/`, `Accounts/`, later `Buildings/`, `Players/`, `Quests/`, `Trading/`).
The system clock is injected as `TimeProvider`, so tests can control time.
