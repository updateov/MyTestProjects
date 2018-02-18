namespace PeriGen.Patterns.GEInterfaceTest
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
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
			System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.btnRefresh = new System.Windows.Forms.Button();
			this.gridCtl = new System.Windows.Forms.DataGridView();
			this.bindingSourceCtl = new System.Windows.Forms.BindingSource(this.components);
			this.PatientUniqueId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PatientId = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.EpisodeStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PatientLocation = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PatientName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PatientEDD = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.PatientFetus = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.MergeInProgress = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ReconciliationConflict = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.CreationTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.LastMonitored = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.LastUpdated = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.Updated = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			((System.ComponentModel.ISupportInitialize)(this.gridCtl)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.bindingSourceCtl)).BeginInit();
			this.SuspendLayout();
			// 
			// btnRefresh
			// 
			this.btnRefresh.Location = new System.Drawing.Point(12, 12);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(194, 45);
			this.btnRefresh.TabIndex = 0;
			this.btnRefresh.Text = "Refresh (F5)";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// gridCtl
			// 
			this.gridCtl.AllowUserToAddRows = false;
			this.gridCtl.AllowUserToDeleteRows = false;
			this.gridCtl.AllowUserToResizeRows = false;
			this.gridCtl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.gridCtl.AutoGenerateColumns = false;
			this.gridCtl.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.gridCtl.BorderStyle = System.Windows.Forms.BorderStyle.None;
			dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.gridCtl.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
			this.gridCtl.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
			this.gridCtl.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PatientUniqueId,
            this.PatientId,
            this.EpisodeStatus,
            this.PatientLocation,
            this.PatientName,
            this.PatientEDD,
            this.PatientFetus,
            this.MergeInProgress,
            this.ReconciliationConflict,
            this.CreationTime,
            this.LastMonitored,
            this.LastUpdated,
            this.Updated});
			this.gridCtl.DataSource = this.bindingSourceCtl;
			dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle7.BackColor = System.Drawing.SystemColors.Window;
			dataGridViewCellStyle7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle7.ForeColor = System.Drawing.SystemColors.ControlText;
			dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
			this.gridCtl.DefaultCellStyle = dataGridViewCellStyle7;
			this.gridCtl.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
			this.gridCtl.Location = new System.Drawing.Point(12, 63);
			this.gridCtl.MultiSelect = false;
			this.gridCtl.Name = "gridCtl";
			this.gridCtl.ReadOnly = true;
			dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
			dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Control;
			dataGridViewCellStyle8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.WindowText;
			dataGridViewCellStyle8.SelectionBackColor = System.Drawing.SystemColors.Highlight;
			dataGridViewCellStyle8.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
			dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
			this.gridCtl.RowHeadersDefaultCellStyle = dataGridViewCellStyle8;
			this.gridCtl.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
			this.gridCtl.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.gridCtl.Size = new System.Drawing.Size(1160, 431);
			this.gridCtl.TabIndex = 12;
			// 
			// bindingSourceCtl
			// 
			this.bindingSourceCtl.AllowNew = false;
			this.bindingSourceCtl.DataSource = typeof(PeriGen.Patterns.GE.Interface.Episode);
			// 
			// PatientUniqueId
			// 
			this.PatientUniqueId.DataPropertyName = "PatientUniqueId";
			this.PatientUniqueId.FillWeight = 50F;
			this.PatientUniqueId.HeaderText = "ID";
			this.PatientUniqueId.Name = "PatientUniqueId";
			this.PatientUniqueId.ReadOnly = true;
			this.PatientUniqueId.ToolTipText = "Internal unique ID associated to the episode in Patterns";
			// 
			// PatientId
			// 
			this.PatientId.DataPropertyName = "MRN";
			this.PatientId.FillWeight = 200F;
			this.PatientId.HeaderText = "MRN";
			this.PatientId.Name = "PatientId";
			this.PatientId.ReadOnly = true;
			this.PatientId.ToolTipText = "Medical record number";
			// 
			// EpisodeStatus
			// 
			this.EpisodeStatus.DataPropertyName = "EpisodeStatus";
			this.EpisodeStatus.HeaderText = "Status";
			this.EpisodeStatus.Name = "EpisodeStatus";
			this.EpisodeStatus.ReadOnly = true;
			this.EpisodeStatus.ToolTipText = "Status of the episode";
			// 
			// PatientLocation
			// 
			this.PatientLocation.DataPropertyName = "Location";
			this.PatientLocation.FillWeight = 200F;
			this.PatientLocation.HeaderText = "Location";
			this.PatientLocation.Name = "PatientLocation";
			this.PatientLocation.ReadOnly = true;
			this.PatientLocation.ToolTipText = "Location of the patient";
			// 
			// PatientName
			// 
			this.PatientName.DataPropertyName = "PatientName";
			this.PatientName.FillWeight = 300F;
			this.PatientName.HeaderText = "Name";
			this.PatientName.Name = "PatientName";
			this.PatientName.ReadOnly = true;
			this.PatientName.ToolTipText = "Name of the patient";
			// 
			// PatientEDD
			// 
			this.PatientEDD.DataPropertyName = "PatientEDD";
			dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle2.Format = "d";
			dataGridViewCellStyle2.NullValue = null;
			this.PatientEDD.DefaultCellStyle = dataGridViewCellStyle2;
			this.PatientEDD.FillWeight = 130F;
			this.PatientEDD.HeaderText = "EDD";
			this.PatientEDD.Name = "PatientEDD";
			this.PatientEDD.ReadOnly = true;
			this.PatientEDD.ToolTipText = "Estimated date of delivery";
			// 
			// PatientFetus
			// 
			this.PatientFetus.DataPropertyName = "PatientFetus";
			this.PatientFetus.HeaderText = "Fetus";
			this.PatientFetus.Name = "PatientFetus";
			this.PatientFetus.FillWeight = 75F;
			this.PatientFetus.ReadOnly = true;
			this.PatientFetus.ToolTipText = "Number of fetuses for the pregnancy";
			// 
			// MergeInProgress
			// 
			this.MergeInProgress.DataPropertyName = "MergeInProgress";
			this.MergeInProgress.HeaderText = "Merging";
			this.MergeInProgress.Name = "MergeInProgress";
			this.MergeInProgress.ReadOnly = true;
			this.MergeInProgress.ToolTipText = "Is there a merge operation in progress in GE for that episode?";
			// 
			// ReconciliationConflict
			// 
			this.ReconciliationConflict.DataPropertyName = "ReconciliationConflict";
			this.ReconciliationConflict.FillWeight = 150F;
			this.ReconciliationConflict.HeaderText = "Conflict";
			this.ReconciliationConflict.Name = "ReconciliationConflict";
			this.ReconciliationConflict.ReadOnly = true;
			this.ReconciliationConflict.ToolTipText = "List of MRN of episodes that are conflicting with this one for reconciliation";
			// 
			// CreationTime
			// 
			this.CreationTime.DataPropertyName = "Created";
			dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle3.Format = "T";
			this.CreationTime.DefaultCellStyle = dataGridViewCellStyle3;
			this.CreationTime.FillWeight = 150F;
			this.CreationTime.HeaderText = "Created";
			this.CreationTime.Name = "Created";
			this.CreationTime.ReadOnly = true;
			this.CreationTime.ToolTipText = "When the episode was created in Patterns";
			// 
			// LastMonitored
			// 
			this.LastMonitored.DataPropertyName = "LastMonitored";
			dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle4.Format = "T";
			this.LastMonitored.DefaultCellStyle = dataGridViewCellStyle4;
			this.LastMonitored.FillWeight = 150F;
			this.LastMonitored.HeaderText = "Last Monitored";
			this.LastMonitored.Name = "LastMonitored";
			this.LastMonitored.ReadOnly = true;
			this.LastMonitored.ToolTipText = "When the episode was seen in an active bed for the last time";
			// 
			// LastUpdated
			// 
			this.LastUpdated.DataPropertyName = "LastUpdated";
			dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle5.Format = "T";
			this.LastUpdated.DefaultCellStyle = dataGridViewCellStyle5;
			this.LastUpdated.FillWeight = 150F;
			this.LastUpdated.HeaderText = "Last Updated";
			this.LastUpdated.Name = "LastUpdated";
			this.LastUpdated.ReadOnly = true;
			this.LastUpdated.ToolTipText = "When the episode was update with data from GE for the last time";
			// 
			// Updated
			// 
			this.Updated.DataPropertyName = "Updated";
			dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
			dataGridViewCellStyle6.NullValue = "False";
			this.Updated.DefaultCellStyle = dataGridViewCellStyle6;
			this.Updated.FillWeight = 130F;
			this.Updated.HeaderText = "Updated";
			this.Updated.Name = "Updated";
			this.Updated.ReadOnly = true;
			this.Updated.ToolTipText = "Was the episode updated by the last refresh?";
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1184, 510);
			this.Controls.Add(this.gridCtl);
			this.Controls.Add(this.btnRefresh);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MinimumSize = new System.Drawing.Size(725, 400);
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "PeriCALM® Patterns™ - Test GE Synchronization";
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.frmMain_KeyUp);
			((System.ComponentModel.ISupportInitialize)(this.gridCtl)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.bindingSourceCtl)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.BindingSource bindingSourceCtl;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.DataGridView gridCtl;
		private System.Windows.Forms.DataGridViewTextBoxColumn PatientUniqueId;
		private System.Windows.Forms.DataGridViewTextBoxColumn PatientId;
		private System.Windows.Forms.DataGridViewTextBoxColumn EpisodeStatus;
		private System.Windows.Forms.DataGridViewTextBoxColumn PatientLocation;
		private System.Windows.Forms.DataGridViewTextBoxColumn PatientName;
		private System.Windows.Forms.DataGridViewTextBoxColumn PatientEDD;
		private System.Windows.Forms.DataGridViewTextBoxColumn PatientFetus;
		private System.Windows.Forms.DataGridViewCheckBoxColumn MergeInProgress;
		private System.Windows.Forms.DataGridViewTextBoxColumn ReconciliationConflict;
		private System.Windows.Forms.DataGridViewTextBoxColumn CreationTime;
		private System.Windows.Forms.DataGridViewTextBoxColumn LastMonitored;
		private System.Windows.Forms.DataGridViewTextBoxColumn LastUpdated;
		private System.Windows.Forms.DataGridViewCheckBoxColumn Updated;
    }
}

