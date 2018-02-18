namespace TracingsSplitter
{
    partial class Form1
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
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxInput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownChunkLength = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numericUpDownCapReoccurance = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.numericUpDownGapSize = new System.Windows.Forms.NumericUpDown();
            this.checkBoxAddGap = new System.Windows.Forms.CheckBox();
            this.buttonRun = new System.Windows.Forms.Button();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonBrowseOut = new System.Windows.Forms.Button();
            this.radioButtonToFile = new System.Windows.Forms.RadioButton();
            this.radioButtonOutputFolder = new System.Windows.Forms.RadioButton();
            this.buttonBrowseFolderOut = new System.Windows.Forms.Button();
            this.textBoxFolderPath = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownChunkLength)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCapReoccurance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGapSize)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(511, 12);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowse.TabIndex = 0;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Input XML:";
            // 
            // textBoxInput
            // 
            this.textBoxInput.Location = new System.Drawing.Point(85, 14);
            this.textBoxInput.Name = "textBoxInput";
            this.textBoxInput.Size = new System.Drawing.Size(420, 20);
            this.textBoxInput.TabIndex = 2;
            this.textBoxInput.TextChanged += new System.EventHandler(this.textBoxInput_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 21);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(92, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Split to chunks of:";
            // 
            // numericUpDownChunkLength
            // 
            this.numericUpDownChunkLength.Location = new System.Drawing.Point(108, 19);
            this.numericUpDownChunkLength.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownChunkLength.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownChunkLength.Name = "numericUpDownChunkLength";
            this.numericUpDownChunkLength.Size = new System.Drawing.Size(72, 20);
            this.numericUpDownChunkLength.TabIndex = 4;
            this.numericUpDownChunkLength.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(186, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(26, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Sec";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.numericUpDownCapReoccurance);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.numericUpDownGapSize);
            this.groupBox1.Controls.Add(this.checkBoxAddGap);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.numericUpDownChunkLength);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(12, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(523, 87);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Split Data";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(349, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Sec";
            this.label5.Visible = false;
            // 
            // numericUpDownCapReoccurance
            // 
            this.numericUpDownCapReoccurance.Location = new System.Drawing.Point(271, 53);
            this.numericUpDownCapReoccurance.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownCapReoccurance.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCapReoccurance.Name = "numericUpDownCapReoccurance";
            this.numericUpDownCapReoccurance.Size = new System.Drawing.Size(72, 20);
            this.numericUpDownCapReoccurance.TabIndex = 9;
            this.numericUpDownCapReoccurance.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownCapReoccurance.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(186, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Seconds, each";
            this.label4.Visible = false;
            // 
            // numericUpDownGapSize
            // 
            this.numericUpDownGapSize.Location = new System.Drawing.Point(108, 53);
            this.numericUpDownGapSize.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numericUpDownGapSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownGapSize.Name = "numericUpDownGapSize";
            this.numericUpDownGapSize.Size = new System.Drawing.Size(72, 20);
            this.numericUpDownGapSize.TabIndex = 7;
            this.numericUpDownGapSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownGapSize.Visible = false;
            // 
            // checkBoxAddGap
            // 
            this.checkBoxAddGap.AutoSize = true;
            this.checkBoxAddGap.Location = new System.Drawing.Point(9, 54);
            this.checkBoxAddGap.Name = "checkBoxAddGap";
            this.checkBoxAddGap.Size = new System.Drawing.Size(93, 17);
            this.checkBoxAddGap.TabIndex = 6;
            this.checkBoxAddGap.Text = "Create gap of:";
            this.checkBoxAddGap.UseVisualStyleBackColor = true;
            this.checkBoxAddGap.Visible = false;
            this.checkBoxAddGap.CheckedChanged += new System.EventHandler(this.checkBoxAddGap_CheckedChanged);
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(460, 197);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 7;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(110, 134);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(395, 20);
            this.textBoxOutput.TabIndex = 10;
            // 
            // buttonBrowseOut
            // 
            this.buttonBrowseOut.Location = new System.Drawing.Point(511, 132);
            this.buttonBrowseOut.Name = "buttonBrowseOut";
            this.buttonBrowseOut.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowseOut.TabIndex = 8;
            this.buttonBrowseOut.Text = "...";
            this.buttonBrowseOut.UseVisualStyleBackColor = true;
            this.buttonBrowseOut.Click += new System.EventHandler(this.buttonBrowseOut_Click);
            // 
            // radioButtonToFile
            // 
            this.radioButtonToFile.AutoSize = true;
            this.radioButtonToFile.Checked = true;
            this.radioButtonToFile.Location = new System.Drawing.Point(12, 135);
            this.radioButtonToFile.Name = "radioButtonToFile";
            this.radioButtonToFile.Size = new System.Drawing.Size(79, 17);
            this.radioButtonToFile.TabIndex = 11;
            this.radioButtonToFile.TabStop = true;
            this.radioButtonToFile.Text = "Output File:";
            this.radioButtonToFile.UseVisualStyleBackColor = true;
            // 
            // radioButtonOutputFolder
            // 
            this.radioButtonOutputFolder.AutoSize = true;
            this.radioButtonOutputFolder.Location = new System.Drawing.Point(12, 164);
            this.radioButtonOutputFolder.Name = "radioButtonOutputFolder";
            this.radioButtonOutputFolder.Size = new System.Drawing.Size(92, 17);
            this.radioButtonOutputFolder.TabIndex = 11;
            this.radioButtonOutputFolder.Text = "Output Folder:";
            this.radioButtonOutputFolder.UseVisualStyleBackColor = true;
            this.radioButtonOutputFolder.CheckedChanged += new System.EventHandler(this.radioButtonOutputFolder_CheckedChanged);
            // 
            // buttonBrowseFolderOut
            // 
            this.buttonBrowseFolderOut.Enabled = false;
            this.buttonBrowseFolderOut.Location = new System.Drawing.Point(511, 161);
            this.buttonBrowseFolderOut.Name = "buttonBrowseFolderOut";
            this.buttonBrowseFolderOut.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowseFolderOut.TabIndex = 8;
            this.buttonBrowseFolderOut.Text = "...";
            this.buttonBrowseFolderOut.UseVisualStyleBackColor = true;
            this.buttonBrowseFolderOut.Click += new System.EventHandler(this.buttonBrowseFolderOut_Click);
            // 
            // textBoxFolderPath
            // 
            this.textBoxFolderPath.Enabled = false;
            this.textBoxFolderPath.Location = new System.Drawing.Point(110, 163);
            this.textBoxFolderPath.Name = "textBoxFolderPath";
            this.textBoxFolderPath.Size = new System.Drawing.Size(395, 20);
            this.textBoxFolderPath.TabIndex = 10;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(547, 232);
            this.Controls.Add(this.radioButtonOutputFolder);
            this.Controls.Add(this.radioButtonToFile);
            this.Controls.Add(this.textBoxFolderPath);
            this.Controls.Add(this.buttonBrowseFolderOut);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.buttonBrowseOut);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBoxInput);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonBrowse);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownChunkLength)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownCapReoccurance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownGapSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxInput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownChunkLength;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDownCapReoccurance;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numericUpDownGapSize;
        private System.Windows.Forms.CheckBox checkBoxAddGap;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonBrowseOut;
        private System.Windows.Forms.RadioButton radioButtonToFile;
        private System.Windows.Forms.RadioButton radioButtonOutputFolder;
        private System.Windows.Forms.Button buttonBrowseFolderOut;
        private System.Windows.Forms.TextBox textBoxFolderPath;
    }
}

