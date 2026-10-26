# Galaxxis

Browser-based space strategy game (OGame-style): players own planets, upgrade buildings,
produce ships/defense and act against other players. ASP.NET Core MVC on .NET 10, server-rendered Razor with a
few scripts. Single developer hobby project, target ~1000 concurrent players, later on Azure.

## Status: foundation refactoring done (released as 0.1.0)

The base architecture was rebuilt in six steps and accepted on 2026-10-26. All code follows "Rules for new code" below.
The legacy projects (`Galaxxis.Core`, `Galaxxis.Data`, the old Home page) were removed in step 5; the git history
before that commit still contains them.

| Step | Content | State |
| --- | --- | --- |
| 1 | Foundation: new projects, central build/package settings, configuration template, tests, docs | done |
| 2 | Domain: pure game rules, O(1) resource settlement, seeded universe generation | done |
| 3 | Persistence: Code-First (`GameDbContext`, `AccountDbContext`), `UniverseGenerator` tool | done |
| 4 | Engine: wall-clock universe time, per-event transactions, handlers via DI, single-instance lock | done |
| 5 | Application services + web switch-over; remove Home, AddAdmin, `Galaxxis.Core`, `Galaxxis.Data` | done |
| 6 | Acceptance: load test (1000 players), failure drills, audit status | done |

Decisions: wall-clock universe time (runs while the server is down, explicit pause); the engine is
the only component that changes game state and decides whether a command is accepted; engine hosted
in the web process for now; two separate databases (game, accounts); fresh start without data
migration; multiple universes; quests and player trading must fit in later without changing the engine.
Full approved plan: `docs/plans/FOUNDATION-REFACTORING-PLAN.md`. Open work after the refactoring: `docs/plans/TODO.md`.
Findings that motivated it (with done status): `docs/audits/AUDIT-2026-09-16.md`. Acceptance results
(load test, failure drills): `docs/audits/ACCEPTANCE-2026-10-26.md`.

## Solution layout (`Galaxxis.slnx` in the repository root)

All projects target `net10.0`.

| Project | Role | May use |
| --- | --- | --- |
| `src/Galaxxis.Domain` | Game rules only: growth rates, buildings, resources, races, universe generation | nothing |
| `src/Galaxxis.Persistence` | EF Core Code-First: `GameDbContext`, `AccountDbContext`, migrations | Domain |
| `src/Galaxxis.Application` | Use cases for UI and tools; ownership checks; records commands | Domain, Persistence |
| `src/Galaxxis.Engine` | Universe clock, event processor, one handler per event type, background service | Domain, Persistence |
| `src/Galaxxis.Web` | Browser game (MVC host): sign-in, map, planet, building pages, admin; hosts the engine | Application, Domain, Persistence and Engine (registration) |
| `src/Galaxxis.Tools.UniverseGenerator` | Console: generate universes, bootstrap admin | Application, Persistence |
| `src/Galaxxis.Tools.LoadTest` | Console: simulate many players and measure the engine (`run`), check a universe after a restart (`verify`) | Application, Engine, Persistence |
| `tests/Galaxxis.Domain.Tests` | Rules vs. concept spreadsheet | |
| `tests/Galaxxis.Persistence.Tests` | Migrations match the model; mappings and constraints (LocalDB) | |
| `tests/Galaxxis.Application.Tests` | Application services against LocalDB | |
| `tests/Galaxxis.Engine.Tests` | Event processing against SQL Server LocalDB | |
| `tests/Galaxxis.Web.Tests` | Web application over HTTP (in-memory server): sign-in, join, commands, ownership, admin role, antiforgery | |
| `tests/Galaxxis.Architecture.Tests` | Enforces the "May use" column | |

Not part of the build: `src/DebugWebRoot` (empty root folder for a local IIS site), `docs/` (concept, plans, audits,
TODO, and `docs/archive/dummy-prototype`, the old static HTML/Vue prototype kept for reference; see below).

## Build, test, configure

```
dotnet tool restore                              # once: installs dotnet-ef from dotnet-tools.json
dotnet build                                     # in the repository root: builds Galaxxis.slnx
dotnet test                                      # xUnit v3 on Microsoft.Testing.Platform (global.json); needs LocalDB
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- help
dotnet run --project src/Galaxxis.Web            # the game, https://localhost:5001
cd src/Galaxxis.Web && npx gulp all              # only after changing SCSS/JS sources (compiled files are checked in)
```

**CI (GitHub Actions):** `.github/workflows/build.yml` builds (Release) and runs all tests on every push to
`main`/`develop` and on every pull request, against a SQL Server 2022 container on Linux (`GALAXXIS_TEST_SQLSERVER`,
see `tests/README.md`). Dependabot (`.github/dependabot.yml`) opens weekly NuGet and Actions updates against `develop`.
Repository: `https://github.com/Jazzman1976/galaxxis` (public), git-flow: `feature/*` → `develop` (pull request, CI must
pass) → `release/x.y.z` → `main` with an annotated tag.

**VS Code debugging:** `.vscode/launch.json` and `tasks.json` are checked in (the rest of `.vscode/` is ignored).
F5 configurations: `Galaxxis.Web` (logs in the Debug Console, opens the browser), `UniverseGenerator: Befehl`
(e.g. `bootstrap-admin`) and `UniverseGenerator: interaktives Menü` (integrated terminal). Needs the extensions
C# Dev Kit and C#.

**Fresh installation:** copy `appsettings.Local.json.template` to `appsettings.Local.json` and set
the admin password → `UniverseGenerator -- bootstrap-admin` (creates both databases via migrations on first run)
→ `UniverseGenerator -- generate` → `UniverseGenerator -- resume <id>` → start `Galaxxis.Web`, register, enter the
universe. Sign in with the bootstrap admin to reach `/admin`, where universes can also be generated, started and paused. Schema changes and migration commands:
`src/Galaxxis.Persistence/README.md`.

- `global.json` pins the .NET 10 SDK and the test runner.
- `Directory.Build.props` holds the shared settings of all projects (net10.0, nullable warnings as
  errors, XML documentation required). `Directory.Packages.props` holds **all** package versions;
  `.csproj` files reference packages without a version.
- **Configuration:** secrets never go into checked-in files. Copy `appsettings.Local.json.template`
  (repository root) to `appsettings.Local.json` (git-ignored); every host copies it next to its
  executable. Priority: host `appsettings.json` < `appsettings.Local.json` < environment variables <
  command line. Every settings class is validated at startup (`ValidateDataAnnotations().ValidateOnStart()`);
  invalid configuration stops the host with a readable message.
- **Nothing essential may exist only on one machine.** Every git-ignored file a program needs has a checked-in
  copy template `<original file name>.template` (e.g. `appsettings.Local.json.template`): secrets removed, every
  setting explained (purpose, requirements, allowed format, examples). Templates are never read by code;
  removing the `.template` suffix and adjusting the values must give a working file. When a new required setting
  or ignored file is added, extend the template in the same change. `Galaxxis.Architecture.Tests` checks that the
  template satisfies all startup validations, contains no password, and that every ignored file has a template.
  The same applies to plans and design decisions: they belong in `docs/`, not only in local notes.

**Where documents go.**
- **`docs/`** — everything decided together and meant for other contributors, sorted into subfolders
  (index: `docs/README.md`): `concept/` game design concept, `plans/` approved plans and the TODO list,
  `audits/` reviews and their findings. A new kind of document gets its own subfolder and a line in the index.
- **`.ignore/`** (repository root) — local-only working material: scratch notes, drafts, tool files that
  should not be shared (e.g. `.ignore/.github/`). The folder is ignored by the developer's global git
  excludes, not by the repository's `.gitignore`. Nothing in it may be needed to build, run or understand the
  project; once something there becomes a decision, it moves to `docs/`.

## Rules for new code

**Modules and dependencies.** Respect the "May use" column above; `Galaxxis.Architecture.Tests`
fails otherwise. The web project and tools are hosts: they wire modules together and contain no game logic.

**Web application** (details in `src/Galaxxis.Web/README.md`). Feature folders `Views/<Feature>/` hold controller,
view model and views together; views are returned by explicit name. Every page requires sign-in unless marked
`[AllowAnonymous]`; every POST is antiforgery-checked globally; controllers pass `User.AccountId()` to Application
services, which check ownership (foreign planets answer 404). Player texts are German (`Infrastructure/GameTexts`).
One generic `Building` page serves all resource buildings; new building UIs follow its command pattern
(`data-command-form` + status polling).

**Feature folders inside every module.** Organise by game feature (`Buildings/`, `Fleets/`,
`Universe/`, `Players/`), not by technical type. A new feature (for example `Quests/`, `Trading/`)
gets its own folder in each module it touches and should not require changes to existing features.

**Dependency injection.**
- Each module has one registration method: `AddGalaxxisPersistence(configuration)`,
  `AddGalaxxisApplication(configuration)`, `AddGalaxxisEngine(configuration)` in `<Module>Module.cs`. Domain has
  none. Persistence additionally offers `AddGalaxxisAccounts()` (Identity), which `AddGalaxxisApplication` calls.
  A module registers everything its services need, so a host cannot forget a dependency; database test fixtures
  build providers with `ValidateOnBuild` to catch gaps.
- Interfaces only for services that cross module boundaries or need a test double (application
  services, event handlers, clock). No interfaces for pure rule classes or records.
- No generic repository layer: use the `DbContext` directly (it is the unit of work).
- Lifetimes: `DbContext` scoped in web requests; `IDbContextFactory` in the engine (one context per
  event); services and handlers scoped; `TimeProvider` singleton.
- No static state: no static providers, no `static Instance` singletons, no static caches,
  no `Activator.CreateInstance`.

**Readability over DRY.** Repetition is fine when it keeps code obvious: one explicit handler class
per event type, one rule class per building. Only mechanical repetition (the same block for all seven
resources) is folded into a type such as `ResourceAmounts`.

**Comments: thorough, in English.** This deliberately overrides a minimal-comment style.
- Every public type and member has an XML doc comment (enforced: warning CS1591).
- Class summaries say what the class is for, who uses it, and which module it belongs to.
- Inline comments explain game rules and non-obvious decisions, referencing the concept
  (`// Concept: KonzeptWord §6 growth rates`, `// Concept: ConzeptExcel "Kosten und Erträge"`).
- Every project has a `README.md`: purpose, allowed dependencies, registration, configuration.
- Settings classes carry a `SectionName` constant and error messages that tell the reader how to fix the problem.

**Naming.** "Resource" is spelled correctly (`ResourceAmounts`); the legacy misspelling `Ressource` is gone.

**Game state changes only through events.** The web application and tools record commands as
`GameEvent` rows (`Requested`); only the engine accepts and applies them (see `src/Galaxxis.Engine/README.md`
for the processing rules and how to add an event type). Handlers never call `SaveChanges`, always settle
planets to the event's tick (`planet.SettleResourcesTo(tick, universe.Timeline)`), and schedule follow-ups through
`GameEventContext`. Durations in rules are `TimeSpan`; `UniverseTimeline` converts them to ticks.

**Extensibility for quests and trading.** Game events belong to a universe; player and planet are
optional. Event types are strings, handlers are discovered via DI. System-triggered recurring events
reschedule themselves inside their own transaction. Trades reserve goods when accepted and transfer
them when completed. `Galaxxis.Engine.Tests/ExtensibilityTests.cs` proves both work without engine changes.

## Game design concept (`docs/concept/`, German)

All three files are flat XML (OpenDocument / SpreadsheetML) — extract text with a script, they are
too verbose to read raw.

- **`ConzeptExcel.fods`** (2023) — **source of truth for all numbers**: base build/upkeep costs per
  building, income, growth-rate tables, ship and defense stats. The rule classes in `Galaxxis.Domain`
  (buildings, ships, defenses, `BaseIncome`) match this sheet.
- **`KonzeptWord.fodt`** (V1.1, 2015) — game design prose: six races with production bonuses and
  weapon/armor types, points and player levels, full combat rules (hit chance, shields, initiative,
  capture chance, debris), resource buildings. Its per-building income numbers are **older** than the
  spreadsheet — when they disagree, the spreadsheet wins.
- **`archive/ConzeptExcel.xml`** (2015) — earlier version of the spreadsheet, superseded.

Core formula (implemented in `Galaxxis.Domain/Growth/GrowthRate.ValueAt`):
`BASE * (FACTOR * (LEVEL - 1)^2 + 1)` with factor 1 = resources, 2 = upkeep, 3 = research,
4 = construction, 5 = build time. Base values: very few 5, few 10, normal 15, much 20, very much 25.

Resources: category I Ore/Energy/Tools, category II Mana/Crystals/Antimatter, category III Elementium.

**Economy rules not written down in the concept** (confirmed by the user 2026-09-16, implemented in
`Galaxxis.Domain/Economy`): **every building that works consumes supply resources** (energy, mana,
antimatter, elementium), and only while it works: resource buildings (mines etc.) continuously, order
buildings (shipyard, defense system, later research and processing) while an order is in progress. A new
building must define consumption values. The player sets each resource building's production speed in
10 % steps; consumption scales in the same ratio. Running everything at maximum is deliberately not
sustainable, to force specialisation and trading. When a supply resource is empty and demand exceeds
production, its consumers are throttled proportionally. The game never lowers a speed on its own, stocks
never go below 0, and the base income is never throttled, so a bad decision can never lock a player out.
Shipyard and DefenseSystem are order buildings. Unclaimed planets produce their base income (no race factors, no
buildings), so there is always something to plunder and plundered planets refill. (The removed legacy code differed:
auto-reduction loop, no throttling, production-building upkeep never charged.)

The concept is far larger than the implementation. Not yet built: combat resolution, points and
levels, trading, quests, research, equipment sockets, special buildings (storage, research lab,
sensor phalanx, bunker, construction base, terraformer). `ShipType` and `DefenseType`
list all classes of the concept, but only `Explorer`, `Carrier` and `LightManualCannon` have rules in the
catalogs. Web UIs for shipyard, defense system and fleets are still missing (`docs/plans/TODO.md`).

See `docs/audits/AUDIT-2026-09-16.md` for the audit findings that motivated the rebuild.
