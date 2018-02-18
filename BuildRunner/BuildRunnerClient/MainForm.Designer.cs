namespace BuildRunnerClient
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
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxAvailableBuilds = new System.Windows.Forms.ComboBox();
            this.dataGridViewAwaitingBuilds = new System.Windows.Forms.DataGridView();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonBuild = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.buttonClose = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dataGridViewCompleted = new System.Windows.Forms.DataGridView();
            this.comboBoxHost = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAwaitingBuilds)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCompleted)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select Build:";
            // 
            // comboBoxAvailableBuilds
            // 
            this.comboBoxAvailableBuilds.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxAvailableBuilds.Enabled = false;
            this.comboBoxAvailableBuilds.FormattingEnabled = true;
            this.comboBoxAvailableBuilds.Location = new System.Drawing.Point(77, 41);
            this.comboBoxAvailableBuilds.Name = "comboBoxAvailableBuilds";
            this.comboBoxAvailableBuilds.Size = new System.Drawing.Size(238, 21);
            this.comboBoxAvailableBuilds.TabIndex = 1;
            this.comboBoxAvailableBuilds.SelectedIndexChanged += new System.EventHandler(this.comboBoxAvailableBuilds_SelectedIndexChanged);
            // 
            // dataGridViewAwaitingBuilds
            // 
            this.dataGridViewAwaitingBuilds.AllowUserToAddRows = false;
            this.dataGridViewAwaitingBuilds.AllowUserToDeleteRows = false;
            this.dataGridViewAwaitingBuilds.AllowUserToResizeRows = false;
            this.dataGridViewAwaitingBuilds.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewAwaitingBuilds.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewAwaitingBuilds.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAwaitingBuilds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewAwaitingBuilds.Location = new System.Drawing.Point(0, 0);
            this.dataGridViewAwaitingBuilds.Name = "dataGridViewAwaitingBuilds";
            this.dataGridViewAwaitingBuilds.RowHeadersVisible = false;
            this.dataGridViewAwaitingBuilds.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewAwaitingBuilds.Size = new System.Drawing.Size(402, 206);
            this.dataGridViewAwaitingBuilds.TabIndex = 2;
            this.dataGridViewAwaitingBuilds.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewAwaitingBuilds_CellDoubleClick);
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Location = new System.Drawing.Point(7, 529);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(75, 23);
            this.buttonRefresh.TabIndex = 4;
            this.buttonRefresh.Text = "Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.comboBoxHost);
            this.panel1.Controls.Add(this.buttonConnect);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.buttonBuild);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.comboBoxAvailableBuilds);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 16);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(402, 66);
            this.panel1.TabIndex = 0;
            // 
            // buttonConnect
            // 
            this.buttonConnect.Enabled = false;
            this.buttonConnect.Location = new System.Drawing.Point(321, 6);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(75, 23);
            this.buttonConnect.TabIndex = 5;
            this.buttonConnect.Text = "Connect";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Host:";
            // 
            // buttonBuild
            // 
            this.buttonBuild.Enabled = false;
            this.buttonBuild.Location = new System.Drawing.Point(321, 40);
            this.buttonBuild.Name = "buttonBuild";
            this.buttonBuild.Size = new System.Drawing.Size(75, 23);
            this.buttonBuild.TabIndex = 2;
            this.buttonBuild.Text = "Build";
            this.buttonBuild.UseVisualStyleBackColor = true;
            this.buttonBuild.Click += new System.EventHandler(this.buttonBuild_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.dataGridViewAwaitingBuilds);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 16);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(402, 206);
            this.panel2.TabIndex = 0;
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(337, 529);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 6;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.panel2);
            this.groupBox1.Location = new System.Drawing.Point(4, 92);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(408, 225);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Awaiting Build Requests";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel1);
            this.groupBox2.Location = new System.Drawing.Point(4, 1);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(408, 85);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dataGridViewCompleted);
            this.groupBox3.Location = new System.Drawing.Point(4, 323);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(408, 200);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Completed Builds";
            // 
            // dataGridViewCompleted
            // 
            this.dataGridViewCompleted.AllowUserToAddRows = false;
            this.dataGridViewCompleted.AllowUserToDeleteRows = false;
            this.dataGridViewCompleted.AllowUserToResizeRows = false;
            this.dataGridViewCompleted.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewCompleted.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dataGridViewCompleted.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewCompleted.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridViewCompleted.Location = new System.Drawing.Point(3, 16);
            this.dataGridViewCompleted.MultiSelect = false;
            this.dataGridViewCompleted.Name = "dataGridViewCompleted";
            this.dataGridViewCompleted.ReadOnly = true;
            this.dataGridViewCompleted.RowHeadersVisible = false;
            this.dataGridViewCompleted.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewCompleted.Size = new System.Drawing.Size(402, 181);
            this.dataGridViewCompleted.TabIndex = 0;
            this.dataGridViewCompleted.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewCompleted_CellDoubleClick);
            // 
            // comboBoxHost
            // 
            this.comboBoxHost.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxHost.FormattingEnabled = true;
            this.comboBoxHost.Location = new System.Drawing.Point(77, 8);
            this.comboBoxHost.Name = "comboBoxHost";
            this.comboBoxHost.Size = new System.Drawing.Size(238, 21);
            this.comboBoxHost.TabIndex = 6;
            this.comboBoxHost.SelectedIndexChanged += new System.EventHandler(this.comboBoxHost_SelectedIndexChanged);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(415, 564);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.buttonClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "Build Agent";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAwaitingBuilds)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewCompleted)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxAvailableBuilds;
        private System.Windows.Forms.DataGridView dataGridViewAwaitingBuilds;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button buttonBuild;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView dataGridViewCompleted;
        private System.Windows.Forms.ComboBox comboBoxHost;
    }
}

