namespace PeriGen.Patterns.BatchProcessor
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxInputFilePath = new System.Windows.Forms.TextBox();
            this.buttonBrowseInput = new System.Windows.Forms.Button();
            this.buttonBrowseOutput = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxOutFilePath = new System.Windows.Forms.TextBox();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.checkBoxV01 = new System.Windows.Forms.CheckBox();
            this.checkBoxIn = new System.Windows.Forms.CheckBox();
            this.checkBoxXML = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.labelProcessing = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input Folder:";
            // 
            // textBoxInputFilePath
            // 
            this.textBoxInputFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxInputFilePath.Location = new System.Drawing.Point(92, 13);
            this.textBoxInputFilePath.Name = "textBoxInputFilePath";
            this.textBoxInputFilePath.Size = new System.Drawing.Size(355, 20);
            this.textBoxInputFilePath.TabIndex = 1;
            this.textBoxInputFilePath.TextChanged += new System.EventHandler(this.textBoxFilePath_TextChanged);
            // 
            // buttonBrowseInput
            // 
            this.buttonBrowseInput.Location = new System.Drawing.Point(453, 11);
            this.buttonBrowseInput.Name = "buttonBrowseInput";
            this.buttonBrowseInput.Size = new System.Drawing.Size(28, 23);
            this.buttonBrowseInput.TabIndex = 2;
            this.buttonBrowseInput.Text = "...";
            this.buttonBrowseInput.UseVisualStyleBackColor = true;
            this.buttonBrowseInput.Click += new System.EventHandler(this.buttonBrowseInput_Click);
            // 
            // buttonBrowseOutput
            // 
            this.buttonBrowseOutput.Location = new System.Drawing.Point(453, 62);
            this.buttonBrowseOutput.Name = "buttonBrowseOutput";
            this.buttonBrowseOutput.Size = new System.Drawing.Size(28, 23);
            this.buttonBrowseOutput.TabIndex = 3;
            this.buttonBrowseOutput.Text = "...";
            this.buttonBrowseOutput.UseVisualStyleBackColor = true;
            this.buttonBrowseOutput.Click += new System.EventHandler(this.buttonBrowseOutput_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Output Folder:";
            // 
            // textBoxOutFilePath
            // 
            this.textBoxOutFilePath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxOutFilePath.Location = new System.Drawing.Point(92, 62);
            this.textBoxOutFilePath.Name = "textBoxOutFilePath";
            this.textBoxOutFilePath.Size = new System.Drawing.Size(355, 20);
            this.textBoxOutFilePath.TabIndex = 5;
            this.textBoxOutFilePath.TextChanged += new System.EventHandler(this.textBoxFilePath_TextChanged);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(406, 124);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 6;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Enabled = false;
            this.buttonRun.Location = new System.Drawing.Point(325, 124);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 6;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // checkBoxV01
            // 
            this.checkBoxV01.AutoSize = true;
            this.checkBoxV01.Enabled = false;
            this.checkBoxV01.Location = new System.Drawing.Point(181, 39);
            this.checkBoxV01.Name = "checkBoxV01";
            this.checkBoxV01.Size = new System.Drawing.Size(69, 17);
            this.checkBoxV01.TabIndex = 7;
            this.checkBoxV01.Text = "V01 Files";
            this.checkBoxV01.UseVisualStyleBackColor = true;
            this.checkBoxV01.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // checkBoxIn
            // 
            this.checkBoxIn.AutoSize = true;
            this.checkBoxIn.Enabled = false;
            this.checkBoxIn.Location = new System.Drawing.Point(256, 39);
            this.checkBoxIn.Name = "checkBoxIn";
            this.checkBoxIn.Size = new System.Drawing.Size(61, 17);
            this.checkBoxIn.TabIndex = 8;
            this.checkBoxIn.Text = "IN Files";
            this.checkBoxIn.UseVisualStyleBackColor = true;
            this.checkBoxIn.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // checkBoxXML
            // 
            this.checkBoxXML.AutoSize = true;
            this.checkBoxXML.Enabled = false;
            this.checkBoxXML.Location = new System.Drawing.Point(323, 39);
            this.checkBoxXML.Name = "checkBoxXML";
            this.checkBoxXML.Size = new System.Drawing.Size(72, 17);
            this.checkBoxXML.TabIndex = 9;
            this.checkBoxXML.Text = "XML Files";
            this.checkBoxXML.UseVisualStyleBackColor = true;
            this.checkBoxXML.CheckedChanged += new System.EventHandler(this.CheckBox_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(89, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Files to process:";
            // 
            // labelProcessing
            // 
            this.labelProcessing.AutoSize = true;
            this.labelProcessing.Location = new System.Drawing.Point(92, 89);
            this.labelProcessing.Name = "labelProcessing";
            this.labelProcessing.Size = new System.Drawing.Size(0, 13);
            this.labelProcessing.TabIndex = 11;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(493, 159);
            this.Controls.Add(this.labelProcessing);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.checkBoxXML);
            this.Controls.Add(this.checkBoxIn);
            this.Controls.Add(this.checkBoxV01);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.textBoxOutFilePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonBrowseOutput);
            this.Controls.Add(this.buttonBrowseInput);
            this.Controls.Add(this.textBoxInputFilePath);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Types Converter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxInputFilePath;
        private System.Windows.Forms.Button buttonBrowseInput;
        private System.Windows.Forms.Button buttonBrowseOutput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxOutFilePath;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.CheckBox checkBoxV01;
        private System.Windows.Forms.CheckBox checkBoxIn;
        private System.Windows.Forms.CheckBox checkBoxXML;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label labelProcessing;
    }
}

