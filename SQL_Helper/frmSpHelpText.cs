using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SQL_Helper
{
    public partial class frmSpHelpText : Form
    {
        public string DbName { get; set; }
        public string SpName { get; set; }
        public frmSpHelpText()
        {
            InitializeComponent();
        }

        private async void frmSpHelpText_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DbName) || string.IsNullOrWhiteSpace(SpName))
            {
                MessageBox.Show("Database or Procedure name is missing.");
                return;
            }

            string? connectionString = DbConnectionHelper.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                MessageBox.Show("Connection string is missing.");
                return;
            }

            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);
                await conn.OpenAsync();

                conn.ChangeDatabase(DbName);

                using SqlCommand cmd = new SqlCommand("sp_helptext", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@objname", SpName);

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                StringBuilder sb = new StringBuilder();
                while (await reader.ReadAsync())
                {
                    sb.Append(reader.GetString(0));
                }

                string spText = sb.ToString();
                richTextBox1.Text = spText;

                // Check for insert, update, or delete (case-insensitive)
                if (spText.IndexOf("insert", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    spText.IndexOf("update", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    spText.IndexOf("delete", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    button1.Enabled = false; // Disable button
                }
                else
                {
                    button1.Enabled = true;  // Enable button
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading stored procedure text:\n" + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString = DbConnectionHelper.ConnectionString;
            string dbName = this.DbName;
            string spName = this.SpName;

            var parameters = GetStoredProcedureParameters(connectionString, dbName, spName);

            frmParameters paramForm = new frmParameters(parameters);
            if (paramForm.ShowDialog() == DialogResult.OK)
            {
                var userValues = paramForm.ParameterValues;

                // ✅ Update parameter values from user input
                foreach (var param in parameters)
                {
                    if (userValues.TryGetValue(param.ParameterName, out string value))
                    {
                        if (param.SqlDbType == SqlDbType.DateTime || param.SqlDbType == SqlDbType.DateTime2 || param.SqlDbType == SqlDbType.Date)
                        {
                            param.Value = DateTime.TryParse(value, out var dt) ? dt : DBNull.Value;
                        }
                        else
                        {
                            param.Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value;
                        }
                    }
                }

                ViewData data = new ViewData();
                data.SpName = spName;
                data.DbName = dbName;
                data.tableName = string.Empty;

                data.parameters = parameters; // ✅ pass updated parameters
                data.ShowDialog();
            }
        }


        private List<SqlParameter> GetStoredProcedureParameters(string connectionString, string dbName, string spName)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                conn.ChangeDatabase(dbName);

                using (SqlCommand cmd = new SqlCommand(spName, conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    SqlCommandBuilder.DeriveParameters(cmd); // this auto-loads parameters

                    foreach (SqlParameter param in cmd.Parameters)
                    {
                        // Skip return value parameter
                        if (param.Direction == ParameterDirection.ReturnValue) continue;
                        parameters.Add(param);
                    }
                }
            }

            return parameters;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string sqlText = richTextBox1.Text; // Your stored procedure text
            var result = ExtractTableAndParameters(sqlText);

            if (result.Count == 0)
            {
                MessageBox.Show("No tables or parameters found.", "Info");
                return;
            }

            StringBuilder sb = new StringBuilder();
            foreach (var table in result)
            {
                sb.AppendLine("Table: " + table.Key);
                foreach (var param in table.Value)
                {
                    sb.AppendLine("  - " + param);
                }
                sb.AppendLine(); // Blank line between tables
            }

            MessageBox.Show(sb.ToString(), "Tables and Parameters");
        }



        private Dictionary<string, List<string>> ExtractTableAndParameters(string sqlText)
        {
            var result = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // Match UPDATE/INSERT INTO/DELETE FROM tableName
            var tableRegex = new Regex(@"\b(UPDATE|INSERT\s+INTO|DELETE\s+FROM)\s+([a-zA-Z0-9_\.\[\]]+)", RegexOptions.IgnoreCase);
            var setWhereRegex = new Regex(@"\b(SET|WHERE|VALUES)\s+(.*?)\b(WHERE|FROM|SET|$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match tableMatch in tableRegex.Matches(sqlText))
            {
                string table = tableMatch.Groups[2].Value.Trim();
                if (!result.ContainsKey(table))
                    result[table] = new List<string>();

                // Look for SET or WHERE clause after this table
                var startIndex = tableMatch.Index + tableMatch.Length;
                var remainder = sqlText.Substring(startIndex);

                foreach (Match clauseMatch in setWhereRegex.Matches(remainder))
                {
                    var clauseText = clauseMatch.Groups[2].Value;

                    // Match @params in the clause
                    var paramRegex = new Regex(@"@[a-zA-Z0-9_]+");
                    foreach (Match paramMatch in paramRegex.Matches(clauseText))
                    {
                        string param = paramMatch.Value;
                        if (!result[table].Contains(param))
                            result[table].Add(param);
                    }
                }
            }

            return result;
        }


    }
}
