namespace PatternsApplication
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainerPatientWeb = new System.Windows.Forms.SplitContainer();
            this.patientListCtrl = new PatternsApplication.PatientList();
            this.webPageControlCtrl = new PatternsApplication.WebPageControl();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPatientWeb)).BeginInit();
            this.splitContainerPatientWeb.Panel1.SuspendLayout();
            this.splitContainerPatientWeb.Panel2.SuspendLayout();
            this.splitContainerPatientWeb.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1126, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(39, 20);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // splitContainerPatientWeb
            // 
            this.splitContainerPatientWeb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerPatientWeb.IsSplitterFixed = true;
            this.splitContainerPatientWeb.Location = new System.Drawing.Point(0, 24);
            this.splitContainerPatientWeb.Name = "splitContainerPatientWeb";
            // 
            // splitContainerPatientWeb.Panel1
            // 
            this.splitContainerPatientWeb.Panel1.Controls.Add(this.patientListCtrl);
            this.splitContainerPatientWeb.Panel1MinSize = 0;
            // 
            // splitContainerPatientWeb.Panel2
            // 
            this.splitContainerPatientWeb.Panel2.Controls.Add(this.webPageControlCtrl);
            this.splitContainerPatientWeb.Size = new System.Drawing.Size(1126, 367);
            this.splitContainerPatientWeb.SplitterDistance = 243;
            this.splitContainerPatientWeb.SplitterWidth = 1;
            this.splitContainerPatientWeb.TabIndex = 1;
            // 
            // patientListCtrl
            // 
            this.patientListCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.patientListCtrl.Location = new System.Drawing.Point(0, 0);
            this.patientListCtrl.Name = "patientListCtrl";
            this.patientListCtrl.Size = new System.Drawing.Size(243, 367);
            this.patientListCtrl.TabIndex = 0;
            this.patientListCtrl.CollapseClicked += new System.EventHandler(this.patientListCtrl_CollapseClicked);
            // 
            // webPageControlCtrl
            // 
            this.webPageControlCtrl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webPageControlCtrl.Location = new System.Drawing.Point(0, 0);
            this.webPageControlCtrl.Name = "webPageControlCtrl";
            this.webPageControlCtrl.Size = new System.Drawing.Size(882, 367);
            this.webPageControlCtrl.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1126, 391);
            this.Controls.Add(this.splitContainerPatientWeb);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Patterns";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainerPatientWeb.Panel1.ResumeLayout(false);
            this.splitContainerPatientWeb.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerPatientWeb)).EndInit();
            this.splitContainerPatientWeb.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainerPatientWeb;
        private WebPageControl webPageControlCtrl;
        private PatientList patientListCtrl;
    }
}

