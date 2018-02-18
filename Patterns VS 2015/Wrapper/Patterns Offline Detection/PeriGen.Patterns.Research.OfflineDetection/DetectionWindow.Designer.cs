namespace PeriGen.Patterns.Research.OfflineDetection
{
	partial class DetectionWindow
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DetectionWindow));
			this.txtFolderSource = new System.Windows.Forms.TextBox();
			this.btnFolder = new System.Windows.Forms.Button();
			this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tsbBatch = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbConvert = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbStop = new System.Windows.Forms.ToolStripButton();
			this.label1 = new System.Windows.Forms.Label();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.tssStatus = new System.Windows.Forms.ToolStripStatusLabel();
			this.tsProgress = new System.Windows.Forms.ToolStripProgressBar();
			this.statusStrip1 = new System.Windows.Forms.StatusStrip();
			this.ofdSelectFiles = new System.Windows.Forms.OpenFileDialog();
			this.cbToSQL = new System.Windows.Forms.CheckBox();
			this.cbToXml = new System.Windows.Forms.CheckBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnTarget = new System.Windows.Forms.Button();
			this.txtFolderTarget = new System.Windows.Forms.TextBox();
			this.folderBrowserTarget = new System.Windows.Forms.FolderBrowserDialog();
			this.chkRecursive = new System.Windows.Forms.CheckBox();
			this.toolStrip1.SuspendLayout();
			this.statusStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// txtFolderSource
			// 
			this.txtFolderSource.Location = new System.Drawing.Point(112, 33);
			this.txtFolderSource.Name = "txtFolderSource";
			this.txtFolderSource.Size = new System.Drawing.Size(454, 20);
			this.txtFolderSource.TabIndex = 2;
			// 
			// btnFolder
			// 
			this.btnFolder.Location = new System.Drawing.Point(575, 32);
			this.btnFolder.Name = "btnFolder";
			this.btnFolder.Size = new System.Drawing.Size(37, 23);
			this.btnFolder.TabIndex = 3;
			this.btnFolder.Text = "...";
			this.btnFolder.UseVisualStyleBackColor = true;
			this.btnFolder.Click += new System.EventHandler(this.OnFolderClick);
			// 
			// folderBrowserDialog
			// 
			this.folderBrowserDialog.Description = "Select the folder containing the V01 files";
			this.folderBrowserDialog.ShowNewFolderButton = false;
			// 
			// toolStrip1
			// 
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbBatch,
            this.toolStripSeparator1,
            this.tsbConvert,
            this.toolStripSeparator2,
            this.tsbStop});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
			this.toolStrip1.Size = new System.Drawing.Size(628, 25);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// tsbBatch
			// 
			this.tsbBatch.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbBatch.Image = ((System.Drawing.Image)(resources.GetObject("tsbBatch.Image")));
			this.tsbBatch.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbBatch.Name = "tsbBatch";
			this.tsbBatch.Size = new System.Drawing.Size(81, 22);
			this.tsbBatch.Text = "Start in batch";
			this.tsbBatch.Click += new System.EventHandler(this.OnBatchModeClick);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbConvert
			// 
			this.tsbConvert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbConvert.Image = ((System.Drawing.Image)(resources.GetObject("tsbConvert.Image")));
			this.tsbConvert.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbConvert.Name = "tsbConvert";
			this.tsbConvert.Size = new System.Drawing.Size(93, 22);
			this.tsbConvert.Text = "Process one file";
			this.tsbConvert.Click += new System.EventHandler(this.OnProcessOneFileClick);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbStop
			// 
			this.tsbStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tsbStop.Enabled = false;
			this.tsbStop.Image = ((System.Drawing.Image)(resources.GetObject("tsbStop.Image")));
			this.tsbStop.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbStop.Name = "tsbStop";
			this.tsbStop.Size = new System.Drawing.Size(35, 22);
			this.tsbStop.Text = "Stop";
			this.tsbStop.Click += new System.EventHandler(this.OnStopClick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 37);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(70, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "Source folder";
			// 
			// tssStatus
			// 
			this.tssStatus.AutoSize = false;
			this.tssStatus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.tssStatus.Name = "tssStatus";
			this.tssStatus.Size = new System.Drawing.Size(613, 17);
			this.tssStatus.Spring = true;
			this.tssStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// tsProgress
			// 
			this.tsProgress.Name = "tsProgress";
			this.tsProgress.Size = new System.Drawing.Size(100, 16);
			this.tsProgress.Visible = false;
			// 
			// statusStrip1
			// 
			this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsProgress,
            this.tssStatus});
			this.statusStrip1.Location = new System.Drawing.Point(0, 115);
			this.statusStrip1.Name = "statusStrip1";
			this.statusStrip1.Size = new System.Drawing.Size(628, 22);
			this.statusStrip1.SizingGrip = false;
			this.statusStrip1.TabIndex = 10;
			this.statusStrip1.Text = "statusStrip1";
			// 
			// ofdSelectFiles
			// 
			this.ofdSelectFiles.Filter = "V01 files|*.v01|XML files|*.xml|IN files|*.in|All files (*.*)|*.*";
			this.ofdSelectFiles.FilterIndex = 4;
			this.ofdSelectFiles.Title = "Select a file";
			// 
			// cbToSQL
			// 
			this.cbToSQL.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cbToSQL.AutoSize = true;
			this.cbToSQL.Checked = true;
			this.cbToSQL.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbToSQL.Location = new System.Drawing.Point(419, 90);
			this.cbToSQL.Name = "cbToSQL";
			this.cbToSQL.Size = new System.Drawing.Size(87, 17);
			this.cbToSQL.TabIndex = 8;
			this.cbToSQL.Text = "Save to SQL";
			this.cbToSQL.UseVisualStyleBackColor = true;
			this.cbToSQL.CheckedChanged += new System.EventHandler(this.cbToSQL_CheckedChanged);
			// 
			// cbToXml
			// 
			this.cbToXml.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cbToXml.AutoSize = true;
			this.cbToXml.Location = new System.Drawing.Point(528, 90);
			this.cbToXml.Name = "cbToXml";
			this.cbToXml.Size = new System.Drawing.Size(88, 17);
			this.cbToXml.TabIndex = 9;
			this.cbToXml.Text = "Save to XML";
			this.cbToXml.UseVisualStyleBackColor = true;
			this.cbToXml.CheckedChanged += new System.EventHandler(this.cbToXml_CheckedChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 63);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Target folder";
			// 
			// btnTarget
			// 
			this.btnTarget.Location = new System.Drawing.Point(575, 58);
			this.btnTarget.Name = "btnTarget";
			this.btnTarget.Size = new System.Drawing.Size(37, 23);
			this.btnTarget.TabIndex = 6;
			this.btnTarget.Text = "...";
			this.btnTarget.UseVisualStyleBackColor = true;
			this.btnTarget.Click += new System.EventHandler(this.btnTarget_Click);
			// 
			// txtFolderTarget
			// 
			this.txtFolderTarget.Location = new System.Drawing.Point(112, 59);
			this.txtFolderTarget.Name = "txtFolderTarget";
			this.txtFolderTarget.Size = new System.Drawing.Size(454, 20);
			this.txtFolderTarget.TabIndex = 5;
			// 
			// folderBrowserTarget
			// 
			this.folderBrowserTarget.Description = "Select the target folder";
			// 
			// chkRecursive
			// 
			this.chkRecursive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkRecursive.AutoSize = true;
			this.chkRecursive.Checked = true;
			this.chkRecursive.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkRecursive.Location = new System.Drawing.Point(295, 90);
			this.chkRecursive.Name = "chkRecursive";
			this.chkRecursive.Size = new System.Drawing.Size(108, 17);
			this.chkRecursive.TabIndex = 7;
			this.chkRecursive.Text = "Recursive folders";
			this.chkRecursive.UseVisualStyleBackColor = true;
			// 
			// DetectionWindow
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(628, 137);
			this.Controls.Add(this.chkRecursive);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnTarget);
			this.Controls.Add(this.txtFolderTarget);
			this.Controls.Add(this.cbToXml);
			this.Controls.Add(this.cbToSQL);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.statusStrip1);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.btnFolder);
			this.Controls.Add(this.txtFolderSource);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "DetectionWindow";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "PeriCALM Pattern\'s offline detection - R&D";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DetectionWindow_FormClosing);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.statusStrip1.ResumeLayout(false);
			this.statusStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtFolderSource;
		private System.Windows.Forms.Button btnFolder;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton tsbBatch;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton tsbStop;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.ToolStripStatusLabel tssStatus;
		private System.Windows.Forms.ToolStripProgressBar tsProgress;
		private System.Windows.Forms.StatusStrip statusStrip1;
		private System.Windows.Forms.ToolStripButton tsbConvert;
		private System.Windows.Forms.OpenFileDialog ofdSelectFiles;
		private System.Windows.Forms.CheckBox cbToSQL;
		private System.Windows.Forms.CheckBox cbToXml;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnTarget;
        private System.Windows.Forms.TextBox txtFolderTarget;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserTarget;
        private System.Windows.Forms.CheckBox chkRecursive;
	}
}

