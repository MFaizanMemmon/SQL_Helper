using Microsoft.Data.SqlClient;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SQL_Helper;

internal sealed record ConnectionProfile(string Server, string Authentication, string UserName, string Password, bool TrustCertificate = false)
{
    internal bool Windows => Authentication.Equals("WINDOW AUTHENTICATION", StringComparison.OrdinalIgnoreCase) || Authentication.Equals("Windows", StringComparison.OrdinalIgnoreCase) || Authentication.Equals("Windows Authentication", StringComparison.OrdinalIgnoreCase);
    internal string ConnectionString
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Server)) throw new ArgumentException("Enter a server name.");
            if (!Windows && string.IsNullOrWhiteSpace(UserName)) throw new ArgumentException("Enter a SQL authentication username.");
            var builder = new SqlConnectionStringBuilder { DataSource = Server.Trim(), InitialCatalog = "master", IntegratedSecurity = Windows, TrustServerCertificate = TrustCertificate, ConnectTimeout = 15 };
            if (!Windows) { builder.UserID = UserName; builder.Password = Password; }
            return builder.ConnectionString;
        }
    }
    internal ConnectionProfile Normalized() => this with { Server = Server.Trim(), Authentication = Windows ? "WINDOW AUTHENTICATION" : "SQL SERVER AUTHENTICATION", UserName = Windows ? "" : UserName.Trim(), Password = Windows ? "" : Password };
    public override string ToString() => Server;
}

internal static class ConnectionStore
{
    internal static string Encode(ConnectionProfile profile) => "v2:" + Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(profile.Normalized())), null, DataProtectionScope.CurrentUser));
    internal static ConnectionProfile Decode(string value)
    {
        if (value.StartsWith("v2:", StringComparison.Ordinal)) return (JsonSerializer.Deserialize<ConnectionProfile>(ProtectedData.Unprotect(Convert.FromBase64String(value[3..]), null, DataProtectionScope.CurrentUser)) ?? throw new FormatException("Invalid saved connection.")).Normalized();
        var parts = value.Split('|', 4);
        if (parts.Length < 2) throw new FormatException("Invalid legacy saved connection.");
        return new ConnectionProfile(parts[0], parts[1], parts.Length > 2 ? parts[2] : "", parts.Length > 3 ? parts[3] : "").Normalized();
    }
    internal static List<ConnectionProfile> Load()
    {
        var saved = (Properties.Settings.Default.SavedConnections ?? new StringCollection()).Cast<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList();
        var profiles = saved.Select(Decode).ToList();
        if (saved.Any(v => !v.StartsWith("v2:", StringComparison.Ordinal))) Write(profiles);
        return profiles;
    }
    internal static void Write(IEnumerable<ConnectionProfile> profiles)
    {
        var encoded = profiles.Select(Encode).ToArray();
        var collection = new StringCollection(); collection.AddRange(encoded);
        Properties.Settings.Default.SavedConnections = collection; Properties.Settings.Default.Save();
    }
    internal static void Save(ConnectionProfile profile, int? index = null)
    {
        profile = profile.Normalized(); var list = Load();
        int existing = index ?? list.FindIndex(p => p.Server.Equals(profile.Server, StringComparison.OrdinalIgnoreCase) && p.Authentication == profile.Authentication && p.UserName.Equals(profile.UserName, StringComparison.OrdinalIgnoreCase));
        if (existing >= 0 && existing < list.Count) list[existing] = profile; else list.Add(profile);
        Write(list);
    }
}
