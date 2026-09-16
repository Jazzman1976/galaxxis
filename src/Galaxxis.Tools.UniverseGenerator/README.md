# Galaxxis.Tools.UniverseGenerator

Console tool to create new universes, pause and resume them, and create the administrator account.

The tool is a thin host: the work is done by services of `Galaxxis.Application`, so a web admin page
can later offer the same functionality.

## Setup

1. Copy `appsettings.Local.json.template` in the repository root to `appsettings.Local.json` and adjust it
   (connection strings, admin account). With `Persistence:ApplyMigrationsOnStartup` set to `true`, both
   databases are created automatically on the first run.
2. Build. The local settings file is copied next to the executable automatically.

## Usage

Run from the repository root. Without a command an interactive menu starts.

```
dotnet run --project src/Galaxxis.Tools.UniverseGenerator                       # interactive menu
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- bootstrap-admin
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- generate --name Andromeda --seed 42
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- list
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- resume 1
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- pause 1
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- help
```

| Command | Effect |
| --- | --- |
| `generate` | Generates a universe in one transaction. Options: `--name`, `--seed`, `--galaxies`, `--systems`, `--planets`, `--tick-length`. Defaults come from `UniverseGeneration` in `appsettings.json`. **The new universe starts paused.** |
| `list` | Shows all universes with state, size and seed. |
| `resume <id>` | Starts or continues universe time. |
| `pause <id>` | Stops universe time, e.g. for maintenance. Nothing is produced or completed while paused. |
| `bootstrap-admin` | Creates the admin account and role from `BootstrapAdmin` settings; safe to repeat. |

The same seed with the same map settings always generates the same map, even if a different name is chosen.
Exit code 0 means success, 1 an error (the message says what to fix).
