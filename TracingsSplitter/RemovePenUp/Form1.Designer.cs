namespace RemovePenUp
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxFileIn = new System.Windows.Forms.TextBox();
            this.buttonBrowseIn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxFileOut = new System.Windows.Forms.TextBox();
            this.buttonBrowseOut = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonRun = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 47);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "File In:";
            // 
            // textBoxFileIn
            // 
            this.textBoxFileIn.Location = new System.Drawing.Point(75, 44);
            this.textBoxFileIn.Name = "textBoxFileIn";
            this.textBoxFileIn.Size = new System.Drawing.Size(490, 20);
            this.textBoxFileIn.TabIndex = 1;
            // 
            // buttonBrowseIn
            // 
            this.buttonBrowseIn.Location = new System.Drawing.Point(571, 42);
            this.buttonBrowseIn.Name = "buttonBrowseIn";
            this.buttonBrowseIn.Size = new System.Drawing.Size(25, 23);
            this.buttonBrowseIn.TabIndex = 2;
            this.buttonBrowseIn.Text = "...";
            this.buttonBrowseIn.UseVisualStyleBackColor = true;
            this.buttonBrowseIn.Click += new System.EventHandler(this.buttonBrowseIn_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "File Out:";
            // 
            // textBoxFileOut
            // 
            this.textBoxFileOut.Location = new System.Drawing.Point(75, 70);
            this.textBoxFileOut.Name = "textBoxFileOut";
            this.textBoxFileOut.Size = new System.Drawing.Size(490, 20);
            this.textBoxFileOut.TabIndex = 1;
            // 
            // buttonBrowseOut
            // 
            this.buttonBrowseOut.Location = new System.Drawing.Point(571, 68);
            this.buttonBrowseOut.Name = "buttonBrowseOut";
            this.buttonBrowseOut.Size = new System.Drawing.Size(25, 23);
            this.buttonBrowseOut.TabIndex = 2;
            this.buttonBrowseOut.Text = "...";
            this.buttonBrowseOut.UseVisualStyleBackColor = true;
            this.buttonBrowseOut.Click += new System.EventHandler(this.buttonBrowseOut_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(521, 125);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 3;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonRun
            // 
            this.buttonRun.Location = new System.Drawing.Point(440, 125);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 3;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(608, 160);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonBrowseOut);
            this.Controls.Add(this.buttonBrowseIn);
            this.Controls.Add(this.textBoxFileOut);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxFileIn);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxFileIn;
        private System.Windows.Forms.Button buttonBrowseIn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxFileOut;
        private System.Windows.Forms.Button buttonBrowseOut;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonRun;
    }
}

