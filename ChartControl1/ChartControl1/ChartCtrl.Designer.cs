namespace ChartControl1
{
    partial class ChartCtrl
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
            // ChartCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.DoubleBuffered = true;
            this.MinimumSize = new System.Drawing.Size(700, 0);
            this.Name = "ChartCtrl";
            this.Size = new System.Drawing.Size(1054, 492);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.ChartCtrl_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ChartCtrl_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ChartCtrl_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ChartCtrl_MouseUp);
            this.Resize += new System.EventHandler(this.ChartCtrl_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}
