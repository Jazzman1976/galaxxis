# Galaxxis.Web

The browser game: ASP.NET Core MVC with server-rendered Razor pages and a few scripts.

## Purpose

The web application is a **host**. It wires the modules together (`Program.cs`), signs players in and shows the
game. It contains no game logic: controllers call the services of `Galaxxis.Application`, which check ownership and
record commands; the engine decides on them. For now the engine runs inside this process as a background service.

## Allowed dependencies

`Galaxxis.Application` (all use cases), `Galaxxis.Domain` (display of resources, buildings, races),
`Galaxxis.Persistence` (registration, Identity account types) and `Galaxxis.Engine` (registration only).
Enforced by `tests/Galaxxis.Architecture.Tests`.

## Run

```
dotnet run --project src/Galaxxis.Web        # https://localhost:5001
```

Needs `appsettings.Local.json` in the repository root (copy of `appsettings.Local.json.template`). With
`Persistence:ApplyMigrationsOnStartup = true` both databases are created on the first start. A universe and the
admin account come from the generator tool (see the root `CLAUDE.md`, "Fresh installation").

## Game flow

```
Start (introduction / universe list)
 ├─ Account: Login, Register, Logout (POST), AccessDenied
 └─ Universe/Join/{universeId}    choose race → random free home planet (once per universe)
     └─ Universe/Index/{universeId} → Galaxy/Index/{galaxyId} → GalaxxisSystem/Index/{starSystemId}
         └─ Planet/Index/{planetId}                     resources, buildings, running constructions
             └─ Building/Index/{planetId}?type=OreMine  one page for all 7 resource buildings:
                                                        upgrade, production speed (10 % steps)
Admin (role "Admin")                                    universes: pause / resume
/health                                                 engine state for monitoring
```

## Structure: feature folders

Controller, view model and views of a feature live together in `Views/<Feature>/`
(for example `Views/Building/BuildingController.cs`, `BuildingViewModel.cs`, `Building.cshtml`). Views are returned
by explicit name (`View("Building", model)`). Shared layouts and partials are in `Views/Shared/`:

| File | Purpose |
| --- | --- |
| `_Game.cshtml` | Layout of the game pages (background, cockpit, interface panel, navigation) |
| `_Layout.cshtml` | Plain layout of the admin page |
| `_ResourceBar.cshtml` | The seven resources with live counting (`resource-bar.js`) |
| `_Countdown.cshtml` | Countdown until an order completes; reloads the page when it ends |

`Infrastructure/` holds the helpers used by several features: `User.AccountId()` and `GameTexts` (German names
and number formats).

## Rules

- **Secure by default.** Every page requires a signed-in account (fallback authorization policy); public pages are
  marked `[AllowAnonymous]`. Every POST needs the antiforgery token (global `AutoValidateAntiforgeryTokenAttribute`);
  forms created with tag helpers include it automatically. The admin page requires the `Admin` role.
- **The account comes from the sign-in, never from the request.** Controllers pass `User.AccountId()` to the
  Application services, which check ownership. Foreign planets and commands answer 404.
- **Commands are recorded, not executed.** A command form marked `data-command-form` is sent by `commands.js`,
  which shows immediate refusals and then polls `Building/Status/{eventId}` until the engine has decided. Without
  JavaScript the forms fall back to a normal post with a redirect.
- **Texts are German**, identifiers and comments English. Translations of game terms live in `GameTexts`.
  Rejection reasons coming from the engine are still English (see `docs/TODO.md`).

## Front-end build (SCSS and scripts)

Sources: `wwwroot/styles/src/**/*.scss` and `wwwroot/scripts/src/*.js`. The compiled files (`wwwroot/styles/main.css`,
`wwwroot/scripts/main.js` and their minified versions) are checked in, so building the .NET solution needs no Node.js.
After changing a source file, rebuild them:

```
cd src/Galaxxis.Web
npm install          # once
npx gulp all         # or "npx gulp" to rebuild on every change
```

## Configuration

| Key | Meaning |
| --- | --- |
| `ConnectionStrings:Game`, `ConnectionStrings:Accounts` | The two databases (from `appsettings.Local.json` or environment variables) |
| `Persistence:ApplyMigrationsOnStartup` | Create/migrate the databases on start (development) |
| `Engine:Enabled`, `Engine:PollInterval` | Event processing in this host (see `src/Galaxxis.Engine/README.md`) |

## IIS

`web.config` and the profile "IIS (DebugWebRoot)" run the site from the build output on a local IIS. Because the
engine runs in this process, the application pool must not idle out: set "Start Mode" to `AlwaysRunning` and
"Idle Time-out" to 0 (or enable preload), otherwise no events are processed while nobody is on the site. Universe
time is wall-clock based, so nothing is lost after a stop; events are processed as soon as the engine runs again.
