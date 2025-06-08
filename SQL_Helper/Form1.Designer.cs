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
            label1 = new Label();
            txtServer = new TextBox();
            label2 = new Label();
            cmbAuthentication = new ComboBox();
            label3 = new Label();
            txtUserName = new TextBox();
            label4 = new Label();
            txtPassword = new TextBox();
            chkTrustedCertificate = new CheckBox();
            btnConnect = new Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(79, 116);
            label1.Name = "label1";
            label1.Size = new Size(66, 25);
            label1.TabIndex = 0;
            label1.Text = "Server ";
            // 
            // txtServer
            // 
            txtServer.Location = new Point(243, 110);
            txtServer.Name = "txtServer";
            txtServer.PlaceholderText = "Please Enter Server";
            txtServer.Size = new Size(362, 31);
            txtServer.TabIndex = 1;
            txtServer.Text = "DESKTOP-VUPCLF0";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(79, 210);
            label2.Name = "label2";
            label2.Size = new Size(127, 25);
            label2.TabIndex = 2;
            label2.Text = "Authentication";
            // 
            // cmbAuthentication
            // 
            cmbAuthentication.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAuthentication.FormattingEnabled = true;
            cmbAuthentication.Items.AddRange(new object[] { "SQL SERVER AUTHENTICATION", "WINDOW AUTHENTICATION" });
            cmbAuthentication.Location = new Point(243, 202);
            cmbAuthentication.Name = "cmbAuthentication";
            cmbAuthentication.Size = new Size(362, 33);
            cmbAuthentication.TabIndex = 3;
            cmbAuthentication.SelectedIndexChanged += cmbAuthentication_SelectedIndexChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(79, 302);
            label3.Name = "label3";
            label3.Size = new Size(99, 25);
            label3.TabIndex = 4;
            label3.Text = "User Name";
            // 
            // txtUserName
            // 
            txtUserName.Location = new Point(243, 296);
            txtUserName.Name = "txtUserName";
            txtUserName.PlaceholderText = "Please Enter User Name";
            txtUserName.ReadOnly = true;
            txtUserName.Size = new Size(362, 31);
            txtUserName.TabIndex = 5;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(79, 394);
            label4.Name = "label4";
            label4.Size = new Size(87, 25);
            label4.TabIndex = 6;
            label4.Text = "Password";
            // 
            // txtPassword
            // 
            txtPassword.Location = new Point(243, 388);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.PlaceholderText = "Please Enter Password";
            txtPassword.ReadOnly = true;
            txtPassword.Size = new Size(362, 31);
            txtPassword.TabIndex = 7;
            // 
            // chkTrustedCertificate
            // 
            chkTrustedCertificate.AutoSize = true;
            chkTrustedCertificate.Location = new Point(243, 480);
            chkTrustedCertificate.Name = "chkTrustedCertificate";
            chkTrustedCertificate.Size = new Size(178, 29);
            chkTrustedCertificate.TabIndex = 8;
            chkTrustedCertificate.Text = "Trusted Certificate";
            chkTrustedCertificate.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            btnConnect.Location = new Point(79, 567);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new Size(526, 50);
            btnConnect.TabIndex = 9;
            btnConnect.Text = "Connect";
            btnConnect.UseVisualStyleBackColor = true;
            btnConnect.Click += btnConnect_Click;
            // 
            // frmConnectSQL
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(679, 667);
            Controls.Add(btnConnect);
            Controls.Add(chkTrustedCertificate);
            Controls.Add(txtPassword);
            Controls.Add(label4);
            Controls.Add(txtUserName);
            Controls.Add(label3);
            Controls.Add(cmbAuthentication);
            Controls.Add(label2);
            Controls.Add(txtServer);
            Controls.Add(label1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmConnectSQL";
            ShowIcon = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Connect SQL Server";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtServer;
        private Label label2;
        private ComboBox cmbAuthentication;
        private Label label3;
        private TextBox txtUserName;
        private Label label4;
        private TextBox txtPassword;
        private CheckBox chkTrustedCertificate;
        private Button btnConnect;
    }
}
