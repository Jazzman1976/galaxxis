# Galaxxis.Tools.LoadTest

Console tool that simulates many players (target: 1000) submitting commands at the same time, and
measures how the engine copes: processing time per event, backlog of due events, and correctness
(no building upgraded twice, no negative resource stocks).

## Setup

Same as the universe generator: create `appsettings.Local.json` from the template in the repository root.

## Usage

The simulation arrives in step 6 of the foundation refactoring. Today the tool only validates its
configuration:

```
dotnet run --project src/Galaxxis.Tools.LoadTest
```
