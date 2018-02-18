namespace PatternsAddOnWinformTestApp
{
    partial class WCFRunnerCtrl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.dataGridViewOutput = new System.Windows.Forms.DataGridView();
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.checkBoxRTInterval = new System.Windows.Forms.CheckBox();
            this.numericUpDownSliceInterval = new System.Windows.Forms.NumericUpDown();
            this.checkBoxSliceData = new System.Windows.Forms.CheckBox();
            this.buttonBrowseInput = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxInPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonRun = new System.Windows.Forms.Button();
            this.checkBoxFlush = new System.Windows.Forms.CheckBox();
            this.textBoxResult = new System.Windows.Forms.TextBox();
            this.buttonBrowseRes = new System.Windows.Forms.Button();
            this.groupBoxResults = new System.Windows.Forms.GroupBox();
            this.checkBoxArcive = new System.Windows.Forms.CheckBox();
            this.groupBoxOutput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOutput)).BeginInit();
            this.groupBoxInput.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSliceInterval)).BeginInit();
            this.groupBoxResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Controls.Add(this.dataGridViewOutput);
            this.groupBoxOutput.Location = new System.Drawing.Point(3, 168);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(582, 165);
            this.groupBoxOutput.TabIndex = 6;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output";
            // 
            // dataGridViewOutput
            // 
            this.dataGridViewOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOutput.Location = new System.Drawing.Point(6, 19);
            this.dataGridViewOutput.Name = "dataGridViewOutput";
            this.dataGridViewOutput.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridViewOutput.RowHeadersVisible = false;
            this.dataGridViewOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.dataGridViewOutput.Size = new System.Drawing.Size(570, 140);
            this.dataGridViewOutput.TabIndex = 0;
            // 
            // groupBoxInput
            // 
            this.groupBoxInput.Controls.Add(this.checkBoxRTInterval);
            this.groupBoxInput.Controls.Add(this.numericUpDownSliceInterval);
            this.groupBoxInput.Controls.Add(this.checkBoxSliceData);
            this.groupBoxInput.Controls.Add(this.buttonBrowseInput);
            this.groupBoxInput.Controls.Add(this.label2);
            this.groupBoxInput.Controls.Add(this.textBoxInPath);
            this.groupBoxInput.Controls.Add(this.label1);
            this.groupBoxInput.Location = new System.Drawing.Point(3, 3);
            this.groupBoxInput.Name = "groupBoxInput";
            this.groupBoxInput.Size = new System.Drawing.Size(582, 97);
            this.groupBoxInput.TabIndex = 5;
            this.groupBoxInput.TabStop = false;
            this.groupBoxInput.Text = "Input";
            // 
            // checkBoxRTInterval
            // 
            this.checkBoxRTInterval.AutoSize = true;
            this.checkBoxRTInterval.Enabled = false;
            this.checkBoxRTInterval.Location = new System.Drawing.Point(184, 69);
            this.checkBoxRTInterval.Name = "checkBoxRTInterval";
            this.checkBoxRTInterval.Size = new System.Drawing.Size(112, 17);
            this.checkBoxRTInterval.TabIndex = 5;
            this.checkBoxRTInterval.Text = "Real Time Interval";
            this.checkBoxRTInterval.UseVisualStyleBackColor = true;
            // 
            // numericUpDownSliceInterval
            // 
            this.numericUpDownSliceInterval.Enabled = false;
            this.numericUpDownSliceInterval.Location = new System.Drawing.Point(103, 68);
            this.numericUpDownSliceInterval.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownSliceInterval.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDownSliceInterval.Name = "numericUpDownSliceInterval";
            this.numericUpDownSliceInterval.Size = new System.Drawing.Size(56, 20);
            this.numericUpDownSliceInterval.TabIndex = 4;
            this.numericUpDownSliceInterval.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // checkBoxSliceData
            // 
            this.checkBoxSliceData.AutoSize = true;
            this.checkBoxSliceData.Location = new System.Drawing.Point(6, 45);
            this.checkBoxSliceData.Name = "checkBoxSliceData";
            this.checkBoxSliceData.Size = new System.Drawing.Size(102, 17);
            this.checkBoxSliceData.TabIndex = 3;
            this.checkBoxSliceData.Text = "Slice Input Data";
            this.checkBoxSliceData.UseVisualStyleBackColor = true;
            this.checkBoxSliceData.CheckedChanged += new System.EventHandler(this.checkBoxSliceData_CheckedChanged);
            // 
            // buttonBrowseInput
            // 
            this.buttonBrowseInput.Location = new System.Drawing.Point(552, 17);
            this.buttonBrowseInput.Name = "buttonBrowseInput";
            this.buttonBrowseInput.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowseInput.TabIndex = 2;
            this.buttonBrowseInput.Text = "...";
            this.buttonBrowseInput.UseVisualStyleBackColor = true;
            this.buttonBrowseInput.Click += new System.EventHandler(this.buttonBrowseInput_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 70);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Interval (sec):";
            // 
            // textBoxInPath
            // 
            this.textBoxInPath.Location = new System.Drawing.Point(90, 19);
            this.textBoxInPath.Name = "textBoxInPath";
            this.textBoxInPath.Size = new System.Drawing.Size(456, 20);
            this.textBoxInPath.TabIndex = 1;
            this.textBoxInPath.TextChanged += new System.EventHandler(this.textBoxInPath_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input File Path:";
            // 
            // buttonRun
            // 
            this.buttonRun.Enabled = false;
            this.buttonRun.Location = new System.Drawing.Point(510, 339);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 5;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // checkBoxFlush
            // 
            this.checkBoxFlush.AutoSize = true;
            this.checkBoxFlush.Location = new System.Drawing.Point(6, 23);
            this.checkBoxFlush.Name = "checkBoxFlush";
            this.checkBoxFlush.Size = new System.Drawing.Size(89, 17);
            this.checkBoxFlush.TabIndex = 6;
            this.checkBoxFlush.Text = "Flush Results";
            this.checkBoxFlush.UseVisualStyleBackColor = true;
            this.checkBoxFlush.CheckedChanged += new System.EventHandler(this.checkBoxFlush_CheckedChanged);
            // 
            // textBoxResult
            // 
            this.textBoxResult.Enabled = false;
            this.textBoxResult.Location = new System.Drawing.Point(101, 20);
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.Size = new System.Drawing.Size(377, 20);
            this.textBoxResult.TabIndex = 7;
            // 
            // buttonBrowseRes
            // 
            this.buttonBrowseRes.Enabled = false;
            this.buttonBrowseRes.Location = new System.Drawing.Point(484, 19);
            this.buttonBrowseRes.Name = "buttonBrowseRes";
            this.buttonBrowseRes.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowseRes.TabIndex = 2;
            this.buttonBrowseRes.Text = "...";
            this.buttonBrowseRes.UseVisualStyleBackColor = true;
            this.buttonBrowseRes.Click += new System.EventHandler(this.buttonBrowseResult_Click);
            // 
            // groupBoxResults
            // 
            this.groupBoxResults.Controls.Add(this.checkBoxArcive);
            this.groupBoxResults.Controls.Add(this.checkBoxFlush);
            this.groupBoxResults.Controls.Add(this.textBoxResult);
            this.groupBoxResults.Controls.Add(this.buttonBrowseRes);
            this.groupBoxResults.Location = new System.Drawing.Point(3, 106);
            this.groupBoxResults.Name = "groupBoxResults";
            this.groupBoxResults.Size = new System.Drawing.Size(582, 56);
            this.groupBoxResults.TabIndex = 8;
            this.groupBoxResults.TabStop = false;
            this.groupBoxResults.Text = "Results";
            // 
            // checkBoxArcive
            // 
            this.checkBoxArcive.AutoSize = true;
            this.checkBoxArcive.Location = new System.Drawing.Point(514, 23);
            this.checkBoxArcive.Name = "checkBoxArcive";
            this.checkBoxArcive.Size = new System.Drawing.Size(62, 17);
            this.checkBoxArcive.TabIndex = 8;
            this.checkBoxArcive.Text = "Archive";
            this.checkBoxArcive.UseVisualStyleBackColor = true;
            // 
            // WCFRunnerCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxResults);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.groupBoxInput);
            this.Controls.Add(this.buttonRun);
            this.Name = "WCFRunnerCtrl";
            this.Size = new System.Drawing.Size(588, 386);
            this.groupBoxOutput.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOutput)).EndInit();
            this.groupBoxInput.ResumeLayout(false);
            this.groupBoxInput.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSliceInterval)).EndInit();
            this.groupBoxResults.ResumeLayout(false);
            this.groupBoxResults.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.DataGridView dataGridViewOutput;
        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.NumericUpDown numericUpDownSliceInterval;
        private System.Windows.Forms.CheckBox checkBoxSliceData;
        private System.Windows.Forms.Button buttonBrowseInput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxInPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.TextBox textBoxResult;
        private System.Windows.Forms.CheckBox checkBoxFlush;
        private System.Windows.Forms.Button buttonBrowseRes;
        private System.Windows.Forms.GroupBox groupBoxResults;
        private System.Windows.Forms.CheckBox checkBoxRTInterval;
        private System.Windows.Forms.CheckBox checkBoxArcive;
    }
}
