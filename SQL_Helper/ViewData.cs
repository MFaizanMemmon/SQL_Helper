using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;
using System.IO;


namespace SQL_Helper
{
    public partial class ViewData : Form
    {
        public string DbName { get; set; }
        public string tableName { get; set; }
        public string SpName { get; set; }

        public List<Microsoft.Data.SqlClient.SqlParameter> parameters;

        public ViewData()
        {
            InitializeComponent();
        }

        private async void ViewData_Load(object sender, EventArgs e)
        {
            await LoadTableDataAsync();
        }

        private async Task LoadTableDataAsync()
        {
            if (string.IsNullOrWhiteSpace(DbConnectionHelper.ConnectionString) || string.IsNullOrWhiteSpace(DbName))
            {
                MessageBox.Show("Missing connection string or database name.");
                return;
            }

            string updatedConnectionString = new SqlConnectionStringBuilder(DbConnectionHelper.ConnectionString)
            {
                InitialCatalog = DbName
            }.ConnectionString;

            try
            {
                using var conn = new Microsoft.Data.SqlClient.SqlConnection(updatedConnectionString);
                await conn.OpenAsync();

                using var cmd = new Microsoft.Data.SqlClient.SqlCommand
                {
                    Connection = conn
                };

                DataTable dt = new DataTable();

                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    string query = $"SELECT TOP 100 * FROM [{tableName}]";
                    using var adapter = new Microsoft.Data.SqlClient.SqlDataAdapter(query, conn);
                    adapter.Fill(dt);
                }
                else if (!string.IsNullOrWhiteSpace(SpName))
                {
                    cmd.CommandText = SpName;
                    cmd.CommandType = CommandType.StoredProcedure;

                    // ✅ Add parameter values if passed from frmParameters
                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (var param in parameters)
                        {
                            // Recreate parameter to avoid object re-use issue
                            var sqlParam = new Microsoft.Data.SqlClient.SqlParameter
                            {
                                ParameterName = param.ParameterName,
                                SqlDbType = param.SqlDbType,
                                Value = param.Value ?? DBNull.Value
                            };
                            cmd.Parameters.Add(sqlParam);
                        }
                    }

                    using var adapter = new Microsoft.Data.SqlClient.SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
                else
                {
                    MessageBox.Show("No table or stored procedure provided.");
                    return;
                }

                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }


        private DataTable? _originalTable = null;  // Store original data once loaded
        private CancellationTokenSource? _cts;

        private async void textBox1_TextChanged(object sender, EventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            string filterText = textBox1.Text.Trim().Replace("'", "''");

            if (_originalTable == null)
            {
                if (dataGridView1.DataSource is DataTable dt)
                    _originalTable = dt.Copy();
                else
                    return;
            }

            try
            {
                DataTable filteredTable = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();

                    if (string.IsNullOrEmpty(filterText))
                    {
                        return _originalTable.Copy(); // Return full original data
                    }

                    var filters = new List<string>();
                    foreach (DataColumn col in _originalTable.Columns)
                    {
                        if (col.DataType == typeof(string) || col.DataType == typeof(object))
                        {
                            filters.Add($"CONVERT([{col.ColumnName}], 'System.String') LIKE '%{filterText}%'");
                        }
                    }
                    string filterExpression = string.Join(" OR ", filters);

                    DataRow[] filteredRows = _originalTable.Select(filterExpression);

                    DataTable dt = _originalTable.Clone();
                    foreach (DataRow row in filteredRows)
                    {
                        dt.ImportRow(row);
                    }

                    return dt;

                }, token);

                if (!token.IsCancellationRequested)
                {
                    dataGridView1.DataSource = filteredTable;
                }
            }
            catch (OperationCanceledException)
            {
                // Task was cancelled - no action needed
            }
        }

        private async void BtnExportCSV_Click(object sender, EventArgs e)
        {
            await ExportDataAsync("csv", BtnExportCSV);
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
                    if (fileType == "csv")
                        ExportToCsv();
                    else if (fileType == "xlsx")
                        ExportToExcel();
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

        private void ExportToCsv()
        {
            var dt = GetDataTableFromDataGridView(dataGridView1);
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string filePath = Path.Combine(downloadsPath, $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            using (var sw = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write headers
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sw.Write(dt.Columns[i].ColumnName);
                    if (i < dt.Columns.Count - 1)
                        sw.Write(",");
                }
                sw.WriteLine();

                // Write rows
                foreach (DataRow row in dt.Rows)
                {
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        var value = row[i]?.ToString() ?? "";
                        // Escape quotes and commas
                        value = value.Contains(",") || value.Contains("\"") ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
                        sw.Write(value);

                        if (i < dt.Columns.Count - 1)
                            sw.Write(",");
                    }
                    sw.WriteLine();
                }
            }
        }

        // Note: Requires Microsoft.Office.Interop.Excel COM reference added to your project
        private void ExportToExcel()
        {
            var dt = GetDataTableFromDataGridView(dataGridView1);
            string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            string filePath = Path.Combine(downloadsPath, $"Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("ExportedData");
                worksheet.Cell(1, 1).InsertTable(dt);
                workbook.SaveAs(filePath);
            }
        }


        private DataTable GetDataTableFromDataGridView(DataGridView dgv)
        {
            if (dgv.DataSource is DataTable dt)
                return dt;

            // If not DataTable, build from rows and columns manually
            dt = new DataTable();

            foreach (DataGridViewColumn column in dgv.Columns)
            {
                dt.Columns.Add(column.HeaderText);
            }

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (!row.IsNewRow)
                {
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < dgv.Columns.Count; i++)
                    {
                        dr[i] = row.Cells[i].Value ?? DBNull.Value;
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }
    }
}
