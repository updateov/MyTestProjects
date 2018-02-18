namespace PatternsApplication
{
    partial class WebPageControl
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
            this.webBrowserTracingPatterns = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // webBrowserTracingPatterns
            // 
            this.webBrowserTracingPatterns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowserTracingPatterns.Location = new System.Drawing.Point(0, 0);
            this.webBrowserTracingPatterns.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowserTracingPatterns.Name = "webBrowserTracingPatterns";
            this.webBrowserTracingPatterns.ScrollBarsEnabled = false;
            this.webBrowserTracingPatterns.Size = new System.Drawing.Size(473, 282);
            this.webBrowserTracingPatterns.TabIndex = 0;
            // 
            // WebPageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.webBrowserTracingPatterns);
            this.Name = "WebPageControl";
            this.Size = new System.Drawing.Size(473, 282);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowserTracingPatterns;
    }
}
