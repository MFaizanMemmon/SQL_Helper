namespace SQL_Helper
{
    partial class MainDashboard
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
            components = new System.ComponentModel.Container();
            listBoxDB = new ListBox();
            dataGridViewDetail = new DataGridView();
            panel1 = new Panel();
            lblTotalTable = new Label();
            label1 = new Label();
            panel2 = new Panel();
            lblTotalStorePrcedure = new Label();
            label4 = new Label();
            panel3 = new Panel();
            lblTotalViews = new Label();
            label6 = new Label();
            panel4 = new Panel();
            lblTotalTrigger = new Label();
            label8 = new Label();
            contextMenuStrip1 = new ContextMenuStrip(components);
            viewDataToolStripMenuItem = new ToolStripMenuItem();
            viewOrChangeSchemaToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)dataGridViewDetail).BeginInit();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxDB
            // 
            listBoxDB.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBoxDB.FormattingEnabled = true;
            listBoxDB.ItemHeight = 25;
            listBoxDB.Location = new Point(12, 37);
            listBoxDB.Name = "listBoxDB";
            listBoxDB.Size = new Size(270, 729);
            listBoxDB.TabIndex = 0;
            listBoxDB.SelectedIndexChanged += listBoxDB_SelectedIndexChanged;
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
            dataGridViewDetail.Location = new Point(288, 143);
            dataGridViewDetail.MultiSelect = false;
            dataGridViewDetail.Name = "dataGridViewDetail";
            dataGridViewDetail.ReadOnly = true;
            dataGridViewDetail.RowHeadersWidth = 62;
            dataGridViewDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewDetail.Size = new Size(1158, 623);
            dataGridViewDetail.TabIndex = 1;
            dataGridViewDetail.CellClick += dataGridViewTableDetail_CellClick;
            dataGridViewDetail.CellPainting += dataGridViewTableDetail_CellPainting;
            dataGridViewDetail.MouseDown += dataGridViewDetail_MouseDown;
            // 
            // panel1
            // 
            panel1.BackColor = Color.WhiteSmoke;
            panel1.Controls.Add(lblTotalTable);
            panel1.Controls.Add(label1);
            panel1.Cursor = Cursors.Hand;
            panel1.Location = new Point(288, 37);
            panel1.Name = "panel1";
            panel1.Size = new Size(285, 100);
            panel1.TabIndex = 2;
            // 
            // lblTotalTable
            // 
            lblTotalTable.AutoSize = true;
            lblTotalTable.Location = new Point(3, 61);
            lblTotalTable.Name = "lblTotalTable";
            lblTotalTable.Size = new Size(22, 25);
            lblTotalTable.TabIndex = 1;
            lblTotalTable.Text = "0";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 15);
            label1.Name = "label1";
            label1.Size = new Size(94, 25);
            label1.TabIndex = 0;
            label1.Text = "Total Table";
            // 
            // panel2
            // 
            panel2.BackColor = Color.WhiteSmoke;
            panel2.Controls.Add(lblTotalStorePrcedure);
            panel2.Controls.Add(label4);
            panel2.Cursor = Cursors.Hand;
            panel2.Location = new Point(579, 37);
            panel2.Name = "panel2";
            panel2.Size = new Size(285, 100);
            panel2.TabIndex = 3;
            panel2.Click += panel2_Click;
            panel2.Paint += panel2_Paint;
            // 
            // lblTotalStorePrcedure
            // 
            lblTotalStorePrcedure.AutoSize = true;
            lblTotalStorePrcedure.Location = new Point(13, 61);
            lblTotalStorePrcedure.Name = "lblTotalStorePrcedure";
            lblTotalStorePrcedure.Size = new Size(22, 25);
            lblTotalStorePrcedure.TabIndex = 3;
            lblTotalStorePrcedure.Text = "0";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(3, 15);
            label4.Name = "label4";
            label4.Size = new Size(180, 25);
            label4.TabIndex = 2;
            label4.Text = "Total Store Procedure";
            // 
            // panel3
            // 
            panel3.BackColor = Color.WhiteSmoke;
            panel3.Controls.Add(lblTotalViews);
            panel3.Controls.Add(label6);
            panel3.Cursor = Cursors.Hand;
            panel3.Location = new Point(870, 37);
            panel3.Name = "panel3";
            panel3.Size = new Size(285, 100);
            panel3.TabIndex = 4;
            panel3.Click += panel3_Click;
            // 
            // lblTotalViews
            // 
            lblTotalViews.AutoSize = true;
            lblTotalViews.Location = new Point(3, 61);
            lblTotalViews.Name = "lblTotalViews";
            lblTotalViews.Size = new Size(22, 25);
            lblTotalViews.TabIndex = 3;
            lblTotalViews.Text = "0";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(3, 15);
            label6.Name = "label6";
            label6.Size = new Size(91, 25);
            label6.TabIndex = 2;
            label6.Text = "Total View";
            // 
            // panel4
            // 
            panel4.BackColor = Color.WhiteSmoke;
            panel4.Controls.Add(lblTotalTrigger);
            panel4.Controls.Add(label8);
            panel4.Cursor = Cursors.Hand;
            panel4.Location = new Point(1161, 37);
            panel4.Name = "panel4";
            panel4.Size = new Size(285, 100);
            panel4.TabIndex = 5;
            panel4.Click += panel4_Click;
            // 
            // lblTotalTrigger
            // 
            lblTotalTrigger.AutoSize = true;
            lblTotalTrigger.Location = new Point(3, 61);
            lblTotalTrigger.Name = "lblTotalTrigger";
            lblTotalTrigger.Size = new Size(22, 25);
            lblTotalTrigger.TabIndex = 3;
            lblTotalTrigger.Text = "0";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(3, 15);
            label8.Name = "label8";
            label8.Size = new Size(108, 25);
            label8.TabIndex = 2;
            label8.Text = "Total Trigger";
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { viewDataToolStripMenuItem, viewOrChangeSchemaToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(276, 101);
            // 
            // viewDataToolStripMenuItem
            // 
            viewDataToolStripMenuItem.Name = "viewDataToolStripMenuItem";
            viewDataToolStripMenuItem.Size = new Size(275, 32);
            viewDataToolStripMenuItem.Text = "View Data";
            viewDataToolStripMenuItem.Click += viewDataToolStripMenuItem_Click;
            // 
            // viewOrChangeSchemaToolStripMenuItem
            // 
            viewOrChangeSchemaToolStripMenuItem.Name = "viewOrChangeSchemaToolStripMenuItem";
            viewOrChangeSchemaToolStripMenuItem.Size = new Size(275, 32);
            viewOrChangeSchemaToolStripMenuItem.Text = "View or Change Schema";
            // 
            // MainDashboard
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1457, 778);
            Controls.Add(panel4);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(dataGridViewDetail);
            Controls.Add(listBoxDB);
            Name = "MainDashboard";
            Text = "Dashboard";
            WindowState = FormWindowState.Maximized;
            Load += MainDashboard_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewDetail).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBoxDB;
        private DataGridView dataGridViewDetail;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private Panel panel4;
        private Label lblTotalTable;
        private Label label1;
        private Label lblTotalStorePrcedure;
        private Label label4;
        private Label lblTotalViews;
        private Label label6;
        private Label lblTotalTrigger;
        private Label label8;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem viewDataToolStripMenuItem;
        private ToolStripMenuItem viewOrChangeSchemaToolStripMenuItem;
    }
}