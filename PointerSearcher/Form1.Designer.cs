namespace PointerSearcher
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.buttonRead = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnMainStart = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnMainEnd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnHeapStart = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnHeapEnd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTargetAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buttonSearch = new System.Windows.Forms.Button();
            this.textBoxDepth = new System.Windows.Forms.TextBox();
            this.textBoxOffsetNum = new System.Windows.Forms.TextBox();
            this.textBoxOffsetAddress = new System.Windows.Forms.TextBox();
            this.buttonNarrowDown = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.Export_button = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonRead
            // 
            this.buttonRead.Location = new System.Drawing.Point(14, 232);
            this.buttonRead.Name = "buttonRead";
            this.buttonRead.Size = new System.Drawing.Size(158, 25);
            this.buttonRead.TabIndex = 0;
            this.buttonRead.Text = "Read 1st Dump Data";
            this.buttonRead.UseVisualStyleBackColor = true;
            this.buttonRead.Click += new System.EventHandler(this.buttonRead_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 263);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(646, 149);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnPath,
            this.ColumnMainStart,
            this.ColumnMainEnd,
            this.ColumnHeapStart,
            this.ColumnHeapEnd,
            this.ColumnTargetAddress});
            this.dataGridView1.Location = new System.Drawing.Point(12, 13);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle1.NullValue = null;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(646, 163);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridView1_CellBeginEdit);
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            this.dataGridView1.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellEnter);
            // 
            // ColumnPath
            // 
            this.ColumnPath.HeaderText = "Path";
            this.ColumnPath.Name = "ColumnPath";
            this.ColumnPath.ToolTipText = "Path of Noexs dump data";
            // 
            // ColumnMainStart
            // 
            this.ColumnMainStart.HeaderText = "MainStart";
            this.ColumnMainStart.Name = "ColumnMainStart";
            this.ColumnMainStart.ToolTipText = "main start address";
            // 
            // ColumnMainEnd
            // 
            this.ColumnMainEnd.HeaderText = "MainEnd";
            this.ColumnMainEnd.Name = "ColumnMainEnd";
            this.ColumnMainEnd.ToolTipText = "main end address";
            // 
            // ColumnHeapStart
            // 
            this.ColumnHeapStart.HeaderText = "HeapStart";
            this.ColumnHeapStart.Name = "ColumnHeapStart";
            this.ColumnHeapStart.ToolTipText = "heap start address";
            // 
            // ColumnHeapEnd
            // 
            this.ColumnHeapEnd.HeaderText = "HeapEnd";
            this.ColumnHeapEnd.Name = "ColumnHeapEnd";
            this.ColumnHeapEnd.ToolTipText = "heap end address";
            // 
            // ColumnTargetAddress
            // 
            this.ColumnTargetAddress.HeaderText = "TargetAddress";
            this.ColumnTargetAddress.Name = "ColumnTargetAddress";
            this.ColumnTargetAddress.ToolTipText = "address you want to find a pointer of this dump data";
            // 
            // buttonSearch
            // 
            this.buttonSearch.Location = new System.Drawing.Point(178, 232);
            this.buttonSearch.Name = "buttonSearch";
            this.buttonSearch.Size = new System.Drawing.Size(158, 25);
            this.buttonSearch.TabIndex = 3;
            this.buttonSearch.Text = "Reset and Search";
            this.buttonSearch.UseVisualStyleBackColor = true;
            this.buttonSearch.Click += new System.EventHandler(this.buttonSearch_Click);
            // 
            // textBoxDepth
            // 
            this.textBoxDepth.Location = new System.Drawing.Point(14, 196);
            this.textBoxDepth.Name = "textBoxDepth";
            this.textBoxDepth.Size = new System.Drawing.Size(100, 21);
            this.textBoxDepth.TabIndex = 4;
            this.textBoxDepth.TextChanged += new System.EventHandler(this.textBoxDepth_TextChanged);
            // 
            // textBoxOffsetNum
            // 
            this.textBoxOffsetNum.Location = new System.Drawing.Point(141, 196);
            this.textBoxOffsetNum.Name = "textBoxOffsetNum";
            this.textBoxOffsetNum.Size = new System.Drawing.Size(100, 21);
            this.textBoxOffsetNum.TabIndex = 5;
            // 
            // textBoxOffsetAddress
            // 
            this.textBoxOffsetAddress.Location = new System.Drawing.Point(263, 196);
            this.textBoxOffsetAddress.Name = "textBoxOffsetAddress";
            this.textBoxOffsetAddress.Size = new System.Drawing.Size(100, 21);
            this.textBoxOffsetAddress.TabIndex = 6;
            // 
            // buttonNarrowDown
            // 
            this.buttonNarrowDown.Location = new System.Drawing.Point(342, 232);
            this.buttonNarrowDown.Name = "buttonNarrowDown";
            this.buttonNarrowDown.Size = new System.Drawing.Size(158, 25);
            this.buttonNarrowDown.TabIndex = 7;
            this.buttonNarrowDown.Text = "Narrow Down Result";
            this.buttonNarrowDown.UseVisualStyleBackColor = true;
            this.buttonNarrowDown.Click += new System.EventHandler(this.buttonNarrowDown_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 180);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "MaxDepth";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(138, 180);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "OffsetNum";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(260, 180);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "OffsetRange";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(12, 420);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(565, 25);
            this.progressBar1.TabIndex = 11;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(583, 420);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 25);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click_1);
            // 
            // Export_button
            // 
            this.Export_button.Location = new System.Drawing.Point(534, 232);
            this.Export_button.Name = "Export_button";
            this.Export_button.Size = new System.Drawing.Size(124, 25);
            this.Export_button.TabIndex = 13;
            this.Export_button.Text = "Export To EdiZon SE";
            this.Export_button.UseVisualStyleBackColor = true;
            this.Export_button.Click += new System.EventHandler(this.Export_to_SE_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(389, 196);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(269, 21);
            this.textBox2.TabIndex = 14;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(386, 179);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Book Mark File";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(505, 232);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(23, 23);
            this.button1.TabIndex = 16;
            this.button1.Text = "P";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 452);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.Export_button);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonNarrowDown);
            this.Controls.Add(this.textBoxOffsetAddress);
            this.Controls.Add(this.textBoxOffsetNum);
            this.Controls.Add(this.textBoxDepth);
            this.Controls.Add(this.buttonSearch);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonRead);
            this.Name = "Form1";
            this.Text = "EdiZon SE PointerSearcher 0.3b";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonRead;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button buttonSearch;
        private System.Windows.Forms.TextBox textBoxDepth;
        private System.Windows.Forms.TextBox textBoxOffsetNum;
        private System.Windows.Forms.TextBox textBoxOffsetAddress;
        private System.Windows.Forms.Button buttonNarrowDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMainStart;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMainEnd;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnHeapStart;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnHeapEnd;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTargetAddress;
        private System.Windows.Forms.Button Export_button;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
    }
}

