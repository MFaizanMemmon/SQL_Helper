using System.Text;
namespace SQL_Helper;
internal static class SqlIdentifier
{
    internal static (string Schema, string Name) Parse(string value)
    {
        var parts = new List<string>(); var part = new StringBuilder(); bool bracket = false;
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (bracket)
            {
                if (c == ']' && i + 1 < value.Length && value[i + 1] == ']') { part.Append(']'); i++; }
                else if (c == ']') bracket = false;
                else part.Append(c);
            }
            else if (c == '[') { if (part.ToString().Trim().Length != 0) throw new FormatException("Invalid SQL object name."); bracket = true; }
            else if (c == '.') { parts.Add(part.ToString().Trim()); part.Clear(); }
            else part.Append(c);
        }
        if (bracket) throw new FormatException("Unclosed bracket in SQL object name.");
        parts.Add(part.ToString().Trim());
        if (parts.Any(string.IsNullOrWhiteSpace) || parts.Count > 2) throw new FormatException("Use an object name or schema.object name.");
        return parts.Count == 1 ? ("dbo", parts[0]) : (parts[0], parts[1]);
    }
    internal static string Quote(string value) { var (schema, name) = Parse(value); return SqlTools.Qualify(schema, name); }
}
