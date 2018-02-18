namespace REgexTest
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
            this.textBoxTest = new System.Windows.Forms.TextBox();
            this.textBoxValid = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBoxTest
            // 
            this.textBoxTest.Location = new System.Drawing.Point(12, 29);
            this.textBoxTest.Name = "textBoxTest";
            this.textBoxTest.Size = new System.Drawing.Size(440, 20);
            this.textBoxTest.TabIndex = 0;
            this.textBoxTest.TextChanged += new System.EventHandler(this.textBoxTest_TextChanged);
            // 
            // textBoxValid
            // 
            this.textBoxValid.Location = new System.Drawing.Point(12, 120);
            this.textBoxValid.Multiline = true;
            this.textBoxValid.Name = "textBoxValid";
            this.textBoxValid.ReadOnly = true;
            this.textBoxValid.Size = new System.Drawing.Size(440, 66);
            this.textBoxValid.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 261);
            this.Controls.Add(this.textBoxValid);
            this.Controls.Add(this.textBoxTest);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxTest;
        private System.Windows.Forms.TextBox textBoxValid;
    }
}

