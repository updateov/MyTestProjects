namespace reader2
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
            this.Start = new System.Windows.Forms.Button();
            this.RequestInterval = new System.Windows.Forms.TextBox();
            this.Duration = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.Outputfile = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.Outdir = new System.Windows.Forms.TextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.Stop = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(46, 153);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(75, 23);
            this.Start.TabIndex = 0;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // RequestInterval
            // 
            this.RequestInterval.Location = new System.Drawing.Point(160, 11);
            this.RequestInterval.Name = "RequestInterval";
            this.RequestInterval.Size = new System.Drawing.Size(28, 20);
            this.RequestInterval.TabIndex = 2;
            this.RequestInterval.Text = "10";
            // 
            // Duration
            // 
            this.Duration.Location = new System.Drawing.Point(160, 47);
            this.Duration.Name = "Duration";
            this.Duration.Size = new System.Drawing.Size(28, 20);
            this.Duration.TabIndex = 3;
            this.Duration.Text = "300";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Request Interval (sec):";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Duration (min):";
            // 
            // button1
            // 
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(46, 82);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Directory:";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.HelpRequest += new System.EventHandler(this.folderBrowserDialog1_HelpRequest);
            // 
            // Outputfile
            // 
            this.Outputfile.Location = new System.Drawing.Point(159, 115);
            this.Outputfile.Name = "Outputfile";
            this.Outputfile.Size = new System.Drawing.Size(123, 20);
            this.Outputfile.TabIndex = 7;
            this.Outputfile.Text = "CriReader.csv";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(43, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "File name:";
            // 
            // Outdir
            // 
            this.Outdir.Location = new System.Drawing.Point(160, 84);
            this.Outdir.Name = "Outdir";
            this.Outdir.Size = new System.Drawing.Size(123, 20);
            this.Outdir.TabIndex = 9;
            //this.Outdir.Text = "d:\\eandc";
            this.Outdir.Text = "d:\\eandc";
            this.Outdir.Text = "";

            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(46, 194);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(246, 19);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 10;
            // 
            // Stop
            // 
            this.Stop.Location = new System.Drawing.Point(160, 153);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(75, 23);
            this.Stop.TabIndex = 11;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(294, 212);
            this.Controls.Add(this.Stop);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.Outdir);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Outputfile);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Duration);
            this.Controls.Add(this.RequestInterval);
            this.Controls.Add(this.Start);
            this.Name = "Form1";
            this.Text = "CRI Status  Reader";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.TextBox RequestInterval;
        private System.Windows.Forms.TextBox Duration;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox Outputfile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox Outdir;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button Stop;
    }
}

