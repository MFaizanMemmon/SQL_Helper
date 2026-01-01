using Microsoft.Data.SqlClient; // make sure you have this using
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SQL_Helper
{
    public partial class frmTableSearching : Form
    {
        DataTable originalTable;

        public frmTableSearching()
        {
            InitializeComponent();

        }

        private async void frmTableSearching_Load(object sender, EventArgs e)
        {
           // CenterLoader();
            //pictureBox1.Visible = true;      // Show loader
            await LoadAllTablesInAllDatabases();  // Async load data
            //pictureBox1.Visible = false;     // Hide loader after load
        }

        //private void CenterLoader()
        //{
        //    if (pictureBox1.Image == null)
        //        return;

        //    int x = (this.ClientSize.Width - pictureBox1.Width) / 2;
        //    int y = (this.ClientSize.Height - pictureBox1.Height) / 2;
        //    pictureBox1.Location = new Point(x, y);
        //}
        public async Task LoadAllTablesInAllDatabases()
        {
            try
            {
                string? masterConnectionString = DbConnectionHelper.ConnectionString;

                DataTable result = await Task.Run(() =>
                {
                    DataTable localResult = new DataTable();
                    localResult.Columns.Add("Database");
                    localResult.Columns.Add("Schema");
                    localResult.Columns.Add("Table");

                    using (SqlConnection masterConn = new SqlConnection(masterConnectionString))
                    {
                        masterConn.Open();

                        SqlCommand getDatabasesCmd = new SqlCommand(@"
                    SELECT name 
                    FROM sys.databases 
                    WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')", masterConn);

                        SqlDataAdapter da = new SqlDataAdapter(getDatabasesCmd);
                        DataTable databases = new DataTable();
                        da.Fill(databases);

                        foreach (DataRow dbRow in databases.Rows)
                        {
                            string dbName = dbRow["name"].ToString();

                            var builder = new SqlConnectionStringBuilder(masterConnectionString)
                            {
                                InitialCatalog = dbName
                            };
                            string dbConnectionString = builder.ToString();

                            try
                            {
                                using (SqlConnection dbConn = new SqlConnection(dbConnectionString))
                                {
                                    dbConn.Open();

                                    SqlCommand getTablesCmd = new SqlCommand(@"
                                SELECT TABLE_SCHEMA, TABLE_NAME 
                                FROM INFORMATION_SCHEMA.TABLES 
                                WHERE TABLE_TYPE = 'BASE TABLE'", dbConn);

                                    using (SqlDataReader reader = getTablesCmd.ExecuteReader())
                                    {
                                        while (reader.Read())
                                        {
                                            localResult.Rows.Add(dbName, reader["TABLE_SCHEMA"].ToString(), reader["TABLE_NAME"].ToString());
                                        }
                                    }
                                }
                            }
                            catch (Exception innerEx)
                            {
                                Console.WriteLine($"Error in DB '{dbName}': {innerEx.Message}");
                            }
                        }
                    }

                    return localResult;
                });

                dataGridView1.Invoke((Action)(() =>
                {
                    dataGridView1.DataSource = result;
                    originalTable = result;
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string filterText = textBox1.Text.Trim().Replace("'", "''"); // Avoid SQL injection issues

            if (originalTable == null || originalTable.Rows.Count == 0)
                return;

            DataView dv = new DataView(originalTable);
            dv.RowFilter = $"Database LIKE '%{filterText}%' OR Schema LIKE '%{filterText}%' OR Table LIKE '%{filterText}%'";
            dataGridView1.DataSource = dv;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                string dbName = dataGridView1.CurrentRow.Cells["Database"].Value?.ToString() ?? "";
                string tableName = dataGridView1.CurrentRow.Cells["Table"].Value?.ToString() ?? "";

                ViewData data = new ViewData();
                data.DbName = dbName;
                data.tableName = tableName;

                data.ShowDialog();
            }
            else
            {
                MessageBox.Show("Please select a row first.");
            }
        }

    }
}
