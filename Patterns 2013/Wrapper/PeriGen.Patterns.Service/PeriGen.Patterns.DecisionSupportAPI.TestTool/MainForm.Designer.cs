namespace PeriGen.Patterns.DecisionSupportAPI.TestTool
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
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.panelSettings = new System.Windows.Forms.Panel();
			this.cboMinimumUP = new System.Windows.Forms.ComboBox();
			this.cboDuration = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.txtURL = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.panel2 = new System.Windows.Forms.Panel();
			this.chkShowOnlyUpdated = new System.Windows.Forms.CheckBox();
			this.numDelay = new System.Windows.Forms.NumericUpDown();
			this.btnAuto = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			this.btnQuery = new System.Windows.Forms.Button();
			this.dataGridView = new System.Windows.Forms.DataGridView();
			this.Time = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Count = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Updated = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.AutomaticQueryTimer = new System.Windows.Forms.Timer(this.components);
			this.txtVisitKey = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.panelSettings.SuspendLayout();
			this.panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numDelay)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// panelSettings
			// 
			this.panelSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panelSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panelSettings.Controls.Add(this.txtVisitKey);
			this.panelSettings.Controls.Add(this.label2);
			this.panelSettings.Controls.Add(this.cboMinimumUP);
			this.panelSettings.Controls.Add(this.cboDuration);
			this.panelSettings.Controls.Add(this.label4);
			this.panelSettings.Controls.Add(this.label3);
			this.panelSettings.Controls.Add(this.txtURL);
			this.panelSettings.Controls.Add(this.label1);
			this.panelSettings.Location = new System.Drawing.Point(7, 12);
			this.panelSettings.Name = "panelSettings";
			this.panelSettings.Size = new System.Drawing.Size(710, 109);
			this.panelSettings.TabIndex = 0;
			// 
			// cboMinimumUP
			// 
			this.cboMinimumUP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboMinimumUP.FormattingEnabled = true;
			this.cboMinimumUP.Items.AddRange(new object[] {
            "0%",
            "10%",
            "20%",
            "30%",
            "40%",
            "50%",
            "60%",
            "70%",
            "80%",
            "90%",
            "100%"});
			this.cboMinimumUP.Location = new System.Drawing.Point(359, 44);
			this.cboMinimumUP.MaxDropDownItems = 20;
			this.cboMinimumUP.Name = "cboMinimumUP";
			this.cboMinimumUP.Size = new System.Drawing.Size(58, 21);
			this.cboMinimumUP.TabIndex = 5;
			// 
			// cboDuration
			// 
			this.cboDuration.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDuration.FormattingEnabled = true;
			this.cboDuration.Items.AddRange(new object[] {
            "5 minutes",
            "10 minutes",
            "12 minutes",
            "15 minutes",
            "20 minutes",
            "30 minutes"});
			this.cboDuration.Location = new System.Drawing.Point(98, 44);
			this.cboDuration.MaxDropDownItems = 20;
			this.cboDuration.Name = "cboDuration";
			this.cboDuration.Size = new System.Drawing.Size(95, 21);
			this.cboDuration.TabIndex = 3;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(233, 48);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(106, 13);
			this.label4.TabIndex = 4;
			this.label4.Text = "Minimim UP required:";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 48);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(78, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Block duration:";
			// 
			// txtURL
			// 
			this.txtURL.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtURL.Location = new System.Drawing.Point(98, 11);
			this.txtURL.Name = "txtURL";
			this.txtURL.Size = new System.Drawing.Size(604, 20);
			this.txtURL.TabIndex = 1;
			this.txtURL.Text = "http://localhost:7802/PatternsDataFeed/decisionSupport";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(76, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Pattern\'s URL:";
			// 
			// panel2
			// 
			this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panel2.BackColor = System.Drawing.SystemColors.Control;
			this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel2.Controls.Add(this.chkShowOnlyUpdated);
			this.panel2.Controls.Add(this.numDelay);
			this.panel2.Controls.Add(this.btnAuto);
			this.panel2.Controls.Add(this.btnReset);
			this.panel2.Controls.Add(this.btnQuery);
			this.panel2.Controls.Add(this.dataGridView);
			this.panel2.Location = new System.Drawing.Point(7, 127);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(710, 341);
			this.panel2.TabIndex = 1;
			// 
			// chkShowOnlyUpdated
			// 
			this.chkShowOnlyUpdated.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkShowOnlyUpdated.AutoSize = true;
			this.chkShowOnlyUpdated.Checked = true;
			this.chkShowOnlyUpdated.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkShowOnlyUpdated.Location = new System.Drawing.Point(551, 316);
			this.chkShowOnlyUpdated.Name = "chkShowOnlyUpdated";
			this.chkShowOnlyUpdated.Size = new System.Drawing.Size(151, 17);
			this.chkShowOnlyUpdated.TabIndex = 5;
			this.chkShowOnlyUpdated.Text = "Show only updated entries";
			this.chkShowOnlyUpdated.UseVisualStyleBackColor = true;
			this.chkShowOnlyUpdated.CheckedChanged += new System.EventHandler(this.chkShowOnlyUpdated_CheckedChanged);
			// 
			// numDelay
			// 
			this.numDelay.Location = new System.Drawing.Point(282, 9);
			this.numDelay.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
			this.numDelay.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numDelay.Name = "numDelay";
			this.numDelay.Size = new System.Drawing.Size(47, 20);
			this.numDelay.TabIndex = 2;
			this.numDelay.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// btnAuto
			// 
			this.btnAuto.Location = new System.Drawing.Point(144, 8);
			this.btnAuto.Name = "btnAuto";
			this.btnAuto.Size = new System.Drawing.Size(132, 23);
			this.btnAuto.TabIndex = 1;
			this.btnAuto.Text = "Start auto-query";
			this.btnAuto.UseVisualStyleBackColor = true;
			this.btnAuto.Click += new System.EventHandler(this.btnAuto_Click);
			// 
			// btnReset
			// 
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnReset.Location = new System.Drawing.Point(570, 8);
			this.btnReset.Name = "btnReset";
			this.btnReset.Size = new System.Drawing.Size(132, 23);
			this.btnReset.TabIndex = 3;
			this.btnReset.Text = "Reset";
			this.btnReset.UseVisualStyleBackColor = true;
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// btnQuery
			// 
			this.btnQuery.Location = new System.Drawing.Point(6, 8);
			this.btnQuery.Name = "btnQuery";
			this.btnQuery.Size = new System.Drawing.Size(132, 23);
			this.btnQuery.TabIndex = 0;
			this.btnQuery.Text = "Query";
			this.btnQuery.UseVisualStyleBackColor = true;
			this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
			// 
			// dataGridView
			// 
			this.dataGridView.AllowUserToAddRows = false;
			this.dataGridView.AllowUserToDeleteRows = false;
			this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Time,
            this.Count,
            this.Updated});
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.dataGridView.DefaultCellStyle = dataGridViewCellStyle2;
			this.dataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.dataGridView.Location = new System.Drawing.Point(6, 37);
			this.dataGridView.Name = "dataGridView";
			this.dataGridView.ReadOnly = true;
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.dataGridView.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
			this.dataGridView.RowHeadersVisible = false;
			this.dataGridView.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.dataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dataGridView.ShowEditingIcon = false;
			this.dataGridView.Size = new System.Drawing.Size(696, 270);
			this.dataGridView.TabIndex = 4;
			// 
			// Time
			// 
			this.Time.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Time.DataPropertyName = "TimeLocal";
			this.Time.FillWeight = 300F;
			this.Time.HeaderText = "Time";
			this.Time.Name = "Time";
			this.Time.ReadOnly = true;
			// 
			// Count
			// 
			this.Count.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.Count.DataPropertyName = "Count";
			this.Count.HeaderText = "Count";
			this.Count.Name = "Count";
			this.Count.ReadOnly = true;
			// 
			// Updated
			// 
			this.Updated.DataPropertyName = "Updated";
			this.Updated.HeaderText = "Updated";
			this.Updated.Name = "Updated";
			this.Updated.ReadOnly = true;
			this.Updated.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
			// 
			// AutomaticQueryTimer
			// 
			this.AutomaticQueryTimer.Interval = 1000;
			this.AutomaticQueryTimer.Tick += new System.EventHandler(this.AutomaticQueryTimer_Tick);
			// 
			// txtVisitKey
			// 
			this.txtVisitKey.Location = new System.Drawing.Point(98, 77);
			this.txtVisitKey.Name = "txtVisitKey";
			this.txtVisitKey.Size = new System.Drawing.Size(100, 20);
			this.txtVisitKey.TabIndex = 7;
			this.txtVisitKey.Text = "x-x-x-x";
			this.txtVisitKey.Leave += new System.EventHandler(this.txtVisitKey_Leave);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 81);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(49, 13);
			this.label2.TabIndex = 6;
			this.label2.Text = "Visit key:";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(729, 480);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panelSettings);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(600, 400);
			this.Name = "MainForm";
			this.Text = "PeriCALM® Patterns™ Decision Support API Test Tool";
			this.panelSettings.ResumeLayout(false);
			this.panelSettings.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numDelay)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelSettings;
		private System.Windows.Forms.TextBox txtURL;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.DataGridView dataGridView;
		private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Timer AutomaticQueryTimer;
		private System.Windows.Forms.CheckBox chkShowOnlyUpdated;
		private System.Windows.Forms.DataGridViewTextBoxColumn Time;
		private System.Windows.Forms.DataGridViewTextBoxColumn Count;
		private System.Windows.Forms.DataGridViewCheckBoxColumn Updated;
		private System.Windows.Forms.ComboBox cboMinimumUP;
		private System.Windows.Forms.ComboBox cboDuration;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown numDelay;
		private System.Windows.Forms.Button btnAuto;
		private System.Windows.Forms.Button btnQuery;
		private System.Windows.Forms.TextBox txtVisitKey;
		private System.Windows.Forms.Label label2;
	}
}

