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

namespace SQL_Helper
{
    public partial class frmStoreProcedureTracking : Form
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public frmStoreProcedureTracking()
        {
            InitializeComponent();

        }

        private async void frmStoreProcedureTracking_Load(object sender, EventArgs e)
        {
            await LoadAllDatabasesAsync();
            await LoadAllStoredProceduresAsync(_cancellationTokenSource.Token);

        }

        private void checkedListBoxDatabses_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private async Task LoadAllDatabasesAsync()
        {
            string? baseConnectionString = DbConnectionHelper.ConnectionString;

            if (string.IsNullOrWhiteSpace(baseConnectionString))
            {
                MessageBox.Show("Connection string is missing.");
                return;
            }

            try
            {
                using SqlConnection con = new SqlConnection(baseConnectionString);
                await con.OpenAsync();

                string query = @"
            SELECT name 
            FROM sys.databases 
            WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb') 
            ORDER BY name;";

                using SqlCommand cmd = new SqlCommand(query, con);
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                // Clear items safely on UI thread
                checkedListBoxDatabses.Invoke((Action)(() =>
                {
                    checkedListBoxDatabses.Items.Clear();
                }));

                // Add items safely on UI thread
                while (await reader.ReadAsync())
                {
                    string dbName = reader["name"].ToString();
                    checkedListBoxDatabses.Invoke((Action)(() =>
                    {
                        checkedListBoxDatabses.Items.Add(dbName);
                    }));
                }

                // After adding all items, check all of them on UI thread
                checkedListBoxDatabses.Invoke((Action)(() =>
                {
                    for (int i = 0; i < checkedListBoxDatabses.Items.Count; i++)
                    {
                        checkedListBoxDatabses.SetItemChecked(i, true);
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading databases: " + ex.Message);
            }
        }



        private async Task LoadAllStoredProceduresAsync(CancellationToken cancellationToken)
        {
            string? connectionString = DbConnectionHelper.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Connection string is missing.");
                return;
            }

            DataTable resultTable = new DataTable();
            resultTable.Columns.Add("DatabaseName", typeof(string));
            resultTable.Columns.Add("ProcedureName", typeof(string));
            resultTable.Columns.Add("CreateDate", typeof(DateTime));
            resultTable.Columns.Add("ModifyDate", typeof(DateTime));
            resultTable.Columns.Add("UsesAPI", typeof(string));

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                await conn.OpenAsync(cancellationToken);

                var dbNames = new List<string>();
                using (SqlCommand cmd = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4", conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        if (cancellationToken.IsCancellationRequested) return;
                        dbNames.Add(reader.GetString(0));
                    }
                }

                foreach (string dbName in dbNames)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    try
                    {
                        conn.ChangeDatabase(dbName);

                        string query = @"
                    SELECT 
                        @dbName AS DatabaseName,
                        SCHEMA_NAME(p.schema_id) + '.' + p.name AS ProcedureName,
                        p.create_date AS CreateDate,
                        p.modify_date AS ModifyDate
                    FROM sys.procedures p;";

                        using SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@dbName", dbName);

                        using SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow row in dt.Rows)
                        {
                            if (cancellationToken.IsCancellationRequested) return;

                            DataRow newRow = resultTable.NewRow();
                            newRow["DatabaseName"] = row["DatabaseName"];
                            newRow["ProcedureName"] = row["ProcedureName"];
                            newRow["CreateDate"] = row["CreateDate"];
                            newRow["ModifyDate"] = row["ModifyDate"];
                            newRow["UsesAPI"] = "Loading...";
                            resultTable.Rows.Add(newRow);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading from DB '{dbName}': {ex.Message}");
                    }
                }
            }

            dataGridView1.DataSource = resultTable;

            await UpdateApiUsageAsync(resultTable, cancellationToken);
        }

        private async Task UpdateApiUsageAsync(DataTable spTable, CancellationToken cancellationToken)
        {
            string? connectionString = DbConnectionHelper.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString)) return;

            using SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);

            foreach (DataRow row in spTable.Rows)
            {
                if (cancellationToken.IsCancellationRequested) return;

                string dbName = row["DatabaseName"].ToString() ?? "";
                string procName = row["ProcedureName"].ToString() ?? "";

                if (string.IsNullOrEmpty(dbName) || string.IsNullOrEmpty(procName)) continue;

                try
                {
                    conn.ChangeDatabase(dbName);

                    string[] parts = procName.Split('.');
                    if (parts.Length != 2) continue;

                    string schema = parts[0];
                    string proc = parts[1];

                    string query = @"
                SELECT m.definition 
                FROM sys.procedures p
                INNER JOIN sys.sql_modules m ON p.object_id = m.object_id
                WHERE SCHEMA_NAME(p.schema_id) = @schema AND p.name = @proc;";

                    using SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@schema", schema);
                    cmd.Parameters.AddWithValue("@proc", proc);

                    string? definition = (string?)await cmd.ExecuteScalarAsync(cancellationToken);

                    if (!string.IsNullOrEmpty(definition) &&
                        (definition.Contains("http://", StringComparison.OrdinalIgnoreCase) ||
                         definition.Contains("https://", StringComparison.OrdinalIgnoreCase)))
                    {
                        row["UsesAPI"] = "Yes";
                    }
                    else
                    {
                        row["UsesAPI"] = "No";
                    }
                }
                catch
                {
                    row["UsesAPI"] = "Error";
                }

                if (dataGridView1 != null && dataGridView1.IsHandleCreated && !dataGridView1.IsDisposed)
                {
                    try
                    {
                        dataGridView1.Invoke((Action)(() =>
                        {
                            dataGridView1.Refresh();
                        }));
                    }
                    catch (ObjectDisposedException)
                    {
                        // Control already disposed, ignore
                    }
                }

            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore header clicks or invalid rows
            if (e.RowIndex < 0 || dataGridView1.Rows[e.RowIndex].Cells["DatabaseName"].Value == null)
                return;

            string dbName = dataGridView1.Rows[e.RowIndex].Cells["DatabaseName"].Value.ToString() ?? "";
            string spName = dataGridView1.Rows[e.RowIndex].Cells["ProcedureName"].Value.ToString() ?? "";

            // Open helptext form with these values
            frmSpHelpText helptext = new frmSpHelpText
            {
                DbName = dbName,
                SpName = spName
            };

            helptext.ShowDialog();
        }

        private void frmStoreProcedureTracking_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource is DataView dv)
            {
                string search = textBox1.Text.Trim().Replace("'", "''");

                if (string.IsNullOrEmpty(search))
                    dv.RowFilter = "";
                else
                    dv.RowFilter = $"ProcedureName LIKE '%{search}%'";
            }
        }


        private void checkedListBoxDatabses_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Cast sender to CheckedListBox type (not the control name)
            CheckedListBox clb = sender as CheckedListBox;

            List<string> checkedItems = new List<string>();

            for (int i = 0; i < clb.Items.Count; i++)
            {
                if (i == e.Index)
                {
                    if (e.NewValue == CheckState.Checked)
                    {
                        checkedItems.Add(clb.Items[i].ToString());
                    }
                }
                else
                {
                    if (clb.GetItemChecked(i))
                    {
                        checkedItems.Add(clb.Items[i].ToString());
                    }
                }
            }

         
        }

    }
}
