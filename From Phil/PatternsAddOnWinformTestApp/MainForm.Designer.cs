namespace PatternsAddOnWinformTestApp
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.wcfInitCtrl = new PatternsAddOnWinformTestApp.WCFInit();
            this.wcfRunnerCtrl = new PatternsAddOnWinformTestApp.WCFRunnerCtrl();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.buttonNext = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.wcfInitCtrl);
            this.splitContainer1.Panel1.Controls.Add(this.wcfRunnerCtrl);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.panelButtons);
            this.splitContainer1.Size = new System.Drawing.Size(591, 436);
            this.splitContainer1.SplitterDistance = 386;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 0;
            // 
            // wcfInitCtrl
            // 
            this.wcfInitCtrl.Location = new System.Drawing.Point(292, 43);
            this.wcfInitCtrl.MainForm = null;
            this.wcfInitCtrl.Name = "wcfInitCtrl";
            this.wcfInitCtrl.Size = new System.Drawing.Size(588, 309);
            this.wcfInitCtrl.TabIndex = 1;
            this.wcfInitCtrl.TokenID = null;
            // 
            // wcfRunnerCtrl
            // 
            this.wcfRunnerCtrl.Location = new System.Drawing.Point(0, 0);
            this.wcfRunnerCtrl.MainForm = null;
            this.wcfRunnerCtrl.Name = "wcfRunnerCtrl";
            this.wcfRunnerCtrl.ResultsArtifacts = null;
            this.wcfRunnerCtrl.ResultsDAT = ((System.Collections.Generic.List<string>)(resources.GetObject("wcfRunnerCtrl.ResultsDAT")));
            this.wcfRunnerCtrl.ResultsXML = null;
            this.wcfRunnerCtrl.ResultTimerToStop = false;
            this.wcfRunnerCtrl.Size = new System.Drawing.Size(397, 233);
            this.wcfRunnerCtrl.TabIndex = 0;
            this.wcfRunnerCtrl.Load += new System.EventHandler(this.wcfRunnerCtrl_Load);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.buttonNext);
            this.panelButtons.Controls.Add(this.buttonClose);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(0, 0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(591, 49);
            this.panelButtons.TabIndex = 0;
            // 
            // buttonNext
            // 
            this.buttonNext.Location = new System.Drawing.Point(423, 5);
            this.buttonNext.Name = "buttonNext";
            this.buttonNext.Size = new System.Drawing.Size(75, 23);
            this.buttonNext.TabIndex = 0;
            this.buttonNext.Text = "Next";
            this.buttonNext.UseVisualStyleBackColor = true;
            this.buttonNext.Click += new System.EventHandler(this.buttonNext_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(504, 5);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 0;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(591, 436);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.Text = "WCF Tracings Test";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button buttonNext;
        private System.Windows.Forms.Button buttonClose;
        private WCFRunnerCtrl wcfRunnerCtrl;
        private WCFInit wcfInitCtrl;

    }
}

