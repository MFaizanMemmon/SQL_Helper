namespace SQL_Helper
{
    partial class frmViewPagesOnPosWorker
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmViewPagesOnPosWorker));
            groupBox1 = new GroupBox();
            btnBrowse = new Button();
            txtBrowse = new TextBox();
            label1 = new Label();
            panel1 = new Panel();
            richTextBox1 = new RichTextBox();
            pictureBox1 = new PictureBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnBrowse);
            groupBox1.Controls.Add(txtBrowse);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(576, 89);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Upload only POS Worker";
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(367, 56);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(147, 29);
            btnBrowse.TabIndex = 2;
            btnBrowse.Text = "Brows";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // txtBrowse
            // 
            txtBrowse.Location = new Point(6, 56);
            txtBrowse.Name = "txtBrowse";
            txtBrowse.ReadOnly = true;
            txtBrowse.Size = new Size(355, 27);
            txtBrowse.TabIndex = 1;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 36);
            label1.Name = "label1";
            label1.Size = new Size(212, 20);
            label1.TabIndex = 0;
            label1.Text = "Folder Location of POS Worker";
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.AutoScroll = true;
            panel1.Location = new Point(12, 107);
            panel1.Name = "panel1";
            panel1.Size = new Size(576, 488);
            panel1.TabIndex = 1;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.Location = new Point(594, 107);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(573, 488);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(460, 194);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(258, 232);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            pictureBox1.Visible = false;
            // 
            // frmViewPagesOnPosWorker
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1179, 621);
            Controls.Add(pictureBox1);
            Controls.Add(richTextBox1);
            Controls.Add(panel1);
            Controls.Add(groupBox1);
            Name = "frmViewPagesOnPosWorker";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "POS Worker";
            WindowState = FormWindowState.Maximized;
            Load += frmViewPagesOnPosWorker_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private Button btnBrowse;
        private TextBox txtBrowse;
        private Label label1;
        private Panel panel1;
        private RichTextBox richTextBox1;
        private PictureBox pictureBox1;
    }
}