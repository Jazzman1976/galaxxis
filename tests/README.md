# Tests

| Project | What it tests | Needs a database |
| --- | --- | --- |
| `Galaxxis.Domain.Tests` | Game rules and formulas, checked against the concept spreadsheet | No |
| `Galaxxis.Persistence.Tests` | Migrations match the model; mappings, indexes and constraints | SQL Server LocalDB |
| `Galaxxis.Application.Tests` | Universe administration, admin bootstrap, entering a universe, map, planet view, recording commands | SQL Server LocalDB |
| `Galaxxis.Engine.Tests` | Event processing: order, failure isolation, double requests, outages, pauses, single instance, extensibility | SQL Server LocalDB |
| `Galaxxis.Web.Tests` | The web application over HTTP (in-memory server, engine disabled): sign-in, lockout, joining, commands, ownership (404), admin role, antiforgery, error pages | SQL Server LocalDB |
| `Galaxxis.Architecture.Tests` | Module dependency rules, configuration template | No |

Run all tests:

```
dotnet test --solution src/Galaxxis.sln
```

The tests use xUnit v3 on Microsoft.Testing.Platform (configured in `global.json`).

## Database tests

`Shared/TestDatabases.cs` (linked into each database test project) creates a fresh pair of databases with the
real migrations per test class and deletes them afterwards. It uses SQL Server LocalDB, which comes with
Visual Studio. To use another SQL Server with Windows authentication, set the environment variable
`GALAXXIS_TEST_SQLSERVER` to its server name, for example `GALAXXIS_TEST_SQLSERVER=localhost\SQLEXPRESS`.

Tests in one class share the databases, so each test works with the data it created itself (for example by id)
instead of counting everything in a table.
