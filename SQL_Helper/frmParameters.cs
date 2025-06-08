using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Data;



namespace SQL_Helper
{
    public partial class frmParameters : Form
    {
        private List<Microsoft.Data.SqlClient.SqlParameter> parameters;

        public Dictionary<string, string> ParameterValues { get; private set; } = new();

        public frmParameters(List<Microsoft.Data.SqlClient.SqlParameter> parameters)
        {
            InitializeComponent();
            this.parameters = parameters;

            // Set scroll and layout options
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.WrapContents = false;
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Padding = new Padding(10);

            // Add parameters as label + textbox
            foreach (var param in parameters)
            {
                Panel panel = new Panel
                {
                    Width = flowLayoutPanel1.Width - 25,
                    Height = 30,
                    Margin = new Padding(3)
                };

                Label lbl = new Label
                {
                    Text = param.ParameterName,
                    Width = 150,
                    Location = new System.Drawing.Point(0, 5)
                };

                Control inputControl;

                if (param.SqlDbType == SqlDbType.DateTime || param.SqlDbType == SqlDbType.Date || param.SqlDbType == SqlDbType.DateTime2)
                {
                    DateTimePicker dtPicker = new DateTimePicker
                    {
                        Name = param.ParameterName,
                        Width = 200,
                        Location = new System.Drawing.Point(160, 2),
                        Format = DateTimePickerFormat.Custom,
                        CustomFormat = "yyyy-MM-dd HH:mm:ss"
                    };
                    inputControl = dtPicker;
                }
                else
                {
                    TextBox txt = new TextBox
                    {
                        Name = param.ParameterName,
                        Width = 200,
                        Location = new System.Drawing.Point(160, 2)
                    };
                    inputControl = txt;
                }

                panel.Controls.Add(inputControl);


                panel.Controls.Add(lbl);
                panel.Controls.Add(inputControl);

                flowLayoutPanel1.Controls.Add(panel);
            }

            // Add OK button in a separate bottom panel
            FlowLayoutPanel bottomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.RightToLeft,
                Height = 50,
                Padding = new Padding(10)
            };

            Button btnOk = new Button
            {
                Text = "OK",
                Width = 80,
                Height = 30
            };
            btnOk.Click += BtnOk_Click;
            bottomPanel.Controls.Add(btnOk);

            this.Controls.Add(bottomPanel);
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            foreach (Control ctrl in flowLayoutPanel1.Controls)
            {
                if (ctrl is Panel panel)
                {
                    foreach (Control inner in panel.Controls)
                    {
                        if (inner is TextBox txt)
                            ParameterValues[txt.Name] = txt.Text;
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

}
