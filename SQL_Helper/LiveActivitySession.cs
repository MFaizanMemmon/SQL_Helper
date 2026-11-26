using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using System.Xml.Linq;

namespace SQL_Helper;

internal sealed record LiveSqlEvent(long Sequence, DateTimeOffset Timestamp, string Event, int SessionId,
    int ObjectId, string Module, string Statement, long? DurationUs, long? RowCount, string Application, string Login);
internal sealed record LiveSqlBatch(IReadOnlyList<LiveSqlEvent> Events, string Warning);

/// <summary>Owns one bounded, database-filtered SQL Server XE session. No database tables are created.</summary>
internal sealed class LiveActivitySession
{
    private readonly string connection;
    private readonly string databaseConnection;
    private readonly string database;
    private readonly string name = "SqlHelperLive_" + Guid.NewGuid().ToString("N");
    private readonly Dictionary<int, string> modules = new();
    private readonly HashSet<long> seen = new();
    private readonly Queue<long> seenOrder = new();
    private bool owned;
    internal string SessionName => name;

    internal LiveActivitySession(string connectionString, string database)
    {
        this.database = database;
        var builder = new SqlConnectionStringBuilder(SqlTools.DatabaseConnection(connectionString, "master"))
        { ApplicationName = name, ConnectTimeout = 10 };
        connection = builder.ConnectionString;
        builder.InitialCatalog = database;
        databaseConnection = builder.ConnectionString;
    }

    internal async Task StartAsync()
    {
        if (owned) throw new InvalidOperationException("Stop the previous monitoring session first.");
        var db = await SqlTools.QueryAsync(connection, "SELECT DB_ID(@db)", default, new SqlParameter("@db", database));
        if (db.Rows.Count != 1 || db.Rows[0].IsNull(0)) throw new InvalidOperationException("Database is not accessible.");
        int id = Convert.ToInt32(db.Rows[0][0], CultureInfo.InvariantCulture);
        var objects = await SqlTools.QueryAsync(databaseConnection,
            "SELECT object_id, QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(name) FROM sys.objects WHERE type IN ('P','PC','TR','TA','FN','IF','TF','FS','FT')");
        foreach (DataRow row in objects.Rows) modules[(int)row[0]] = (string)row[1];

        string predicate = $"WHERE ([sqlserver].[database_id]=({id}) AND [sqlserver].[client_app_name]<>N'{name}')";
        string actions = "ACTION(package0.event_sequence,sqlserver.session_id,sqlserver.client_app_name,sqlserver.username)";
        string[] events = { "module_start", "module_end", "sp_statement_completed", "sql_statement_completed", "rpc_completed", "error_reported" };
        string sql = "CREATE EVENT SESSION " + SqlTools.Quote(name) + " ON SERVER\n"
            + string.Join(",\n", events.Select(e => $"ADD EVENT sqlserver.{e}({actions} {predicate})"))
            + "\nADD TARGET package0.ring_buffer(SET max_memory=(1024),max_events_limit=(2000))"
            + "\nWITH (MAX_MEMORY=4096 KB, EVENT_RETENTION_MODE=ALLOW_SINGLE_EVENT_LOSS, MAX_DISPATCH_LATENCY=1 SECONDS, STARTUP_STATE=OFF)";
        // Claim cleanup responsibility before sending CREATE: a network failure may occur after creation.
        owned = true;
        try
        {
            await ExecuteAsync(sql);
            await ExecuteAsync($"ALTER EVENT SESSION {SqlTools.Quote(name)} ON SERVER STATE=START");
        }
        catch (Exception startError)
        {
            try { await StopAsync(); }
            catch (Exception cleanupError) { throw new InvalidOperationException($"Start failed: {startError.Message}. Cleanup failed for {name}: {cleanupError.Message}", startError); }
            throw;
        }
    }

    private async Task ExecuteAsync(string sql)
    {
        using var con = new SqlConnection(connection);
        await con.OpenAsync();
        using var cmd = new SqlCommand(sql, con) { CommandTimeout = 15 };
        await cmd.ExecuteNonQueryAsync();
    }

    internal async Task<LiveSqlBatch> ReadAsync()
    {
        const string query = """
            SELECT CONVERT(nvarchar(max),t.target_data),s.dropped_event_count,s.dropped_buffer_count
            FROM sys.dm_xe_sessions s JOIN sys.dm_xe_session_targets t ON s.address=t.event_session_address
            WHERE s.name=@name AND t.target_name=N'ring_buffer'
            """;
        var data = await SqlTools.QueryAsync(connection, query, default, new SqlParameter("@name", name));
        if (data.Rows.Count == 0) throw new InvalidOperationException("The Extended Events session is no longer running.");
        var row = data.Rows[0];
        var root = XElement.Parse((string)row[0]);
        var events = new List<LiveSqlEvent>();
        foreach (var e in root.Elements("event"))
        {
            string Field(string field, string element = "data") =>
                e.Elements(element).FirstOrDefault(x => (string?)x.Attribute("name") == field)?.Element("value")?.Value ?? "";
            long? Number(string field, string element = "data") =>
                long.TryParse(Field(field, element), NumberStyles.Integer, CultureInfo.InvariantCulture, out long n) ? n : null;
            long? sequence = Number("event_sequence", "action");
            if (sequence is null) throw new InvalidOperationException("Event sequence is missing; cannot safely deduplicate the live stream.");
            if (!DateTimeOffset.TryParse((string?)e.Attribute("timestamp"), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var timestamp)) continue;
            if (!seen.Add(sequence.Value)) continue;
            seenOrder.Enqueue(sequence.Value);
            while (seenOrder.Count > 20000) seen.Remove(seenOrder.Dequeue());

            int objectId = (int)(Number("object_id") ?? 0);
            string module = modules.GetValueOrDefault(objectId) ?? Field("object_name");
            if (module.Length == 0 && objectId != 0) module = "Object #" + objectId;
            string statement = Field("statement");
            if (statement.Length == 0) statement = Field("batch_text");
            if (statement.Length == 0) statement = Field("message");
            events.Add(new LiveSqlEvent(sequence.Value, timestamp, (string?)e.Attribute("name") ?? "", (int)(Number("session_id", "action") ?? 0),
                objectId, module, statement, Number("duration"), Number("row_count"), Field("client_app_name", "action"), Field("username", "action")));
        }
        var warnings = new List<string>();
        if ((string?)root.Attribute("truncated") == "1") warnings.Add("Ring-buffer XML truncated");
        foreach (string attribute in new[] { "droppedCount", "dropped_count" })
            if (long.TryParse((string?)root.Attribute(attribute), out long n) && n > 0) warnings.Add($"Ring buffer reports {n:N0} dropped events");
        if (Convert.ToInt64(row[1]) > 0 || Convert.ToInt64(row[2]) > 0) warnings.Add("SQL Server reports dropped events/buffers");
        if (long.TryParse((string?)root.Attribute("totalEventsProcessed"), out long total)
            && long.TryParse((string?)root.Attribute("eventCount"), out long count) && total > count)
            warnings.Add("Older events have left the ring buffer; capture may be incomplete");
        return new LiveSqlBatch(events.OrderBy(e => e.Sequence).ToList(), string.Join(" • ", warnings));
    }

    internal async Task StopAsync()
    {
        if (!owned) return;
        // Only this randomly named session may be removed. Dropping a running session also stops it.
        await ExecuteAsync($"IF EXISTS (SELECT 1 FROM sys.server_event_sessions WHERE name=N'{name}') DROP EVENT SESSION {SqlTools.Quote(name)} ON SERVER");
        owned = false;
    }
}
