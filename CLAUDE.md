# Galaxxis

Browser-based space strategy game (OGame-style): players own planets, upgrade buildings,
produce ships/defense and act against other players. ASP.NET Core MVC on .NET 8, server-rendered
Razor with a few AJAX/Vue sprinkles. Single developer, early-stage hobby project.

## Solution layout (`src/Galaxxis.sln`)

| Project | Type | Role |
| --- | --- | --- |
| `Galaxxis.Data` | Console exe | EF Core model (DB-first, scaffolded), event/event-data classes, and a CLI universe seeder (`Program.cs`) |
| `Galaxxis.Core` | Library | Game rules: buildings, ships, defense, growth math, providers, tick engine |
| `Galaxxis.Web` | ASP.NET Core MVC | UI, controllers, Identity-based login |

Dependency direction: `Web` → `Core` → `Data`. Nothing references `Web`.

Not part of the build: `src/Dummy` (old static HTML/Vue prototype, reference only),
`src/Backup`, `src/DebugWebRoot`, `docs/` (game design concept, see below).

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

The concept is far larger than the implementation. Not yet built: combat resolution, points and
levels, trading, quests, research, equipment sockets, special buildings (storage, research lab,
sensor phalanx, bunker, construction base, terraformer). `ShipTypeEnum` and `DefenseTypeEnum`
already list all eight classes each, but only `Explorer`, `Carrier` and `LightManualCannon` have
classes.

## Two databases, two contexts — read this before touching data access

This is the single most surprising thing about the project.

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

## Setup from scratch

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
dotnet build src/Galaxxis.sln
dotnet run --project src/Galaxxis.Web        # https://localhost:5001 (default route → Start)
dotnet run --project src/Galaxxis.Data       # universe seeder CLI
cd src/Galaxxis.Web && npx gulp              # SCSS/JS build (gulp 4 + node-sass)
```

There is no test project and no CI pipeline.

## Conventions

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
