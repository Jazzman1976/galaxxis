# Tests

| Project | What it tests | Needs a database |
| --- | --- | --- |
| `Galaxxis.Domain.Tests` | Game rules and formulas, checked against the concept spreadsheet | No |
| `Galaxxis.Engine.Tests` | Event processing: failure isolation, double requests, restarts, extensibility | SQL Server LocalDB |
| `Galaxxis.Architecture.Tests` | Module dependency rules | No |

Run all tests:

```
dotnet test --solution src/Galaxxis.sln
```

The tests use xUnit v3 on Microsoft.Testing.Platform (configured in `global.json`).
