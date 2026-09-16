# Galaxxis

Browser-based space strategy game (OGame-style): players own planets, upgrade buildings,
produce ships/defense and act against other players. ASP.NET Core MVC, server-rendered Razor with a
few AJAX/Vue sprinkles. Single developer hobby project, target ~1000 concurrent players, later on Azure.

## Status: foundation refactoring in progress (branch `feature/stable-foundation`)

The base architecture is being rebuilt in six steps. **New code goes into the new modules and
follows "Rules for new code" below.** The legacy projects keep running unchanged until step 5
replaces them; the sections under "Legacy code" describe them.

| Step | Content | State |
| --- | --- | --- |
| 1 | Foundation: new projects, central build/package settings, configuration template, tests, docs | done |
| 2 | Domain: pure game rules, O(1) resource settlement, seeded universe generation | done |
| 3 | Persistence: Code-First (`GameDbContext`, `AccountDbContext`), `UniverseGenerator` tool | done |
| 4 | Engine: wall-clock universe time, per-event transactions, handlers via DI, single-instance lock | open |
| 5 | Application services + web switch-over; remove Home, AddAdmin, `Galaxxis.Core`, `Galaxxis.Data` | open |
| 6 | Acceptance: load test (1000 players), failure drills | open |

Decisions: wall-clock universe time (runs while the server is down, explicit pause); the engine is
the only component that changes game state and decides whether a command is accepted; engine hosted
in the web process for now; two separate databases (game, accounts); fresh start without data
migration; multiple universes; quests and player trading must fit in later without changing the engine.
Full approved plan: `docs/FOUNDATION-REFACTORING-PLAN.md`. Open work after the refactoring: `docs/TODO.md`.
Findings that motivated it: `docs/AUDIT-2026-09-16.md`.

## Solution layout (`src/Galaxxis.sln`)

New modules (`net10.0`):

| Project | Role | May use |
| --- | --- | --- |
| `src/Galaxxis.Domain` | Game rules only: growth rates, buildings, resources, races, universe generation | nothing |
| `src/Galaxxis.Persistence` | EF Core Code-First: `GameDbContext`, `AccountDbContext`, migrations | Domain |
| `src/Galaxxis.Application` | Use cases for UI and tools; ownership checks; records commands | Domain, Persistence |
| `src/Galaxxis.Engine` | Universe clock, event processor, one handler per event type, background service | Domain, Persistence |
| `src/Galaxxis.Tools.UniverseGenerator` | Console: generate universes, bootstrap admin | Application, Persistence |
| `src/Galaxxis.Tools.LoadTest` | Console: simulate many players, measure the engine | Application, Engine, Persistence |
| `tests/Galaxxis.Domain.Tests` | Rules vs. concept spreadsheet | |
| `tests/Galaxxis.Persistence.Tests` | Migrations match the model; mappings and constraints (LocalDB) | |
| `tests/Galaxxis.Application.Tests` | Application services against LocalDB | |
| `tests/Galaxxis.Engine.Tests` | Event processing against SQL Server LocalDB | |
| `tests/Galaxxis.Architecture.Tests` | Enforces the "May use" column | |

Legacy projects (`net8.0`, replaced in step 5):

| Project | Type | Role |
| --- | --- | --- |
| `Galaxxis.Data` | Console exe | EF Core model (DB-first, scaffolded), event/event-data classes, and a CLI universe seeder (`Program.cs`) |
| `Galaxxis.Core` | Library | Game rules: buildings, ships, defense, growth math, providers, tick engine |
| `Galaxxis.Web` | ASP.NET Core MVC | UI, controllers, Identity-based login |

Legacy dependency direction: `Web` → `Core` → `Data`. Nothing references `Web`.

Not part of the build: `src/Dummy` (old static HTML/Vue prototype, reference only),
`src/Backup`, `src/DebugWebRoot`, `docs/` (game design concept, see below).

## Build, test, configure

```
dotnet tool restore                              # once: installs dotnet-ef from dotnet-tools.json
dotnet build src/Galaxxis.sln
dotnet test --solution src/Galaxxis.sln          # xUnit v3 on Microsoft.Testing.Platform (global.json); needs LocalDB
dotnet run --project src/Galaxxis.Tools.UniverseGenerator -- help
```

**Fresh installation (new modules):** copy `appsettings.Local.json.template` to `appsettings.Local.json` and set
the admin password → `UniverseGenerator -- bootstrap-admin` (creates both databases via migrations on first run)
→ `UniverseGenerator -- generate` → `UniverseGenerator -- resume <id>`. Schema changes and migration commands:
`src/Galaxxis.Persistence/README.md`.

- `global.json` pins the .NET 10 SDK and the test runner.
- `Directory.Build.props` holds the shared settings of all new projects (net10.0, nullable warnings as
  errors, XML documentation required). `Directory.Packages.props` holds **all** package versions;
  `.csproj` files reference packages without a version. Both exclude the legacy projects.
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

## Rules for new code

**Modules and dependencies.** Respect the "May use" column above; `Galaxxis.Architecture.Tests`
fails otherwise. The web project and tools are hosts: they wire modules together and contain no game logic.

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

**Naming.** New code spells "resource" correctly (`ResourceAmounts`). The legacy misspelling
`Ressource` stays confined to the legacy projects.

**Extensibility for quests and trading.** Game events belong to a universe; player and planet are
optional. Event types are strings, handlers are discovered via DI. System-triggered recurring events
reschedule themselves inside their own transaction. Trades reserve goods when accepted and transfer
them when completed.

## Game design concept (`docs/`, German)

All three files are flat XML (OpenDocument / SpreadsheetML) — extract text with a script, they are
too verbose to read raw.

- **`ConzeptExcel.fods`** (2023) — **source of truth for all numbers**: base build/upkeep costs per
  building, income, growth-rate tables, ship and defense stats. `Static.Constants` and the building
  classes match this sheet.
- **`KonzeptWord.fodt`** (V1.1, 2015) — game design prose: six races with production bonuses and
  weapon/armor types, points and player levels, full combat rules (hit chance, shields, initiative,
  capture chance, debris), resource buildings. Its per-building income numbers are **older** than the
  spreadsheet — when they disagree, the spreadsheet wins.
- **`ConzeptExcel.xml`** (2015) — earlier version of the spreadsheet, superseded.

Core formula (implemented in `GrowthRate.Value`):
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
Shipyard and DefenseSystem are order buildings. The legacy code differs (auto-reduction loop, no
throttling, production-building upkeep never charged); the new rules win.

The concept is far larger than the implementation. Not yet built: combat resolution, points and
levels, trading, quests, research, equipment sockets, special buildings (storage, research lab,
sensor phalanx, bunker, construction base, terraformer). `ShipTypeEnum` and `DefenseTypeEnum`
already list all eight classes each, but only `Explorer`, `Carrier` and `LightManualCannon` have
classes.

# Legacy code (`Galaxxis.Core`, `Galaxxis.Data`, `Galaxxis.Web`)

Everything below describes the legacy projects as they are until step 5 of the foundation
refactoring. Do not extend them; they are kept only so the game keeps running during the rebuild.

## Legacy: two databases, two contexts — read this before touching legacy data access

This is the single most surprising thing about the legacy code.

1. **`GalaxxisIdentity`** — ASP.NET Identity (logins, passwords). Accessed through
   `IdentityAppContext`, registered via DI in `Startup.ConfigureServices`, connection string from
   `appsettings.json` → `ConnectionStrings:Identity`.
2. **`Galaxxis`** — the actual game world (users, planets, events, ships). Accessed through
   `GalaxxisContext`, which is **never registered in DI**. Every consumer does `new GalaxxisContext()`
   and relies on the hardcoded connection string in `GalaxxisContext.OnConfiguring`.

The link between the two is the **e-mail address**: `CurrentUserService.GetGalaxxisUserAsync()` reads
the Identity user, then looks up the game `User` by e-mail via `UserProvider.GetUser(email)`.
An Identity account without a matching game `User` row makes `UserProvider` throw
`UserNotFoundException` — several call sites do not handle this.

`GalaxxisContext` is scaffolded from the database, not code-first. Regenerate with the
`Scaffold-DbContext` command documented at the top of `Galaxxis.Data/Program.cs`; never hand-edit
`Models/*.cs` except the `*.custom.cs` partials. The Identity migrations in `Galaxxis.Web/Migrations`
are **excluded from compilation** (`<Compile Remove="Migrations\**" />`), so `dotnet ef database update`
does not work here.

## Legacy: setup from scratch

1. SQL Server instance; run `dist/db/create-galaxxis-db.sql` to create the `Galaxxis` game database.
2. Create the `GalaxxisIdentity` database (schema currently only exists inside `src/Backup/Databases.zip`).
3. Point both connection strings at it — `appsettings.json` for Identity,
   `GalaxxisContext.OnConfiguring` for the game DB.
4. Run `Galaxxis.Data` (console) and seed in order: `a` admin → `n` nobody → `u` universe →
   `g` galaxies → `s` systems → `p` planets → `m` moons.
5. Run `Galaxxis.Web`, visit `/account/addadmin` to create the Identity admin, then register/login.
6. **The universe is stopped by default.** Go to `/admin` and start the ticker, otherwise no
   resources accrue and no build finishes.

```
dotnet run --project src/Galaxxis.Web        # https://localhost:5001 (default route → Start)
dotnet run --project src/Galaxxis.Data       # universe seeder CLI
cd src/Galaxxis.Web && npx gulp              # SCSS/JS build (gulp 4 + node-sass)
```

There is no CI pipeline yet.

## Legacy: conventions

**Feature folders, not `Controllers/`.** Controller, view model and views live together in
`Galaxxis.Web/Views/<Feature>/` — e.g. `Views/OreMine/OreMineController.cs`,
`OreMineViewModel.cs`, `OreMine.cshtml`. Views are returned by explicit name
(`return View("OreMine", model)`) because the conventional lookup path does not match this layout.

**View models load their own data.** `<Feature>ViewModel` takes `ICurrentUserService` in its
constructor and resolves the current player and their planets itself; controllers stay thin.

**German-flavoured English identifiers.** The codebase consistently misspells "resource" as
`Ressource` (`BaseRessourceBuilding`, `UpdateRessources`, `RessourceGrowthRate`). Match the existing
spelling rather than fixing it piecemeal — a rename has to be all-or-nothing.

**Region-and-banner comment style.** Files use `#region Properties ----...----` banners padded to
column 95. Follow it in existing files.

## Game engine

```
Ticker (singleton, System.Timers.Timer)
  └─ every UniverseTimePerTick ms: Interlocked.Increment(_ticks) → Processor.ProcessAsync(tick)
       └─ Processor.Process: for each unprocessed tick
            ├─ drain EventQueue → Add_EventAction  (pays costs, writes Event row with absolute EventTick)
            └─ load due Events → Do_EventAction    (looks up IAction, executes, deletes Event row)
```

- **Queued events store a relative tick count; DB events store an absolute tick.** `Add_EventAction`
  converts by adding the current universe tick.
- **Costs are paid when the event is enqueued**, not when it completes.
- **Registering a new action is mandatory.** `Processor._actionStore` is a hand-maintained static
  list. An event whose `IAction` is missing there is silently skipped with a `Debug.WriteLine`
  warning. Its own comment says this should move to DI.
- `Ticker.Ticks` is the universe clock. Planets store `PlanetDataTick` (last time resources were
  settled); `Planet.UpdateRessources()` replays the gap **one loop iteration per tick** and mutates
  the instance without saving — the caller decides whether to persist.

## Building hierarchy

```
IBuilding
└─ BaseBuilding<TBuildEvent>                                 Build(), CanUpgradeToNextLevel, IsUnderConstruction
   ├─ BaseRessourceBuilding<TBuildEvent, TChangeUsageEvent>  ChangeUsage(), IncomePerTick, consumption rates
   │  └─ OreMine, PowerSupply, Factory, MageAcademy, CrystalMine, NeutronAccelerator, ElementiumSynthesizer
   └─ BaseProducingBuilding<TBuildEvent>
      └─ Shipyard, DefenseSystem
```

A concrete building only declares `GrowthRate` properties built from `Static.Constants`
(base value × factor × level) plus a race modifier. Build/usage logic lives entirely in the base
classes; `BuildingsHelper.BuildOnPlanet<TBuildEvent>` instantiates the event via `Activator.CreateInstance`,
so the event type's constructor signature `(EventLocationEnum, int, long, IEventTypeData)` is load-bearing.

Buildings are reached through extension methods on `Planet`:
`planet.GetOreMine()`, `planet.GetShipyard()`, … defined in
`Core/Extensions/PlanetExtensions/PlanetCache.*.cs`. These are backed by **static, process-wide
`Dictionary<int, TBuilding>` caches** keyed by planet id. Pass `forceReload: true` whenever the
planet instance you hold must be the one the building mutates — this matters for partial-view
rendering (see `OreMineController.BuildOreMine`) and is a frequent source of stale-data bugs.

## Two UI generations coexist

- **`HomeController` + `Home.cshtml`** — scaffolding from the earliest phase, built to make all game
  logic visible on one page. **Slated for complete removal; do not extend or harden it.** The real
  game flow from login is `Start` → `Universe` → `Galaxy` → `GalaxxisSystem` → `Planet` → building
  controllers; `Home` is only still reachable via the header link in `_Layout.cshtml`, a link in
  `NewPlayer.cshtml` and the `Logout` redirect. One POST action per building, no ownership checks,
  no antiforgery validation.
- **Per-building controllers** (`OreMine`, `CrystalMine`, `Factory`, `PowerSupply`,
  `NeutronAccelerator`) — `[Authorize]` + `[ValidateAntiForgeryToken]`, view model scopes the planet
  to the current user. `OreMine` is the furthest along and is the template to copy: it returns JSON
  with pre-rendered partials (`_OreMineBuildPanel`, `_ResourceBar`) for AJAX updates; the others
  still PRP-redirect.

New building UI work should follow the OreMine pattern. `MageAcademy`, `ElementiumSynthesizer`,
`Shipyard` and `DefenseSystem` have no dedicated controller yet and are only reachable via `Home`.

## Known sharp edges

- `Ticker.Instance` and `Processor.Instance` are lazy singletons **without locking**; concurrent
  first access can construct two instances and lose queued events.
- `PlanetCache` dictionaries are never evicted and not thread-safe; they hold entity instances from
  disposed contexts across requests.
- `PlanetProvider.FindRandomNobodyPlanetInUniverse` hardcodes `PlanetUserId == 2` as the
  "unowned" marker (user id 1 = admin). There is a TODO in `Galaxxis.Data/Program.cs` to replace
  this with explicit flags.
- Resource deduction happens twice by design: optimistically in memory in `BuildingsHelper` (so the
  UI updates immediately) and authoritatively in `Processor.Add_EventAction`. The in-memory one must
  never be saved to a live context.
- Hardcoded credentials in this repo are deliberate fresh-install scaffolding, not leftover debug
  code. When replacing them, move the value into configuration **and** check in a template file with
  placeholders, so a fresh install stays reproducible.

See `docs/AUDIT-2026-09-16.md` for the current findings list and priorities.
