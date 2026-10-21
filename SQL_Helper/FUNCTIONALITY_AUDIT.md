# Functionality audit and completion report

Scope: all forms, designer event bindings, SQL helpers, saved connections, schema comparison, scripts, data generation, execution, exports, cancellation, and dependencies. The previous responsive UI work is retained.

| Area | Findings at audit | Implemented result |
| --- | --- | --- |
| POS Worker | Empty legacy handler. | Removed from the designer and code as requested. |
| Procedure X-Ray | Unwired button; no analysis. | SQL parser extracts variables and schema-qualified references, excludes CTE aliases, detects HTTP literals, and loads related triggers from the selected database. Parse and permission errors are shown. |
| Sample data | Incomplete generators; mismatched columns/values; broken PK/FK logic; no atomicity. | Editable generator grid, typed parameters, numeric/date ranges, text formats, bit percentage, UUIDs/binary/XML, nullable/default values, numeric primary keys, and consistent composite FK tuples. Identity/computed/generated columns are omitted. Transactions roll back on failure or cancellation. |
| Saved connections | No editor binding; edits appended duplicates; inconsistent authentication; certificate option ignored; plaintext credentials. | Select/edit/new/delete flows, consistent authentication and certificate handling, async connection tests, encrypted Windows-user-bound storage, automatic legacy migration, preserved password whitespace/delimiters, and no password grid column. |
| Comparison connections | Typed servers ignored; blocking tests; inconsistent authentication. | Typed and saved servers work, both connections are tested asynchronously, and state is committed only after both succeed. |
| Database selection | Blocking loads and stale selection risk. | Asynchronous lists of accessible online user databases, disabled comparison until both selections are available, and explicit errors. |
| Table search | Schema lost on preview; wildcard failures; hidden database errors. | Schema-qualified previews, literal searches, cancellation, and visible partial-results messages. |
| Procedure tracking | Blocking metadata reads, silent failures, stale filters. | Asynchronous reads, cancellation between selections and on close, full-row selection, preserved search, and visible partial-results messages. |
| Definition / execution | Incomplete regex analysis; substring execution blocking; incomplete parameter handling. | AST-based references, async parameter discovery, typed validation, explicit NULL vs empty string vs procedure defaults, table-valued JSON parameters, and execution from the procedure definition window. |
| Result preview | Only first result; parameter metadata discarded. | Multiple result tabs, preserved parameter metadata, output/return values, filtering and exports for the current result, cancellation, and schema-aware table names. |
| Database comparison | Schema collisions, names-only table checks, omitted views/triggers, altered string literals, wrong direction. | Schema-qualified metadata comparison for columns, table/alias types, routines, views, triggers, keys, checks, FKs, and ordinary indexes. Literal-preserving module comparison. Development is the left connection; production is the right. Failures do not masquerade as empty schemas. |
| SQL generation | Wrong database routing, omitted object types, incomplete DDL, no save/copy. | Development-to-production scripts, schema/type/table creation, column additions/alterations, constraints, indexes, module dependencies, identity metadata, save/copy controls, optional production-only removals, and explicit blockers for unsupported migrations. Scripts are never run automatically. |
| Exports | Worker threads accessed grids; missing destination selection. | Shared CSV/Excel export, destination picker, UI-thread snapshots, filtered/current-result data, correct CSV quoting and newline handling. |
| Shared lifecycle / dependencies | Inconsistent cancellation/errors, obsolete packages, nullability warnings. | Operation cancellation, recoverable error feedback, unused ADGV/Excel Interop/System.Data.SqlClient removed, and a clean build with zero warnings. |

## Verification

The opt-in regression suite is `ProjectVerification.cs`; run `dotnet run -- --verify --with-sql verification-results.txt` to repeat the local SQL checks. Without `--with-sql`, only offline checks run.

SQL verification uses Windows authentication to localhost and creates two uniquely named `SqlHelper_Verify_<guid>_*` databases. Cleanup verifies the generated names before dropping only those databases. Existing databases and saved connection settings are not used by the suite.

Latest result: **32 checks passed**, including live SQL Server checks; verification databases were cleaned up successfully. The build completes with **0 warnings and 0 errors**. The separate UI pass completed **70 layout cases**.

The suite covers identifier escaping, SQL parsing and literal preservation, encrypted credential round trips, legacy data compatibility, typed parameter conversion, literal search, CSV/Excel round trips, generator formatting/ranges, unsupported-migration guards, live schema convergence, identity preservation, TVPs, multiple procedure results, output/return values, procedure defaults, atomic sample insertion, rollback after constraint failure, rollback after mid-insertion cancellation, composite foreign keys, and explicit destructive removals.

All 14 forms were also rendered at five window sizes: 1200x800 (subject to the current monitor work-area limit), 800x600, 420x640, 380x480, and 1366x768. The layout harness suppresses database/settings load handlers and checks resized controls for clipping outside non-scrollable containers. Actual mixed-DPI monitor movement still needs device testing.

## Explicit boundaries

These are reported to the user; they are not silently treated as completed migrations:

- Changing an existing identity, computed expression, or ROWGUIDCOL requires a deliberate data-preserving table rebuild.
- Replacing existing alias/table-type definitions requires migrating their dependent columns/parameters first.
- Temporal, memory-optimized, FILETABLE/FILESTREAM/generated-column tables, CLR types, and XML/spatial/columnstore indexes require specialized deployment logic.
- Encrypted or unreadable SQL definitions always require review, even when both sides are unreadable.
- This is an object/schema comparison tool, not a full database deployment engine for logins, permissions, jobs, replication, partition/storage settings, or data synchronization.
- Static SQL analysis cannot resolve runtime-generated dynamic SQL or infer every external API call.
- Sample generation uses existing FK values (up to 10,000 distinct referenced tuples), up to 100,000 inserted rows per operation. Cyclic/overlapping FK requirements and arbitrary business constraints may need explicit constants, defaults, or pre-seeded parent data; failed inserts roll back.
- CLR procedure parameter values require their provider-specific runtime types and cannot be supplied as plain text.

These boundaries prevent an unsafe or incomplete generated script from being presented as executable synchronization. The unresolved specialized cases are listed in the script instead of a partially executable migration.
