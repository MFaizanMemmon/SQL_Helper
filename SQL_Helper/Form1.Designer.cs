namespace SQL_Helper
{
    partial class frmConnectSQL
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmConnectSQL));
            label1 = new Label();
            label2 = new Label();
            cmbAuthentication = new ComboBox();
            label3 = new Label();
            txtUserName = new TextBox();
            label4 = new Label();
            txtPassword = new TextBox();
            chkTrustedCertificate = new CheckBox();
            btnConnect = new Button();
            cmbServer = new ComboBox();
            lblLinkConnections = new LinkLabel();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(55, 70);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 0;
            label1.Text = "Server ";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(55, 126);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(86, 15);
            label2.TabIndex = 2;
            label2.Text = "Authentication";
            // 
            // cmbAuthentication
            // 
            cmbAuthentication.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAuthentication.FormattingEnabled = true;
            cmbAuthentication.Items.AddRange(new object[] { "SQL SERVER AUTHENTICATION", "WINDOW AUTHENTICATION" });
            cmbAuthentication.Location = new Point(170, 122);
            cmbAuthentication.Margin = new Padding(2);
            cmbAuthentication.Name = "cmbAuthentication";
            cmbAuthentication.Size = new Size(254, 23);
            cmbAuthentication.TabIndex = 3;
            cmbAuthentication.SelectedIndexChanged += cmbAuthentication_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(55, 182);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(65, 15);
            label3.TabIndex = 4;
            label3.Text = "User Name";
            // 
            // txtUserName
            // 
            txtUserName.Location = new Point(170, 178);
            txtUserName.Margin = new Padding(2);
            txtUserName.Name = "txtUserName";
            txtUserName.PlaceholderText = "Please Enter User Name";
            txtUserName.ReadOnly = true;
            txtUserName.Size = new Size(254, 23);
            txtUserName.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(55, 236);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(57, 15);
            label4.TabIndex = 6;
            label4.Text = "Password";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(170, 232);
            txtPassword.Margin = new Padding(2);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.PlaceholderText = "Please Enter Password";
            txtPassword.ReadOnly = true;
            txtPassword.Size = new Size(254, 23);
            txtPassword.TabIndex = 7;
            // 
            // chkTrustedCertificate
            // 
            chkTrustedCertificate.AutoSize = true;
            chkTrustedCertificate.Checked = true;
            chkTrustedCertificate.CheckState = CheckState.Checked;
            chkTrustedCertificate.Location = new Point(170, 288);
            chkTrustedCertificate.Margin = new Padding(2);
            chkTrustedCertificate.Name = "chkTrustedCertificate";
            chkTrustedCertificate.Size = new Size(122, 19);
            chkTrustedCertificate.TabIndex = 8;
            chkTrustedCertificate.Text = "Trusted Certificate";
            chkTrustedCertificate.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(55, 340);
            btnConnect.Margin = new Padding(2);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(368, 30);
            btnConnect.TabIndex = 9;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // cmbServer
            // 
            cmbServer.FormattingEnabled = true;
            cmbServer.Location = new Point(170, 64);
            cmbServer.Margin = new Padding(3, 2, 3, 2);
            cmbServer.Name = "cmbServer";
            cmbServer.Size = new Size(254, 23);
            cmbServer.TabIndex = 10;
            cmbServer.SelectedIndexChanged += cmbServer_SelectedIndexChanged_1;
            // 
            // lblLinkConnections
            // 
            lblLinkConnections.AutoSize = true;
            lblLinkConnections.Location = new Point(349, 89);
            lblLinkConnections.Name = "lblLinkConnections";
            lblLinkConnections.Size = new Size(74, 15);
            lblLinkConnections.TabIndex = 11;
            lblLinkConnections.TabStop = true;
            lblLinkConnections.Text = "Connections";
            lblLinkConnections.LinkClicked += lblLinkConnections_LinkClicked;
            // 
            // frmConnectSQL
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(475, 417);
            Controls.Add(lblLinkConnections);
            Controls.Add(cmbServer);
            Controls.Add(btnConnect);
            Controls.Add(chkTrustedCertificate);
            Controls.Add(txtPassword);
            Controls.Add(label4);
            Controls.Add(txtUserName);
            Controls.Add(label3);
            Controls.Add(cmbAuthentication);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmConnectSQL";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Connect SQL Server";
            Load += frmConnectSQL_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label label2;
        private ComboBox cmbAuthentication;
        private Label label3;
        private TextBox txtUserName;
        private Label label4;
        private TextBox txtPassword;
        private CheckBox chkTrustedCertificate;
        private Button btnConnect;
        private ComboBox cmbServer;
        private LinkLabel lblLinkConnections;
    }
}
