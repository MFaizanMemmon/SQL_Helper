using ADGV;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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

        private void CompareDatabases(string connStr1, string connStr2, string serverLabel1, string serverLabel2, string dbName)
        {
            var tablesDb1 = SafeGetTableList(connStr1);
            var tablesDb2 = SafeGetTableList(connStr2);

            foreach (string table in tablesDb1)
            {
                if (!tablesDb2.Contains(table))
                {
                    _dtResults.Rows.Add(serverLabel1, dbName, "Table", table, $"Table missing in {serverLabel2}", null, null);
                }
                else
                {
                    var colsDb1 = SafeGetColumnList(connStr1, table);
                    var colsDb2 = SafeGetColumnList(connStr2, table);

                    foreach (var col in colsDb1)
                        if (!colsDb2.Contains(col))
                            _dtResults.Rows.Add(serverLabel1, dbName, "Column", $"{table}.{col}", $"Missing column in {serverLabel2}", null, null);

                    foreach (var col in colsDb2)
                        if (!colsDb1.Contains(col))
                            _dtResults.Rows.Add(serverLabel2, dbName, "Column", $"{table}.{col}", $"Extra column in {serverLabel2}", null, null);
                }
            }

            foreach (string table in tablesDb2)
                if (!tablesDb1.Contains(table))
                    _dtResults.Rows.Add(serverLabel2, dbName, "Table", table, $"Table missing in {serverLabel1}", null, null);

            var spDb1 = SafeGetStoredProcedures(connStr1);
            var spDb2 = SafeGetStoredProcedures(connStr2);

            foreach (var sp in spDb1.Keys)
            {
                if (!spDb2.ContainsKey(sp))
                {
                    _dtResults.Rows.Add(serverLabel1, dbName, "Stored Procedure", sp, $"Exists only in {serverLabel1}", spDb1[sp].ModifiedDate, null);
                }
                else
                {
                    var defChanged = spDb1[sp].Definition != spDb2[sp].Definition;
                    var dateChanged = spDb1[sp].ModifiedDate > spDb2[sp].ModifiedDate;

                    if (defChanged || dateChanged)
                    {
                        string desc = defChanged
                            ? "Definition differs."
                            : "Source modified later than Target.";

                        _dtResults.Rows.Add(serverLabel1, dbName, "Stored Procedure", sp, desc, spDb1[sp].ModifiedDate, spDb2[sp].ModifiedDate);
                    }
                }
            }

            foreach (var sp in spDb2.Keys)
                if (!spDb1.ContainsKey(sp))
                    _dtResults.Rows.Add(serverLabel2, dbName, "Stored Procedure", sp, $"Exists only in {serverLabel2}", null, spDb2[sp].ModifiedDate);
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
    }
}
