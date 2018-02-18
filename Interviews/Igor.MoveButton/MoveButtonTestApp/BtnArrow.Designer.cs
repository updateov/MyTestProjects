namespace MoveButtonTestApp
{
    partial class BtnArrow
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
            this.SuspendLayout();
            // 
            // BtnArrow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(50, 50);
            this.Name = "BtnArrow";
            this.Size = new System.Drawing.Size(50, 50);
            this.Load += new System.EventHandler(this.BtnArrow_Load);
            this.Click += new System.EventHandler(this.BtnArrow_Click);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.BtnArrow_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.BtnArrow_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
