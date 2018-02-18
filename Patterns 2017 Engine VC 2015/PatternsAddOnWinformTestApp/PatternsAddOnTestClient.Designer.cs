namespace PatternsAddOnWinformTestApp
{
    partial class PatternsAddOnTestClient
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PatternsAddOnTestClient));
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageService = new System.Windows.Forms.TabPage();
            this.tabPageRunner = new System.Windows.Forms.TabPage();
            this.tabPageRequest = new System.Windows.Forms.TabPage();
            this.tabPageGetResults = new System.Windows.Forms.TabPage();
            this.buttonClose = new System.Windows.Forms.Button();
            this.initServerCtrl = new PatternsAddOnWinformTestApp.InitServerCtrl();
            this.patternsRunnerCtrl = new PatternsAddOnWinformTestApp.PatternsRunnerCtrl();
            this.requestCtrl1 = new PatternsAddOnWinformTestApp.RequestCtrl();
            this.getResultsCtrl1 = new PatternsAddOnWinformTestApp.GetResultsCtrl();
            this.tabControl.SuspendLayout();
            this.tabPageService.SuspendLayout();
            this.tabPageRunner.SuspendLayout();
            this.tabPageRequest.SuspendLayout();
            this.tabPageGetResults.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageService);
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(670, 428);
            this.tabControl.TabIndex = 0;
            // 
            // tabPageService
            // 
            this.tabPageService.Controls.Add(this.initServerCtrl);
            this.tabPageService.Location = new System.Drawing.Point(4, 22);
            this.tabPageService.Name = "tabPageService";
            this.tabPageService.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageService.Size = new System.Drawing.Size(662, 402);
            this.tabPageService.TabIndex = 0;
            this.tabPageService.Text = "Service Operations";
            this.tabPageService.UseVisualStyleBackColor = true;
            // 
            // tabPageRunner
            // 
            this.tabPageRunner.Controls.Add(this.patternsRunnerCtrl);
            this.tabPageRunner.Location = new System.Drawing.Point(4, 22);
            this.tabPageRunner.Name = "tabPageRunner";
            this.tabPageRunner.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRunner.Size = new System.Drawing.Size(662, 402);
            this.tabPageRunner.TabIndex = 1;
            this.tabPageRunner.Text = "Patterns Runner";
            this.tabPageRunner.UseVisualStyleBackColor = true;
            // 
            // tabPageRequest
            // 
            this.tabPageRequest.Controls.Add(this.requestCtrl1);
            this.tabPageRequest.Location = new System.Drawing.Point(4, 22);
            this.tabPageRequest.Name = "tabPageRequest";
            this.tabPageRequest.Size = new System.Drawing.Size(662, 402);
            this.tabPageRequest.TabIndex = 3;
            this.tabPageRequest.Text = "Send Request";
            this.tabPageRequest.UseVisualStyleBackColor = true;
            // 
            // tabPageGetResults
            // 
            this.tabPageGetResults.Controls.Add(this.getResultsCtrl1);
            this.tabPageGetResults.Location = new System.Drawing.Point(4, 22);
            this.tabPageGetResults.Name = "tabPageGetResults";
            this.tabPageGetResults.Size = new System.Drawing.Size(662, 402);
            this.tabPageGetResults.TabIndex = 2;
            this.tabPageGetResults.Text = "Get Results";
            this.tabPageGetResults.UseVisualStyleBackColor = true;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.Location = new System.Drawing.Point(588, 434);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 1;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // initServerCtrl
            // 
            this.initServerCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.initServerCtrl.Location = new System.Drawing.Point(3, 3);
            this.initServerCtrl.Name = "initServerCtrl";
            this.initServerCtrl.Size = new System.Drawing.Size(656, 396);
            this.initServerCtrl.TabIndex = 0;
            this.initServerCtrl.TokenID = null;
            // 
            // patternsRunnerCtrl
            // 
            this.patternsRunnerCtrl.AbsoluteStart = new System.DateTime(((long)(0)));
            this.patternsRunnerCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patternsRunnerCtrl.Location = new System.Drawing.Point(3, 3);
            this.patternsRunnerCtrl.Name = "patternsRunnerCtrl";
            this.patternsRunnerCtrl.ResultsArtifacts = null;
            this.patternsRunnerCtrl.ResultsDAT = ((System.Collections.Generic.List<string>)(resources.GetObject("patternsRunnerCtrl.ResultsDAT")));
            this.patternsRunnerCtrl.ResultsXML = null;
            this.patternsRunnerCtrl.ResultTimerToStop = false;
            this.patternsRunnerCtrl.Size = new System.Drawing.Size(656, 396);
            this.patternsRunnerCtrl.TabIndex = 0;
            // 
            // requestCtrl1
            // 
            this.requestCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.requestCtrl1.Location = new System.Drawing.Point(0, 0);
            this.requestCtrl1.Name = "requestCtrl1";
            this.requestCtrl1.Size = new System.Drawing.Size(662, 402);
            this.requestCtrl1.TabIndex = 0;
            // 
            // getResultsCtrl1
            // 
            this.getResultsCtrl1.AbsoluteStart = new System.DateTime(((long)(0)));
            this.getResultsCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.getResultsCtrl1.Location = new System.Drawing.Point(0, 0);
            this.getResultsCtrl1.Name = "getResultsCtrl1";
            this.getResultsCtrl1.ResultsArtifacts = null;
            this.getResultsCtrl1.ResultsDAT = ((System.Collections.Generic.List<string>)(resources.GetObject("getResultsCtrl1.ResultsDAT")));
            this.getResultsCtrl1.ResultsXML = null;
            this.getResultsCtrl1.Size = new System.Drawing.Size(662, 402);
            this.getResultsCtrl1.TabIndex = 0;
            // 
            // PatternsAddOnTestClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 469);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.tabControl);
            this.Name = "PatternsAddOnTestClient";
            this.Text = "Patterns Add On Test Client";
            this.tabControl.ResumeLayout(false);
            this.tabPageService.ResumeLayout(false);
            this.tabPageRunner.ResumeLayout(false);
            this.tabPageRequest.ResumeLayout(false);
            this.tabPageGetResults.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        public System.Windows.Forms.TabPage tabPageService;
        public System.Windows.Forms.TabPage tabPageRunner;
        private System.Windows.Forms.Button buttonClose;
        private InitServerCtrl initServerCtrl;
        private PatternsRunnerCtrl patternsRunnerCtrl;
        private System.Windows.Forms.TabPage tabPageGetResults;
        private GetResultsCtrl getResultsCtrl1;
        private System.Windows.Forms.TabPage tabPageRequest;
        private RequestCtrl requestCtrl1;
    }
}