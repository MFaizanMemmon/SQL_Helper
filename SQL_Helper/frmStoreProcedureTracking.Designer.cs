namespace SQL_Helper
{
    partial class frmStoreProcedureTracking
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
            dataGridView1 = new DataGridView();
            checkedListBoxDatabses = new CheckedListBox();
            label1 = new Label();
            textBox1 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)dataGridView1).BeginInit();
            SuspendLayout();
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.BackgroundColor = Color.WhiteSmoke;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.Location = new Point(240, 80);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.RowHeadersWidth = 62;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Size = new Size(1172, 821);
            dataGridView1.TabIndex = 2;
            dataGridView1.CellClick += dataGridView1_CellClick;
            // 
            // checkedListBoxDatabses
            // 
            checkedListBoxDatabses.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            checkedListBoxDatabses.FormattingEnabled = true;
            checkedListBoxDatabses.Location = new Point(12, 12);
            checkedListBoxDatabses.Name = "checkedListBoxDatabses";
            checkedListBoxDatabses.Size = new Size(222, 900);
            checkedListBoxDatabses.TabIndex = 3;
            checkedListBoxDatabses.ItemCheck += checkedListBoxDatabses_ItemCheck;
            checkedListBoxDatabses.SelectedIndexChanged += checkedListBoxDatabses_SelectedIndexChanged;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(240, 12);
            label1.Name = "label1";
            label1.Size = new Size(64, 25);
            label1.TabIndex = 4;
            label1.Text = "Search";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(240, 40);
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "Search Store Procedure Name";
            textBox1.Size = new Size(510, 31);
            textBox1.TabIndex = 5;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // frmStoreProcedureTracking
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1438, 941);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(checkedListBoxDatabses);
            Controls.Add(dataGridView1);
            MaximizeBox = false;
            Name = "frmStoreProcedureTracking";
            Text = "Track Store Procedure";
            WindowState = FormWindowState.Maximized;
            FormClosed += frmStoreProcedureTracking_FormClosed;
            Load += frmStoreProcedureTracking_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridView1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private DataGridView dataGridView1;
        private CheckedListBox checkedListBoxDatabses;
        private Label label1;
        private TextBox textBox1;
    }
}