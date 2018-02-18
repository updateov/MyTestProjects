namespace SplitToFiles
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
            this.buttonBrowseSource = new System.Windows.Forms.Button();
            this.buttonBrowseDest = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSource = new System.Windows.Forms.TextBox();
            this.textBoxDest = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // buttonBrowseSource
            // 
            this.buttonBrowseSource.Location = new System.Drawing.Point(475, 12);
            this.buttonBrowseSource.Name = "buttonBrowseSource";
            this.buttonBrowseSource.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseSource.TabIndex = 0;
            this.buttonBrowseSource.Text = "...";
            this.buttonBrowseSource.UseVisualStyleBackColor = true;
            this.buttonBrowseSource.Click += new System.EventHandler(this.buttonBrowseSource_Click);
            // 
            // buttonBrowseDest
            // 
            this.buttonBrowseDest.Location = new System.Drawing.Point(475, 41);
            this.buttonBrowseDest.Name = "buttonBrowseDest";
            this.buttonBrowseDest.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseDest.TabIndex = 1;
            this.buttonBrowseDest.Text = "...";
            this.buttonBrowseDest.UseVisualStyleBackColor = true;
            this.buttonBrowseDest.Click += new System.EventHandler(this.buttonBrowseDest_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(345, 98);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 2;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(426, 98);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Source Path:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Destination Folder:";
            // 
            // textBoxSource
            // 
            this.textBoxSource.Location = new System.Drawing.Point(113, 14);
            this.textBoxSource.Name = "textBoxSource";
            this.textBoxSource.Size = new System.Drawing.Size(356, 20);
            this.textBoxSource.TabIndex = 6;
            // 
            // textBoxDest
            // 
            this.textBoxDest.Location = new System.Drawing.Point(113, 43);
            this.textBoxDest.Name = "textBoxDest";
            this.textBoxDest.Size = new System.Drawing.Size(356, 20);
            this.textBoxDest.TabIndex = 7;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 133);
            this.Controls.Add(this.textBoxDest);
            this.Controls.Add(this.textBoxSource);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonBrowseDest);
            this.Controls.Add(this.buttonBrowseSource);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonBrowseSource;
        private System.Windows.Forms.Button buttonBrowseDest;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSource;
        private System.Windows.Forms.TextBox textBoxDest;
    }
}

