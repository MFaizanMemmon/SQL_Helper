using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace SQL_Helper
{
    public partial class frmSelectCompareDatabase : Form
    {
        public frmSelectCompareDatabase()
        {
            InitializeComponent();
        }

        private void frmSelectCompareDatabase_Load(object sender, EventArgs e)
        {
            LoadDatabases(DbConnectionHelper.TargetConnectionString, comboBox1);
            LoadDatabases(DbConnectionHelper.CompareToConnectionString, comboBox2);
        }

        private void LoadDatabases(string connectionString, ComboBox comboBox)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    DataTable dt = conn.GetSchema("Databases");

                    comboBox.Items.Clear();
                    foreach (DataRow row in dt.Rows)
                    {
                        string dbName = row["database_name"].ToString();
                        comboBox.Items.Add(dbName);
                    }

                    if (comboBox.Items.Count > 0)
                        comboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load databases:\n" + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optional: handle target DB selection
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optional: handle compare DB selection
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string selectedTargetDb = comboBox1.SelectedItem?.ToString();
            string selectedCompareDb = comboBox2.SelectedItem?.ToString();

            if (string.IsNullOrWhiteSpace(selectedTargetDb) || string.IsNullOrWhiteSpace(selectedCompareDb))
            {
                MessageBox.Show("Please select both Target and Compare databases.", "Selection Required",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DbConnectionHelper.TargetDatabase = selectedTargetDb;
            DbConnectionHelper.SourceDatabase = selectedCompareDb;

            DbConnectionHelper.TargetConnectionString = UpdateConnectionStringDb(
                DbConnectionHelper.TargetConnectionString, selectedTargetDb);

            DbConnectionHelper.CompareToConnectionString = UpdateConnectionStringDb(
                DbConnectionHelper.CompareToConnectionString, selectedCompareDb);

            this.DialogResult = DialogResult.OK; // Mark success
            this.Close();
        }


        public static string UpdateConnectionStringDb(string originalConnStr, string database)
        {
            var builder = new SqlConnectionStringBuilder(originalConnStr);
            builder.InitialCatalog = database;
            return builder.ToString();
        }



    }
}
