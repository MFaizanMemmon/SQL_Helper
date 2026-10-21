using System.Data;
using ClosedXML.Excel;
using System.Text;
namespace SQL_Helper;
internal static class DataExport
{
    internal static async Task SaveAsync(Form owner, DataTable? table, string extension, Button button)
    {
        if (table == null || table.Columns.Count == 0) { MessageBox.Show(owner, "No data to export."); return; }
        using var picker = new SaveFileDialog { Filter = extension == "csv" ? "CSV files (*.csv)|*.csv" : "Excel workbook (*.xlsx)|*.xlsx", FileName = $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}", DefaultExt = extension, AddExtension = true };
        if (picker.ShowDialog(owner) != DialogResult.OK) return;
        var snapshot = table.Copy(); string path = picker.FileName; string title = button.Text; button.Enabled = false; button.Text = "Exporting…";
        try { await Task.Run(() => Write(snapshot, path, extension)); MessageBox.Show(owner, "Saved to " + path, "Export complete"); }
        catch (Exception ex) { SqlTools.Error(owner, ex); }
        finally { if (!button.IsDisposed) { button.Text = title; button.Enabled = true; } }
    }
    internal static void Write(DataTable table, string path, string extension)
    {
        if (extension == "xlsx") { using var book = new XLWorkbook(); book.Worksheets.Add(table, "ExportedData"); book.SaveAs(path); return; }
        static string Escape(object? value) => "\"" + (Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "").Replace("\"", "\"\"") + "\"";
        using var writer = new StreamWriter(path, false, new UTF8Encoding(true));
        writer.WriteLine(string.Join(",", table.Columns.Cast<DataColumn>().Select(c => Escape(c.ColumnName))));
        foreach (DataRow row in table.Rows) writer.WriteLine(string.Join(",", row.ItemArray.Select(Escape)));
    }
}
