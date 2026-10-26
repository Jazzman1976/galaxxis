# Galaxxis.Tools.LoadTest

Console tool that simulates many players (target: 1000) giving commands at the same time and measures how the
engine copes: processing time per event, backlog of due events, and correctness afterwards (no building upgraded
twice, no negative resource stocks, no failed or forgotten commands). Used for the acceptance of the foundation
refactoring (step 6); the results are in `docs/audits/`.

## Allowed dependencies

`Galaxxis.Application`, `Galaxxis.Engine`, `Galaxxis.Persistence` (and Domain through them). The tool is a host:
it wires the modules together like the web application and contains no game logic.

## Setup

Same as the universe generator: create `appsettings.Local.json` from the template in the repository root.
**Use separate databases for load tests**, so the development databases do not fill up with thousands of synthetic
players. Environment variables have the highest priority (PowerShell):

```powershell
$env:ConnectionStrings__Game = "Server=(localdb)\MSSQLLocalDB;Database=GalaxxisLoadTest;Trusted_Connection=True;TrustServerCertificate=True"
$env:ConnectionStrings__Accounts = "Server=(localdb)\MSSQLLocalDB;Database=GalaxxisLoadTestAccounts;Trusted_Connection=True;TrustServerCertificate=True"
$env:Persistence__ApplyMigrationsOnStartup = "true"   # creates the databases on the first run
```

Measure with a Release build (`-c Release`); Debug builds are noticeably slower.

## Usage

```
dotnet run --project src/Galaxxis.Tools.LoadTest -c Release -- help
dotnet run --project src/Galaxxis.Tools.LoadTest -c Release -- run --report loadtest.md
dotnet run --project src/Galaxxis.Tools.LoadTest -c Release -- run --players 200 --rate 20 --duration 00:01:00
dotnet run --project src/Galaxxis.Tools.LoadTest -c Release -- verify <universe id>
```

`run` without options is the acceptance test: 1000 players, 50 commands per second, 30 minutes, 20 % of the
planets not settled for 3 days. Ctrl+C ends the sending early; the run still catches up, checks and reports. The exit
code is 0 when all acceptance criteria are met.

## What `run` does

1. **World** (`LoadTestWorld`): generates a universe through `IUniverseAdminService`, moves its start a few days into
   the past, resumes it, and lets every synthetic account join through `IPlayerService` (accounts
   `loadtest-<universe>-<n>`; no login accounts are created). Then, as test preparation written directly to the
   database: all resource buildings at level 1, a large stock (constructions are limited by the queue, not by
   resources), and a share of the planets not settled for days.
2. **Players** (`PlayerSimulation`): at the given rate, a random player gives a command through `ICommandService`,
   exactly like the web application: 60 % "upgrade a resource building", 40 % "change a production speed". Each
   recorded command is followed with `GetStatusAsync` once per second, like the browser, until the engine decided.
3. **Engine**: runs inside the tool as in the web application (`AddGalaxxisEngine`). Its processing times are read
   from its metrics (`EngineMetrics`, meter `Galaxxis.Engine`) by `EngineMeasurements`. With `Engine:Enabled = false`
   another process (for example the running web application) processes, and only the backlog is measured.
4. **Backlog** (`BacklogMonitor`): once per second, the number of due events and the age of the oldest.
5. **Checks** (`InvariantChecks`) after the engine caught up: building levels equal starting level plus completed
   constructions, no negative stock (stored and settled to now), at most one scheduled order per planet and queue,
   no `Failed` events, no command left undecided.
6. **Report** (`LoadTestReport`): Markdown with the acceptance criteria, timings, backlog, event counts and checks,
   printed and optionally written to `--report <file>`. The universe is paused at the end.

## Failure drill "hard restart" with `verify`

1. `run --duration 00:05:00` and note the universe id from the first lines.
2. After a minute, kill the process hard (Task Manager or `Stop-Process -Name Galaxxis.Tools.LoadTest -Force`).
   The universe stays running in the database, with commands in every state.
3. Wait a few minutes, then `verify <id>`: the engine starts, takes the universe without an admin action, processes
   everything that fell due during the outage in tick order, and the checks run on the result.

## Configuration

Only the shared sections (`ConnectionStrings`, `Persistence`, `Engine`); the tool has no settings of its own.
`appsettings.json` keeps the log quiet during joining (`Galaxxis.Application.Players: Warning`).
