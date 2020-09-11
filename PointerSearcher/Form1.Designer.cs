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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.buttonRead = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnMainStart = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnMainEnd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnHeapStart = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnHeapEnd = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTargetAddress1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTargetAddress2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnTargetAddress3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.ipBox = new System.Windows.Forms.TextBox();
            this.connectBtn = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.statusBox = new System.Windows.Forms.TextBox();
            this.getstatus = new System.Windows.Forms.Button();
            this.pidBox = new System.Windows.Forms.TextBox();
            this.curpidBox = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.pid0Box = new System.Windows.Forms.TextBox();
            this.TIDBox = new System.Windows.Forms.TextBox();
            this.TID0Box = new System.Windows.Forms.TextBox();
            this.button8 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.errorBox = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
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
            this.ColumnTargetAddress1,
            this.ColumnTargetAddress2,
            this.ColumnTargetAddress3});
            this.dataGridView1.Location = new System.Drawing.Point(12, 12);
            this.dataGridView1.Name = "dataGridView1";
            dataGridViewCellStyle4.NullValue = null;
            this.dataGridView1.RowsDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(1094, 163);
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
            this.ColumnPath.Width = 350;
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
            // ColumnTargetAddress1
            // 
            this.ColumnTargetAddress1.HeaderText = "TargetAddress1";
            this.ColumnTargetAddress1.Name = "ColumnTargetAddress1";
            this.ColumnTargetAddress1.ToolTipText = "address you want to find a pointer of this dump data";
            // 
            // ColumnTargetAddress2
            // 
            this.ColumnTargetAddress2.HeaderText = "TargetAddress2";
            this.ColumnTargetAddress2.Name = "ColumnTargetAddress2";
            // 
            // ColumnTargetAddress3
            // 
            this.ColumnTargetAddress3.HeaderText = "TargetAddress3";
            this.ColumnTargetAddress3.Name = "ColumnTargetAddress3";
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
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(675, 200);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(62, 17);
            this.radioButton1.TabIndex = 17;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Target 1";
            this.radioButton1.UseVisualStyleBackColor = true;
            this.radioButton1.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(675, 232);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(62, 17);
            this.radioButton2.TabIndex = 18;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Target 2";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(675, 263);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(62, 17);
            this.radioButton3.TabIndex = 19;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Target 3";
            this.radioButton3.UseVisualStyleBackColor = true;
            this.radioButton3.CheckedChanged += new System.EventHandler(this.radioButton3_CheckedChanged);
            // 
            // ipBox
            // 
            this.ipBox.Location = new System.Drawing.Point(897, 407);
            this.ipBox.Name = "ipBox";
            this.ipBox.Size = new System.Drawing.Size(100, 21);
            this.ipBox.TabIndex = 20;
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(1021, 405);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(85, 23);
            this.connectBtn.TabIndex = 21;
            this.connectBtn.Text = "Connect";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(894, 391);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "IP address";
            // 
            // statusBox
            // 
            this.statusBox.Location = new System.Drawing.Point(897, 307);
            this.statusBox.Name = "statusBox";
            this.statusBox.Size = new System.Drawing.Size(100, 21);
            this.statusBox.TabIndex = 23;
            // 
            // getstatus
            // 
            this.getstatus.Location = new System.Drawing.Point(1021, 377);
            this.getstatus.Name = "getstatus";
            this.getstatus.Size = new System.Drawing.Size(85, 23);
            this.getstatus.TabIndex = 24;
            this.getstatus.Text = "get status";
            this.getstatus.UseVisualStyleBackColor = true;
            this.getstatus.Click += new System.EventHandler(this.getstatus_Click);
            // 
            // pidBox
            // 
            this.pidBox.Location = new System.Drawing.Point(897, 334);
            this.pidBox.Name = "pidBox";
            this.pidBox.Size = new System.Drawing.Size(100, 21);
            this.pidBox.TabIndex = 25;
            this.pidBox.TextChanged += new System.EventHandler(this.pidBox_TextChanged);
            // 
            // curpidBox
            // 
            this.curpidBox.Location = new System.Drawing.Point(897, 280);
            this.curpidBox.Name = "curpidBox";
            this.curpidBox.Size = new System.Drawing.Size(100, 21);
            this.curpidBox.TabIndex = 26;
            this.curpidBox.TextChanged += new System.EventHandler(this.curpidBox_TextChanged);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1021, 350);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 23);
            this.button2.TabIndex = 27;
            this.button2.Text = "Detach";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1021, 321);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(85, 23);
            this.button3.TabIndex = 28;
            this.button3.Text = "Dump ptr";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(1021, 293);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(85, 23);
            this.button4.TabIndex = 29;
            this.button4.Text = "Attach dmnt";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(1021, 264);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(85, 23);
            this.button5.TabIndex = 30;
            this.button5.Text = "Detach dmnt";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(688, 334);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(59, 23);
            this.button6.TabIndex = 31;
            this.button6.Text = "Attach";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // pid0Box
            // 
            this.pid0Box.Location = new System.Drawing.Point(897, 361);
            this.pid0Box.Name = "pid0Box";
            this.pid0Box.Size = new System.Drawing.Size(100, 21);
            this.pid0Box.TabIndex = 33;
            this.pid0Box.TextChanged += new System.EventHandler(this.pid0Box_TextChanged);
            // 
            // TIDBox
            // 
            this.TIDBox.Location = new System.Drawing.Point(753, 334);
            this.TIDBox.Name = "TIDBox";
            this.TIDBox.Size = new System.Drawing.Size(138, 21);
            this.TIDBox.TabIndex = 34;
            // 
            // TID0Box
            // 
            this.TID0Box.Location = new System.Drawing.Point(753, 361);
            this.TID0Box.Name = "TID0Box";
            this.TID0Box.Size = new System.Drawing.Size(138, 21);
            this.TID0Box.TabIndex = 35;
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(688, 361);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(59, 23);
            this.button8.TabIndex = 36;
            this.button8.Text = "Attach";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(794, 312);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(97, 13);
            this.label6.TabIndex = 37;
            this.label6.Text = "Sys Module Version";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(794, 286);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(93, 13);
            this.label7.TabIndex = 38;
            this.label7.Text = "pminfo Program Id";
            // 
            // errorBox
            // 
            this.errorBox.Location = new System.Drawing.Point(753, 407);
            this.errorBox.Name = "errorBox";
            this.errorBox.Size = new System.Drawing.Size(134, 21);
            this.errorBox.TabIndex = 39;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(750, 391);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(56, 13);
            this.label8.TabIndex = 40;
            this.label8.Text = "Error Code";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1117, 452);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.errorBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.TID0Box);
            this.Controls.Add(this.TIDBox);
            this.Controls.Add(this.pid0Box);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
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
            this.Controls.Add(this.curpidBox);
            this.Controls.Add(this.pidBox);
            this.Controls.Add(this.getstatus);
            this.Controls.Add(this.statusBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.ipBox);
            this.Name = "Form1";
            this.Text = "EdiZon SE PointerSearcher 0.4";
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
        private System.Windows.Forms.Button Export_button;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMainStart;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnMainEnd;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnHeapStart;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnHeapEnd;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTargetAddress1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTargetAddress2;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnTargetAddress3;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.TextBox ipBox;
        private System.Windows.Forms.Button connectBtn;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox statusBox;
        private System.Windows.Forms.Button getstatus;
        private System.Windows.Forms.TextBox pidBox;
        private System.Windows.Forms.TextBox curpidBox;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.TextBox pid0Box;
        private System.Windows.Forms.TextBox TIDBox;
        private System.Windows.Forms.TextBox TID0Box;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox errorBox;
        private System.Windows.Forms.Label label8;
    }
}

