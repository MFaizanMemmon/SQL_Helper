using Microsoft.Data.SqlClient;
using System.Data;
namespace SQL_Helper;
public partial class frmSelectCompareDatabase : Form
{
    public frmSelectCompareDatabase() { InitializeComponent(); AppUi.Apply(this); }
    private async void frmSelectCompareDatabase_Load(object sender, EventArgs e)
    {
        button1.Enabled = false;
        try
        {
            var development = await SqlTools.DatabasesAsync(DbConnectionHelper.TargetConnectionString);
            var production = await SqlTools.DatabasesAsync(DbConnectionHelper.CompareToConnectionString);
            foreach (DataRow row in development.Rows) comboBox1.Items.Add(row[0]); foreach (DataRow row in production.Rows) comboBox2.Items.Add(row[0]);
            if (comboBox1.Items.Count > 0) comboBox1.SelectedIndex = 0; if (comboBox2.Items.Count > 0) comboBox2.SelectedIndex = 0;
            button1.Enabled = comboBox1.Items.Count > 0 && comboBox2.Items.Count > 0;
        }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private void button1_Click(object sender, EventArgs e)
    {
        if (comboBox1.SelectedItem is not string development || comboBox2.SelectedItem is not string production) { MessageBox.Show(this, "Select both databases."); return; }
        DbConnectionHelper.TargetDatabase = development; DbConnectionHelper.SourceDatabase = production;
        DbConnectionHelper.TargetConnectionString = UpdateConnectionStringDb(DbConnectionHelper.TargetConnectionString!, development);
        DbConnectionHelper.CompareToConnectionString = UpdateConnectionStringDb(DbConnectionHelper.CompareToConnectionString!, production);
        DialogResult = DialogResult.OK;
    }
    public static string UpdateConnectionStringDb(string originalConnStr, string database) => new SqlConnectionStringBuilder(originalConnStr) { InitialCatalog = database }.ConnectionString;
    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) { }
    private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) { }
}
