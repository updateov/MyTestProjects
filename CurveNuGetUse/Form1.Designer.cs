namespace CurveNuGetUse
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
            this.checkListControl = new CLRPatternsUserControls.ChecklistControl();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageCurve = new System.Windows.Forms.TabPage();
            this.tabPagePatterns = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPageCurve.SuspendLayout();
            this.tabPagePatterns.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkListControl
            // 
            this.checkListControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkListControl.Location = new System.Drawing.Point(3, 3);
            this.checkListControl.Name = "checkListControl";
            this.checkListControl.Size = new System.Drawing.Size(854, 551);
            this.checkListControl.TabIndex = 0;
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(3, 3);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(797, 493);
            this.elementHost1.TabIndex = 0;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = null;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageCurve);
            this.tabControl1.Controls.Add(this.tabPagePatterns);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(868, 583);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPageCurve
            // 
            this.tabPageCurve.Controls.Add(this.elementHost1);
            this.tabPageCurve.Location = new System.Drawing.Point(4, 22);
            this.tabPageCurve.Name = "tabPageCurve";
            this.tabPageCurve.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageCurve.Size = new System.Drawing.Size(803, 499);
            this.tabPageCurve.TabIndex = 0;
            this.tabPageCurve.Text = "Curve";
            this.tabPageCurve.UseVisualStyleBackColor = true;
            // 
            // tabPagePatterns
            // 
            this.tabPagePatterns.Controls.Add(this.checkListControl);
            this.tabPagePatterns.Location = new System.Drawing.Point(4, 22);
            this.tabPagePatterns.Name = "tabPagePatterns";
            this.tabPagePatterns.Padding = new System.Windows.Forms.Padding(3);
            this.tabPagePatterns.Size = new System.Drawing.Size(860, 557);
            this.tabPagePatterns.TabIndex = 1;
            this.tabPagePatterns.Text = "Patterns";
            this.tabPagePatterns.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(868, 583);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.tabPageCurve.ResumeLayout(false);
            this.tabPagePatterns.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageCurve;
        private System.Windows.Forms.TabPage tabPagePatterns;
        private CLRPatternsUserControls.ChecklistControl checkListControl;
    }
}

