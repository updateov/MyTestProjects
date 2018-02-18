namespace Base64ToByteArray
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
            this.textBoxBase64 = new System.Windows.Forms.TextBox();
            this.buttonRun = new System.Windows.Forms.Button();
            this.listBoxBytes = new System.Windows.Forms.ListBox();
            this.labelTotal = new System.Windows.Forms.Label();
            this.labelSamples = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxBase64
            // 
            this.textBoxBase64.Location = new System.Drawing.Point(13, 13);
            this.textBoxBase64.Multiline = true;
            this.textBoxBase64.Name = "textBoxBase64";
            this.textBoxBase64.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxBase64.Size = new System.Drawing.Size(1043, 153);
            this.textBoxBase64.TabIndex = 0;
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(981, 445);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 2;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // listBoxBytes
            // 
            this.listBoxBytes.FormattingEnabled = true;
            this.listBoxBytes.IntegralHeight = false;
            this.listBoxBytes.Location = new System.Drawing.Point(13, 172);
            this.listBoxBytes.Name = "listBoxBytes";
            this.listBoxBytes.Size = new System.Drawing.Size(363, 296);
            this.listBoxBytes.TabIndex = 3;
            // 
            // labelTotal
            // 
            this.labelTotal.AutoSize = true;
            this.labelTotal.Location = new System.Drawing.Point(485, 299);
            this.labelTotal.Name = "labelTotal";
            this.labelTotal.Size = new System.Drawing.Size(10, 13);
            this.labelTotal.TabIndex = 4;
            this.labelTotal.Text = ".";
            // 
            // labelSamples
            // 
            this.labelSamples.AutoSize = true;
            this.labelSamples.Location = new System.Drawing.Point(485, 286);
            this.labelSamples.Name = "labelSamples";
            this.labelSamples.Size = new System.Drawing.Size(10, 13);
            this.labelSamples.TabIndex = 5;
            this.labelSamples.Text = ".";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1068, 480);
            this.Controls.Add(this.labelSamples);
            this.Controls.Add(this.labelTotal);
            this.Controls.Add(this.listBoxBytes);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.textBoxBase64);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxBase64;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.ListBox listBoxBytes;
        private System.Windows.Forms.Label labelTotal;
        private System.Windows.Forms.Label labelSamples;
    }
}

