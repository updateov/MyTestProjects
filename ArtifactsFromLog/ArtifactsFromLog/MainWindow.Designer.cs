namespace ArtifactsFromLog
{
    partial class MainWindow
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
            this.textBoxInPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.buttonExtract = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxInPath
            // 
            this.textBoxInPath.Location = new System.Drawing.Point(71, 12);
            this.textBoxInPath.Name = "textBoxInPath";
            this.textBoxInPath.Size = new System.Drawing.Size(535, 20);
            this.textBoxInPath.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Input File:";
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Location = new System.Drawing.Point(612, 10);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowse.TabIndex = 2;
            this.buttonBrowse.Text = "...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // buttonExtract
            // 
            this.buttonExtract.Enabled = false;
            this.buttonExtract.Location = new System.Drawing.Point(563, 39);
            this.buttonExtract.Name = "buttonExtract";
            this.buttonExtract.Size = new System.Drawing.Size(75, 23);
            this.buttonExtract.TabIndex = 3;
            this.buttonExtract.Text = "Extract";
            this.buttonExtract.UseVisualStyleBackColor = true;
            this.buttonExtract.Click += new System.EventHandler(this.buttonExtract_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 74);
            this.Controls.Add(this.buttonExtract);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxInPath);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainWindow";
            this.ShowIcon = false;
            this.Text = "Extract Artifacts";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxInPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.Button buttonExtract;
    }
}

