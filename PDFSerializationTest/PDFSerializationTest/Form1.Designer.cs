namespace PDFSerializationTest
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
            this.textBoxFrom = new System.Windows.Forms.TextBox();
            this.buttonFrom = new System.Windows.Forms.Button();
            this.textBoxTo = new System.Windows.Forms.TextBox();
            this.buttonTo = new System.Windows.Forms.Button();
            this.buttonStart = new System.Windows.Forms.Button();
            this.textBoxOut = new System.Windows.Forms.TextBox();
            this.buttonOut = new System.Windows.Forms.Button();
            this.buttonDeserialize = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBoxFrom
            // 
            this.textBoxFrom.Location = new System.Drawing.Point(13, 13);
            this.textBoxFrom.Name = "textBoxFrom";
            this.textBoxFrom.Size = new System.Drawing.Size(421, 20);
            this.textBoxFrom.TabIndex = 0;
            // 
            // buttonFrom
            // 
            this.buttonFrom.Location = new System.Drawing.Point(440, 11);
            this.buttonFrom.Name = "buttonFrom";
            this.buttonFrom.Size = new System.Drawing.Size(75, 23);
            this.buttonFrom.TabIndex = 1;
            this.buttonFrom.Text = "From";
            this.buttonFrom.UseVisualStyleBackColor = true;
            this.buttonFrom.Click += new System.EventHandler(this.buttonFrom_Click);
            // 
            // textBoxTo
            // 
            this.textBoxTo.Location = new System.Drawing.Point(13, 54);
            this.textBoxTo.Name = "textBoxTo";
            this.textBoxTo.Size = new System.Drawing.Size(421, 20);
            this.textBoxTo.TabIndex = 0;
            // 
            // buttonTo
            // 
            this.buttonTo.Location = new System.Drawing.Point(440, 52);
            this.buttonTo.Name = "buttonTo";
            this.buttonTo.Size = new System.Drawing.Size(75, 23);
            this.buttonTo.TabIndex = 1;
            this.buttonTo.Text = "To";
            this.buttonTo.UseVisualStyleBackColor = true;
            this.buttonTo.Click += new System.EventHandler(this.buttonTo_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(440, 148);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(75, 23);
            this.buttonStart.TabIndex = 1;
            this.buttonStart.Text = "Serialize";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // textBoxOut
            // 
            this.textBoxOut.Location = new System.Drawing.Point(13, 98);
            this.textBoxOut.Name = "textBoxOut";
            this.textBoxOut.Size = new System.Drawing.Size(421, 20);
            this.textBoxOut.TabIndex = 0;
            // 
            // buttonOut
            // 
            this.buttonOut.Location = new System.Drawing.Point(440, 96);
            this.buttonOut.Name = "buttonOut";
            this.buttonOut.Size = new System.Drawing.Size(75, 23);
            this.buttonOut.TabIndex = 1;
            this.buttonOut.Text = "Out";
            this.buttonOut.UseVisualStyleBackColor = true;
            this.buttonOut.Click += new System.EventHandler(this.buttonOut_Click);
            // 
            // buttonDeserialize
            // 
            this.buttonDeserialize.Location = new System.Drawing.Point(440, 178);
            this.buttonDeserialize.Name = "buttonDeserialize";
            this.buttonDeserialize.Size = new System.Drawing.Size(75, 23);
            this.buttonDeserialize.TabIndex = 2;
            this.buttonDeserialize.Text = "Deserialize";
            this.buttonDeserialize.UseVisualStyleBackColor = true;
            this.buttonDeserialize.Click += new System.EventHandler(this.buttonDeserialize_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 291);
            this.Controls.Add(this.buttonDeserialize);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.buttonOut);
            this.Controls.Add(this.buttonTo);
            this.Controls.Add(this.textBoxOut);
            this.Controls.Add(this.textBoxTo);
            this.Controls.Add(this.buttonFrom);
            this.Controls.Add(this.textBoxFrom);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxFrom;
        private System.Windows.Forms.Button buttonFrom;
        private System.Windows.Forms.TextBox textBoxTo;
        private System.Windows.Forms.Button buttonTo;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.TextBox textBoxOut;
        private System.Windows.Forms.Button buttonOut;
        private System.Windows.Forms.Button buttonDeserialize;
    }
}

