# Galaxxis.Domain

The game rules of Galaxxis, and nothing else.

## Purpose

Everything that answers "how does the game work?" lives here: growth rates, build costs and
durations, resource production and consumption, race bonuses, and the algorithm that generates a
universe. The code is pure calculation: no database, no configuration, no web, no background
threads. That makes every rule easy to read, easy to test and usable from every other module.

## Allowed dependencies

None. Only the .NET base class library.
Enforced by `tests/Galaxxis.Architecture.Tests`.

## Registration

None. Domain classes are plain objects created with `new`; there are no services to register.

## Structure (grows with step 2 of the foundation refactoring)

Organised by game feature, not by technical type:

| Folder | Content |
| --- | --- |
| `Resources/` | Resource types, resource amounts, the O(1) resource settlement formula |
| `Buildings/` | One rule class per building (costs, durations, production, consumption) |
| `Races/` | Race bonuses and penalties |
| `Universe/Generation/` | Seeded, reproducible universe generation |

Later features (`Quests/`, `Trading/`, `Combat/`) get their own folders.

## Conventions

- Every number comes from the concept (`docs/ConzeptExcel.fods` for values, `docs/KonzeptWord.fodt`
  for rules) and carries a comment pointing to its source.
- Every public type and member has an XML documentation comment.
