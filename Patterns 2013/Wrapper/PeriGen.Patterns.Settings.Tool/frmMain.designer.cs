namespace PeriGen.Patterns.Settings.Tool
{
	partial class frmMain
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
			if(disposing && (components != null))
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.grpSettings = new System.Windows.Forms.GroupBox();
			this.btnRemoveLevel = new System.Windows.Forms.Button();
			this.lstLevels = new System.Windows.Forms.ListBox();
			this.btnAddLevel = new System.Windows.Forms.Button();
			this.lstItems = new System.Windows.Forms.ListBox();
			this.lblGrpDetails = new System.Windows.Forms.Label();
			this.grpValues = new System.Windows.Forms.GroupBox();
			this.btnEdit = new System.Windows.Forms.Button();
			this.lblCommentDetails = new System.Windows.Forms.Label();
			this.txtValueDetails = new System.Windows.Forms.TextBox();
			this.lblValueDetails = new System.Windows.Forms.Label();
			this.lblLstSettings = new System.Windows.Forms.Label();
			this.lblLstLevels = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtNewThumbprint = new System.Windows.Forms.TextBox();
			this.lblNewThumbprint = new System.Windows.Forms.Label();
			this.txtThumbprint = new System.Windows.Forms.TextBox();
			this.lblThumbprint = new System.Windows.Forms.Label();
			this.picLogo = new System.Windows.Forms.PictureBox();
			this.toolTipCtl = new System.Windows.Forms.ToolTip(this.components);
			this.mnuCtl = new System.Windows.Forms.MenuStrip();
			this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.toolReset = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
			this.grpSettings.SuspendLayout();
			this.grpValues.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
			this.mnuCtl.SuspendLayout();
			this.toolStripContainer1.ContentPanel.SuspendLayout();
			this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
			this.toolStripContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// grpSettings
			// 
			this.grpSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.grpSettings.Controls.Add(this.btnRemoveLevel);
			this.grpSettings.Controls.Add(this.lstLevels);
			this.grpSettings.Controls.Add(this.btnAddLevel);
			this.grpSettings.Controls.Add(this.lstItems);
			this.grpSettings.Controls.Add(this.lblGrpDetails);
			this.grpSettings.Controls.Add(this.grpValues);
			this.grpSettings.Controls.Add(this.lblLstSettings);
			this.grpSettings.Controls.Add(this.lblLstLevels);
			this.grpSettings.ForeColor = System.Drawing.Color.Red;
			this.grpSettings.Location = new System.Drawing.Point(6, 85);
			this.grpSettings.Name = "grpSettings";
			this.grpSettings.Size = new System.Drawing.Size(951, 240);
			this.grpSettings.TabIndex = 1;
			this.grpSettings.TabStop = false;
			// 
			// btnRemoveLevel
			// 
			this.btnRemoveLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnRemoveLevel.Enabled = false;
			this.btnRemoveLevel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btnRemoveLevel.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveLevel.Image")));
			this.btnRemoveLevel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnRemoveLevel.Location = new System.Drawing.Point(9, 207);
			this.btnRemoveLevel.Name = "btnRemoveLevel";
			this.btnRemoveLevel.Size = new System.Drawing.Size(70, 23);
			this.btnRemoveLevel.TabIndex = 2;
			this.btnRemoveLevel.Tag = "0";
			this.btnRemoveLevel.Text = "Less";
			this.btnRemoveLevel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.toolTipCtl.SetToolTip(this.btnRemoveLevel, "Show less configuration levels");
			this.btnRemoveLevel.UseVisualStyleBackColor = true;
			this.btnRemoveLevel.Click += new System.EventHandler(this.btnRemoveLevel_Click);
			// 
			// lstLevels
			// 
			this.lstLevels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.lstLevels.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lstLevels.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lstLevels.FormattingEnabled = true;
			this.lstLevels.ItemHeight = 14;
			this.lstLevels.Location = new System.Drawing.Point(9, 31);
			this.lstLevels.Name = "lstLevels";
			this.lstLevels.Size = new System.Drawing.Size(141, 172);
			this.lstLevels.TabIndex = 1;
			// 
			// btnAddLevel
			// 
			this.btnAddLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnAddLevel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.btnAddLevel.Image = ((System.Drawing.Image)(resources.GetObject("btnAddLevel.Image")));
			this.btnAddLevel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.btnAddLevel.Location = new System.Drawing.Point(80, 207);
			this.btnAddLevel.Name = "btnAddLevel";
			this.btnAddLevel.Size = new System.Drawing.Size(70, 23);
			this.btnAddLevel.TabIndex = 3;
			this.btnAddLevel.Tag = "0";
			this.btnAddLevel.Text = "More";
			this.btnAddLevel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.toolTipCtl.SetToolTip(this.btnAddLevel, "Show more configuration levels");
			this.btnAddLevel.UseVisualStyleBackColor = true;
			this.btnAddLevel.Click += new System.EventHandler(this.btnAddLevel_Click);
			// 
			// lstItems
			// 
			this.lstItems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lstItems.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lstItems.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lstItems.FormattingEnabled = true;
			this.lstItems.ItemHeight = 14;
			this.lstItems.Location = new System.Drawing.Point(156, 31);
			this.lstItems.Name = "lstItems";
			this.lstItems.Size = new System.Drawing.Size(378, 200);
			this.lstItems.TabIndex = 5;
			this.lstItems.DoubleClick += new System.EventHandler(this.lstItems_DoubleClick);
			// 
			// lblGrpDetails
			// 
			this.lblGrpDetails.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lblGrpDetails.AutoSize = true;
			this.lblGrpDetails.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblGrpDetails.Location = new System.Drawing.Point(537, 15);
			this.lblGrpDetails.Name = "lblGrpDetails";
			this.lblGrpDetails.Size = new System.Drawing.Size(60, 13);
			this.lblGrpDetails.TabIndex = 6;
			this.lblGrpDetails.Text = "Item details";
			// 
			// grpValues
			// 
			this.grpValues.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.grpValues.Controls.Add(this.btnEdit);
			this.grpValues.Controls.Add(this.lblCommentDetails);
			this.grpValues.Controls.Add(this.txtValueDetails);
			this.grpValues.Controls.Add(this.lblValueDetails);
			this.grpValues.Location = new System.Drawing.Point(540, 24);
			this.grpValues.Name = "grpValues";
			this.grpValues.Size = new System.Drawing.Size(400, 208);
			this.grpValues.TabIndex = 7;
			this.grpValues.TabStop = false;
			// 
			// btnEdit
			// 
			this.btnEdit.Image = ((System.Drawing.Image)(resources.GetObject("btnEdit.Image")));
			this.btnEdit.Location = new System.Drawing.Point(360, 31);
			this.btnEdit.Name = "btnEdit";
			this.btnEdit.Size = new System.Drawing.Size(31, 23);
			this.btnEdit.TabIndex = 2;
			this.toolTipCtl.SetToolTip(this.btnEdit, "Edit value");
			this.btnEdit.UseVisualStyleBackColor = true;
			this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
			// 
			// lblCommentDetails
			// 
			this.lblCommentDetails.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCommentDetails.ForeColor = System.Drawing.Color.DarkSlateGray;
			this.lblCommentDetails.Location = new System.Drawing.Point(9, 59);
			this.lblCommentDetails.Margin = new System.Windows.Forms.Padding(0);
			this.lblCommentDetails.Name = "lblCommentDetails";
			this.lblCommentDetails.Size = new System.Drawing.Size(381, 135);
			this.lblCommentDetails.TabIndex = 3;
			this.lblCommentDetails.Text = "Comment";
			// 
			// txtValueDetails
			// 
			this.txtValueDetails.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtValueDetails.ForeColor = System.Drawing.SystemColors.ControlText;
			this.txtValueDetails.Location = new System.Drawing.Point(9, 32);
			this.txtValueDetails.Name = "txtValueDetails";
			this.txtValueDetails.ReadOnly = true;
			this.txtValueDetails.Size = new System.Drawing.Size(352, 22);
			this.txtValueDetails.TabIndex = 1;
			// 
			// lblValueDetails
			// 
			this.lblValueDetails.AutoSize = true;
			this.lblValueDetails.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblValueDetails.Location = new System.Drawing.Point(6, 16);
			this.lblValueDetails.Name = "lblValueDetails";
			this.lblValueDetails.Size = new System.Drawing.Size(34, 13);
			this.lblValueDetails.TabIndex = 0;
			this.lblValueDetails.Text = "Value";
			// 
			// lblLstSettings
			// 
			this.lblLstSettings.AutoSize = true;
			this.lblLstSettings.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblLstSettings.Location = new System.Drawing.Point(153, 15);
			this.lblLstSettings.Name = "lblLstSettings";
			this.lblLstSettings.Size = new System.Drawing.Size(72, 13);
			this.lblLstSettings.TabIndex = 4;
			this.lblLstSettings.Text = "Settings items";
			// 
			// lblLstLevels
			// 
			this.lblLstLevels.AutoSize = true;
			this.lblLstLevels.ForeColor = System.Drawing.SystemColors.ControlText;
			this.lblLstLevels.Location = new System.Drawing.Point(7, 15);
			this.lblLstLevels.Name = "lblLstLevels";
			this.lblLstLevels.Size = new System.Drawing.Size(79, 13);
			this.lblLstLevels.TabIndex = 0;
			this.lblLstLevels.Text = "Settings Levels";
			// 
			// groupBox2
			// 
			this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox2.Controls.Add(this.txtNewThumbprint);
			this.groupBox2.Controls.Add(this.lblNewThumbprint);
			this.groupBox2.Controls.Add(this.txtThumbprint);
			this.groupBox2.Controls.Add(this.lblThumbprint);
			this.groupBox2.Location = new System.Drawing.Point(6, 3);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(782, 79);
			this.groupBox2.TabIndex = 0;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Certificate";
			// 
			// txtNewThumbprint
			// 
			this.txtNewThumbprint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtNewThumbprint.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtNewThumbprint.Location = new System.Drawing.Point(116, 50);
			this.txtNewThumbprint.Name = "txtNewThumbprint";
			this.txtNewThumbprint.Size = new System.Drawing.Size(650, 22);
			this.txtNewThumbprint.TabIndex = 3;
			// 
			// lblNewThumbprint
			// 
			this.lblNewThumbprint.AutoSize = true;
			this.lblNewThumbprint.Location = new System.Drawing.Point(10, 53);
			this.lblNewThumbprint.Name = "lblNewThumbprint";
			this.lblNewThumbprint.Size = new System.Drawing.Size(85, 13);
			this.lblNewThumbprint.TabIndex = 2;
			this.lblNewThumbprint.Text = "New Thumbprint";
			// 
			// txtThumbprint
			// 
			this.txtThumbprint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtThumbprint.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtThumbprint.Location = new System.Drawing.Point(116, 19);
			this.txtThumbprint.Name = "txtThumbprint";
			this.txtThumbprint.ReadOnly = true;
			this.txtThumbprint.Size = new System.Drawing.Size(650, 22);
			this.txtThumbprint.TabIndex = 1;
			// 
			// lblThumbprint
			// 
			this.lblThumbprint.AutoSize = true;
			this.lblThumbprint.Location = new System.Drawing.Point(10, 22);
			this.lblThumbprint.Name = "lblThumbprint";
			this.lblThumbprint.Size = new System.Drawing.Size(97, 13);
			this.lblThumbprint.TabIndex = 0;
			this.lblThumbprint.Text = "Current Thumbprint";
			// 
			// picLogo
			// 
			this.picLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.picLogo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.picLogo.Image = ((System.Drawing.Image)(resources.GetObject("picLogo.Image")));
			this.picLogo.Location = new System.Drawing.Point(792, 9);
			this.picLogo.Name = "picLogo";
			this.picLogo.Size = new System.Drawing.Size(165, 72);
			this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.picLogo.TabIndex = 6;
			this.picLogo.TabStop = false;
			// 
			// mnuCtl
			// 
			this.mnuCtl.BackColor = System.Drawing.Color.SlateGray;
			this.mnuCtl.Dock = System.Windows.Forms.DockStyle.None;
			this.mnuCtl.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
			this.mnuCtl.Location = new System.Drawing.Point(0, 0);
			this.mnuCtl.Name = "mnuCtl";
			this.mnuCtl.Size = new System.Drawing.Size(966, 24);
			this.mnuCtl.TabIndex = 7;
			this.mnuCtl.Text = "menuStrip1";
			// 
			// fileToolStripMenuItem
			// 
			this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.toolStripSeparator2,
            this.toolReset,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
			this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
			this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.fileToolStripMenuItem.Text = "File";
			// 
			// openToolStripMenuItem
			// 
			this.openToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("openToolStripMenuItem.Image")));
			this.openToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.openToolStripMenuItem.Name = "openToolStripMenuItem";
			this.openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.openToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
			this.openToolStripMenuItem.Text = "Open ...";
			this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
			// 
			// saveToolStripMenuItem
			// 
			this.saveToolStripMenuItem.Enabled = false;
			this.saveToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveToolStripMenuItem.Image")));
			this.saveToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.Fuchsia;
			this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
			this.saveToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.saveToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
			this.saveToolStripMenuItem.Text = "Save";
			this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(155, 6);
			// 
			// toolReset
			// 
			this.toolReset.Enabled = false;
			this.toolReset.Image = ((System.Drawing.Image)(resources.GetObject("toolReset.Image")));
			this.toolReset.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolReset.Name = "toolReset";
			this.toolReset.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
			this.toolReset.Size = new System.Drawing.Size(158, 22);
			this.toolReset.Text = "Reset";
			this.toolReset.Click += new System.EventHandler(this.toolReset_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(155, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// toolStripContainer1
			// 
			// 
			// toolStripContainer1.ContentPanel
			// 
			this.toolStripContainer1.ContentPanel.AutoScroll = true;
			this.toolStripContainer1.ContentPanel.Controls.Add(this.picLogo);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.groupBox2);
			this.toolStripContainer1.ContentPanel.Controls.Add(this.grpSettings);
			this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(966, 332);
			this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
			this.toolStripContainer1.Name = "toolStripContainer1";
			this.toolStripContainer1.Size = new System.Drawing.Size(966, 356);
			this.toolStripContainer1.TabIndex = 9;
			this.toolStripContainer1.Text = "toolStripContainer1";
			// 
			// toolStripContainer1.TopToolStripPanel
			// 
			this.toolStripContainer1.TopToolStripPanel.BackColor = System.Drawing.Color.SlateGray;
			this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mnuCtl);
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.AliceBlue;
			this.ClientSize = new System.Drawing.Size(966, 356);
			this.Controls.Add(this.toolStripContainer1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimumSize = new System.Drawing.Size(972, 384);
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "PeriCALM® Patterns™ Settings Editor";
			this.grpSettings.ResumeLayout(false);
			this.grpSettings.PerformLayout();
			this.grpValues.ResumeLayout(false);
			this.grpValues.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
			this.mnuCtl.ResumeLayout(false);
			this.mnuCtl.PerformLayout();
			this.toolStripContainer1.ContentPanel.ResumeLayout(false);
			this.toolStripContainer1.ContentPanel.PerformLayout();
			this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
			this.toolStripContainer1.TopToolStripPanel.PerformLayout();
			this.toolStripContainer1.ResumeLayout(false);
			this.toolStripContainer1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox grpSettings;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.TextBox txtNewThumbprint;
		private System.Windows.Forms.Label lblNewThumbprint;
		private System.Windows.Forms.TextBox txtThumbprint;
		private System.Windows.Forms.Label lblThumbprint;
		private System.Windows.Forms.PictureBox picLogo;
		private System.Windows.Forms.Button btnEdit;
		private System.Windows.Forms.ToolTip toolTipCtl;
		private System.Windows.Forms.ListBox lstLevels;
		private System.Windows.Forms.ListBox lstItems;
		private System.Windows.Forms.GroupBox grpValues;
		private System.Windows.Forms.Label lblLstLevels;
		private System.Windows.Forms.Label lblLstSettings;
		private System.Windows.Forms.Label lblGrpDetails;
		private System.Windows.Forms.Label lblValueDetails;
		private System.Windows.Forms.TextBox txtValueDetails;
		private System.Windows.Forms.Label lblCommentDetails;
		private System.Windows.Forms.Button btnAddLevel;
		private System.Windows.Forms.Button btnRemoveLevel;
		private System.Windows.Forms.MenuStrip mnuCtl;
		private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripContainer toolStripContainer1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem toolReset;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
	}
}

