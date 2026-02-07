using ADGV;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQL_Helper
{
    public partial class frmComparisionDatabase : Form
    {
        private AdvancedDataGridView dataGridView1;
        private DataTable _dtResults;
        private DataView _dvResults;

        public frmComparisionDatabase()
        {
            InitializeComponent();
            this.Text = "Connect Database";

            dataGridView1 = new AdvancedDataGridView
            {
                Dock = DockStyle.Fill
            };
            panel1.Controls.Add(dataGridView1);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Text = "Loading...";
            var dlg = new frmCompareDBConnection();
            dlg.ShowDialog();
            if (dlg.IsConnectSuccessfull)
            {

                string csTarget = DbConnectionHelper.TargetConnectionString;
                string csCompare = DbConnectionHelper.CompareToConnectionString;

                if (!string.IsNullOrEmpty(csTarget) && !string.IsNullOrEmpty(csCompare))
                {
                    var selectDbForm = new frmSelectCompareDatabase();
                    if (selectDbForm.ShowDialog() == DialogResult.OK)
                    {
                        string db1 = DbConnectionHelper.TargetDatabase;
                        string db2 = DbConnectionHelper.SourceDatabase;

                        if (!string.IsNullOrEmpty(db1) && !string.IsNullOrEmpty(db2))
                        {
                            CompareAllDatabases(csTarget, csCompare);
                        }
                        else
                        {
                            MessageBox.Show("Both databases must be selected.");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Connection was not established.");
                }

                this.Text = "Connect Database";
            }

        }

        private void CompareAllDatabases(string serverConn1, string serverConn2)
        {
            _dtResults = new DataTable();
            _dtResults.Columns.Add("Server");
            _dtResults.Columns.Add("Database");
            _dtResults.Columns.Add("Type");
            _dtResults.Columns.Add("Name");
            _dtResults.Columns.Add("Description");
            _dtResults.Columns.Add("SourceModifiedDate");
            _dtResults.Columns.Add("TargetModifiedDate");

            string sourceDb = DbConnectionHelper.SourceDatabase;
            string targetDb = DbConnectionHelper.TargetDatabase;

            string cs1 = ChangeDefaultDb(serverConn2, sourceDb); // Source
            string cs2 = ChangeDefaultDb(serverConn1, targetDb); // Target

            CompareDatabases(cs1, cs2, "Source", "Target", targetDb);

            _dvResults = new DataView(_dtResults);
            dataGridView1.DataSource = _dvResults;
        }

        private void CompareDatabases(
     string connStr1,
     string connStr2,
     string serverLabel1,
     string serverLabel2,
     string dbName)
        {
            var tablesDb1 = SafeGetTableList(connStr1);
            var tablesDb2 = SafeGetTableList(connStr2);

            string environmentName =
                serverLabel1.Equals("Target", StringComparison.OrdinalIgnoreCase)
                    ? "Production"
                    : "Development";

            string actualDbName =
                environmentName.Equals("Development", StringComparison.OrdinalIgnoreCase)
                    ? GetDatabaseNameFromConnectionString(connStr1)
                    : GetDatabaseNameFromConnectionString(connStr2);

            // --- Tables & Columns ---
            foreach (string table in tablesDb1)
            {
                if (!tablesDb2.Contains(table))
                {
                    _dtResults.Rows.Add(environmentName, actualDbName, "Table", table,
                        $"Table missing in {serverLabel2}", null, null);
                }
                else
                {
                    var colsDb1 = SafeGetColumnList(connStr1, table);
                    var colsDb2 = SafeGetColumnList(connStr2, table);

                    foreach (var col in colsDb1)
                        if (!colsDb2.Contains(col))
                            _dtResults.Rows.Add(environmentName, actualDbName, "Column",
                                $"{table}.{col}", $"Missing column in {serverLabel2}", null, null);

                    foreach (var col in colsDb2)
                        if (!colsDb1.Contains(col))
                            _dtResults.Rows.Add(environmentName, actualDbName, "Column",
                                $"{table}.{col}", $"Extra column in {serverLabel2}", null, null);
                }
            }

            foreach (string table in tablesDb2)
                if (!tablesDb1.Contains(table))
                    _dtResults.Rows.Add(environmentName, actualDbName, "Table", table,
                        $"Table missing in {environmentName}", null, null);

            // --- Table Types ---
            var ttDb1 = SafeGetTableTypes(connStr1);
            var ttDb2 = SafeGetTableTypes(connStr2);

            foreach (var tt in ttDb1)
                if (!ttDb2.Contains(tt))
                    _dtResults.Rows.Add(environmentName, actualDbName, "Table Type",
                        tt, $"Table type missing in {serverLabel2}", null, null);

            foreach (var tt in ttDb2)
                if (!ttDb1.Contains(tt))
                    _dtResults.Rows.Add(environmentName, actualDbName, "Table Type",
                        tt, $"Table type missing in {serverLabel1}", null, null);

            // --- Stored Procedures ---
            var spDb1 = SafeGetStoredProcedures(connStr1);
            var spDb2 = SafeGetStoredProcedures(connStr2);

            foreach (var sp in spDb1.Keys)
            {
                if (!spDb2.ContainsKey(sp))
                {
                    _dtResults.Rows.Add(environmentName, actualDbName, "Stored Procedure",
                        sp, $"Exists only in {environmentName}", spDb1[sp].ModifiedDate, null);
                }
                else
                {
                    bool defChanged =
                        NormalizeSql(spDb1[sp].Definition) !=
                        NormalizeSql(spDb2[sp].Definition);

                    bool dateChanged =
                        spDb2[sp].ModifiedDate > spDb1[sp].ModifiedDate;

                    if (defChanged || dateChanged)
                    {
                        string desc = defChanged
                            ? "Definition differs."
                            : "Source modified later than Target.";

                        _dtResults.Rows.Add(environmentName, actualDbName, "Stored Procedure",
                            sp, desc, spDb1[sp].ModifiedDate, spDb2[sp].ModifiedDate);
                    }
                }
            }

            foreach (var sp in spDb2.Keys)
                if (!spDb1.ContainsKey(sp))
                    _dtResults.Rows.Add(environmentName, actualDbName, "Stored Procedure",
                        sp, $"Exists only in {serverLabel2}", null, spDb2[sp].ModifiedDate);

            // --- Functions ---
            var fnDb1 = SafeGetFunctions(connStr1);
            var fnDb2 = SafeGetFunctions(connStr2);

            foreach (var fn in fnDb1.Keys)
            {
                if (!fnDb2.ContainsKey(fn))
                {
                    _dtResults.Rows.Add(environmentName, actualDbName, "Function",
                        fn, $"Exists only in {serverLabel1}", fnDb1[fn].ModifiedDate, null);
                }
                else
                {
                    bool defChanged =
                        NormalizeSql(fnDb1[fn].Definition) !=
                        NormalizeSql(fnDb2[fn].Definition);

                    bool dateChanged =
                        fnDb2[fn].ModifiedDate > fnDb1[fn].ModifiedDate;

                    if (defChanged || dateChanged)
                    {
                        var desc = new List<string>();
                        if (defChanged) desc.Add("Definition differs");
                        if (dateChanged) desc.Add("Source modified later than Target");

                        _dtResults.Rows.Add(environmentName, actualDbName, "Function",
                            fn, string.Join("; ", desc),
                            fnDb1[fn].ModifiedDate, fnDb2[fn].ModifiedDate);
                    }
                }
            }

            foreach (var fn in fnDb2.Keys)
                if (!fnDb1.ContainsKey(fn))
                    _dtResults.Rows.Add(environmentName, actualDbName, "Function",
                        fn, $"Exists only in {serverLabel2}", null, fnDb2[fn].ModifiedDate);
        }


        private HashSet<string> SafeGetTableTypes(string connectionString)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var conn = new SqlConnection(connectionString);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT name
        FROM sys.types
        WHERE is_table_type = 1
        ORDER BY name";

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                result.Add(reader.GetString(0));

            return result;
        }


        private string GetDatabaseNameFromConnectionString(string connectionString)  
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }



        private string NormalizeSql(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return string.Empty;

            return System.Text.RegularExpressions.Regex
                .Replace(sql, @"\s+", " ")
                .Trim()
                .ToUpperInvariant();
        }


        private List<string> SafeGetTableList(string connStr)
        {
            try
            {
                var list = new List<string>();
                using var conn = new SqlConnection(connStr);
                conn.Open();
                using var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) list.Add(reader.GetString(0));
                return list;
            }
            catch { return new List<string>(); }
        }

        private List<string> SafeGetColumnList(string connStr, string tableName)
        {
            try
            {
                var list = new List<string>();
                using var conn = new SqlConnection(connStr);
                conn.Open();
                using var cmd = new SqlCommand("SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName", conn);
                cmd.Parameters.AddWithValue("@TableName", tableName);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) list.Add(reader.GetString(0));
                return list;
            }
            catch { return new List<string>(); }
        }

        private Dictionary<string, (string Definition, DateTime? ModifiedDate)> SafeGetStoredProcedures(string connStr)
        {
            var dict = new Dictionary<string, (string, DateTime?)>();
            try
            {
                using var conn = new SqlConnection(connStr);
                conn.Open();
                using var cmd = new SqlCommand(@"SELECT p.name, m.definition, p.modify_date 
                                                 FROM sys.procedures p 
                                                 JOIN sys.sql_modules m ON p.object_id = m.object_id", conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    var def = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var modified = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);
                    dict[name] = (def, modified);
                }
            }
            catch { }
            return dict;
        }

        private Dictionary<string, (string Definition, DateTime? ModifiedDate)> SafeGetFunctions(string connStr)
        {
            var dict = new Dictionary<string, (string, DateTime?)>();

            try
            {
                using var conn = new SqlConnection(connStr);
                conn.Open();

                using var cmd = new SqlCommand(@"
            SELECT 
                o.name, 
                m.definition, 
                o.modify_date
            FROM sys.objects o
            JOIN sys.sql_modules m ON o.object_id = m.object_id
            WHERE o.type IN ('FN', 'IF', 'TF') -- Scalar, Inline Table-Valued, Table-Valued functions
        ", conn);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var name = reader.GetString(0);
                    var def = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var modified = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);

                    dict[name] = (def, modified);
                }
            }
            catch
            {
                // Optional: log error
            }

            return dict;
        }


        private string ChangeDefaultDb(string connStr, string databaseName)
        {
            var builder = new SqlConnectionStringBuilder(connStr)
            {
                InitialCatalog = databaseName
            };
            return builder.ConnectionString;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (_dvResults != null)
            {
                var ft = textBox1.Text.Replace("'", "''");
                _dvResults.RowFilter = $"Server LIKE '%{ft}%' OR Database LIKE '%{ft}%' OR Type LIKE '%{ft}%' OR Name LIKE '%{ft}%' OR Description LIKE '%{ft}%'";
            }
        }

        private void frmComparisionDatabase_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.ScrollBars = ScrollBars.Both;
            dataGridView1.AutoResizeColumns();
            dataGridView1.AllowUserToResizeColumns = true;
            dataGridView1.AllowUserToResizeRows = true;
        }

        private async void btnExportExcel_Click(object sender, EventArgs e)
        {
            await ExportDataAsync("xlsx", btnExportExcel);
        }

        private async Task ExportDataAsync(string fileType, Button btn)
        {
            if (dataGridView1.DataSource == null)
            {
                MessageBox.Show("No data to export.");
                return;
            }

            btn.Enabled = false;
            string originalText = btn.Text;
            btn.Text = "Loading...";

            try
            {
                await Task.Run(() =>
                {
                    var dt = GetDataTableFromDataGridView(dataGridView1);
                    ExportToExcel(dt);
                });

                MessageBox.Show($"{fileType.ToUpper()} export completed.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message);
            }
            finally
            {
                btn.Text = originalText;
                btn.Enabled = true;
            }
        }

        private DataTable GetDataTableFromDataGridView(DataGridView dgv)
        {
            if (dgv.DataSource is DataTable dt)
                return dt;

            dt = new DataTable();
            foreach (DataGridViewColumn column in dgv.Columns)
                dt.Columns.Add(column.HeaderText ?? column.Name);

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (!row.IsNewRow)
                {
                    var dr = dt.NewRow();
                    for (int i = 0; i < dgv.Columns.Count; i++)
                        dr[i] = row.Cells[i].Value ?? DBNull.Value;
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }

        private void ExportToExcel(DataTable dt)
        {
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string filePath = Path.Combine(downloadsPath, $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("ExportedData");
            worksheet.Cell(1, 1).InsertTable(dt);
            workbook.SaveAs(filePath);
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btnGenereateScript_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show(
                "This will generate scripts for all differences. It may take some time depending on the number of differences. Do you want to proceed?",
                "Generate Scripts",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            using (var frm = new frmGenerateScript())
            {
                frm.SourceGrid = dataGridView1;
                frm.ProductionDatabase = GetDatabaseNameFromConnectionString(DbConnectionHelper.TargetConnectionString);
                frm.DevelopmentDatabase = GetDatabaseNameFromConnectionString(DbConnectionHelper.CompareToConnectionString);
                frm.ProductionConnectionString = DbConnectionHelper.TargetConnectionString;
                frm.DevelopmentConnectionString = DbConnectionHelper.CompareToConnectionString;
                frm.ShowDialog(this);
            }
        }



        public class RoutineInfo
        {
            public string Definition { get; set; }
            public DateTime ModifiedDate { get; set; }
        }

    }
}
