namespace SQL_Helper
{
    partial class frmDummyData
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
            groupBox1 = new GroupBox();
            btnViewSchema = new Button();
            cmbTable = new ComboBox();
            label2 = new Label();
            cmbDatabaes = new ComboBox();
            label1 = new Label();
            pnlSchema = new Panel();
            label3 = new Label();
            txtTotalRows = new TextBox();
            btnFeedData = new Button();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnViewSchema);
            groupBox1.Controls.Add(cmbTable);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(cmbDatabaes);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(776, 100);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Information";
            // 
            // btnViewSchema
            // 
            btnViewSchema.Location = new Point(621, 48);
            btnViewSchema.Name = "btnViewSchema";
            btnViewSchema.Size = new Size(135, 23);
            btnViewSchema.TabIndex = 4;
            btnViewSchema.Text = "View Schema";
            btnViewSchema.UseVisualStyleBackColor = true;
            btnViewSchema.Click += btnViewSchema_Click;
            // 
            // cmbTable
            // 
            cmbTable.FormattingEnabled = true;
            cmbTable.Location = new Point(320, 48);
            cmbTable.Name = "cmbTable";
            cmbTable.Size = new Size(283, 23);
            cmbTable.TabIndex = 3;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(320, 30);
            label2.Name = "label2";
            label2.Size = new Size(40, 15);
            label2.TabIndex = 2;
            label2.Text = "Tables";
            // 
            // cmbDatabaes
            // 
            cmbDatabaes.FormattingEnabled = true;
            cmbDatabaes.Location = new Point(26, 48);
            cmbDatabaes.Name = "cmbDatabaes";
            cmbDatabaes.Size = new Size(283, 23);
            cmbDatabaes.TabIndex = 1;
            cmbDatabaes.SelectedIndexChanged += cmbDatabaes_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(26, 30);
            label1.Name = "label1";
            label1.Size = new Size(60, 15);
            label1.TabIndex = 0;
            label1.Text = "Databases";
            // 
            // pnlSchema
            // 
            pnlSchema.Location = new Point(12, 118);
            pnlSchema.Name = "pnlSchema";
            pnlSchema.Size = new Size(776, 458);
            pnlSchema.TabIndex = 1;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 590);
            label3.Name = "label3";
            label3.Size = new Size(157, 15);
            label3.TabIndex = 2;
            label3.Text = "Numbers of Rows Genereate";
            // 
            // txtTotalRows
            // 
            txtTotalRows.Location = new Point(175, 582);
            txtTotalRows.Name = "txtTotalRows";
            txtTotalRows.Size = new Size(197, 23);
            txtTotalRows.TabIndex = 3;
            // 
            // btnFeedData
            // 
            btnFeedData.Location = new Point(676, 586);
            btnFeedData.Name = "btnFeedData";
            btnFeedData.Size = new Size(112, 23);
            btnFeedData.TabIndex = 4;
            btnFeedData.Text = "Feed Data";
            btnFeedData.UseVisualStyleBackColor = true;
            btnFeedData.Click += btnFeedData_Click;
            // 
            // frmDummyData
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(800, 637);
            Controls.Add(btnFeedData);
            Controls.Add(txtTotalRows);
            Controls.Add(label3);
            Controls.Add(pnlSchema);
            Controls.Add(groupBox1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmDummyData";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Create Data";
            Load += frmDummyData_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private Button btnViewSchema;
        private ComboBox cmbTable;
        private Label label2;
        private ComboBox cmbDatabaes;
        private Label label1;
        private Panel pnlSchema;
        private Label label3;
        private TextBox txtTotalRows;
        private Button btnFeedData;
    }
}