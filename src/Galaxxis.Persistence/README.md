# Galaxxis.Persistence

All database access of Galaxxis, built with EF Core (Code-First).

## Purpose

Defines how game data and login accounts are stored. Other modules use the database contexts
provided here directly. There is deliberately no repository layer on top of EF Core: the `DbContext`
already is a unit of work, and an extra layer would only add indirection.

Two separate databases:

| Database | Context | Content |
| --- | --- | --- |
| `GalaxxisGame` | `GameDbContext` | Universes, galaxies, star systems, planets, moons, players, ships, defenses, game events |
| `GalaxxisAccounts` | `AccountDbContext` | Login accounts and roles (ASP.NET Core Identity) |

A player is linked to their login account by the account id, not by e-mail address.

## Allowed dependencies

`Galaxxis.Domain`. Enforced by `tests/Galaxxis.Architecture.Tests`.

## Registration

```csharp
services.AddGalaxxisPersistence(configuration);
```

## Configuration

| Key | Meaning |
| --- | --- |
| `ConnectionStrings:Game` | Game database (required) |
| `ConnectionStrings:Accounts` | Accounts database (required) |
| `Persistence:ApplyMigrationsOnStartup` | Apply pending migrations when a host starts (developer machines only) |

Missing or invalid values stop the host at startup with a readable message.

## Migrations: the principle

The database schema is defined by the entity classes and their configurations in this project.
Every schema change is recorded as a migration with `dotnet ef migrations add <Name> --context <Context>`
and checked in. On a developer machine migrations are applied automatically at startup; for Azure an
idempotent SQL script is generated with `dotnet ef migrations script --idempotent`.

(Contexts, entities and migrations arrive in step 3 of the foundation refactoring.)
