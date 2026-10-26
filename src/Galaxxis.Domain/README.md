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

## Structure

Organised by game feature, not by technical type:

| Folder | Content | Start reading at |
| --- | --- | --- |
| `Growth/` | The growth formula every cost, production and duration is based on | `GrowthRate` |
| `Resources/` | The seven resources, amounts of all resources at once, base income | `ResourceAmounts` |
| `Races/` | The six races: production factors, strategy, weapons, armor | `RaceProfiles` |
| `Buildings/` | One rule class per building, production speed, a planet's building state | `BuildingRules`, `OreMine` |
| `Economy/` | Daily production/consumption with shortage throttling; settling stocks over time | `PlanetEconomy`, `ResourceSettlement` |
| `Ships/`, `Defenses/`, `Combat/` | Costs and attributes of ships and defenses | `ShipCatalog`, `DefenseCatalog` |
| `Planets/` | Starting stock and plots of a new planet | `PlanetDefaults` |
| `Fleets/` | Mission travel time, plunder calculation | `FleetRules` |
| `Universes/` | Coordinates, universe clock, seeded and reproducible universe generation | `UniverseTimeline`, `Generation/UniverseGenerator` |

Later features (`Quests/`, `Trading/`, combat resolution) get their own folders.

## The economy in five sentences

Every planet receives a base income that nothing can reduce. Every building that works consumes supply
resources (energy, mana, antimatter, elementium): resource buildings work continuously and scale production
and consumption with the speed the player sets in 10 % steps; order buildings (shipyard, defense system,
later research) consume while an order is in progress. When a supply
resource is empty and demanded more than produced, all its consumers run proportionally slower. Stocks
are settled lazily with one formula per period instead of a loop per tick, and never become negative.
Because the base income always flows, a planet can always recover from a bad decision. Unclaimed planets produce
only their base income (no race factors, no buildings), so there is always something to plunder.

Time is expressed as `TimeSpan` (production per day, durations in seconds). `Universes/UniverseTimeline` is the
universe clock: it derives the current tick from the wall clock (minus pauses) and converts durations to ticks.
Fleet missions (`Fleets/FleetRules`) still use the legacy placeholders: fixed travel time, plunder without combat.

## Conventions

- Every number comes from the concept (`docs/concept/ConzeptExcel.fods` for values, `docs/concept/KonzeptWord.fodt`
  for rules) and carries a comment pointing to its source.
- Every public type and member has an XML documentation comment.
