using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.ComponentModel.Design.ObjectSelectorEditor;

namespace SQL_Helper
{
    public partial class MainDashboard : Form
    {
        private int selectedRowIndex = -1;
        private int selectedColumnIndex = -1;
        private DataTable databaseDetailTable;
        private CancellationTokenSource _cts;



        public MainDashboard()
        {
            InitializeComponent();
        }

        private void MainDashboard_Load(object sender, EventArgs e)
        {

            LoadDatabases();
        }

        private async void listBoxDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxDB.Enabled = false;

            string selectedDB = listBoxDB.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selectedDB))
            {
                await LoadObjectCountsAsync(selectedDB);
                await LoadDatabaseDetail(selectedDB);
            }

            listBoxDB.Enabled = true;
            btnTable_Click(sender, e);
        }

        private void LoadDatabases()
        {
            string? connectionString = DbConnectionHelper.ConnectionString?.ToString();
            string query = "SELECT name FROM sys.databases WHERE database_id > 4";

            using (SqlConnection con = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        listBoxDB.Items.Clear();
                        while (reader.Read())
                        {
                            listBoxDB.Items.Add(reader["name"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading databases: " + ex.Message);
                }
            }
        }

        private async Task LoadDatabaseDetail(string databaseName)
        {
            string? baseConnectionString = DbConnectionHelper.ConnectionString;

            if (string.IsNullOrWhiteSpace(baseConnectionString))
            {
                MessageBox.Show("Connection string is missing.");
                return;
            }



            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = databaseName
            };

            string query = @"
                    SELECT
                        s.name AS SchemaName,
                        t.name AS TableName,

                        (
                            SELECT COUNT(*) FROM sys.triggers tr2 WHERE tr2.parent_id = t.object_id
                        ) AS TriggerCount,

                        STUFF((
                            SELECT ', ' + tr.name
                            FROM sys.triggers tr
                            WHERE tr.parent_id = t.object_id
                            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS TriggerNames,

                        (
                            SELECT COUNT(DISTINCT pt.object_id)
                            FROM sys.foreign_keys fk2
                            INNER JOIN sys.tables pt ON fk2.referenced_object_id = pt.object_id
                            WHERE fk2.parent_object_id = t.object_id
                        ) AS ParentCount,

                        

                        (
                            SELECT COUNT(DISTINCT ct.object_id)
                            FROM sys.foreign_keys cfk2
                            INNER JOIN sys.tables ct ON cfk2.parent_object_id = ct.object_id
                            WHERE cfk2.referenced_object_id = t.object_id
                        ) AS ChildCount

                    FROM
                        sys.tables t
                    INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
                    ORDER BY
                        s.name, t.name;
            ";

            //STUFF((
            //    SELECT DISTINCT ', ' + ps.name + '.' + pt.name
            //    FROM sys.foreign_keys fk2
            //    INNER JOIN sys.tables pt ON fk2.referenced_object_id = pt.object_id
            //    INNER JOIN sys.schemas ps ON pt.schema_id = ps.schema_id
            //    WHERE fk2.parent_object_id = t.object_id
            //    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ParentTables,

            //,

            //            STUFF((
            //                SELECT DISTINCT ', ' + cs.name + '.' + ct.name
            //                FROM sys.foreign_keys cfk2
            //                INNER JOIN sys.tables ct ON cfk2.parent_object_id = ct.object_id
            //                INNER JOIN sys.schemas cs ON ct.schema_id = cs.schema_id
            //                WHERE cfk2.referenced_object_id = t.object_id
            //                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS ChildTables

            try
            {
                using SqlConnection con = new SqlConnection(builder.ToString());
                await con.OpenAsync();

                using SqlCommand cmd = new SqlCommand(query, con);
                using SqlDataAdapter da = new SqlDataAdapter(cmd);

                DataTable dt = new DataTable();
                await Task.Run(() => da.Fill(dt));

                databaseDetailTable = dt; // Store full data for filtering

                dataGridViewDetail.Invoke(() =>
                {
                    dataGridViewDetail.DataSource = databaseDetailTable;

                    dataGridViewDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dataGridViewDetail.ScrollBars = ScrollBars.Both;
                    dataGridViewDetail.AutoResizeColumns();
                    dataGridViewDetail.AutoResizeRows();
                    dataGridViewDetail.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
                });

                dataGridViewDetail.CellPainting += dataGridViewTableDetail_CellPainting;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading table details: " + ex.Message);
            }

        }




        private async Task LoadObjectCountsAsync(string databaseName)
        {
            string? baseConnectionString = DbConnectionHelper.ConnectionString;

            if (string.IsNullOrWhiteSpace(baseConnectionString))
            {
                MessageBox.Show("Connection string is missing.");
                return;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = databaseName
            };

            string query = @"
        SELECT 
            (SELECT COUNT(*) FROM sys.tables) AS TableCount,
            (SELECT COUNT(*) FROM sys.views) AS ViewCount,
            (SELECT COUNT(*) FROM sys.procedures) AS ProcedureCount,
            (SELECT COUNT(*) FROM sys.triggers) AS TriggerCount;";

            try
            {
                using SqlConnection con = new SqlConnection(builder.ToString());
                await con.OpenAsync();

                using SqlCommand cmd = new SqlCommand(query, con);
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    lblTotalTable.Invoke(() => lblTotalTable.Text = reader["TableCount"].ToString());
                    lblTotalViews.Invoke(() => lblTotalViews.Text = reader["ViewCount"].ToString());
                    lblTotalStorePrcedure.Invoke(() => lblTotalStorePrcedure.Text = reader["ProcedureCount"].ToString());
                    lblTotalTrigger.Invoke(() => lblTotalTrigger.Text = reader["TriggerCount"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading object counts: " + ex.Message);
            }
        }

        private void dataGridViewTableDetail_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //if (e.RowIndex < 0 || e.ColumnIndex < 0)
            //    return;

            //string? cellText = e.FormattedValue?.ToString();
            //if (string.IsNullOrEmpty(cellText))
            //    return;
            //if (cellText.Contains(","))
            //{
            //    e.Handled = true;  // we will fully paint the cell


            //    using (Graphics g = e.Graphics)
            //    {
            //        Font font = e.CellStyle.Font;
            //        Brush normalBrush = new SolidBrush(Color.Black);

            //        float x = e.CellBounds.Left + 2;
            //        float y = e.CellBounds.Top + (e.CellBounds.Height - font.Height) / 2;

            //        int commaIndex = 0; // to track which comma we are on

            //        for (int i = 0; i < cellText.Length; i++)
            //        {
            //            string ch = cellText[i].ToString();

            //            Brush brushToUse;

            //            if (ch == ",")
            //            {
            //                brushToUse = new SolidBrush(GetColorByIndex(commaIndex));
            //                commaIndex++;
            //            }
            //            else
            //            {
            //                brushToUse = normalBrush;
            //            }

            //            SizeF charSize = g.MeasureString(ch, font);

            //            g.DrawString(ch, font, brushToUse, x, y);
            //            x += charSize.Width;

            //            if (brushToUse != normalBrush)
            //                brushToUse.Dispose();
            //        }

            //        normalBrush.Dispose();
            //    }
            //}


        }

        //private Color GetColorByIndex(int index)
        //{
        //    Color[] colors = new Color[]
        //    {
        //Color.Black,        // High contrast default
        //Color.DarkOrange,   // Bright and warm
        //Color.MediumBlue,   // Strong blue
        //Color.DarkViolet,   // Very distinct
        //Color.Crimson,      // Dark red tone
        //Color.SeaGreen,     // Deep green
        //Color.Goldenrod,    // Yellowish tone
        //Color.SlateGray     // Neutral contrast
        //    };

        //    return colors[index % colors.Length];
        //}




        private void dataGridViewTableDetail_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;  // Ignore header or invalid clicks

            //selectedRowIndex = e.RowIndex;
            //selectedColumnIndex = e.ColumnIndex;
            //dataGridViewTableDetail.Invalidate();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panel2_Click(object sender, EventArgs e)
        {
            //frmStoreProcedureTracking sp = new frmStoreProcedureTracking();
            //sp.ShowDialog();
        }

        private async Task LoadAllTriggersAsync()
        {
            string? connectionString = DbConnectionHelper.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Connection string is missing.");
                return;
            }

            DataTable resultTable = new DataTable();
            resultTable.Columns.Add("DatabaseName");
            resultTable.Columns.Add("TriggerName");
            resultTable.Columns.Add("TableName");
            resultTable.Columns.Add("IsInsteadOf", typeof(bool));
            resultTable.Columns.Add("CreateDate", typeof(DateTime));
            resultTable.Columns.Add("ModifyDate", typeof(DateTime));
            //resultTable.Columns.Add("UsesAPI"); // placeholder, filled later

            using SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            List<string> dbNames = new List<string>();
            using (SqlCommand cmd = new SqlCommand("SELECT name FROM sys.databases WHERE database_id > 4", conn))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    dbNames.Add(reader.GetString(0));
                }
            }

            foreach (string dbName in dbNames)
            {
                try
                {
                    conn.ChangeDatabase(dbName);

                    string query = @"
                SELECT 
                    '" + dbName + @"' AS DatabaseName,
                    tr.name AS TriggerName,
                    OBJECT_NAME(tr.parent_id) AS TableName,
                    tr.is_instead_of_trigger AS IsInsteadOf,
                    tr.create_date AS CreateDate,
                    tr.modify_date AS ModifyDate
                FROM sys.triggers tr;";

                    using SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // Add default placeholder for UsesAPI
                    //foreach (DataRow row in dt.Rows)
                    //    row["UsesAPI"] = "Loading...";

                    resultTable.Merge(dt);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading triggers from {dbName}: {ex.Message}");
                }
            }

            dataGridViewDetail.DataSource = resultTable;

            // Start checking UsesAPI in background
            // _ = CheckTriggerApiUsageAsync(resultTable);
        }


        private async Task CheckTriggerApiUsageAsync(DataTable resultTable)
        {
            string? connectionString = DbConnectionHelper.ConnectionString;
            using SqlConnection conn = new SqlConnection(connectionString);
            await conn.OpenAsync();

            foreach (DataRow row in resultTable.Rows)
            {
                string dbName = row["DatabaseName"].ToString()!;
                string triggerName = row["TriggerName"].ToString()!;

                try
                {
                    conn.ChangeDatabase(dbName);

                    string query = $@"
                SELECT m.definition
                FROM sys.triggers t
                JOIN sys.sql_modules m ON t.object_id = m.object_id
                WHERE t.name = @triggerName;";

                    using SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@triggerName", triggerName);
                    string? definition = (string?)await cmd.ExecuteScalarAsync();

                    if (!string.IsNullOrEmpty(definition) && (definition.Contains("http://") || definition.Contains("https://")))
                        row["UsesAPI"] = "Yes";
                    else
                        row["UsesAPI"] = "No";
                }
                catch
                {
                    row["UsesAPI"] = "Error";
                }

                // Refresh only the current row in UI (optional)
                dataGridViewDetail.Invoke(() =>
                {
                    dataGridViewDetail.Refresh();
                });
            }
        }


        private void panel3_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Click(object sender, EventArgs e)
        {
            LoadAllTriggersAsync();
        }

        private void dataGridViewDetail_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hit = dataGridViewDetail.HitTest(e.X, e.Y);
                if (hit.RowIndex >= 0)
                {
                    dataGridViewDetail.ClearSelection();
                    dataGridViewDetail.Rows[hit.RowIndex].Selected = true;

                    // Show the context menu at mouse position
                    contextMenuStrip1.Show(dataGridViewDetail, new Point(e.X, e.Y));
                }
            }
        }

        private void viewDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridViewDetail.SelectedRows.Count > 0)
            {
                ViewData data = new ViewData();


                data.DbName = listBoxDB.SelectedItem.ToString();
                data.tableName = dataGridViewDetail.SelectedRows[0].Cells[1].Value?.ToString();
                data.SpName = null;

                data.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a row.");
            }
        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            frmStoreProcedureTracking sp = new frmStoreProcedureTracking();
            sp.ShowDialog();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            frmTableSearching frmTableSearching = new frmTableSearching();
            frmTableSearching.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Get current data source
            var source = dataGridViewDetail.DataSource;

            if (source == null)
                return;

            // Extract DataTable from various source types
            DataTable table = null;

            if (source is DataView dv)
                table = dv.Table;
            else if (source is DataTable dt)
                table = dt;

            if (table == null || table.Rows.Count == 0)
                return;

            string filterText = textBox1.Text.Trim();

            // Clear filter if search is empty
            if (string.IsNullOrEmpty(filterText))
            {
                if (source is DataView originalView)
                    originalView.RowFilter = "";
                dataGridViewDetail.DataSource = source;
                return;
            }

            // Escape for DataView filter
            filterText = filterText.Replace("'", "''")
                                  .Replace("[", "[[]")
                                  .Replace("%", "[%]")
                                  .Replace("_", "[_]");

            // Build filter for ALL string columns
            var stringColumns = new List<string>();
            foreach (DataColumn col in table.Columns)
            {
                if (col.DataType == typeof(string))
                {
                    stringColumns.Add(col.ColumnName);
                }
            }

            if (stringColumns.Count == 0)
                return;

            // Create the filter condition
            string filter = string.Join(" OR ",
                stringColumns.Select(col => $"[{col}] LIKE '%{filterText}%'"));

            // Apply filter
            DataView filteredView = new DataView(table);
            filteredView.RowFilter = filter;
            dataGridViewDetail.DataSource = filteredView;
        }
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
           
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            frmConnectSQL conn = new frmConnectSQL();
            conn.ShowDialog();
            MainDashboard_Load(null, null);
            if (DbConnectionHelper.ConnectionString != null)
            {
                toolStripButton1.Enabled = toolStripButton2.Enabled = toolStripButton7.Enabled = true;
            }


        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            frmComparisionDatabase comDb = new frmComparisionDatabase();
            comDb.ShowDialog();
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            frm_Sp_Xray xray = new frm_Sp_Xray();
            xray.ShowDialog();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            frmDummyData dummy = new frmDummyData();
            dummy.ShowDialog();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {


        }

        private void btnTable_Click(object sender, EventArgs e)
        {
            if (listBoxDB.SelectedItem == null)
            {
                MessageBox.Show("Please select a database first.");
                return;
            }

            btnTable.BackColor = System.Drawing.Color.LightBlue;
            btnShowProcedures.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowView.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowTriggers.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowTypes.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewDetail.DataSource = null;
            LoadDatabaseDetail(listBoxDB.SelectedItem.ToString()).ConfigureAwait(false);

        }

        private async void btnShowProcedures_Click(object sender, EventArgs e)
        {
            if (listBoxDB.SelectedItem == null)
            {
                MessageBox.Show("Please select a database first.");
                return;
            }

            // 🔹 UI highlighting
            btnTable.BackColor = System.Drawing.Color.White;
            btnShowProcedures.BackColor = System.Drawing.Color.LightBlue;
            btnShowView.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowTriggers.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowTypes.BackColor = System.Drawing.Color.WhiteSmoke;

            dataGridViewDetail.DataSource = null;

            string selectedDb = listBoxDB.SelectedItem.ToString();

            try
            {
                // ✅ Correct title
                this.Text = "Loading Procedures...";
                this.Refresh();

                // ✅ Load procedures for ONE database
                await LoadProceduresForSingleDatabaseAsync(selectedDb);

                // ✅ Restore title
                this.Text = "SQL Explorer - Procedures";
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }


        private async void btnShowView_Click(object sender, EventArgs e)
        {
            if (listBoxDB.SelectedItem == null)
            {
                MessageBox.Show("Please select a database first.");
                return;
            }

            btnTable.BackColor = System.Drawing.Color.White;
            btnShowProcedures.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowView.BackColor = System.Drawing.Color.LightBlue;
            btnShowTriggers.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowTypes.BackColor = System.Drawing.Color.WhiteSmoke;
            dataGridViewDetail.DataSource = null;

            string selectedDb = listBoxDB.SelectedItem.ToString();

            try
            {
                this.Text = "Loading Views...";
                this.Refresh();

                // Load views data
                string query = $@"
            USE [{selectedDb}];
            SELECT 
                s.name AS SchemaName,
                v.name AS ViewName,
                v.create_date AS CreatedDate,
                v.modify_date AS ModifiedDate
            FROM sys.views v
            INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
            ORDER BY s.name, v.name";

                DataTable viewsTable = await ExecuteQueryAsync(query);
                dataGridViewDetail.DataSource = viewsTable;

                this.Text = "SQL Explorer - Views";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading views: {ex.Message}");
                this.Text = "SQL Explorer";
            }
        }
        private async Task<DataTable> ExecuteQueryAsync(string query)
        {
            string? connectionString = DbConnectionHelper.ConnectionString;
            DataTable table = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
            {
                await conn.OpenAsync();
                await Task.Run(() => adapter.Fill(table));
            }

            return table;
        }
        private async void btnShowTriggers_Click(object sender, EventArgs e)
        {
            if (listBoxDB.SelectedItem == null)
            {
                MessageBox.Show("Please select a database first.");
                return;
            }

            btnTable.BackColor = System.Drawing.Color.White;
            btnShowProcedures.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowView.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowTriggers.BackColor = System.Drawing.Color.LightBlue;
            btnShowTypes.BackColor = System.Drawing.Color.WhiteSmoke;

            dataGridViewDetail.DataSource = null;

            // ✅ Show loading title
            this.Text = "Loading Triggers...";
            this.Refresh();

            // ✅ WAIT until loading finishes
            await LoadAllTriggersAsync();

            // ✅ Restore title AFTER loading completes
            this.Text = "SQL Explorer - Triggers";
        }


        private async void btnShowTypes_Click(object sender, EventArgs e)
        {
            if (listBoxDB.SelectedItem == null)
            {
                MessageBox.Show("Please select a database first.");
                return;
            }

            btnTable.BackColor = System.Drawing.Color.White;
            btnShowProcedures.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowView.BackColor = System.Drawing.Color.WhiteSmoke;
            btnShowTriggers.BackColor = System.Drawing.Color.White;
            btnShowTypes.BackColor = System.Drawing.Color.LightBlue;
            dataGridViewDetail.DataSource = null;

            string selectedDb = listBoxDB.SelectedItem.ToString();

            try
            {
                this.Text = "Loading Types...";
                this.Refresh();

                // Load user-defined types data
                string query = $@"
            USE [{selectedDb}];
            SELECT 
                s.name AS SchemaName,
                t.name AS TypeName,
                t.is_user_defined AS IsUserDefined,
                t.is_table_type AS IsTableType,
                t.max_length AS MaxLength,
                t.precision AS Precision,
                t.scale AS Scale
            FROM sys.types t
            INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE t.is_user_defined = 1
            ORDER BY s.name, t.name";

                DataTable typesTable = await ExecuteQueryAsync(query);
                dataGridViewDetail.DataSource = typesTable;

                this.Text = "SQL Explorer - Types";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading types: {ex.Message}");
                this.Text = "SQL Explorer";
            }
        }

        private async Task LoadProceduresForSingleDatabaseAsync(string databaseName)
        {
            if (string.IsNullOrWhiteSpace(databaseName))
            {
                MessageBox.Show("Invalid database selected.");
                return;
            }

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                this.Text = "Loading Procedures...";
                this.Refresh();

                // ✅ CALL METHOD ON CURRENT FORM
                await LoadStoredProceduresAsync(
                    new List<string> { databaseName },
                    _cts.Token
                );

                this.Text = "SQL Explorer - Procedures";
            }
            catch (OperationCanceledException)
            {
                // ignored
            }
        }

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

            dataGridViewDetail.DataSource = table;
        }



    }
}
