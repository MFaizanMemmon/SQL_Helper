namespace SQL_Helper
{
    partial class ViewData
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewData));
            dataGridView1 = new DataGridView();
            btnExportExcel = new Button();
            BtnExportCSV = new Button();
            label1 = new Label();
            textBox1 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.WhiteSmoke;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(8, 46);
            dataGridView1.Margin = new Padding(2);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1049, 479);
            dataGridView1.TabIndex = 0;
            // 
            // btnExportExcel
            // 
            btnExportExcel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportExcel.Location = new Point(939, 17);
            btnExportExcel.Margin = new Padding(2);
            btnExportExcel.Name = "btnExportExcel";
            btnExportExcel.Size = new Size(119, 20);
            btnExportExcel.TabIndex = 1;
            btnExportExcel.Text = "Export Excel";
            btnExportExcel.UseVisualStyleBackColor = true;
            btnExportExcel.Click += btnExportExcel_Click;
            // 
            // BtnExportCSV
            // 
            BtnExportCSV.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnExportCSV.Location = new Point(816, 17);
            BtnExportCSV.Margin = new Padding(2);
            BtnExportCSV.Name = "BtnExportCSV";
            BtnExportCSV.Size = new Size(119, 20);
            BtnExportCSV.TabIndex = 2;
            BtnExportCSV.Text = "Export CSV";
            BtnExportCSV.UseVisualStyleBackColor = true;
            BtnExportCSV.Click += BtnExportCSV_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 20);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 3;
            label1.Text = "Search";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(57, 19);
            textBox1.Margin = new Padding(2);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(383, 23);
            textBox1.TabIndex = 4;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // ViewData
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1066, 532);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(BtnExportCSV);
            Controls.Add(btnExportExcel);
            Controls.Add(dataGridView1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(2);
            MaximizeBox = false;
            Name = "ViewData";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ViewData";
            Load += ViewData_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private DataGridView dataGridView1;
        private Button btnExportExcel;
        private Button BtnExportCSV;
        private Label label1;
        private TextBox textBox1;
    }
}