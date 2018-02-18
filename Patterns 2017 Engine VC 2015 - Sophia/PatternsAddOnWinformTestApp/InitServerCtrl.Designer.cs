namespace PatternsAddOnWinformTestApp
{
    partial class InitServerCtrl
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
            this.groupBoxServiceInit = new System.Windows.Forms.GroupBox();
            this.radioButtonFromFile = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.radioButtonManual = new System.Windows.Forms.RadioButton();
            this.textBoxGA = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBoxRESTPath = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonInit = new System.Windows.Forms.Button();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.textBoxHost = new System.Windows.Forms.TextBox();
            this.groupBoxInitResponse = new System.Windows.Forms.GroupBox();
            this.textBoxGARet = new System.Windows.Forms.TextBox();
            this.textBoxTokenId = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBoxServiceStatus = new System.Windows.Forms.GroupBox();
            this.buttonGetStatus = new System.Windows.Forms.Button();
            this.listBoxStatus = new System.Windows.Forms.ListBox();
            this.groupBoxLiveSessions = new System.Windows.Forms.GroupBox();
            this.listViewSessions = new System.Windows.Forms.ListView();
            this.columnHeaderSessionID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderGA = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonGetSession = new System.Windows.Forms.Button();
            this.buttonAbout = new System.Windows.Forms.Button();
            this.groupBoxServiceInit.SuspendLayout();
            this.groupBoxInitResponse.SuspendLayout();
            this.groupBoxServiceStatus.SuspendLayout();
            this.groupBoxLiveSessions.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxServiceInit
            // 
            this.groupBoxServiceInit.Controls.Add(this.radioButtonFromFile);
            this.groupBoxServiceInit.Controls.Add(this.label7);
            this.groupBoxServiceInit.Controls.Add(this.radioButtonManual);
            this.groupBoxServiceInit.Controls.Add(this.textBoxGA);
            this.groupBoxServiceInit.Controls.Add(this.label6);
            this.groupBoxServiceInit.Controls.Add(this.comboBoxRESTPath);
            this.groupBoxServiceInit.Controls.Add(this.label4);
            this.groupBoxServiceInit.Controls.Add(this.label3);
            this.groupBoxServiceInit.Controls.Add(this.label1);
            this.groupBoxServiceInit.Controls.Add(this.buttonInit);
            this.groupBoxServiceInit.Controls.Add(this.textBoxPort);
            this.groupBoxServiceInit.Controls.Add(this.textBoxHost);
            this.groupBoxServiceInit.Location = new System.Drawing.Point(3, 3);
            this.groupBoxServiceInit.Name = "groupBoxServiceInit";
            this.groupBoxServiceInit.Size = new System.Drawing.Size(314, 136);
            this.groupBoxServiceInit.TabIndex = 1;
            this.groupBoxServiceInit.TabStop = false;
            this.groupBoxServiceInit.Text = "Service Initialize";
            // 
            // radioButtonFromFile
            // 
            this.radioButtonFromFile.AutoSize = true;
            this.radioButtonFromFile.Checked = true;
            this.radioButtonFromFile.Location = new System.Drawing.Point(50, 78);
            this.radioButtonFromFile.Name = "radioButtonFromFile";
            this.radioButtonFromFile.Size = new System.Drawing.Size(67, 17);
            this.radioButtonFromFile.TabIndex = 9;
            this.radioButtonFromFile.TabStop = true;
            this.radioButtonFromFile.Text = "From File";
            this.radioButtonFromFile.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(37, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Mode:";
            // 
            // radioButtonManual
            // 
            this.radioButtonManual.AutoSize = true;
            this.radioButtonManual.Location = new System.Drawing.Point(123, 78);
            this.radioButtonManual.Name = "radioButtonManual";
            this.radioButtonManual.Size = new System.Drawing.Size(60, 17);
            this.radioButtonManual.TabIndex = 7;
            this.radioButtonManual.Text = "Manual";
            this.radioButtonManual.UseVisualStyleBackColor = true;
            // 
            // textBoxGA
            // 
            this.textBoxGA.Location = new System.Drawing.Point(233, 45);
            this.textBoxGA.Name = "textBoxGA";
            this.textBoxGA.Size = new System.Drawing.Size(75, 20);
            this.textBoxGA.TabIndex = 6;
            this.textBoxGA.Text = "36";
            this.textBoxGA.TextChanged += new System.EventHandler(this.textBoxGA_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(198, 48);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(25, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "GA:";
            // 
            // comboBoxRESTPath
            // 
            this.comboBoxRESTPath.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRESTPath.FormattingEnabled = true;
            this.comboBoxRESTPath.Items.AddRange(new object[] {
            "Patterns Add-On"});
            this.comboBoxRESTPath.Location = new System.Drawing.Point(50, 45);
            this.comboBoxRESTPath.Name = "comboBoxRESTPath";
            this.comboBoxRESTPath.Size = new System.Drawing.Size(142, 21);
            this.comboBoxRESTPath.TabIndex = 4;
            this.comboBoxRESTPath.SelectedIndexChanged += new System.EventHandler(this.comboBoxRESTPath_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(198, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Port:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Host:";
            // 
            // buttonInit
            // 
            this.buttonInit.Enabled = false;
            this.buttonInit.Location = new System.Drawing.Point(233, 107);
            this.buttonInit.Name = "buttonInit";
            this.buttonInit.Size = new System.Drawing.Size(75, 23);
            this.buttonInit.TabIndex = 3;
            this.buttonInit.Text = "Initialize";
            this.buttonInit.UseVisualStyleBackColor = true;
            this.buttonInit.Click += new System.EventHandler(this.buttonInit_Click);
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(233, 19);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(75, 20);
            this.textBoxPort.TabIndex = 1;
            this.textBoxPort.Text = "8000";
            this.textBoxPort.TextChanged += new System.EventHandler(this.textBoxPort_TextChanged);
            // 
            // textBoxHost
            // 
            this.textBoxHost.Location = new System.Drawing.Point(50, 19);
            this.textBoxHost.Name = "textBoxHost";
            this.textBoxHost.Size = new System.Drawing.Size(142, 20);
            this.textBoxHost.TabIndex = 0;
            this.textBoxHost.Text = "127.0.0.1";
            this.textBoxHost.TextChanged += new System.EventHandler(this.textBoxHost_TextChanged);
            // 
            // groupBoxInitResponse
            // 
            this.groupBoxInitResponse.Controls.Add(this.textBoxGARet);
            this.groupBoxInitResponse.Controls.Add(this.textBoxTokenId);
            this.groupBoxInitResponse.Controls.Add(this.label5);
            this.groupBoxInitResponse.Controls.Add(this.label2);
            this.groupBoxInitResponse.Location = new System.Drawing.Point(3, 145);
            this.groupBoxInitResponse.Name = "groupBoxInitResponse";
            this.groupBoxInitResponse.Size = new System.Drawing.Size(314, 78);
            this.groupBoxInitResponse.TabIndex = 2;
            this.groupBoxInitResponse.TabStop = false;
            this.groupBoxInitResponse.Text = "Service Initialization Response";
            // 
            // textBoxGARet
            // 
            this.textBoxGARet.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxGARet.Location = new System.Drawing.Point(67, 45);
            this.textBoxGARet.Name = "textBoxGARet";
            this.textBoxGARet.ReadOnly = true;
            this.textBoxGARet.Size = new System.Drawing.Size(218, 20);
            this.textBoxGARet.TabIndex = 7;
            // 
            // textBoxTokenId
            // 
            this.textBoxTokenId.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxTokenId.Location = new System.Drawing.Point(67, 19);
            this.textBoxTokenId.Name = "textBoxTokenId";
            this.textBoxTokenId.ReadOnly = true;
            this.textBoxTokenId.Size = new System.Drawing.Size(218, 20);
            this.textBoxTokenId.TabIndex = 6;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(25, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "GA:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Token ID:";
            // 
            // groupBoxServiceStatus
            // 
            this.groupBoxServiceStatus.Controls.Add(this.buttonAbout);
            this.groupBoxServiceStatus.Controls.Add(this.buttonGetStatus);
            this.groupBoxServiceStatus.Controls.Add(this.listBoxStatus);
            this.groupBoxServiceStatus.Location = new System.Drawing.Point(3, 229);
            this.groupBoxServiceStatus.Name = "groupBoxServiceStatus";
            this.groupBoxServiceStatus.Size = new System.Drawing.Size(314, 160);
            this.groupBoxServiceStatus.TabIndex = 3;
            this.groupBoxServiceStatus.TabStop = false;
            this.groupBoxServiceStatus.Text = "Service Status";
            // 
            // buttonGetStatus
            // 
            this.buttonGetStatus.Enabled = false;
            this.buttonGetStatus.Location = new System.Drawing.Point(232, 131);
            this.buttonGetStatus.Name = "buttonGetStatus";
            this.buttonGetStatus.Size = new System.Drawing.Size(75, 23);
            this.buttonGetStatus.TabIndex = 1;
            this.buttonGetStatus.Text = "Get Status";
            this.buttonGetStatus.UseVisualStyleBackColor = true;
            this.buttonGetStatus.Click += new System.EventHandler(this.buttonGetStatus_Click);
            // 
            // listBoxStatus
            // 
            this.listBoxStatus.FormattingEnabled = true;
            this.listBoxStatus.IntegralHeight = false;
            this.listBoxStatus.Location = new System.Drawing.Point(7, 20);
            this.listBoxStatus.Name = "listBoxStatus";
            this.listBoxStatus.Size = new System.Drawing.Size(300, 105);
            this.listBoxStatus.TabIndex = 0;
            // 
            // groupBoxLiveSessions
            // 
            this.groupBoxLiveSessions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxLiveSessions.Controls.Add(this.listViewSessions);
            this.groupBoxLiveSessions.Controls.Add(this.buttonGetSession);
            this.groupBoxLiveSessions.Location = new System.Drawing.Point(323, 3);
            this.groupBoxLiveSessions.Name = "groupBoxLiveSessions";
            this.groupBoxLiveSessions.Size = new System.Drawing.Size(328, 386);
            this.groupBoxLiveSessions.TabIndex = 4;
            this.groupBoxLiveSessions.TabStop = false;
            this.groupBoxLiveSessions.Text = "Live Sessions";
            // 
            // listViewSessions
            // 
            this.listViewSessions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewSessions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderSessionID,
            this.columnHeaderGA});
            this.listViewSessions.GridLines = true;
            this.listViewSessions.Location = new System.Drawing.Point(6, 19);
            this.listViewSessions.Name = "listViewSessions";
            this.listViewSessions.Size = new System.Drawing.Size(316, 332);
            this.listViewSessions.TabIndex = 1;
            this.listViewSessions.UseCompatibleStateImageBehavior = false;
            this.listViewSessions.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderSessionID
            // 
            this.columnHeaderSessionID.Text = "Session ID";
            this.columnHeaderSessionID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderSessionID.Width = 253;
            // 
            // columnHeaderGA
            // 
            this.columnHeaderGA.Text = "GA";
            this.columnHeaderGA.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeaderGA.Width = 57;
            // 
            // buttonGetSession
            // 
            this.buttonGetSession.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGetSession.Enabled = false;
            this.buttonGetSession.Location = new System.Drawing.Point(240, 357);
            this.buttonGetSession.Name = "buttonGetSession";
            this.buttonGetSession.Size = new System.Drawing.Size(82, 23);
            this.buttonGetSession.TabIndex = 0;
            this.buttonGetSession.Text = "Get Sessions";
            this.buttonGetSession.UseVisualStyleBackColor = true;
            this.buttonGetSession.Click += new System.EventHandler(this.buttonGetSession_Click);
            // 
            // buttonAbout
            // 
            this.buttonAbout.Location = new System.Drawing.Point(151, 131);
            this.buttonAbout.Name = "buttonAbout";
            this.buttonAbout.Size = new System.Drawing.Size(75, 23);
            this.buttonAbout.TabIndex = 2;
            this.buttonAbout.Text = "About";
            this.buttonAbout.UseVisualStyleBackColor = true;
            this.buttonAbout.Click += new System.EventHandler(this.buttonAbout_Click);
            // 
            // InitServerCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxLiveSessions);
            this.Controls.Add(this.groupBoxServiceStatus);
            this.Controls.Add(this.groupBoxInitResponse);
            this.Controls.Add(this.groupBoxServiceInit);
            this.MinimumSize = new System.Drawing.Size(654, 392);
            this.Name = "InitServerCtrl";
            this.Size = new System.Drawing.Size(654, 392);
            this.groupBoxServiceInit.ResumeLayout(false);
            this.groupBoxServiceInit.PerformLayout();
            this.groupBoxInitResponse.ResumeLayout(false);
            this.groupBoxInitResponse.PerformLayout();
            this.groupBoxServiceStatus.ResumeLayout(false);
            this.groupBoxLiveSessions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxServiceInit;
        private System.Windows.Forms.TextBox textBoxGA;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBoxRESTPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonInit;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.TextBox textBoxHost;
        private System.Windows.Forms.GroupBox groupBoxInitResponse;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBoxServiceStatus;
        private System.Windows.Forms.Button buttonGetStatus;
        private System.Windows.Forms.ListBox listBoxStatus;
        private System.Windows.Forms.GroupBox groupBoxLiveSessions;
        private System.Windows.Forms.Button buttonGetSession;
        private System.Windows.Forms.ListView listViewSessions;
        private System.Windows.Forms.ColumnHeader columnHeaderSessionID;
        private System.Windows.Forms.ColumnHeader columnHeaderGA;
        private System.Windows.Forms.TextBox textBoxGARet;
        private System.Windows.Forms.TextBox textBoxTokenId;
        private System.Windows.Forms.RadioButton radioButtonFromFile;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton radioButtonManual;
        private System.Windows.Forms.Button buttonAbout;
    }
}
