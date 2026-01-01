namespace SQL_Helper
{
    partial class frmManageConnections
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmManageConnections));
            chkTrustedCertificate = new CheckBox();
            txtPassword = new TextBox();
            label4 = new Label();
            txtUserName = new TextBox();
            label3 = new Label();
            cmbAuthentication = new ComboBox();
            label2 = new Label();
            label1 = new Label();
            groupBox1 = new GroupBox();
            txtServer = new TextBox();
            btnDelete = new Button();
            btnConnect = new Button();
            dataGridViewDetail = new DataGridView();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewDetail).BeginInit();
            SuspendLayout();
            // 
            // chkTrustedCertificate
            // 
            chkTrustedCertificate.AutoSize = true;
            chkTrustedCertificate.Checked = true;
            chkTrustedCertificate.CheckState = CheckState.Checked;
            chkTrustedCertificate.Location = new Point(138, 139);
            chkTrustedCertificate.Margin = new Padding(2);
            chkTrustedCertificate.Name = "chkTrustedCertificate";
            chkTrustedCertificate.Size = new Size(122, 19);
            chkTrustedCertificate.TabIndex = 18;
            chkTrustedCertificate.Text = "Trusted Certificate";
            chkTrustedCertificate.UseVisualStyleBackColor = true;
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(297, 105);
            txtPassword.Margin = new Padding(2);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.PlaceholderText = "Please Enter Password";
            txtPassword.ReadOnly = true;
            txtPassword.Size = new Size(254, 23);
            txtPassword.TabIndex = 17;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(297, 88);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(57, 15);
            label4.TabIndex = 16;
            label4.Text = "Password";
            // 
            // txtUserName
            // 
            txtUserName.Location = new Point(6, 105);
            txtUserName.Margin = new Padding(2);
            txtUserName.Name = "txtUserName";
            txtUserName.PlaceholderText = "Please Enter User Name";
            txtUserName.ReadOnly = true;
            txtUserName.Size = new Size(254, 23);
            txtUserName.TabIndex = 15;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 88);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(65, 15);
            label3.TabIndex = 14;
            label3.Text = "User Name";
            // 
            // cmbAuthentication
            // 
            cmbAuthentication.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAuthentication.FormattingEnabled = true;
            cmbAuthentication.Items.AddRange(new object[] { "SQL SERVER AUTHENTICATION", "WINDOW AUTHENTICATION" });
            cmbAuthentication.Location = new Point(297, 46);
            cmbAuthentication.Margin = new Padding(2);
            cmbAuthentication.Name = "cmbAuthentication";
            cmbAuthentication.Size = new Size(254, 23);
            cmbAuthentication.TabIndex = 13;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(297, 29);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(86, 15);
            label2.TabIndex = 12;
            label2.Text = "Authentication";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(5, 29);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 11;
            label1.Text = "Server ";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtServer);
            groupBox1.Controls.Add(btnDelete);
            groupBox1.Controls.Add(btnConnect);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(chkTrustedCertificate);
            groupBox1.Controls.Add(txtPassword);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(cmbAuthentication);
            groupBox1.Controls.Add(txtUserName);
            groupBox1.Controls.Add(label3);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(567, 173);
            groupBox1.TabIndex = 20;
            groupBox1.TabStop = false;
            groupBox1.Text = "Information";
            // 
            // txtServer
            // 
            txtServer.Location = new Point(6, 46);
            txtServer.Margin = new Padding(2);
            txtServer.Name = "txtServer";
            txtServer.PlaceholderText = "Please Enter Server IP";
            txtServer.ReadOnly = true;
            txtServer.Size = new Size(254, 23);
            txtServer.TabIndex = 23;
            // 
            // btnDelete
            // 
            btnDelete.Location = new Point(454, 132);
            btnDelete.Margin = new Padding(2);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(97, 30);
            btnDelete.TabIndex = 22;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(338, 132);
            btnConnect.Margin = new Padding(2);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(112, 30);
            btnConnect.TabIndex = 21;
            btnConnect.Text = "Connect && Save";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // dataGridViewDetail
            // 
            dataGridViewDetail.AllowUserToAddRows = false;
            dataGridViewDetail.AllowUserToDeleteRows = false;
            dataGridViewDetail.AllowUserToOrderColumns = true;
            dataGridViewDetail.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewDetail.BackgroundColor = Color.WhiteSmoke;
            dataGridViewDetail.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewDetail.Cursor = Cursors.Hand;
            dataGridViewDetail.Location = new Point(11, 190);
            dataGridViewDetail.Margin = new Padding(2);
            dataGridViewDetail.MultiSelect = false;
            dataGridViewDetail.Name = "dataGridViewDetail";
            dataGridViewDetail.ReadOnly = true;
            dataGridViewDetail.RowHeadersWidth = 62;
            dataGridViewDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewDetail.Size = new Size(570, 263);
            dataGridViewDetail.TabIndex = 21;
            // 
            // frmManageConnections
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(592, 464);
            Controls.Add(dataGridViewDetail);
            Controls.Add(groupBox1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmManageConnections";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Manage Connection";
            Load += frmManageConnections_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewDetail).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private CheckBox chkTrustedCertificate;
        private TextBox txtPassword;
        private Label label4;
        private TextBox txtUserName;
        private Label label3;
        private ComboBox cmbAuthentication;
        private Label label2;
        private Label label1;
        private GroupBox groupBox1;
        private Button btnDelete;
        private Button btnConnect;
        private TextBox txtServer;
        private DataGridView dataGridViewDetail;
    }
}