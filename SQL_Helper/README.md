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


## Live Activity Monitor (preview)

Select a database in the explorer and click **Live Activity Monitor**, then **Start**. In another application or SQL connection, execute a procedure against that database. The monitor captures module start/end, completed RPCs, procedure/ad-hoc statements and reported errors using a uniquely named server-level Extended Events session. Select an event to inspect SQL, session, client, login, duration, rows and the execution tree. Pause freezes the display while collection continues; Clear removes the local history; Stop removes the owned XE session. Close also attempts cleanup.

- SQL Server permissions: permission to create/alter/drop server event sessions and read XE DMVs (typically ALTER ANY EVENT SESSION and VIEW SERVER STATE on older SQL Server versions; SQL Server 2022+ has more granular XE permissions and VIEW SERVER PERFORMANCE STATE). Azure SQL Database is not supported by this server-level implementation.
- Uses a bounded in-memory ring buffer, polls every second, and retains at most 5,000 events in memory. Busy servers may lose events; warnings are shown where reported. Do not use as a complete audit log. SQL text may contain confidential values, and capture should be restricted to short diagnostic sessions.
- ScriptDom identifies direct statement targets and named references, including ordinary UPDATE/DELETE aliases. It does not prove a write committed, resolve every CTE/view/synonym/dynamic or cross-database reference, or expose exact before/after row values. Row count/duration are event-specific, not per-table. Module association can be incomplete with parallel/concurrent requests. RPC parameters are visible only when SQL Server provides them in the event text. No explicit transaction outcome tracking.
- If the application is killed or connectivity is lost, the XE session may remain running. An administrator can inspect sys.server_event_sessions and remove the exact SqlHelperLive_ session name shown by the monitor; do not remove unrelated sessions. Startup state is OFF.

### Manual verification (not yet executed)

1. Build on Windows with the .NET 8 SDK. Connect to a development SQL Server and select a test database.
2. Start monitoring; execute a test stored procedure with INSERT, UPDATE, DELETE and SELECT against disposable test tables in another connection.
3. Verify module and statement events, table target labels, time, rows, duration, SQL details and tree. Test nested procedures, alias updates/deletes, RPC and an error. Test rollback: captured statements must not be interpreted as committed writes.
4. Execute SQL against a different database; verify filtering. Verify pause/resume, search, Clear, Stop and closing the form.
5. Confirm the exact owned XE session is removed from sys.server_event_sessions after Stop/Close, and insufficient permissions produce a visible error.
