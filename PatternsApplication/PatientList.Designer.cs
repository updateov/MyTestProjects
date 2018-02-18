namespace PatternsApplication
{
    partial class PatientList
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCollapse = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonCollapse
            // 
            this.buttonCollapse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCollapse.Location = new System.Drawing.Point(220, 3);
            this.buttonCollapse.Name = "buttonCollapse";
            this.buttonCollapse.Size = new System.Drawing.Size(15, 23);
            this.buttonCollapse.TabIndex = 0;
            this.buttonCollapse.Text = "<";
            this.buttonCollapse.UseVisualStyleBackColor = true;
            this.buttonCollapse.Click += new System.EventHandler(this.buttonCollapse_Click);
            // 
            // PatientList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonCollapse);
            this.Name = "PatientList";
            this.Size = new System.Drawing.Size(238, 447);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonCollapse;
    }
}
