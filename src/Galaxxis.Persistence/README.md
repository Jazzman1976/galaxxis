# Galaxxis.Persistence

All database access of Galaxxis, built with EF Core (Code-First).

## Purpose

Defines how game data and login accounts are stored. Other modules use the database contexts
provided here directly. There is deliberately no repository layer on top of EF Core: the `DbContext`
already is a unit of work, and an extra layer would only add indirection.

Two separate databases:

| Database | Context | Content |
| --- | --- | --- |
| `GalaxxisGame` | `Game/GameDbContext` | Universes, galaxies, star systems, planets, moons, players, ships, defenses, game events |
| `GalaxxisAccounts` | `Accounts/AccountDbContext` | Login accounts and roles (ASP.NET Core Identity) |

A player is linked to their login account by the account id (`Player.AccountId`), not by e-mail address.
Because the databases are separate, this link is not a database foreign key.

## Allowed dependencies

`Galaxxis.Domain`. Enforced by `tests/Galaxxis.Architecture.Tests`.

## Registration

```csharp
services.AddGalaxxisPersistence(configuration);   // both contexts, settings, startup migration
services.AddGalaxxisAccounts();                   // Identity user/role management (called by AddGalaxxisApplication)
```

| Service | Lifetime | Used by |
| --- | --- | --- |
| `GameDbContext` | scoped | web requests, application services |
| `IDbContextFactory<GameDbContext>` | singleton | the engine (one short-lived context per event) |
| `AccountDbContext` | scoped | Identity |

SQL Server connections retry transient errors automatically (important on Azure SQL). Code that opens an
explicit transaction must therefore run it inside `context.Database.CreateExecutionStrategy().ExecuteAsync(...)`.
A single `SaveChangesAsync` call is already one transaction and needs nothing extra.

## Configuration

| Key | Meaning |
| --- | --- |
| `ConnectionStrings:Game` | Game database (required) |
| `ConnectionStrings:Accounts` | Accounts database (required) |
| `Persistence:ApplyMigrationsOnStartup` | Apply pending migrations when a host starts (developer machines only) |

Missing or invalid values stop the host at startup with a readable message.

## How the schema is organised

- **Entities** (`Game/Entities`) are plain classes, one per table. Resources and building levels are ordinary
  columns (`Ore`, `OreMineLevel`, `OreMineSpeedPercent`, …) so the tables are easy to read in SQL. Code uses the
  grouped Domain views instead: `planet.Stock` (all resources) and `planet.Buildings` (all buildings).
- **Configurations** (`Game/Configurations`) hold column lengths, relationships and indexes, with a comment for
  every non-obvious choice. Enums are stored as text (`"Scheduled"`, `"Dwarf"`).
- **Deleting** cascades only along the map (universe → galaxy → star system → planet → moon). Players, ships,
  defenses and game events are never removed implicitly.
- **Game events** carry a free-text `Type` and a JSON `Payload`, so new features add event types without schema
  changes. A filtered unique index guarantees at most one *scheduled* event per planet and queue (for example
  one construction at a time), enforced by the database itself.

## Migrations: the principle

The database schema is defined by the entity classes and their configurations in this project.
Every schema change is recorded as a migration and checked in. On a developer machine migrations are applied
automatically at startup; for Azure an idempotent SQL script is generated and deployed.

The `dotnet ef` tool is installed as a local tool of the repository (`dotnet-tools.json`); run `dotnet tool restore`
once. All commands run from the repository root:

```
# after changing an entity or configuration of the game database
dotnet ef migrations add <DescriptiveName> --project src/Galaxxis.Persistence --startup-project src/Galaxxis.Persistence --context GameDbContext --output-dir Game/Migrations

# after changing the accounts model
dotnet ef migrations add <DescriptiveName> --project src/Galaxxis.Persistence --startup-project src/Galaxxis.Persistence --context AccountDbContext --output-dir Accounts/Migrations

# SQL script for a server (safe to run on any schema version)
dotnet ef migrations script --idempotent --project src/Galaxxis.Persistence --startup-project src/Galaxxis.Persistence --context GameDbContext
```

`tests/Galaxxis.Persistence.Tests` fails if an entity changed without a matching migration.
