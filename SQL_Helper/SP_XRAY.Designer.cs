namespace SQL_Helper
{
    partial class frm_Sp_Xray
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
            textBox1 = new TextBox();
            button1 = new Button();
            listView1 = new ListView();
            label1 = new Label();
            label2 = new Label();
            listView2 = new ListView();
            label3 = new Label();
            listView3 = new ListView();
            checkBox1 = new CheckBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(button1);
            groupBox1.Controls.Add(textBox1);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(433, 61);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Store Procedures";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(6, 22);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(340, 23);
            textBox1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(352, 22);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 1;
            button1.Text = "X-Ray";
            button1.UseVisualStyleBackColor = true;
            // 
            // listView1
            // 
            listView1.Location = new Point(158, 95);
            listView1.Name = "listView1";
            listView1.Size = new Size(140, 396);
            listView1.TabIndex = 1;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(158, 77);
            label1.Name = "label1";
            label1.Size = new Size(49, 15);
            label1.TabIndex = 2;
            label1.Text = "Triggers";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(304, 77);
            label2.Name = "label2";
            label2.Size = new Size(62, 15);
            label2.TabIndex = 4;
            label2.Text = "Use Tables";
            // 
            // listView2
            // 
            listView2.Location = new Point(304, 95);
            listView2.Name = "listView2";
            listView2.Size = new Size(140, 396);
            listView2.TabIndex = 3;
            listView2.UseCompatibleStateImageBehavior = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 77);
            label3.Name = "label3";
            label3.Size = new Size(53, 15);
            label3.TabIndex = 6;
            label3.Text = "Variables";
            // 
            // listView3
            // 
            listView3.Location = new Point(12, 95);
            listView3.Name = "listView3";
            listView3.Size = new Size(140, 396);
            listView3.TabIndex = 5;
            listView3.UseCompatibleStateImageBehavior = false;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new Point(12, 507);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(81, 19);
            checkBox1.TabIndex = 8;
            checkBox1.Text = "Is User API";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // frm_Sp_Xray
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            ClientSize = new Size(457, 535);
            Controls.Add(checkBox1);
            Controls.Add(label3);
            Controls.Add(listView3);
            Controls.Add(label2);
            Controls.Add(listView2);
            Controls.Add(label1);
            Controls.Add(listView1);
            Controls.Add(groupBox1);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frm_Sp_Xray";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SP_XRAY";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private GroupBox groupBox1;
        private Button button1;
        private TextBox textBox1;
        private ListView listView1;
        private Label label1;
        private Label label2;
        private ListView listView2;
        private Label label3;
        private ListView listView3;
        private CheckBox checkBox1;
    }
}