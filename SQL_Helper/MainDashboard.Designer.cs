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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainDashboard));
            listBoxDB = new ListBox();
            dataGridViewDetail = new DataGridView();
            panel1 = new Panel();
            btnTable = new Button();
            label1 = new Label();
            pictureBox4 = new PictureBox();
            lblTotalTable = new Label();
            panel2 = new Panel();
            btnShowProcedures = new Button();
            lblTotalStorePrcedure = new Label();
            label4 = new Label();
            pictureBox3 = new PictureBox();
            panel3 = new Panel();
            btnShowView = new Button();
            lblTotalViews = new Label();
            label6 = new Label();
            pictureBox2 = new PictureBox();
            panel4 = new Panel();
            btnShowTriggers = new Button();
            lblTotalTrigger = new Label();
            label8 = new Label();
            pictureBox1 = new PictureBox();
            contextMenuStrip1 = new ContextMenuStrip(components);
            viewDataToolStripMenuItem = new ToolStripMenuItem();
            viewOrChangeSchemaToolStripMenuItem = new ToolStripMenuItem();
            toolStrip1 = new ToolStrip();
            toolStripButton2 = new ToolStripButton();
            toolStripButton3 = new ToolStripSeparator();
            toolStripButton1 = new ToolStripButton();
            toolStripSeparator4 = new ToolStripSeparator();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripButton7 = new ToolStripButton();
            toolStripButton4 = new ToolStripButton();
            toolStripButton5 = new ToolStripButton();
            toolStripSeparator2 = new ToolStripSeparator();
            toolStripButton6 = new ToolStripButton();
            toolStripSeparator3 = new ToolStripSeparator();
            toolStripTextBox1 = new ToolStripTextBox();
            label2 = new Label();
            textBox1 = new TextBox();
            panel5 = new Panel();
            btnShowTypes = new Button();
            label3 = new Label();
            label5 = new Label();
            pictureBox5 = new PictureBox();
            pictureBox6 = new PictureBox();
            panel6 = new Panel();
            lblTotalRowsCount = new Label();
            label7 = new Label();
            ((System.ComponentModel.ISupportInitialize)dataGridViewDetail).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            contextMenuStrip1.SuspendLayout();
            toolStrip1.SuspendLayout();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            panel6.SuspendLayout();
            SuspendLayout();
            // 
            // listBoxDB
            // 
            listBoxDB.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            listBoxDB.FormattingEnabled = true;
            listBoxDB.ItemHeight = 15;
            listBoxDB.Location = new Point(7, 109);
            listBoxDB.Margin = new Padding(2);
            listBoxDB.Name = "listBoxDB";
            listBoxDB.Size = new Size(190, 574);
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
            dataGridViewDetail.Location = new Point(200, 222);
            dataGridViewDetail.Margin = new Padding(2);
            dataGridViewDetail.MultiSelect = false;
            dataGridViewDetail.Name = "dataGridViewDetail";
            dataGridViewDetail.ReadOnly = true;
            dataGridViewDetail.RowHeadersWidth = 62;
            dataGridViewDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewDetail.Size = new Size(1013, 461);
            dataGridViewDetail.TabIndex = 1;
            dataGridViewDetail.CellClick += dataGridViewTableDetail_CellClick;
            dataGridViewDetail.CellPainting += dataGridViewTableDetail_CellPainting;
            dataGridViewDetail.MouseDown += dataGridViewDetail_MouseDown;
            // 
            // panel1
            // 
            panel1.BackColor = Color.WhiteSmoke;
            panel1.Controls.Add(btnTable);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(pictureBox4);
            panel1.Controls.Add(lblTotalTable);
            panel1.Cursor = Cursors.Hand;
            panel1.Location = new Point(199, 109);
            panel1.Margin = new Padding(2);
            panel1.Name = "panel1";
            panel1.Size = new Size(200, 60);
            panel1.TabIndex = 2;
            panel1.Paint += panel1_Paint;
            // 
            // btnTable
            // 
            btnTable.Location = new Point(152, 34);
            btnTable.Name = "btnTable";
            btnTable.Size = new Size(45, 23);
            btnTable.TabIndex = 3;
            btnTable.Text = "Show";
            btnTable.UseVisualStyleBackColor = true;
            btnTable.Click += btnTable_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label1.Location = new Point(74, 9);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(61, 21);
            label1.TabIndex = 0;
            label1.Text = "TABLES";
            // 
            // pictureBox4
            // 
            pictureBox4.Image = (Image)resources.GetObject("pictureBox4.Image");
            pictureBox4.Location = new Point(12, 6);
            pictureBox4.Margin = new Padding(3, 2, 3, 2);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(57, 52);
            pictureBox4.TabIndex = 2;
            pictureBox4.TabStop = false;
            // 
            // lblTotalTable
            // 
            lblTotalTable.AutoSize = true;
            lblTotalTable.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalTable.Location = new Point(74, 37);
            lblTotalTable.Margin = new Padding(2, 0, 2, 0);
            lblTotalTable.Name = "lblTotalTable";
            lblTotalTable.Size = new Size(14, 15);
            lblTotalTable.TabIndex = 1;
            lblTotalTable.Text = "0";
            // 
            // panel2
            // 
            panel2.BackColor = Color.WhiteSmoke;
            panel2.Controls.Add(btnShowProcedures);
            panel2.Controls.Add(lblTotalStorePrcedure);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(pictureBox3);
            panel2.Cursor = Cursors.Hand;
            panel2.Location = new Point(405, 109);
            panel2.Margin = new Padding(2);
            panel2.Name = "panel2";
            panel2.Size = new Size(200, 60);
            panel2.TabIndex = 3;
            panel2.Click += panel2_Click;
            panel2.Paint += panel2_Paint;
            // 
            // btnShowProcedures
            // 
            btnShowProcedures.Location = new Point(152, 34);
            btnShowProcedures.Name = "btnShowProcedures";
            btnShowProcedures.Size = new Size(45, 23);
            btnShowProcedures.TabIndex = 4;
            btnShowProcedures.Text = "Show";
            btnShowProcedures.UseVisualStyleBackColor = true;
            btnShowProcedures.Click += btnShowProcedures_Click;
            // 
            // lblTotalStorePrcedure
            // 
            lblTotalStorePrcedure.AutoSize = true;
            lblTotalStorePrcedure.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalStorePrcedure.Location = new Point(64, 37);
            lblTotalStorePrcedure.Margin = new Padding(2, 0, 2, 0);
            lblTotalStorePrcedure.Name = "lblTotalStorePrcedure";
            lblTotalStorePrcedure.Size = new Size(14, 15);
            lblTotalStorePrcedure.TabIndex = 3;
            lblTotalStorePrcedure.Text = "0";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.Location = new Point(64, 9);
            label4.Margin = new Padding(2, 0, 2, 0);
            label4.Name = "label4";
            label4.Size = new Size(108, 21);
            label4.TabIndex = 2;
            label4.Text = "PROCEDURES";
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = Color.Transparent;
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(-4, 0);
            pictureBox3.Margin = new Padding(3, 2, 3, 2);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(84, 60);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 9;
            pictureBox3.TabStop = false;
            // 
            // panel3
            // 
            panel3.BackColor = Color.WhiteSmoke;
            panel3.Controls.Add(btnShowView);
            panel3.Controls.Add(lblTotalViews);
            panel3.Controls.Add(label6);
            panel3.Controls.Add(pictureBox2);
            panel3.Cursor = Cursors.Hand;
            panel3.Location = new Point(607, 109);
            panel3.Margin = new Padding(2);
            panel3.Name = "panel3";
            panel3.Size = new Size(200, 60);
            panel3.TabIndex = 4;
            panel3.Click += panel3_Click;
            // 
            // btnShowView
            // 
            btnShowView.Location = new Point(152, 33);
            btnShowView.Name = "btnShowView";
            btnShowView.Size = new Size(45, 23);
            btnShowView.TabIndex = 10;
            btnShowView.Text = "Show";
            btnShowView.UseVisualStyleBackColor = true;
            btnShowView.Click += btnShowView_Click;
            // 
            // lblTotalViews
            // 
            lblTotalViews.AutoSize = true;
            lblTotalViews.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalViews.Location = new Point(73, 37);
            lblTotalViews.Margin = new Padding(2, 0, 2, 0);
            lblTotalViews.Name = "lblTotalViews";
            lblTotalViews.Size = new Size(14, 15);
            lblTotalViews.TabIndex = 3;
            lblTotalViews.Text = "0";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label6.Location = new Point(73, 9);
            label6.Margin = new Padding(2, 0, 2, 0);
            label6.Name = "label6";
            label6.Size = new Size(56, 21);
            label6.TabIndex = 2;
            label6.Text = "VIEWS";
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = Color.Transparent;
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(3, 0);
            pictureBox2.Margin = new Padding(3, 2, 3, 2);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(84, 60);
            pictureBox2.TabIndex = 4;
            pictureBox2.TabStop = false;
            // 
            // panel4
            // 
            panel4.BackColor = Color.WhiteSmoke;
            panel4.Controls.Add(btnShowTriggers);
            panel4.Controls.Add(lblTotalTrigger);
            panel4.Controls.Add(label8);
            panel4.Controls.Add(pictureBox1);
            panel4.Cursor = Cursors.Hand;
            panel4.Location = new Point(810, 109);
            panel4.Margin = new Padding(2);
            panel4.Name = "panel4";
            panel4.Size = new Size(200, 60);
            panel4.TabIndex = 5;
            panel4.Click += panel4_Click;
            panel4.Paint += panel4_Paint;
            // 
            // btnShowTriggers
            // 
            btnShowTriggers.Location = new Point(152, 34);
            btnShowTriggers.Name = "btnShowTriggers";
            btnShowTriggers.Size = new Size(45, 23);
            btnShowTriggers.TabIndex = 11;
            btnShowTriggers.Text = "Show";
            btnShowTriggers.UseVisualStyleBackColor = true;
            btnShowTriggers.Click += btnShowTriggers_Click;
            // 
            // lblTotalTrigger
            // 
            lblTotalTrigger.AutoSize = true;
            lblTotalTrigger.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblTotalTrigger.Location = new Point(89, 37);
            lblTotalTrigger.Margin = new Padding(2, 0, 2, 0);
            lblTotalTrigger.Name = "lblTotalTrigger";
            lblTotalTrigger.Size = new Size(14, 15);
            lblTotalTrigger.TabIndex = 3;
            lblTotalTrigger.Text = "0";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label8.Location = new Point(89, 9);
            label8.Margin = new Padding(2, 0, 2, 0);
            label8.Name = "label8";
            label8.Size = new Size(81, 21);
            label8.TabIndex = 2;
            label8.Text = "TRIGGERS";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(111, 60);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new Size(24, 24);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { viewDataToolStripMenuItem, viewOrChangeSchemaToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(203, 48);
            // 
            // viewDataToolStripMenuItem
            // 
            viewDataToolStripMenuItem.Name = "viewDataToolStripMenuItem";
            viewDataToolStripMenuItem.Size = new Size(202, 22);
            viewDataToolStripMenuItem.Text = "View Data";
            viewDataToolStripMenuItem.Click += viewDataToolStripMenuItem_Click;
            // 
            // viewOrChangeSchemaToolStripMenuItem
            // 
            viewOrChangeSchemaToolStripMenuItem.Name = "viewOrChangeSchemaToolStripMenuItem";
            viewOrChangeSchemaToolStripMenuItem.Size = new Size(202, 22);
            viewOrChangeSchemaToolStripMenuItem.Text = "View or Change Schema";
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(20, 20);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButton2, toolStripButton3, toolStripButton1, toolStripSeparator4, toolStripSeparator1, toolStripButton7, toolStripButton4, toolStripButton5, toolStripSeparator2, toolStripButton6, toolStripSeparator3, toolStripTextBox1 });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1222, 83);
            toolStrip1.TabIndex = 6;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton2
            // 
            toolStripButton2.AutoSize = false;
            toolStripButton2.Enabled = false;
            toolStripButton2.Image = (Image)resources.GetObject("toolStripButton2.Image");
            toolStripButton2.ImageTransparentColor = Color.Magenta;
            toolStripButton2.Name = "toolStripButton2";
            toolStripButton2.Size = new Size(120, 80);
            toolStripButton2.Text = "Track Table";
            toolStripButton2.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButton2.ToolTipText = "Track Table";
            toolStripButton2.Click += toolStripButton2_Click;
            // 
            // toolStripButton3
            // 
            toolStripButton3.Name = "toolStripButton3";
            toolStripButton3.Size = new Size(6, 83);
            // 
            // toolStripButton1
            // 
            toolStripButton1.AutoSize = false;
            toolStripButton1.Enabled = false;
            toolStripButton1.Image = (Image)resources.GetObject("toolStripButton1.Image");
            toolStripButton1.ImageTransparentColor = Color.Magenta;
            toolStripButton1.Name = "toolStripButton1";
            toolStripButton1.Size = new Size(120, 80);
            toolStripButton1.Text = "Track Procedures";
            toolStripButton1.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButton1.Click += toolStripButton1_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(6, 83);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 83);
            toolStripSeparator1.Visible = false;
            // 
            // toolStripButton7
            // 
            toolStripButton7.Enabled = false;
            toolStripButton7.Image = (Image)resources.GetObject("toolStripButton7.Image");
            toolStripButton7.ImageTransparentColor = Color.Magenta;
            toolStripButton7.Name = "toolStripButton7";
            toolStripButton7.Size = new Size(81, 80);
            toolStripButton7.Text = "Dummy Data";
            toolStripButton7.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButton7.Click += toolStripButton7_Click;
            // 
            // toolStripButton4
            // 
            toolStripButton4.AutoSize = false;
            toolStripButton4.Image = (Image)resources.GetObject("toolStripButton4.Image");
            toolStripButton4.ImageTransparentColor = Color.Magenta;
            toolStripButton4.Name = "toolStripButton4";
            toolStripButton4.Size = new Size(140, 80);
            toolStripButton4.Text = "Upload POS Worker";
            toolStripButton4.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButton4.Visible = false;
            toolStripButton4.Click += toolStripButton4_Click;
            // 
            // toolStripButton5
            // 
            toolStripButton5.Alignment = ToolStripItemAlignment.Right;
            toolStripButton5.Image = (Image)resources.GetObject("toolStripButton5.Image");
            toolStripButton5.ImageScaling = ToolStripItemImageScaling.None;
            toolStripButton5.ImageTransparentColor = Color.Magenta;
            toolStripButton5.Name = "toolStripButton5";
            toolStripButton5.Size = new Size(107, 80);
            toolStripButton5.Text = "Connect Database";
            toolStripButton5.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButton5.Click += toolStripButton5_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 83);
            // 
            // toolStripButton6
            // 
            toolStripButton6.Image = (Image)resources.GetObject("toolStripButton6.Image");
            toolStripButton6.ImageTransparentColor = Color.Magenta;
            toolStripButton6.Name = "toolStripButton6";
            toolStripButton6.Size = new Size(152, 80);
            toolStripButton6.Text = "Database Compare System";
            toolStripButton6.TextImageRelation = TextImageRelation.ImageAboveText;
            toolStripButton6.Click += toolStripButton6_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(6, 83);
            toolStripSeparator3.Visible = false;
            // 
            // toolStripTextBox1
            // 
            toolStripTextBox1.Name = "toolStripTextBox1";
            toolStripTextBox1.Size = new Size(100, 83);
            toolStripTextBox1.Text = "S_P X Ray";
            toolStripTextBox1.Visible = false;
            toolStripTextBox1.Click += toolStripTextBox1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(205, 203);
            label2.Name = "label2";
            label2.Size = new Size(42, 15);
            label2.TabIndex = 7;
            label2.Text = "Search";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(253, 195);
            textBox1.Margin = new Padding(3, 2, 3, 2);
            textBox1.Name = "textBox1";
            textBox1.PlaceholderText = "Search Tables";
            textBox1.Size = new Size(353, 23);
            textBox1.TabIndex = 8;
            textBox1.TextChanged += textBox1_TextChanged;
            // 
            // panel5
            // 
            panel5.BackColor = Color.WhiteSmoke;
            panel5.Controls.Add(btnShowTypes);
            panel5.Controls.Add(label3);
            panel5.Controls.Add(label5);
            panel5.Controls.Add(pictureBox5);
            panel5.Cursor = Cursors.Hand;
            panel5.Location = new Point(1013, 109);
            panel5.Margin = new Padding(2);
            panel5.Name = "panel5";
            panel5.Size = new Size(200, 60);
            panel5.TabIndex = 6;
            // 
            // btnShowTypes
            // 
            btnShowTypes.Location = new Point(152, 33);
            btnShowTypes.Name = "btnShowTypes";
            btnShowTypes.Size = new Size(45, 23);
            btnShowTypes.TabIndex = 12;
            btnShowTypes.Text = "Show";
            btnShowTypes.UseVisualStyleBackColor = true;
            btnShowTypes.Click += btnShowTypes_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(68, 37);
            label3.Margin = new Padding(2, 0, 2, 0);
            label3.Name = "label3";
            label3.Size = new Size(14, 15);
            label3.TabIndex = 3;
            label3.Text = "0";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.Location = new Point(58, 15);
            label5.Margin = new Padding(2, 0, 2, 0);
            label5.Name = "label5";
            label5.Size = new Size(99, 21);
            label5.TabIndex = 2;
            label5.Text = "TABLE TYPES";
            // 
            // pictureBox5
            // 
            pictureBox5.BackColor = Color.Transparent;
            pictureBox5.Image = (Image)resources.GetObject("pictureBox5.Image");
            pictureBox5.Location = new Point(0, 0);
            pictureBox5.Margin = new Padding(3, 2, 3, 2);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(63, 60);
            pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox5.TabIndex = 9;
            pictureBox5.TabStop = false;
            // 
            // pictureBox6
            // 
            pictureBox6.Image = (Image)resources.GetObject("pictureBox6.Image");
            pictureBox6.Location = new Point(563, 281);
            pictureBox6.Margin = new Padding(3, 2, 3, 2);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(234, 235);
            pictureBox6.TabIndex = 9;
            pictureBox6.TabStop = false;
            pictureBox6.Visible = false;
            // 
            // panel6
            // 
            panel6.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel6.BackColor = SystemColors.ButtonFace;
            panel6.Controls.Add(lblTotalRowsCount);
            panel6.Controls.Add(label7);
            panel6.Location = new Point(0, 688);
            panel6.Name = "panel6";
            panel6.Size = new Size(1222, 27);
            panel6.TabIndex = 10;
            // 
            // lblTotalRowsCount
            // 
            lblTotalRowsCount.AutoSize = true;
            lblTotalRowsCount.Location = new Point(47, 4);
            lblTotalRowsCount.Name = "lblTotalRowsCount";
            lblTotalRowsCount.Size = new Size(13, 15);
            lblTotalRowsCount.TabIndex = 1;
            lblTotalRowsCount.Text = "0";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(6, 4);
            label7.Name = "label7";
            label7.Size = new Size(35, 15);
            label7.TabIndex = 0;
            label7.Text = "Rows";
            // 
            // MainDashboard
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(1222, 711);
            Controls.Add(panel6);
            Controls.Add(pictureBox6);
            Controls.Add(panel5);
            Controls.Add(textBox1);
            Controls.Add(label2);
            Controls.Add(toolStrip1);
            Controls.Add(panel4);
            Controls.Add(panel3);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Controls.Add(dataGridViewDetail);
            Controls.Add(listBoxDB);
            Margin = new Padding(2);
            MaximizeBox = false;
            Name = "MainDashboard";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Dashboard";
            Load += MainDashboard_Load;
            ((System.ComponentModel.ISupportInitialize)dataGridViewDetail).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
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
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton1;
        private ToolStripSeparator toolStripButton3;
        private Label label2;
        private TextBox textBox1;
        private Panel panel5;
        private Label label3;
        private Label label5;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButton4;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private PictureBox pictureBox4;
        private PictureBox pictureBox5;
        private PictureBox pictureBox6;
        private ToolStripButton toolStripButton5;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton toolStripButton6;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripTextBox toolStripTextBox1;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripButton toolStripButton7;
        private Panel panel6;
        private Label label7;
        private Label lblTotalRowsCount;
        private Button btnTable;
        private Button btnShowProcedures;
        private Button btnShowView;
        private Button btnShowTriggers;
        private Button btnShowTypes;
    }
}