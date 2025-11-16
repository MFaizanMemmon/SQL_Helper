using ClosedXML.Excel;
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
    public partial class frmDummyData : Form
    {
        private static readonly Random _random = new Random();



        public frmDummyData()
        {
            InitializeComponent();
        }


        public int GetRandomNumber(int start, int end)
        {
            if (start > end)
            {
                MessageBox.Show("Start must be less than or equal to End.");
                return 0;
            }

            return _random.Next(start, end + 1);
        }

        /// <summary>
        /// Generates random data based on a given format.
        /// Use:
        ///   # = digit (0-9)
        ///   A = uppercase letter (A-Z)
        ///   a = lowercase letter (a-z)
        ///   * = letter or digit
        /// Any other character will be kept as-is.
        /// </summary>
        public string GenerateRandomByFormat(string format)
        {
            StringBuilder result = new StringBuilder();

            foreach (char c in format)
            {
                switch (c)
                {
                    case '#': // Digit
                        result.Append(_random.Next(0, 10));
                        break;

                    case 'A': // Uppercase letter
                        result.Append((char)_random.Next('A', 'Z' + 1));
                        break;

                    case 'a': // Lowercase letter
                        result.Append((char)_random.Next('a', 'z' + 1));
                        break;

                    case '*': // Letter or digit
                        int choice = _random.Next(0, 3);
                        if (choice == 0)
                            result.Append(_random.Next(0, 10));
                        else if (choice == 1)
                            result.Append((char)_random.Next('A', 'Z' + 1));
                        else
                            result.Append((char)_random.Next('a', 'z' + 1));
                        break;

                    default: // Keep original symbol
                        result.Append(c);
                        break;
                }
            }

            return result.ToString();
        }

        private void frmDummyData_Load(object sender, EventArgs e)
        {
            LoadDatabases();
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
                        cmbDatabaes.Items.Clear();
                        while (reader.Read())
                        {
                            cmbDatabaes.Items.Add(reader["name"].ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading databases: " + ex.Message);
                }
            }
        }

        private void LoadTables(string databaseName)
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

            string query = "SELECT s.name + '.' + t.name AS TableName " +
                           "FROM sys.tables t " +
                           "INNER JOIN sys.schemas s ON t.schema_id = s.schema_id " +
                           "ORDER BY s.name, t.name";

            try
            {
                using SqlConnection con = new SqlConnection(builder.ToString());
                using SqlCommand cmd = new SqlCommand(query, con);
                con.Open();

                using SqlDataReader reader = cmd.ExecuteReader();
                cmbTable.Items.Clear();

                while (reader.Read())
                {
                    cmbTable.Items.Add(reader["TableName"].ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading tables: " + ex.Message);
            }
        }

        private void cmbDatabaes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDatabaes.SelectedItem != null)
            {
                string selectedDb = cmbDatabaes.SelectedItem.ToString();
                LoadTables(selectedDb);

            }
        }

        private void btnViewSchema_Click(object sender, EventArgs e)
        {
            if (cmbDatabaes.SelectedItem == null || cmbTable.SelectedItem == null)
            {
                MessageBox.Show("Please select a database and a table.");
                return;
            }

            string databaseName = cmbDatabaes.SelectedItem.ToString();
            string tableName = cmbTable.SelectedItem.ToString();

            string? baseConnectionString = DbConnectionHelper.ConnectionString;
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = databaseName
            };

            string columnQuery = @"
SELECT 
    c.COLUMN_NAME, 
    c.DATA_TYPE, 
    c.IS_NULLABLE, 
    c.CHARACTER_MAXIMUM_LENGTH,
    COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') AS IS_IDENTITY
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = @TableName
ORDER BY c.ORDINAL_POSITION;
";


            string pkQuery = @"
        SELECT kcu.COLUMN_NAME
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
            ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
        WHERE tc.TABLE_NAME = @TableName AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY';
    ";

            string fkQuery = @"
        SELECT 
            COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
            OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
            COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
        FROM sys.foreign_keys fk
        INNER JOIN sys.foreign_key_columns fc 
            ON fk.object_id = fc.constraint_object_id
        WHERE fk.parent_object_id = OBJECT_ID(@TableName);
    ";

            try
            {
                using SqlConnection con = new SqlConnection(builder.ToString());
                con.Open();

                pnlSchema.Controls.Clear(); // Clear old schema view
                int y = 10;

                // 1️⃣ Load Primary Keys
                HashSet<string> primaryKeys = new HashSet<string>();
                using (SqlCommand pkCmd = new SqlCommand(pkQuery, con))
                {
                    pkCmd.Parameters.AddWithValue("@TableName", tableName.Contains('.') ? tableName.Split('.')[1] : tableName);
                    using SqlDataReader pkReader = pkCmd.ExecuteReader();
                    while (pkReader.Read())
                    {
                        primaryKeys.Add(pkReader["COLUMN_NAME"].ToString());
                    }
                }

                // 2️⃣ Load Foreign Keys into dictionary
                Dictionary<string, (string RefTable, string RefColumn)> foreignKeys = new();
                using (SqlCommand fkCmd = new SqlCommand(fkQuery, con))
                {
                    fkCmd.Parameters.AddWithValue("@TableName", tableName.Contains('.') ? tableName.Split('.')[1] : tableName);
                    using SqlDataReader fkReader = fkCmd.ExecuteReader();
                    while (fkReader.Read())
                    {
                        string col = fkReader["ColumnName"].ToString();
                        string refTable = fkReader["ReferencedTable"].ToString();
                        string refColumn = fkReader["ReferencedColumn"].ToString();
                        foreignKeys[col] = (refTable, refColumn);
                    }
                }

                // 3️⃣ Load Columns and show FK inline
                using (SqlCommand cmd = new SqlCommand(columnQuery, con))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName.Contains('.') ? tableName.Split('.')[1] : tableName);

                    using SqlDataReader reader = cmd.ExecuteReader();
                    Label header = new Label
                    {
                        Text = "Columns:",
                        Font = new Font("Segoe UI", 10, FontStyle.Bold),
                        AutoSize = true,
                        Location = new Point(10, y)
                    };
                    pnlSchema.Controls.Add(header);
                    y += 25;

                    while (reader.Read())
                    {
                        string columnName = reader["COLUMN_NAME"].ToString();
                        string dataType = reader["DATA_TYPE"].ToString();
                        string isNullable = reader["IS_NULLABLE"].ToString();
                        string length = reader["CHARACTER_MAXIMUM_LENGTH"] != DBNull.Value
                                        ? reader["CHARACTER_MAXIMUM_LENGTH"].ToString()
                                        : "";

                        bool isForeignKey = false;
                        bool isPrimaryKey = primaryKeys.Contains(columnName);
                        bool isIdentity = reader["IS_IDENTITY"] != DBNull.Value && Convert.ToInt32(reader["IS_IDENTITY"]) == 1;


                        string pkMark = isPrimaryKey ? " [PK]" : "";

                        // Column Label
                        Label lbl = new Label
                        {
                            Text = $"{columnName} ({dataType}{(string.IsNullOrEmpty(length) ? "" : $"({length})")}){pkMark}",
                            AutoSize = true,
                            Location = new Point(20, y),
                            Font = new Font("Segoe UI", 9, FontStyle.Regular)
                        };

                        if (isIdentity)
                        {
                            y += 25;
                            continue;
                        }

                        pnlSchema.Controls.Add(lbl);

                        int nextX = lbl.Right + 10;

                        // If column has FK → show two ComboBoxes
                        if (foreignKeys.ContainsKey(columnName))
                        {
                            var fkInfo = foreignKeys[columnName];

                            ComboBox cmbStartFK = new ComboBox
                            {
                                Width = 150,
                                Location = new Point(nextX, y - 2),
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                Tag = fkInfo
                            };

                            ComboBox cmbEndFK = new ComboBox
                            {
                                Width = 150,
                                Location = new Point(cmbStartFK.Right + 5, y - 2),
                                DropDownStyle = ComboBoxStyle.DropDownList,
                                Tag = fkInfo
                            };

                            using (SqlConnection fkConn = new SqlConnection(builder.ToString()))
                            {
                                fkConn.Open();
                                string sql = $"SELECT DISTINCT [{fkInfo.RefColumn}] FROM [{fkInfo.RefTable}] ORDER BY [{fkInfo.RefColumn}]";
                                using (SqlCommand fkCmd = new SqlCommand(sql, fkConn))
                                using (SqlDataReader fkReader = fkCmd.ExecuteReader())
                                {
                                    while (fkReader.Read())
                                    {
                                        string value = fkReader[0].ToString();
                                        cmbStartFK.Items.Add(value);
                                        cmbEndFK.Items.Add(value);
                                    }
                                }
                            }

                            if (cmbStartFK.Items.Count > 0) cmbStartFK.SelectedIndex = 0;
                            if (cmbEndFK.Items.Count > 0) cmbEndFK.SelectedIndex = cmbEndFK.Items.Count - 1;

                            pnlSchema.Controls.Add(cmbStartFK);
                            pnlSchema.Controls.Add(cmbEndFK);

                            nextX = cmbEndFK.Right + 10;
                            isForeignKey = true;
                        }

                        // ✅ INT range
                        if ((dataType.Equals("int", StringComparison.OrdinalIgnoreCase)
                                 || dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase))
                                && !isPrimaryKey && !isForeignKey)
                        {
                            TextBox txtStart = new TextBox
                            {
                                Width = 60,
                                Location = new Point(nextX, y - 2),
                                PlaceholderText = "Start"
                            };

                            TextBox txtEnd = new TextBox
                            {
                                Width = 60,
                                Location = new Point(txtStart.Right + 5, y - 2),
                                PlaceholderText = "End"
                            };

                            pnlSchema.Controls.Add(txtStart);
                            pnlSchema.Controls.Add(txtEnd);
                            nextX = txtEnd.Right + 10;
                        }

                        if (dataType.Equals("bit", StringComparison.OrdinalIgnoreCase))
                        {
                            TextBox txtPercentage = new TextBox
                            {
                                Width = 100,
                                Location = new Point(nextX, y - 2),
                                PlaceholderText = "Percentage of True"
                            };

                            pnlSchema.Controls.Add(txtPercentage);
                            nextX = txtPercentage.Right + 10;
                        }

                        // ✅ DATE range
                        if ((dataType.Equals("date", StringComparison.OrdinalIgnoreCase) ||
                             dataType.Equals("datetime", StringComparison.OrdinalIgnoreCase) ||
                             dataType.Equals("datetime2", StringComparison.OrdinalIgnoreCase) ||
                             dataType.Equals("smalldatetime", StringComparison.OrdinalIgnoreCase))
                             && !isPrimaryKey)
                        {
                            DateTimePicker dpStart = new DateTimePicker
                            {
                                Format = DateTimePickerFormat.Short,
                                Width = 120,
                                Location = new Point(nextX, y - 2)
                            };

                            DateTimePicker dpEnd = new DateTimePicker
                            {
                                Format = DateTimePickerFormat.Short,
                                Width = 120,
                                Location = new Point(dpStart.Right + 5, y - 2)
                            };

                            pnlSchema.Controls.Add(dpStart);
                            pnlSchema.Controls.Add(dpEnd);
                            nextX = dpEnd.Right + 10;
                        }

                        // ✅ VARCHAR / NVARCHAR
                        if ((dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase) ||
                             dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase) ||
                             dataType.Equals("char", StringComparison.OrdinalIgnoreCase) ||
                             dataType.Equals("nchar", StringComparison.OrdinalIgnoreCase))
                             && !isPrimaryKey && !isForeignKey)
                        {
                            ComboBox cmbFormat = new ComboBox
                            {
                                Width = 150,
                                Location = new Point(nextX, y - 2),
                                DropDownStyle = ComboBoxStyle.DropDownList
                            };

                            cmbFormat.Items.AddRange(new string[]
                            {
            "Random Name",
            "Random Address",
            "Random Phone",
            "Random CNIC",
            "Random Email",
            "Custom"
                            });

                            cmbFormat.SelectedIndex = 0;
                            pnlSchema.Controls.Add(cmbFormat);

                            nextX = cmbFormat.Right + 10;

                            TextBox txtCustom = new TextBox
                            {
                                Width = 150,
                                Location = new Point(nextX, y - 2),
                                PlaceholderText = "Enter custom format",
                                Visible = false
                            };

                            cmbFormat.SelectedIndexChanged += (s, e) =>
                            {
                                txtCustom.Visible = cmbFormat.SelectedItem.ToString() == "Custom";
                            };

                            pnlSchema.Controls.Add(txtCustom);
                            nextX = txtCustom.Right + 10;
                        }

                        // ✅ Nullable checkbox
                        if (isNullable.Equals("YES", StringComparison.OrdinalIgnoreCase))
                        {
                            CheckBox chkNullable = new CheckBox
                            {
                                Text = "Is Null",
                                AutoSize = true,
                                Location = new Point(nextX, y - 2),
                                Checked = false
                            };
                            pnlSchema.Controls.Add(chkNullable);
                        }

                        y += 25;
                    }




                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading schema: " + ex.Message);
            }
        }

        private void btnFeedData_Click(object sender, EventArgs e)
        {
            if (cmbDatabaes.SelectedItem == null || cmbTable.SelectedItem == null)
            {
                MessageBox.Show("Please select a database and a table.");
                return;
            }

            if (!int.TryParse(txtTotalRows.Text.Trim(), out int rowCount) || rowCount <= 0)
            {
                MessageBox.Show("Please enter a valid number of rows.");
                return;
            }

            string databaseName = cmbDatabaes.SelectedItem.ToString();
            string tableName = cmbTable.SelectedItem.ToString();
            string baseConnectionString = DbConnectionHelper.ConnectionString;

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = databaseName
            };

            using SqlConnection con = new SqlConnection(builder.ToString());
            con.Open();

            for (int i = 0; i < rowCount; i++)
            {
                var insertColumns = new List<string>();
                var insertParams = new List<string>();
                var insertValues = new List<SqlParameter>();
                int startNum = -1;
                int endNum = 0;

                foreach (Control ctrl in pnlSchema.Controls)
                {
                    if (ctrl is Label lbl && lbl.Text.Contains("("))
                    {
                        string columnName = lbl.Text.Split(' ')[0];
                        
                        string dataType = lbl.Text.Contains("(")
                            ? lbl.Text.Split('(')[1].Replace(")", "").ToLower()
                            : "varchar";
                        
                        bool isPk = lbl.Text.Contains("[PK]");


                        insertColumns.Add(columnName);

                        if (isPk)
                        {
                            var lastkey = GetMaxPrimaryKeyValue(tableName);
                        
                        
                        }


                        continue;

                        //// Controls in same row
                        //TextBox txtStart = null, txtEnd = null, txtCustom = null;
                        //ComboBox cmbBox = null;
                        //DateTimePicker dpStart = null, dpEnd = null;
                        //CheckBox chkNullable = null;

                        //foreach (Control rowCtrl in pnlSchema.Controls)
                        //{
                        //    if (rowCtrl.Location.Y == lbl.Location.Y && rowCtrl != lbl)
                        //    {
                        //        if (rowCtrl is TextBox tb)
                        //        {
                        //            if (tb.PlaceholderText == "Start") txtStart = tb;
                        //            else if (tb.PlaceholderText == "End") txtEnd = tb;
                        //            else txtCustom = tb;
                        //        }
                        //        else if (rowCtrl is ComboBox cb) cmbBox = cb;
                        //        else if (rowCtrl is DateTimePicker dp)
                        //        {
                        //            if (dpStart == null) dpStart = dp;
                        //            else dpEnd = dp;
                        //        }
                        //        else if (rowCtrl is CheckBox chk) chkNullable = chk;
                        //    }
                        //}

                        //// Default empty
                        //object insertValue = DBNull.Value;

                        //// Handle NULL checkbox
                        //if (chkNullable != null && chkNullable.Checked)
                        //{
                        //    insertValue = DBNull.Value;
                        //}
                        //else if (dataType.StartsWith("int") && txtStart != null && txtEnd != null)
                        //{
                        //    if (int.TryParse(txtStart.Text, out int s) && int.TryParse(txtEnd.Text, out int end))
                        //        insertValue = GetRandomNumber(s, end);
                        //}
                        //else if (dataType.Contains("date") && dpStart != null && dpEnd != null)
                        //{
                        //    DateTime start = dpStart.Value;
                        //    DateTime end = dpEnd.Value;
                        //    int range = (end - start).Days;
                        //    insertValue = start.AddDays(_random.Next(range + 1));
                        //}
                        //else if ((dataType.Contains("char") || dataType.Contains("text")) && cmbBox != null)
                        //{
                        //    string choice = cmbBox.SelectedItem?.ToString();

                        //    if (choice == "Random Name")
                        //        insertValue = "Name" + _random.Next(1000, 9999);
                        //    else if (choice == "Random Address")
                        //        insertValue = "Street " + _random.Next(1, 200);
                        //    else if (choice == "Random Phone")
                        //        insertValue = "03" + _random.Next(100000000, 999999999);
                        //    else if (choice == "Random CNIC")
                        //        insertValue = _random.Next(10000, 99999) + "-" + _random.Next(1000000, 9999999) + "-" + _random.Next(1, 9);
                        //    else if (choice == "Random Email")
                        //        insertValue = "user" + _random.Next(1000, 9999) + "@mail.com";
                        //    else if (choice == "Custom" && txtCustom != null)
                        //        insertValue = GenerateRandomByFormat(txtCustom.Text);
                        //}

                        // Add to insert parts



                        //insertValues.Add(new SqlParameter("@" + columnName, insertValue ?? DBNull.Value));
                    }
                    object insertValue = DBNull.Value;
                    if (ctrl is ComboBox cmb)
                    {
                        string value = cmb.SelectedItem.ToString();

                        if (value == "Random Name")
                        {
                            insertValue = "Name" + _random.Next(1000, 9999);

                        }
                        else if (value == "Random Email")
                        {
                            insertValue = "user" + _random.Next(1000, 9999) + "@mail.com";
                        }
                        else if (value == "Random Phone")
                        {
                            insertValue = "03" + _random.Next(100000000, 999999999);
                        }

                        insertParams.Add($"'{insertValue.ToString().Replace("'", "''")}'");
                        continue;

                    }

                    if (ctrl is TextBox txt)
                    {

                        if (txt.PlaceholderText == "Start")
                        {
                            startNum = Convert.ToInt32(txt.Text);
                        }
                        if (txt.PlaceholderText == "End")
                        {
                            endNum = Convert.ToInt32(txt.Text);

                        }

                        if (startNum > 0 && endNum > 0)
                        {
                            insertValue = GetRandomNumber(startNum, endNum);

                            insertParams.Add(insertValue.ToString());
                            startNum = endNum = -1;

                            continue;
                        }

                        if (txt.PlaceholderText == "Percentage of True")
                        {
                            insertParams.Add("true");
                        }
                    }


                }


                if (insertColumns.Count > 0)
                {
                    string query = $"INSERT INTO {tableName} ({string.Join(",", insertColumns)}) " +
                                   $"VALUES ({string.Join(",", insertParams)})";

                    using SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddRange(insertValues.ToArray());
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show($"{rowCount} rows inserted successfully into {tableName}.");
        }

        public object GetMaxPrimaryKeyValue(string tableName)
        {
            string databaseName = cmbDatabaes.SelectedItem.ToString();
            string baseConnectionString = DbConnectionHelper.ConnectionString;

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(baseConnectionString)
            {
                InitialCatalog = databaseName
            };

            using (var connection = new SqlConnection(builder.ToString()))
            {
                connection.Open();

                string pkQuery = @"
                        SELECT TOP 1 c.COLUMN_NAME
                        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE c
                            ON tc.CONSTRAINT_NAME = c.CONSTRAINT_NAME
                           AND tc.TABLE_SCHEMA = c.TABLE_SCHEMA
                           AND tc.TABLE_NAME = c.TABLE_NAME
                        WHERE tc.TABLE_NAME = @TableName
                          AND tc.TABLE_SCHEMA = 'dbo'
                          AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'";


                string pkColumn = null;
                using (var cmd = new SqlCommand(pkQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);
                    cmd.Parameters.AddWithValue("@Schema", "dbo");
                    pkColumn = cmd.ExecuteScalar()?.ToString();
                }

                if (string.IsNullOrEmpty(pkColumn))
                    throw new Exception($"No primary key found for table {tableName}");

                // Step 2: Build and run MAX query safely
                string safeTableName = "[" + tableName.Replace("]", "]]") + "]";
                string safeColumnName = "[" + pkColumn.Replace("]", "]]") + "]";
                string sql = $"SELECT MAX({safeColumnName}) FROM {safeTableName}";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    var maxValue = cmd.ExecuteScalar();
                    return maxValue == DBNull.Value ? null : maxValue;
                }
            }
        }



    }
}

