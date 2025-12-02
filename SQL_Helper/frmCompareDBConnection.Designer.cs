namespace SQL_Helper
{
    partial class frmCompareDBConnection
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
            btnConnect = new Button();
            chkTrustedCertificate = new CheckBox();
            txtTargetPassword = new TextBox();
            label4 = new Label();
            txtTargetUserName = new TextBox();
            label3 = new Label();
            cmbTargetAuthentication = new ComboBox();
            label2 = new Label();
            txtTargetServer = new TextBox();
            label1 = new Label();
            groupBox1 = new GroupBox();
            txtTargetDatabase = new TextBox();
            label9 = new Label();
            groupBox2 = new GroupBox();
            txtCompareDatabase = new TextBox();
            label10 = new Label();
            label5 = new Label();
            txtCompareServerName = new TextBox();
            checkBox1 = new CheckBox();
            label6 = new Label();
            txtComparePassword = new TextBox();
            txtCompareAuthType = new ComboBox();
            label7 = new Label();
            label8 = new Label();
            txtCompareUserName = new TextBox();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(10, 288);
            btnConnect.Margin = new Padding(2);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(568, 30);
            btnConnect.TabIndex = 19;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // chkTrustedCertificate
            // 
            chkTrustedCertificate.AutoSize = true;
            chkTrustedCertificate.Checked = true;
            chkTrustedCertificate.CheckState = CheckState.Checked;
            chkTrustedCertificate.Location = new Point(136, 241);
            chkTrustedCertificate.Margin = new Padding(2);
            chkTrustedCertificate.Name = "chkTrustedCertificate";
            chkTrustedCertificate.Size = new Size(122, 19);
            chkTrustedCertificate.TabIndex = 18;
            chkTrustedCertificate.Text = "Trusted Certificate";
            chkTrustedCertificate.UseVisualStyleBackColor = true;
            // 
            // txtTargetPassword
            // 
            txtTargetPassword.Location = new Point(4, 214);
            txtTargetPassword.Margin = new Padding(2);
            txtTargetPassword.Name = "txtTargetPassword";
            txtTargetPassword.PasswordChar = '*';
            txtTargetPassword.PlaceholderText = "Please Enter Password";
            txtTargetPassword.Size = new Size(254, 23);
            txtTargetPassword.TabIndex = 17;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(4, 198);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(57, 15);
            label4.TabIndex = 16;
            label4.Text = "Password";
            // 
            // txtTargetUserName
            // 
            txtTargetUserName.Location = new Point(4, 160);
            txtTargetUserName.Margin = new Padding(2);
            txtTargetUserName.Name = "txtTargetUserName";
            txtTargetUserName.PlaceholderText = "Please Enter User Name";
            txtTargetUserName.Size = new Size(254, 23);
            txtTargetUserName.TabIndex = 15;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(4, 143);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(65, 15);
            label3.TabIndex = 14;
            label3.Text = "User Name";
            // 
            // cmbTargetAuthentication
            // 
            cmbTargetAuthentication.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbTargetAuthentication.FormattingEnabled = true;
            cmbTargetAuthentication.Items.AddRange(new object[] { "SQL SERVER AUTHENTICATION", "WINDOW AUTHENTICATION" });
            cmbTargetAuthentication.Location = new Point(4, 104);
            cmbTargetAuthentication.Margin = new Padding(2);
            cmbTargetAuthentication.Name = "cmbTargetAuthentication";
            cmbTargetAuthentication.Size = new Size(254, 23);
            cmbTargetAuthentication.TabIndex = 13;
            cmbTargetAuthentication.SelectedIndexChanged += cmbTargetAuthentication_SelectedIndexChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(4, 87);
            label2.Margin = new Padding(2, 0, 2, 0);
            label2.Name = "label2";
            label2.Size = new Size(86, 15);
            label2.TabIndex = 12;
            label2.Text = "Authentication";
            // 
            // txtTargetServer
            // 
            txtTargetServer.Location = new Point(4, 48);
            txtTargetServer.Margin = new Padding(2);
            txtTargetServer.Name = "txtTargetServer";
            txtTargetServer.PlaceholderText = "Please Enter Server";
            txtTargetServer.Size = new Size(254, 23);
            txtTargetServer.TabIndex = 11;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(4, 32);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 10;
            label1.Text = "Server ";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtTargetDatabase);
            groupBox1.Controls.Add(label9);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(txtTargetServer);
            groupBox1.Controls.Add(chkTrustedCertificate);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(txtTargetPassword);
            groupBox1.Controls.Add(cmbTargetAuthentication);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(txtTargetUserName);
            groupBox1.Location = new Point(10, 9);
            groupBox1.Margin = new Padding(3, 2, 3, 2);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(3, 2, 3, 2);
            groupBox1.Size = new Size(279, 269);
            groupBox1.TabIndex = 20;
            groupBox1.TabStop = false;
            groupBox1.Text = "Target";
            // 
            // txtTargetDatabase
            // 
            txtTargetDatabase.Location = new Point(1, 313);
            txtTargetDatabase.Margin = new Padding(2);
            txtTargetDatabase.Name = "txtTargetDatabase";
            txtTargetDatabase.PlaceholderText = "Please Enter Server";
            txtTargetDatabase.Size = new Size(254, 23);
            txtTargetDatabase.TabIndex = 20;
            txtTargetDatabase.Text = "tx_db";
            txtTargetDatabase.Visible = false;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(0, 297);
            label9.Margin = new Padding(2, 0, 2, 0);
            label9.Name = "label9";
            label9.Size = new Size(55, 15);
            label9.TabIndex = 19;
            label9.Text = "Database";
            label9.Visible = false;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(txtCompareDatabase);
            groupBox2.Controls.Add(label10);
            groupBox2.Controls.Add(label5);
            groupBox2.Controls.Add(txtCompareServerName);
            groupBox2.Controls.Add(checkBox1);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(txtComparePassword);
            groupBox2.Controls.Add(txtCompareAuthType);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(txtCompareUserName);
            groupBox2.Location = new Point(307, 9);
            groupBox2.Margin = new Padding(3, 2, 3, 2);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(3, 2, 3, 2);
            groupBox2.Size = new Size(271, 269);
            groupBox2.TabIndex = 21;
            groupBox2.TabStop = false;
            groupBox2.Text = "Compare To";
            // 
            // txtCompareDatabase
            // 
            txtCompareDatabase.Location = new Point(5, 313);
            txtCompareDatabase.Margin = new Padding(2);
            txtCompareDatabase.Name = "txtCompareDatabase";
            txtCompareDatabase.PlaceholderText = "Please Enter Server";
            txtCompareDatabase.Size = new Size(254, 23);
            txtCompareDatabase.TabIndex = 21;
            txtCompareDatabase.Text = "tx_db";
            txtCompareDatabase.Visible = false;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(14, 297);
            label10.Margin = new Padding(2, 0, 2, 0);
            label10.Name = "label10";
            label10.Size = new Size(55, 15);
            label10.TabIndex = 21;
            label10.Text = "Database";
            label10.Visible = false;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(4, 32);
            label5.Margin = new Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new Size(42, 15);
            label5.TabIndex = 10;
            label5.Text = "Server ";
            // 
            // txtCompareServerName
            // 
            txtCompareServerName.Location = new Point(4, 48);
            txtCompareServerName.Margin = new Padding(2);
            txtCompareServerName.Name = "txtCompareServerName";
            txtCompareServerName.PlaceholderText = "Please Enter Server";
            txtCompareServerName.Size = new Size(254, 23);
            txtCompareServerName.TabIndex = 11;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Checked = true;
            checkBox1.CheckState = CheckState.Checked;
            checkBox1.Location = new Point(136, 241);
            checkBox1.Margin = new Padding(2);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(122, 19);
            checkBox1.TabIndex = 18;
            checkBox1.Text = "Trusted Certificate";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(4, 87);
            label6.Margin = new Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new Size(86, 15);
            label6.TabIndex = 12;
            label6.Text = "Authentication";
            // 
            // txtComparePassword
            // 
            txtComparePassword.Location = new Point(4, 214);
            txtComparePassword.Margin = new Padding(2);
            txtComparePassword.Name = "txtComparePassword";
            txtComparePassword.PasswordChar = '*';
            txtComparePassword.PlaceholderText = "Please Enter Password";
            txtComparePassword.Size = new Size(254, 23);
            txtComparePassword.TabIndex = 17;
            // 
            // txtCompareAuthType
            // 
            txtCompareAuthType.DropDownStyle = ComboBoxStyle.DropDownList;
            txtCompareAuthType.FormattingEnabled = true;
            txtCompareAuthType.Items.AddRange(new object[] { "SQL SERVER AUTHENTICATION", "WINDOW AUTHENTICATION" });
            txtCompareAuthType.Location = new Point(4, 104);
            txtCompareAuthType.Margin = new Padding(2);
            txtCompareAuthType.Name = "txtCompareAuthType";
            txtCompareAuthType.Size = new Size(254, 23);
            txtCompareAuthType.TabIndex = 13;
            txtCompareAuthType.SelectedIndexChanged += txtCompareAuthType_SelectedIndexChanged;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(4, 198);
            label7.Margin = new Padding(2, 0, 2, 0);
            label7.Name = "label7";
            label7.Size = new Size(57, 15);
            label7.TabIndex = 16;
            label7.Text = "Password";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(4, 143);
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new Size(65, 15);
            label8.TabIndex = 14;
            label8.Text = "User Name";
            // 
            // txtCompareUserName
            // 
            txtCompareUserName.Location = new Point(4, 160);
            txtCompareUserName.Margin = new Padding(2);
            txtCompareUserName.Name = "txtCompareUserName";
            txtCompareUserName.PlaceholderText = "Please Enter User Name";
            txtCompareUserName.Size = new Size(254, 23);
            txtCompareUserName.TabIndex = 15;
            // 
            // frmCompareDBConnection
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(590, 330);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(btnConnect);
            Margin = new Padding(3, 2, 3, 2);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmCompareDBConnection";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Connections";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnConnect;
        private CheckBox chkTrustedCertificate;
        private TextBox txtTargetPassword;
        private Label label4;
        private TextBox txtTargetUserName;
        private Label label3;
        private ComboBox cmbTargetAuthentication;
        private Label label2;
        private TextBox txtTargetServer;
        private Label label1;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label label5;
        private TextBox txtCompareServerName;
        private CheckBox checkBox1;
        private Label label6;
        private TextBox txtComparePassword;
        private ComboBox txtCompareAuthType;
        private Label label7;
        private Label label8;
        private TextBox txtCompareUserName;
        private Label label9;
        private Label label10;
        private TextBox txtTargetDatabase;
        private TextBox txtCompareDatabase;
    }
}