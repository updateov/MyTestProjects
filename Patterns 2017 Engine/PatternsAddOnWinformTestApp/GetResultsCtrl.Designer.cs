namespace PatternsAddOnWinformTestApp
{
    partial class GetResultsCtrl
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
            this.dataGridViewOutput = new System.Windows.Forms.DataGridView();
            this.buttonGetResults = new System.Windows.Forms.Button();
            this.buttonClear = new System.Windows.Forms.Button();
            this.groupBoxResults = new System.Windows.Forms.GroupBox();
            this.checkBoxArcive = new System.Windows.Forms.CheckBox();
            this.checkBoxFlush = new System.Windows.Forms.CheckBox();
            this.textBoxResult = new System.Windows.Forms.TextBox();
            this.buttonBrowseRes = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOutput)).BeginInit();
            this.groupBoxResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewOutput
            // 
            this.dataGridViewOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewOutput.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewOutput.Location = new System.Drawing.Point(3, 46);
            this.dataGridViewOutput.Name = "dataGridViewOutput";
            this.dataGridViewOutput.RowHeadersVisible = false;
            this.dataGridViewOutput.Size = new System.Drawing.Size(642, 308);
            this.dataGridViewOutput.TabIndex = 0;
            // 
            // buttonGetResults
            // 
            this.buttonGetResults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGetResults.Location = new System.Drawing.Point(576, 366);
            this.buttonGetResults.Name = "buttonGetResults";
            this.buttonGetResults.Size = new System.Drawing.Size(75, 23);
            this.buttonGetResults.TabIndex = 1;
            this.buttonGetResults.Text = "Get Results";
            this.buttonGetResults.UseVisualStyleBackColor = true;
            this.buttonGetResults.Click += new System.EventHandler(this.buttonGetResults_Click);
            // 
            // buttonClear
            // 
            this.buttonClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClear.Location = new System.Drawing.Point(495, 366);
            this.buttonClear.Name = "buttonClear";
            this.buttonClear.Size = new System.Drawing.Size(75, 23);
            this.buttonClear.TabIndex = 1;
            this.buttonClear.Text = "Clear";
            this.buttonClear.UseVisualStyleBackColor = true;
            this.buttonClear.Click += new System.EventHandler(this.buttonClear_Click);
            // 
            // groupBoxResults
            // 
            this.groupBoxResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxResults.Controls.Add(this.checkBoxArcive);
            this.groupBoxResults.Controls.Add(this.checkBoxFlush);
            this.groupBoxResults.Controls.Add(this.textBoxResult);
            this.groupBoxResults.Controls.Add(this.dataGridViewOutput);
            this.groupBoxResults.Controls.Add(this.buttonBrowseRes);
            this.groupBoxResults.Location = new System.Drawing.Point(3, 3);
            this.groupBoxResults.Name = "groupBoxResults";
            this.groupBoxResults.Size = new System.Drawing.Size(648, 357);
            this.groupBoxResults.TabIndex = 10;
            this.groupBoxResults.TabStop = false;
            this.groupBoxResults.Text = "Results";
            // 
            // checkBoxArcive
            // 
            this.checkBoxArcive.AutoSize = true;
            this.checkBoxArcive.Location = new System.Drawing.Point(580, 21);
            this.checkBoxArcive.Name = "checkBoxArcive";
            this.checkBoxArcive.Size = new System.Drawing.Size(62, 17);
            this.checkBoxArcive.TabIndex = 8;
            this.checkBoxArcive.Text = "Archive";
            this.checkBoxArcive.UseVisualStyleBackColor = true;
            // 
            // checkBoxFlush
            // 
            this.checkBoxFlush.AutoSize = true;
            this.checkBoxFlush.Location = new System.Drawing.Point(6, 21);
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
            this.textBoxResult.Location = new System.Drawing.Point(101, 19);
            this.textBoxResult.Name = "textBoxResult";
            this.textBoxResult.Size = new System.Drawing.Size(443, 20);
            this.textBoxResult.TabIndex = 7;
            // 
            // buttonBrowseRes
            // 
            this.buttonBrowseRes.Enabled = false;
            this.buttonBrowseRes.Location = new System.Drawing.Point(550, 17);
            this.buttonBrowseRes.Name = "buttonBrowseRes";
            this.buttonBrowseRes.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowseRes.TabIndex = 2;
            this.buttonBrowseRes.Text = "...";
            this.buttonBrowseRes.UseVisualStyleBackColor = true;
            this.buttonBrowseRes.Click += new System.EventHandler(this.buttonBrowseRes_Click);
            // 
            // GetResultsCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxResults);
            this.Controls.Add(this.buttonClear);
            this.Controls.Add(this.buttonGetResults);
            this.Name = "GetResultsCtrl";
            this.Size = new System.Drawing.Size(654, 392);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewOutput)).EndInit();
            this.groupBoxResults.ResumeLayout(false);
            this.groupBoxResults.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewOutput;
        private System.Windows.Forms.Button buttonGetResults;
        private System.Windows.Forms.Button buttonClear;
        private System.Windows.Forms.GroupBox groupBoxResults;
        private System.Windows.Forms.CheckBox checkBoxArcive;
        private System.Windows.Forms.CheckBox checkBoxFlush;
        private System.Windows.Forms.TextBox textBoxResult;
        private System.Windows.Forms.Button buttonBrowseRes;
    }
}
