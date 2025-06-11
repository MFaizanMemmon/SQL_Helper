using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace SQL_Helper
{
    public partial class frmViewPagesOnPosWorker : Form
    {
        public frmViewPagesOnPosWorker()
        {
            InitializeComponent();
        }

        private void frmViewPagesOnPosWorker_Load(object sender, EventArgs e)
        {
            CenterLoader();
            panel1.AutoScroll = true;
        }

        private void CenterLoader()
        {
            if (pictureBox1.Image == null)
                return;

            int x = (this.ClientSize.Width - pictureBox1.Width) / 2;
            int y = (this.ClientSize.Height - pictureBox1.Height) / 2;
            pictureBox1.Location = new Point(x, y);
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            string folderPath = SelectFolder();
            CheckModulesXml(folderPath);
            panel1.AutoScroll = true;
        }

        public string SelectFolder()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select the folder containing modules.xml";
                dialog.ShowNewFolderButton = false;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox1.Visible = true;
                    txtBrowse.Text = dialog.SelectedPath;
                    return dialog.SelectedPath;
                }

                return null;
            }
        }

        public void CheckModulesXml(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                MessageBox.Show("No folder selected.");
                return;
            }

            string xmlFilePath = Path.Combine(folderPath, "XMLFiles", "modules.xml");

            if (File.Exists(xmlFilePath))
            {
                
                LoadModulesToPanel(xmlFilePath);
            }
            else
            {
                MessageBox.Show("❌ modules.xml not found in the selected folder.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public void LoadModulesToPanel(string xmlFilePath)
        {
          
            panel1.Controls.Clear();

            // Add Search Label
            Label lblSearch = new Label
            {
                Text = "Search:",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };
            panel1.Controls.Add(lblSearch);

            // Add Search TextBox
            TextBox txtSearch = new TextBox
            {
                Name = "txtSearchBox",
                Width = 200,
                Location = new Point(lblSearch.Right + 10, 8)
            };
            panel1.Controls.Add(txtSearch);

            int y = txtSearch.Bottom + 10;

            // Load XML and parse modules
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlFilePath);

            XmlNodeList moduleNodes = doc.SelectNodes("//module");

            foreach (XmlNode module in moduleNodes)
            {
                if (!string.IsNullOrEmpty(module.Attributes["Name"]?.Value))
                {
                    AddModuleLabel(module, isChild: false, ref y);
                }

                XmlNodeList subModules = module.SelectNodes("SubModule");
                foreach (XmlNode subModule in subModules)
                {
                    if (!string.IsNullOrEmpty(subModule.Attributes["Name"]?.Value))
                    {
                        AddModuleLabel(subModule, isChild: true, ref y);
                    }
                }
            }

            pictureBox1.Visible = false;
        }

        private void AddModuleLabel(XmlNode node, bool isChild, ref int y)
        {
            string name = node.Attributes["Name"]?.Value;
            string desc = node.Attributes["Description"]?.Value ?? "";
            string url = node.Attributes["url"]?.Value ?? "";

            if (string.IsNullOrEmpty(name))
                return;

            Label lbl = new Label
            {
                Text = $"Name: {name}",
                AutoSize = true,
                Font = new Font("Segoe UI", 9, isChild ? FontStyle.Regular : FontStyle.Bold),
                ForeColor = Color.Black,
                Location = new Point(isChild ? 30 : 10, y),
                Cursor = Cursors.Hand,
                Tag = "normal",
                AccessibleDescription = url // Store the original URL here
            };

            lbl.Click += (s, e) =>
            {
                // Reset other labels
                foreach (Control control in panel1.Controls)
                {
                    if (control is Label otherLbl)
                    {
                        bool isBold = otherLbl.Font.Bold;
                        otherLbl.Font = new Font("Segoe UI", 9,
                            isBold ? FontStyle.Bold : FontStyle.Regular);
                        otherLbl.ForeColor = Color.Black;
                        otherLbl.Tag = "normal";
                    }
                }

                // Style the clicked label
                lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold | FontStyle.Underline);
                lbl.ForeColor = Color.Blue;
                lbl.Tag = "active";

                // Get the original URL
                string rawUrl = lbl.AccessibleDescription;

                if (!string.IsNullOrEmpty(rawUrl))
                {
                    string basePath = txtBrowse.Text;
                    if (!string.IsNullOrEmpty(basePath))
                    {
                        // Clean ~ and convert slashes
                        string relativePath = rawUrl.Replace("~", "").Replace("/", "\\");
                        string[] pathParts = relativePath.Split('\\');

                        // Remove the last part (e.g., "HistoryLog")
                        if (pathParts.Length > 1)
                        {
                            string trimmedPath = string.Join("\\", pathParts, 0, pathParts.Length - 1);
                            string fullPath = Path.Combine(basePath, trimmedPath);

                            if (Directory.Exists(fullPath))
                            {
                                // Pick first .txt or .xml file
                                string[] files = Directory.GetFiles(fullPath, "*.txt");
                                if (files.Length == 0)
                                    files = Directory.GetFiles(fullPath, "*.xml");

                                if (files.Length > 0)
                                {
                                    string fileContent = File.ReadAllText(files[0]);
                                    richTextBox1.Text = fileContent;
                                }
                                else
                                {
                                    MessageBox.Show("❌ No .txt or .xml file found in:\n" + fullPath);
                                }
                            }
                            else
                            {
                                MessageBox.Show("❌ Directory does not exist:\n" + fullPath);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("❗ Please select a base folder first.");
                    }
                }
                else
                {
                    MessageBox.Show("❌ No URL assigned to this module.");
                }
            };

            panel1.Controls.Add(lbl);
            y += lbl.Height + 5;
        }


    }
}
