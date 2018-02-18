namespace TestClient
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
            this.buttonGetData = new System.Windows.Forms.Button();
            this.buttonGetContextData = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonGetData
            // 
            this.buttonGetData.Location = new System.Drawing.Point(96, 75);
            this.buttonGetData.Name = "buttonGetData";
            this.buttonGetData.Size = new System.Drawing.Size(75, 23);
            this.buttonGetData.TabIndex = 0;
            this.buttonGetData.Text = "Get Data";
            this.buttonGetData.UseVisualStyleBackColor = true;
            this.buttonGetData.Click += new System.EventHandler(this.buttonGetData_Click);
            // 
            // buttonGetContextData
            // 
            this.buttonGetContextData.Location = new System.Drawing.Point(294, 75);
            this.buttonGetContextData.Name = "buttonGetContextData";
            this.buttonGetContextData.Size = new System.Drawing.Size(118, 23);
            this.buttonGetContextData.TabIndex = 1;
            this.buttonGetContextData.Text = "Get Composite Data";
            this.buttonGetContextData.UseVisualStyleBackColor = true;
            this.buttonGetContextData.Click += new System.EventHandler(this.buttonGetContextData_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(702, 222);
            this.Controls.Add(this.buttonGetContextData);
            this.Controls.Add(this.buttonGetData);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonGetData;
        private System.Windows.Forms.Button buttonGetContextData;
    }
}

