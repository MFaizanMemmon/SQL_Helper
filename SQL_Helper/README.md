# SQL Helper

Windows desktop tools for exploring SQL Server databases, finding tables and stored procedures, inspecting SQL, previewing/exporting results, generating sample data, and comparing schemas.

## Build and run

Requires Windows and the .NET 8 SDK/runtime.

```powershell
dotnet restore
dotnet build
dotnet run
```

## Workflows

- **Connect server:** type a server or choose a saved profile. Windows and SQL authentication are supported. Saved credentials are encrypted for the current Windows user; legacy records migrate when loaded.
- **Find tables / Track procedures:** search across accessible databases. Open a table preview or a procedure definition. Database-specific read failures are shown as partial results.
- **Run a procedure:** open its definition, choose Run procedure, and enter typed values. NULL, an empty string, and omission for a procedure default are separate choices. Table-valued parameters accept JSON arrays with properties matching the type's column names. Each result set and output/return values gets a tab.
- **Procedure X-Ray:** choose a database in the explorer, then enter a schema-qualified procedure name. Variables, static table references, related triggers, and HTTP literals are shown.
- **Generate sample data:** select a database/table and load its schema. Configure each writable column; numeric/date ranges use invariant formats, bit Format values specify the percentage true, and text custom formats use `#`, `A`, `a`, and `*`. Insertion is transactional and cancellable. Database defaults and existing FK tuples can be used.
- **Compare databases:** the left connection is development (desired schema); the right is production (destination). Inspect differences, filter/export the list, and generate SQL. Production-only objects are retained unless removals are explicitly enabled. Save or copy the script for review; the app never executes it automatically.

All windows resize and reflow for compact screens. Long content scrolls instead of becoming inaccessible. Per-monitor DPI awareness is enabled.

## Repeat verification

```powershell
# Offline regression checks
dotnet run -- --verify verification-results.txt

# Includes live localhost SQL Server checks; requires create-database permission
dotnet run -- --verify --with-sql verification-results.txt
```

The live suite creates and removes only uniquely named verification databases. It does not modify existing databases or saved connection settings.

See [FUNCTIONALITY_AUDIT.md](FUNCTIONALITY_AUDIT.md) for the completed audit, verification coverage, and explicit limits of automatic migration and sample generation.
