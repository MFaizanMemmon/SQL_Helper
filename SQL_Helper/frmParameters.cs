using System.Data;
using Microsoft.Data.SqlClient;
namespace SQL_Helper;
public partial class frmParameters : Form
{
    private readonly List<SqlParameter> parameters;
    private readonly Dictionary<string, (Control Input, CheckBox Null, CheckBox Default)> inputs = new();
    public Dictionary<string, string> ParameterValues { get; private set; } = new();
    public Dictionary<string, object> TypedValues { get; private set; } = new();
    public frmParameters(List<SqlParameter> parameters)
    {
        InitializeComponent(); this.parameters = parameters;
        flowLayoutPanel1.FlowDirection = FlowDirection.TopDown; flowLayoutPanel1.WrapContents = false; flowLayoutPanel1.AutoScroll = true; flowLayoutPanel1.Dock = DockStyle.Fill;
        foreach (var parameter in parameters.Where(p => p.Direction != ParameterDirection.Output))
        {
            Control input = parameter.SqlDbType is SqlDbType.Date or SqlDbType.DateTime or SqlDbType.DateTime2 or SqlDbType.SmallDateTime ? new DateTimePicker { Format = DateTimePickerFormat.Custom, CustomFormat = "yyyy-MM-dd HH:mm:ss" } : new TextBox { PlaceholderText = parameter.SqlDbType == SqlDbType.Structured ? "JSON array, e.g. [{\"Id\":1}]" : parameter.SqlDbType.ToString() };
            input.Name = parameter.ParameterName;
            var field = AppUi.MakeField($"{parameter.ParameterName} • {parameter.SqlDbType}", input);
            var nil = new CheckBox { Text = "NULL", AutoSize = true, Checked = parameter.Direction == ParameterDirection.InputOutput, Dock = DockStyle.Bottom, Enabled = parameter.SqlDbType != SqlDbType.Structured };
            var omit = new CheckBox { Text = "Use procedure default (omit parameter)", AutoSize = true, Dock = DockStyle.Bottom };
            void UpdateInput() { input.Enabled = !nil.Checked && !omit.Checked; nil.Enabled = !omit.Checked && parameter.SqlDbType != SqlDbType.Structured; }
            nil.CheckedChanged += (_, _) => UpdateInput(); omit.CheckedChanged += (_, _) => UpdateInput(); UpdateInput();
            var panel = new Panel { Height = 132, Margin = new Padding(0, 0, 0, 8) }; field.Dock = DockStyle.Top; panel.Controls.Add(field); panel.Controls.Add(nil); panel.Controls.Add(omit);
            flowLayoutPanel1.Controls.Add(panel); inputs.Add(parameter.ParameterName, (input, nil, omit));
        }
        flowLayoutPanel1.SizeChanged += (_, _) => { foreach (Control field in flowLayoutPanel1.Controls) field.Width = Math.Max(180, flowLayoutPanel1.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 24); };
        var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 52 };
        var confirm = new Button { Text = "Use parameters", Name = "confirmParameters", AutoSize = true }; confirm.Click += BtnOk_Click; actions.Controls.Add(confirm); Controls.Add(actions); AcceptButton = confirm; AppUi.Apply(this);
    }
    private void BtnOk_Click(object? sender, EventArgs e)
    {
        try
        {
            var values = new Dictionary<string, object>();
            foreach (var parameter in parameters)
            {
                if (parameter.Direction == ParameterDirection.Output) { values[parameter.ParameterName] = DBNull.Value; continue; }
                var (input, nil, omit) = inputs[parameter.ParameterName]; if (omit.Checked) continue; string text = input is DateTimePicker date ? date.Value.ToString("O") : input.Text;
                values[parameter.ParameterName] = ParameterValuesParser(parameter, text, nil.Checked); ParameterValues[parameter.ParameterName] = text;
            }
            TypedValues = values; DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { SqlTools.Error(this, ex); }
    }
    private static object ParameterValuesParser(SqlParameter parameter, string value, bool nil) => SQL_Helper.ParameterValues.Parse(parameter, value, nil);
}
