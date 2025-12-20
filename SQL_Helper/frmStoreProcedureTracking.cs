using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQL_Helper
{
    public partial class frmStoreProcedureTracking : Form
    {
        private CancellationTokenSource _cts;

        public frmStoreProcedureTracking()
        {
            InitializeComponent();
        }

        // ================= FORM LOAD =================
        private async void frmStoreProcedureTracking_Load(object sender, EventArgs e)
        {
            try
            {
                pictureBox1.Visible = true;
                CenterLoader();
                await LoadAllDatabasesAsync();
            }
            finally
            {
                pictureBox1.Visible = false;
            }
        }

        private void CenterLoader()
        {
            pictureBox1.Left = (ClientSize.Width - pictureBox1.Width) / 2;
            pictureBox1.Top = (ClientSize.Height - pictureBox1.Height) / 2;
            pictureBox1.BringToFront();
        }

        // ================= LOAD DATABASES =================
        private async Task LoadAllDatabasesAsync()
        {
            string cs = DbConnectionHelper.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                MessageBox.Show("Connection string is missing.");
                return;
            }

            using SqlConnection con = new SqlConnection(cs);
            await con.OpenAsync();

            string sql = @"
                SELECT name
                FROM sys.databases
                WHERE name NOT IN ('master','tempdb','model','msdb')
                ORDER BY name";

            using SqlCommand cmd = new SqlCommand(sql, con);
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            checkedListBoxDatabses.Items.Clear();

            while (await reader.ReadAsync())
            {
                checkedListBoxDatabses.Items.Add(reader.GetString(0), false);
            }
        }

        // ================= CHECK → LOAD PROCEDURES =================
        private async void checkedListBoxDatabses_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (sender is not CheckedListBox clb) return;

            List<string> selectedDatabases = new();

            for (int i = 0; i < clb.Items.Count; i++)
            {
                bool isChecked = (i == e.Index)
                    ? e.NewValue == CheckState.Checked
                    : clb.GetItemChecked(i);

                if (isChecked)
                    selectedDatabases.Add(clb.Items[i].ToString());
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            if (selectedDatabases.Count == 0)
            {
                dataGridView1.DataSource = null;
                Text = "Track Store Procedure";
                return;
            }

            try
            {
                Text = "Loading Procedures...";
                Refresh();

                await LoadStoredProceduresAsync(selectedDatabases, _cts.Token);

                Text = "Track Store Procedure";
            }
            catch (OperationCanceledException)
            {
                // ignore
            }
        }

        // ================= LOAD STORED PROCEDURES =================
        public async Task LoadStoredProceduresAsync(
            List<string> databases,
            CancellationToken token)
        {
            string cs = DbConnectionHelper.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs)) return;

            DataTable table = new DataTable();
            table.Columns.Add("DatabaseName");
            table.Columns.Add("ProcedureName");
            table.Columns.Add("CreateDate", typeof(DateTime));
            table.Columns.Add("ModifyDate", typeof(DateTime));

            using SqlConnection conn = new SqlConnection(cs);
            await conn.OpenAsync(token);

            foreach (string db in databases)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    conn.ChangeDatabase(db);

                    string sql = @"
                        SELECT
                            @db AS DatabaseName,
                            SCHEMA_NAME(schema_id) + '.' + name AS ProcedureName,
                            create_date,
                            modify_date
                        FROM sys.procedures
                        ORDER BY name";

                    using SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@db", db);

                    using SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow r in dt.Rows)
                    {
                        table.Rows.Add(
                            r["DatabaseName"],
                            r["ProcedureName"],
                            r["create_date"],
                            r["modify_date"]
                        );
                    }
                }
                catch
                {
                    // optional logging
                }
            }

            dataGridView1.DataSource = table;
        }

        // ================= SEARCH =================
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource is DataTable dt)
            {
                string search = textBox1.Text.Trim().Replace("'", "''");
                dt.DefaultView.RowFilter =
                    string.IsNullOrEmpty(search)
                        ? ""
                        : $"ProcedureName LIKE '%{search}%'";
            }
        }

        // ================= OPEN HELP TEXT =================
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a stored procedure.");
                return;
            }

            DataGridViewRow row = dataGridView1.SelectedRows[0];

            frmSpHelpText help = new frmSpHelpText
            {
                DbName = row.Cells["DatabaseName"].Value.ToString(),
                SpName = row.Cells["ProcedureName"].Value.ToString()
            };

            help.ShowDialog();
        }

        // ================= FORM CLOSE =================
        private void frmStoreProcedureTracking_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cts?.Cancel();
        }

        // ================= UNUSED EVENTS (SAFE EMPTY) =================
        private void checkedListBoxDatabses_SelectedIndexChanged(object sender, EventArgs e)
        {
            // intentionally empty
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // intentionally empty
        }
    }
}
