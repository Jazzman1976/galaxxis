# Galaxxis.Tools.UniverseGenerator

Console tool to create new universes and to bootstrap the administrator account.

The tool is a thin host: the work is done by services of `Galaxxis.Application`, so a web admin page
can later offer the same functionality.

## Setup

1. Copy `appsettings.Local.Template.json` in the repository root to `appsettings.Local.json` and adjust it.
2. Build. The local settings file is copied next to the executable automatically.

## Usage

The commands `generate`, `list`, `resume` and `bootstrap-admin`, plus an interactive mode, arrive in
step 3 of the foundation refactoring. Today the tool only validates its configuration:

```
dotnet run --project src/Galaxxis.Tools.UniverseGenerator
```
