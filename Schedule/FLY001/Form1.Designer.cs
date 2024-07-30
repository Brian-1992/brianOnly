namespace GenTools
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.tbTableScript = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dataGridViewM = new System.Windows.Forms.DataGridView();
            this.textBoxD = new System.Windows.Forms.TextBox();
            this.textBoxM = new System.Windows.Forms.TextBox();
            this.dataGridViewD = new System.Windows.Forms.DataGridView();
            this.buttonQry = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.listBoxM = new System.Windows.Forms.ListBox();
            this.textBoxCodeName = new System.Windows.Forms.TextBox();
            this.textBoxCodeTitle = new System.Windows.Forms.TextBox();
            this.dataGridViewFormM = new System.Windows.Forms.DataGridView();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewM)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewD)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFormM)).BeginInit();
            this.SuspendLayout();
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 15;
            this.listBox1.Location = new System.Drawing.Point(358, 5);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(262, 19);
            this.listBox1.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(268, 1);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "readOraSchema";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(268, 30);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "ParseOra";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // tbTableScript
            // 
            this.tbTableScript.Location = new System.Drawing.Point(0, 1);
            this.tbTableScript.Multiline = true;
            this.tbTableScript.Name = "tbTableScript";
            this.tbTableScript.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbTableScript.Size = new System.Drawing.Size(262, 52);
            this.tbTableScript.TabIndex = 5;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(0, 59);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1179, 585);
            this.tabControl1.TabIndex = 7;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dataGridViewFormM);
            this.tabPage1.Controls.Add(this.textBoxCodeTitle);
            this.tabPage1.Controls.Add(this.textBoxCodeName);
            this.tabPage1.Controls.Add(this.listBoxM);
            this.tabPage1.Controls.Add(this.button3);
            this.tabPage1.Controls.Add(this.buttonQry);
            this.tabPage1.Controls.Add(this.dataGridViewD);
            this.tabPage1.Controls.Add(this.dataGridViewM);
            this.tabPage1.Controls.Add(this.textBoxD);
            this.tabPage1.Controls.Add(this.textBoxM);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1171, 556);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(244, 87);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dataGridViewM
            // 
            this.dataGridViewM.AllowUserToAddRows = false;
            this.dataGridViewM.AllowUserToDeleteRows = false;
            this.dataGridViewM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewM.Location = new System.Drawing.Point(8, 68);
            this.dataGridViewM.Name = "dataGridViewM";
            this.dataGridViewM.Size = new System.Drawing.Size(559, 134);
            this.dataGridViewM.TabIndex = 9;
            // 
            // textBoxD
            // 
            this.textBoxD.Location = new System.Drawing.Point(8, 450);
            this.textBoxD.Multiline = true;
            this.textBoxD.Name = "textBoxD";
            this.textBoxD.Size = new System.Drawing.Size(559, 100);
            this.textBoxD.TabIndex = 8;
            // 
            // textBoxM
            // 
            this.textBoxM.Location = new System.Drawing.Point(8, 208);
            this.textBoxM.Multiline = true;
            this.textBoxM.Name = "textBoxM";
            this.textBoxM.Size = new System.Drawing.Size(559, 96);
            this.textBoxM.TabIndex = 7;
            // 
            // dataGridViewD
            // 
            this.dataGridViewD.AllowUserToAddRows = false;
            this.dataGridViewD.AllowUserToDeleteRows = false;
            this.dataGridViewD.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewD.Location = new System.Drawing.Point(8, 310);
            this.dataGridViewD.Name = "dataGridViewD";
            this.dataGridViewD.ReadOnly = true;
            this.dataGridViewD.Size = new System.Drawing.Size(559, 134);
            this.dataGridViewD.TabIndex = 10;
            // 
            // buttonQry
            // 
            this.buttonQry.Location = new System.Drawing.Point(573, 495);
            this.buttonQry.Name = "buttonQry";
            this.buttonQry.Size = new System.Drawing.Size(75, 23);
            this.buttonQry.TabIndex = 11;
            this.buttonQry.Text = "Query";
            this.buttonQry.UseVisualStyleBackColor = true;
            this.buttonQry.Click += new System.EventHandler(this.buttonQry_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(573, 524);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 12;
            this.button3.Text = "Gen";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // listBoxM
            // 
            this.listBoxM.FormattingEnabled = true;
            this.listBoxM.ItemHeight = 15;
            this.listBoxM.Location = new System.Drawing.Point(669, 318);
            this.listBoxM.Name = "listBoxM";
            this.listBoxM.ScrollAlwaysVisible = true;
            this.listBoxM.Size = new System.Drawing.Size(239, 229);
            this.listBoxM.TabIndex = 13;
            // 
            // textBoxCodeName
            // 
            this.textBoxCodeName.Location = new System.Drawing.Point(8, 6);
            this.textBoxCodeName.Name = "textBoxCodeName";
            this.textBoxCodeName.Size = new System.Drawing.Size(74, 25);
            this.textBoxCodeName.TabIndex = 14;
            this.textBoxCodeName.Text = "CD0010";
            // 
            // textBoxCodeTitle
            // 
            this.textBoxCodeTitle.Location = new System.Drawing.Point(88, 6);
            this.textBoxCodeTitle.Name = "textBoxCodeTitle";
            this.textBoxCodeTitle.Size = new System.Drawing.Size(179, 25);
            this.textBoxCodeTitle.TabIndex = 15;
            this.textBoxCodeTitle.Text = "揀貨資料查詢作業";
            // 
            // dataGridViewFormM
            // 
            this.dataGridViewFormM.AllowUserToAddRows = false;
            this.dataGridViewFormM.AllowUserToDeleteRows = false;
            this.dataGridViewFormM.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewFormM.Location = new System.Drawing.Point(586, 68);
            this.dataGridViewFormM.Name = "dataGridViewFormM";
            this.dataGridViewFormM.Size = new System.Drawing.Size(322, 134);
            this.dataGridViewFormM.TabIndex = 16;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1212, 656);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.tbTableScript);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.listBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewM)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewD)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewFormM)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox tbTableScript;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button buttonQry;
        private System.Windows.Forms.DataGridView dataGridViewD;
        private System.Windows.Forms.DataGridView dataGridViewM;
        private System.Windows.Forms.TextBox textBoxD;
        private System.Windows.Forms.TextBox textBoxM;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListBox listBoxM;
        private System.Windows.Forms.TextBox textBoxCodeTitle;
        private System.Windows.Forms.TextBox textBoxCodeName;
        private System.Windows.Forms.DataGridView dataGridViewFormM;
    }
}

