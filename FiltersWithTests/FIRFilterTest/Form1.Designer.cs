namespace FIRFilterTest
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
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxInputFile = new System.Windows.Forms.TextBox();
            this.textBoxCoeffs = new System.Windows.Forms.TextBox();
            this.buttonBrowseInput = new System.Windows.Forms.Button();
            this.buttonBrowseCoeffs = new System.Windows.Forms.Button();
            this.buttonBrowseOutput = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxOutput = new System.Windows.Forms.TextBox();
            this.buttonRun = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Input File:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 48);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Coeffs File:";
            // 
            // textBoxInputFile
            // 
            this.textBoxInputFile.Location = new System.Drawing.Point(79, 16);
            this.textBoxInputFile.Name = "textBoxInputFile";
            this.textBoxInputFile.Size = new System.Drawing.Size(378, 20);
            this.textBoxInputFile.TabIndex = 0;
            // 
            // textBoxCoeffs
            // 
            this.textBoxCoeffs.Location = new System.Drawing.Point(79, 45);
            this.textBoxCoeffs.Name = "textBoxCoeffs";
            this.textBoxCoeffs.Size = new System.Drawing.Size(378, 20);
            this.textBoxCoeffs.TabIndex = 2;
            // 
            // buttonBrowseInput
            // 
            this.buttonBrowseInput.Location = new System.Drawing.Point(463, 13);
            this.buttonBrowseInput.Name = "buttonBrowseInput";
            this.buttonBrowseInput.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowseInput.TabIndex = 1;
            this.buttonBrowseInput.Text = "...";
            this.buttonBrowseInput.UseVisualStyleBackColor = true;
            this.buttonBrowseInput.Click += new System.EventHandler(this.buttonBrowseInput_Click);
            // 
            // buttonBrowseCoeffs
            // 
            this.buttonBrowseCoeffs.Location = new System.Drawing.Point(463, 42);
            this.buttonBrowseCoeffs.Name = "buttonBrowseCoeffs";
            this.buttonBrowseCoeffs.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowseCoeffs.TabIndex = 3;
            this.buttonBrowseCoeffs.Text = "...";
            this.buttonBrowseCoeffs.UseVisualStyleBackColor = true;
            this.buttonBrowseCoeffs.Click += new System.EventHandler(this.buttonBrowseCoeffs_Click);
            // 
            // buttonBrowseOutput
            // 
            this.buttonBrowseOutput.Location = new System.Drawing.Point(463, 71);
            this.buttonBrowseOutput.Name = "buttonBrowseOutput";
            this.buttonBrowseOutput.Size = new System.Drawing.Size(24, 23);
            this.buttonBrowseOutput.TabIndex = 5;
            this.buttonBrowseOutput.Text = "...";
            this.buttonBrowseOutput.UseVisualStyleBackColor = true;
            this.buttonBrowseOutput.Click += new System.EventHandler(this.buttonBrowseOutput_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 77);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Output File:";
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Location = new System.Drawing.Point(79, 74);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Size = new System.Drawing.Size(378, 20);
            this.textBoxOutput.TabIndex = 4;
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(412, 100);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 6;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 150);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonBrowseOutput);
            this.Controls.Add(this.buttonBrowseCoeffs);
            this.Controls.Add(this.buttonBrowseInput);
            this.Controls.Add(this.textBoxOutput);
            this.Controls.Add(this.textBoxCoeffs);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxInputFile);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "Fir Filter";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxInputFile;
        private System.Windows.Forms.TextBox textBoxCoeffs;
        private System.Windows.Forms.Button buttonBrowseInput;
        private System.Windows.Forms.Button buttonBrowseCoeffs;
        private System.Windows.Forms.Button buttonBrowseOutput;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxOutput;
        private System.Windows.Forms.Button buttonRun;
    }
}

