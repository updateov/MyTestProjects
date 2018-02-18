namespace TestPaternsActiveX
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
			this.numHistory = new System.Windows.Forms.NumericUpDown();
			this.ofdSelectFiles = new System.Windows.Forms.OpenFileDialog();
			this.workerCtl = new System.ComponentModel.BackgroundWorker();
			this.btnPlugUnplug = new System.Windows.Forms.Button();
			this.panelTitle = new System.Windows.Forms.Panel();
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblFileToProcess = new System.Windows.Forms.Label();
			this.txtFile = new System.Windows.Forms.TextBox();
			this.grpSettings = new System.Windows.Forms.GroupBox();
			this.lblOutOf = new System.Windows.Forms.Label();
			this.lblHistory = new System.Windows.Forms.Label();
			this.grpOperations = new System.Windows.Forms.GroupBox();
			this.btnLateRealtimeOnOff = new System.Windows.Forms.Button();
			this.btnDemo = new System.Windows.Forms.Button();
			this.btnOpenURL = new System.Windows.Forms.Button();
			this.btnStartStop = new System.Windows.Forms.Button();
			this.btnSelectFile = new System.Windows.Forms.Button();
			this.btnRecovery = new System.Windows.Forms.Button();
			this.btnDisconnect = new System.Windows.Forms.Button();
			this.btnReset = new System.Windows.Forms.Button();
			this.btnPowerOnOff = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.dtpPatientReset = new System.Windows.Forms.DateTimePicker();
			this.btnUpdate = new System.Windows.Forms.Button();
			this.txtPatientFetus = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.dtpPatientEDD = new System.Windows.Forms.DateTimePicker();
			this.txtPatientLastName = new System.Windows.Forms.TextBox();
			this.txtPatientFirstName = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.txtPatientMRN = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.numTimeoutDlg = new System.Windows.Forms.NumericUpDown();
			this.label16 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.numCRStage2 = new System.Windows.Forms.NumericUpDown();
			this.label14 = new System.Windows.Forms.Label();
			this.numCRStage1 = new System.Windows.Forms.NumericUpDown();
			this.label13 = new System.Windows.Forms.Label();
			this.numCRLimit = new System.Windows.Forms.NumericUpDown();
			this.label12 = new System.Windows.Forms.Label();
			this.numCRWindow = new System.Windows.Forms.NumericUpDown();
			this.cboViewerBanner = new System.Windows.Forms.ComboBox();
			this.label15 = new System.Windows.Forms.Label();
			this.numURLRefresh = new System.Windows.Forms.NumericUpDown();
			this.label10 = new System.Windows.Forms.Label();
			this.txtURLUserPermission = new System.Windows.Forms.ComboBox();
			this.label9 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.txtURLUserName = new System.Windows.Forms.TextBox();
			this.txtURLUserID = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.numHistory)).BeginInit();
			this.panelTitle.SuspendLayout();
			this.grpSettings.SuspendLayout();
			this.grpOperations.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numTimeoutDlg)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCRStage2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCRStage1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCRLimit)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numCRWindow)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numURLRefresh)).BeginInit();
			this.SuspendLayout();
			// 
			// numHistory
			// 
			this.numHistory.Location = new System.Drawing.Point(59, 47);
			this.numHistory.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
			this.numHistory.Name = "numHistory";
			this.numHistory.Size = new System.Drawing.Size(43, 20);
			this.numHistory.TabIndex = 3;
			this.numHistory.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			// 
			// ofdSelectFiles
			// 
			this.ofdSelectFiles.Filter = "XML files|*.xml|IN files|*.in|All files (*.*)|*.*";
			this.ofdSelectFiles.FilterIndex = 3;
			this.ofdSelectFiles.Title = "Select a file";
			// 
			// workerCtl
			// 
			this.workerCtl.WorkerSupportsCancellation = true;
			this.workerCtl.DoWork += new System.ComponentModel.DoWorkEventHandler(this.workerCtl_DoWork);
			// 
			// btnPlugUnplug
			// 
			this.btnPlugUnplug.Location = new System.Drawing.Point(6, 75);
			this.btnPlugUnplug.Name = "btnPlugUnplug";
			this.btnPlugUnplug.Size = new System.Drawing.Size(111, 23);
			this.btnPlugUnplug.TabIndex = 2;
			this.btnPlugUnplug.Text = "Unplug the probes";
			this.btnPlugUnplug.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnPlugUnplug.UseVisualStyleBackColor = true;
			this.btnPlugUnplug.Click += new System.EventHandler(this.btnPlugUnplug_Click);
			// 
			// panelTitle
			// 
			this.panelTitle.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.panelTitle.Controls.Add(this.lblTitle);
			this.panelTitle.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelTitle.Location = new System.Drawing.Point(0, 0);
			this.panelTitle.Name = "panelTitle";
			this.panelTitle.Size = new System.Drawing.Size(762, 37);
			this.panelTitle.TabIndex = 0;
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Arial", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.ForeColor = System.Drawing.Color.White;
			this.lblTitle.Location = new System.Drawing.Point(3, 9);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(290, 18);
			this.lblTitle.TabIndex = 0;
			this.lblTitle.Text = "PeriCALM® Patterns™ ActiveX TestBed";
			// 
			// lblFileToProcess
			// 
			this.lblFileToProcess.AutoSize = true;
			this.lblFileToProcess.Location = new System.Drawing.Point(10, 22);
			this.lblFileToProcess.Name = "lblFileToProcess";
			this.lblFileToProcess.Size = new System.Drawing.Size(23, 13);
			this.lblFileToProcess.TabIndex = 0;
			this.lblFileToProcess.Text = "File";
			// 
			// txtFile
			// 
			this.txtFile.Location = new System.Drawing.Point(59, 19);
			this.txtFile.Name = "txtFile";
			this.txtFile.ReadOnly = true;
			this.txtFile.Size = new System.Drawing.Size(225, 20);
			this.txtFile.TabIndex = 1;
			// 
			// grpSettings
			// 
			this.grpSettings.Controls.Add(this.lblOutOf);
			this.grpSettings.Controls.Add(this.lblHistory);
			this.grpSettings.Controls.Add(this.lblFileToProcess);
			this.grpSettings.Controls.Add(this.txtFile);
			this.grpSettings.Controls.Add(this.numHistory);
			this.grpSettings.Location = new System.Drawing.Point(6, 50);
			this.grpSettings.Name = "grpSettings";
			this.grpSettings.Size = new System.Drawing.Size(298, 74);
			this.grpSettings.TabIndex = 1;
			this.grpSettings.TabStop = false;
			this.grpSettings.Text = "Tracing source";
			// 
			// lblOutOf
			// 
			this.lblOutOf.AutoSize = true;
			this.lblOutOf.Location = new System.Drawing.Point(108, 49);
			this.lblOutOf.Name = "lblOutOf";
			this.lblOutOf.Size = new System.Drawing.Size(154, 13);
			this.lblOutOf.TabIndex = 4;
			this.lblOutOf.Text = "out of 0 hour of tracings loaded";
			// 
			// lblHistory
			// 
			this.lblHistory.AutoSize = true;
			this.lblHistory.Location = new System.Drawing.Point(10, 49);
			this.lblHistory.Name = "lblHistory";
			this.lblHistory.Size = new System.Drawing.Size(43, 13);
			this.lblHistory.TabIndex = 2;
			this.lblHistory.Text = "Preload";
			// 
			// grpOperations
			// 
			this.grpOperations.Controls.Add(this.btnLateRealtimeOnOff);
			this.grpOperations.Controls.Add(this.btnDemo);
			this.grpOperations.Controls.Add(this.btnOpenURL);
			this.grpOperations.Controls.Add(this.btnStartStop);
			this.grpOperations.Controls.Add(this.btnSelectFile);
			this.grpOperations.Controls.Add(this.btnRecovery);
			this.grpOperations.Controls.Add(this.btnDisconnect);
			this.grpOperations.Controls.Add(this.btnReset);
			this.grpOperations.Controls.Add(this.btnPowerOnOff);
			this.grpOperations.Controls.Add(this.btnPlugUnplug);
			this.grpOperations.Location = new System.Drawing.Point(623, 50);
			this.grpOperations.Name = "grpOperations";
			this.grpOperations.Size = new System.Drawing.Size(132, 300);
			this.grpOperations.TabIndex = 4;
			this.grpOperations.TabStop = false;
			this.grpOperations.Text = "Operations";
			// 
			// btnLateRealtimeOnOff
			// 
			this.btnLateRealtimeOnOff.Location = new System.Drawing.Point(6, 129);
			this.btnLateRealtimeOnOff.Name = "btnLateRealtimeOnOff";
			this.btnLateRealtimeOnOff.Size = new System.Drawing.Size(111, 23);
			this.btnLateRealtimeOnOff.TabIndex = 4;
			this.btnLateRealtimeOnOff.Text = "Late realtime Off";
			this.btnLateRealtimeOnOff.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnLateRealtimeOnOff.UseVisualStyleBackColor = true;
			this.btnLateRealtimeOnOff.Click += new System.EventHandler(this.btnLateRealtimeOnOff_Click);
			// 
			// btnDemo
			// 
			this.btnDemo.Location = new System.Drawing.Point(6, 156);
			this.btnDemo.Name = "btnDemo";
			this.btnDemo.Size = new System.Drawing.Size(111, 23);
			this.btnDemo.TabIndex = 5;
			this.btnDemo.Text = "Demo ON";
			this.btnDemo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnDemo.UseVisualStyleBackColor = true;
			this.btnDemo.Click += new System.EventHandler(this.btnDemo_Click);
			// 
			// btnOpenURL
			// 
			this.btnOpenURL.Location = new System.Drawing.Point(6, 264);
			this.btnOpenURL.Name = "btnOpenURL";
			this.btnOpenURL.Size = new System.Drawing.Size(111, 23);
			this.btnOpenURL.TabIndex = 9;
			this.btnOpenURL.Text = "Open Patterns";
			this.btnOpenURL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnOpenURL.UseVisualStyleBackColor = true;
			this.btnOpenURL.Click += new System.EventHandler(this.btnOpenURL_Click);
			// 
			// btnStartStop
			// 
			this.btnStartStop.Location = new System.Drawing.Point(6, 48);
			this.btnStartStop.Name = "btnStartStop";
			this.btnStartStop.Size = new System.Drawing.Size(111, 23);
			this.btnStartStop.TabIndex = 1;
			this.btnStartStop.Text = "Start simulation";
			this.btnStartStop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnStartStop.UseVisualStyleBackColor = true;
			this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
			// 
			// btnSelectFile
			// 
			this.btnSelectFile.Location = new System.Drawing.Point(6, 21);
			this.btnSelectFile.Name = "btnSelectFile";
			this.btnSelectFile.Size = new System.Drawing.Size(111, 23);
			this.btnSelectFile.TabIndex = 0;
			this.btnSelectFile.Text = "Select file...";
			this.btnSelectFile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnSelectFile.UseVisualStyleBackColor = true;
			this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
			// 
			// btnRecovery
			// 
			this.btnRecovery.Location = new System.Drawing.Point(6, 237);
			this.btnRecovery.Name = "btnRecovery";
			this.btnRecovery.Size = new System.Drawing.Size(111, 23);
			this.btnRecovery.TabIndex = 8;
			this.btnRecovery.Text = "Recovery ON";
			this.btnRecovery.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnRecovery.UseVisualStyleBackColor = true;
			this.btnRecovery.Click += new System.EventHandler(this.btnRecovery_Click);
			// 
			// btnDisconnect
			// 
			this.btnDisconnect.Location = new System.Drawing.Point(6, 210);
			this.btnDisconnect.Name = "btnDisconnect";
			this.btnDisconnect.Size = new System.Drawing.Size(111, 23);
			this.btnDisconnect.TabIndex = 7;
			this.btnDisconnect.Text = "Disconnect server";
			this.btnDisconnect.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnDisconnect.UseVisualStyleBackColor = true;
			this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
			// 
			// btnReset
			// 
			this.btnReset.Location = new System.Drawing.Point(6, 183);
			this.btnReset.Name = "btnReset";
			this.btnReset.Size = new System.Drawing.Size(111, 23);
			this.btnReset.TabIndex = 6;
			this.btnReset.Text = "Reset data";
			this.btnReset.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnReset.UseVisualStyleBackColor = true;
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// btnPowerOnOff
			// 
			this.btnPowerOnOff.Location = new System.Drawing.Point(6, 102);
			this.btnPowerOnOff.Name = "btnPowerOnOff";
			this.btnPowerOnOff.Size = new System.Drawing.Size(111, 23);
			this.btnPowerOnOff.TabIndex = 3;
			this.btnPowerOnOff.Text = "Power monitor off";
			this.btnPowerOnOff.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.btnPowerOnOff.UseVisualStyleBackColor = true;
			this.btnPowerOnOff.Click += new System.EventHandler(this.btnPowerOnOff_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.dtpPatientReset);
			this.groupBox1.Controls.Add(this.btnUpdate);
			this.groupBox1.Controls.Add(this.txtPatientFetus);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.dtpPatientEDD);
			this.groupBox1.Controls.Add(this.txtPatientLastName);
			this.groupBox1.Controls.Add(this.txtPatientFirstName);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.txtPatientMRN);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Location = new System.Drawing.Point(6, 132);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(298, 218);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Patient information";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 127);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 11;
			this.label1.Text = "Reset";
			// 
			// dtpPatientReset
			// 
			this.dtpPatientReset.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
			this.dtpPatientReset.Location = new System.Drawing.Point(87, 123);
			this.dtpPatientReset.Name = "dtpPatientReset";
			this.dtpPatientReset.ShowCheckBox = true;
			this.dtpPatientReset.Size = new System.Drawing.Size(165, 20);
			this.dtpPatientReset.TabIndex = 12;
			// 
			// btnUpdate
			// 
			this.btnUpdate.Location = new System.Drawing.Point(13, 181);
			this.btnUpdate.Name = "btnUpdate";
			this.btnUpdate.Size = new System.Drawing.Size(271, 25);
			this.btnUpdate.TabIndex = 10;
			this.btnUpdate.Text = "Update";
			this.btnUpdate.UseVisualStyleBackColor = true;
			this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
			// 
			// txtPatientFetus
			// 
			this.txtPatientFetus.Location = new System.Drawing.Point(87, 152);
			this.txtPatientFetus.Name = "txtPatientFetus";
			this.txtPatientFetus.Size = new System.Drawing.Size(27, 20);
			this.txtPatientFetus.TabIndex = 9;
			this.txtPatientFetus.Text = "1";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(10, 156);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(63, 13);
			this.label8.TabIndex = 8;
			this.label8.Text = "Fetus count";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(10, 101);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(30, 13);
			this.label7.TabIndex = 6;
			this.label7.Text = "EDD";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(10, 75);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(56, 13);
			this.label6.TabIndex = 4;
			this.label6.Text = "Last name";
			// 
			// dtpPatientEDD
			// 
			this.dtpPatientEDD.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dtpPatientEDD.Location = new System.Drawing.Point(87, 97);
			this.dtpPatientEDD.Name = "dtpPatientEDD";
			this.dtpPatientEDD.ShowCheckBox = true;
			this.dtpPatientEDD.Size = new System.Drawing.Size(100, 20);
			this.dtpPatientEDD.TabIndex = 7;
			// 
			// txtPatientLastName
			// 
			this.txtPatientLastName.Location = new System.Drawing.Point(87, 71);
			this.txtPatientLastName.Name = "txtPatientLastName";
			this.txtPatientLastName.Size = new System.Drawing.Size(197, 20);
			this.txtPatientLastName.TabIndex = 5;
			this.txtPatientLastName.Text = "Doe";
			// 
			// txtPatientFirstName
			// 
			this.txtPatientFirstName.Location = new System.Drawing.Point(87, 45);
			this.txtPatientFirstName.Name = "txtPatientFirstName";
			this.txtPatientFirstName.Size = new System.Drawing.Size(197, 20);
			this.txtPatientFirstName.TabIndex = 3;
			this.txtPatientFirstName.Text = "Jane";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 49);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "First name";
			// 
			// txtPatientMRN
			// 
			this.txtPatientMRN.Location = new System.Drawing.Point(87, 19);
			this.txtPatientMRN.Name = "txtPatientMRN";
			this.txtPatientMRN.Size = new System.Drawing.Size(197, 20);
			this.txtPatientMRN.TabIndex = 1;
			this.txtPatientMRN.Text = "A145266";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 23);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(32, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "MRN";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.numTimeoutDlg);
			this.groupBox2.Controls.Add(this.label16);
			this.groupBox2.Controls.Add(this.label11);
			this.groupBox2.Controls.Add(this.numCRStage2);
			this.groupBox2.Controls.Add(this.label14);
			this.groupBox2.Controls.Add(this.numCRStage1);
			this.groupBox2.Controls.Add(this.label13);
			this.groupBox2.Controls.Add(this.numCRLimit);
			this.groupBox2.Controls.Add(this.label12);
			this.groupBox2.Controls.Add(this.numCRWindow);
			this.groupBox2.Controls.Add(this.cboViewerBanner);
			this.groupBox2.Controls.Add(this.label15);
			this.groupBox2.Controls.Add(this.numURLRefresh);
			this.groupBox2.Controls.Add(this.label10);
			this.groupBox2.Controls.Add(this.txtURLUserPermission);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.txtURLUserName);
			this.groupBox2.Controls.Add(this.txtURLUserID);
			this.groupBox2.Controls.Add(this.label3);
			this.groupBox2.Location = new System.Drawing.Point(310, 50);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(298, 300);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Viewer";
			// 
			// numTimeoutDlg
			// 
			this.numTimeoutDlg.Location = new System.Drawing.Point(241, 78);
			this.numTimeoutDlg.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
			this.numTimeoutDlg.Name = "numTimeoutDlg";
			this.numTimeoutDlg.Size = new System.Drawing.Size(43, 20);
			this.numTimeoutDlg.TabIndex = 6;
			this.numTimeoutDlg.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(148, 82);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(87, 13);
			this.label16.TabIndex = 19;
			this.label16.Text = "Timeout dlg (min)";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(10, 194);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(112, 13);
			this.label11.TabIndex = 11;
			this.label11.Text = "Window duration (min)";
			// 
			// numCRStage2
			// 
			this.numCRStage2.Location = new System.Drawing.Point(130, 268);
			this.numCRStage2.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numCRStage2.Name = "numCRStage2";
			this.numCRStage2.Size = new System.Drawing.Size(43, 20);
			this.numCRStage2.TabIndex = 18;
			this.numCRStage2.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(10, 272);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(69, 13);
			this.label14.TabIndex = 17;
			this.label14.Text = "Stage 2 (min)";
			// 
			// numCRStage1
			// 
			this.numCRStage1.Location = new System.Drawing.Point(130, 243);
			this.numCRStage1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numCRStage1.Name = "numCRStage1";
			this.numCRStage1.Size = new System.Drawing.Size(43, 20);
			this.numCRStage1.TabIndex = 16;
			this.numCRStage1.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(10, 247);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(80, 13);
			this.label13.TabIndex = 15;
			this.label13.Text = "Stage 1 (count)";
			// 
			// numCRLimit
			// 
			this.numCRLimit.Location = new System.Drawing.Point(130, 216);
			this.numCRLimit.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numCRLimit.Name = "numCRLimit";
			this.numCRLimit.Size = new System.Drawing.Size(43, 20);
			this.numCRLimit.TabIndex = 14;
			this.numCRLimit.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(10, 220);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(94, 13);
			this.label12.TabIndex = 13;
			this.label12.Text = "Graph max (count)";
			// 
			// numCRWindow
			// 
			this.numCRWindow.Location = new System.Drawing.Point(130, 190);
			this.numCRWindow.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numCRWindow.Name = "numCRWindow";
			this.numCRWindow.Size = new System.Drawing.Size(43, 20);
			this.numCRWindow.TabIndex = 12;
			this.numCRWindow.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// cboViewerBanner
			// 
			this.cboViewerBanner.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboViewerBanner.FormattingEnabled = true;
			this.cboViewerBanner.Items.AddRange(new object[] {
            "0-PeriGen",
            "1-Power by Perigen",
            "2-GE"});
			this.cboViewerBanner.Location = new System.Drawing.Point(87, 135);
			this.cboViewerBanner.Name = "cboViewerBanner";
			this.cboViewerBanner.Size = new System.Drawing.Size(197, 21);
			this.cboViewerBanner.TabIndex = 9;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(9, 139);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(41, 13);
			this.label15.TabIndex = 8;
			this.label15.Text = "Banner";
			// 
			// numURLRefresh
			// 
			this.numURLRefresh.Location = new System.Drawing.Point(87, 78);
			this.numURLRefresh.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numURLRefresh.Name = "numURLRefresh";
			this.numURLRefresh.Size = new System.Drawing.Size(43, 20);
			this.numURLRefresh.TabIndex = 5;
			this.numURLRefresh.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(10, 82);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(70, 13);
			this.label10.TabIndex = 4;
			this.label10.Text = "Refresh (sec)";
			// 
			// txtURLUserPermission
			// 
			this.txtURLUserPermission.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.txtURLUserPermission.FormattingEnabled = true;
			this.txtURLUserPermission.Items.AddRange(new object[] {
            "readonly",
            "fullaccess"});
			this.txtURLUserPermission.Location = new System.Drawing.Point(87, 106);
			this.txtURLUserPermission.Name = "txtURLUserPermission";
			this.txtURLUserPermission.Size = new System.Drawing.Size(197, 21);
			this.txtURLUserPermission.TabIndex = 7;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(10, 110);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(57, 13);
			this.label9.TabIndex = 6;
			this.label9.Text = "Permission";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(10, 54);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(58, 13);
			this.label4.TabIndex = 2;
			this.label4.Text = "User name";
			// 
			// txtURLUserName
			// 
			this.txtURLUserName.Location = new System.Drawing.Point(87, 50);
			this.txtURLUserName.Name = "txtURLUserName";
			this.txtURLUserName.Size = new System.Drawing.Size(197, 20);
			this.txtURLUserName.TabIndex = 3;
			this.txtURLUserName.Text = "Elizabeth Smith";
			// 
			// txtURLUserID
			// 
			this.txtURLUserID.Location = new System.Drawing.Point(87, 22);
			this.txtURLUserID.Name = "txtURLUserID";
			this.txtURLUserID.Size = new System.Drawing.Size(197, 20);
			this.txtURLUserID.TabIndex = 1;
			this.txtURLUserID.Text = "ELISMTD";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(10, 26);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(43, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "User ID";
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(762, 358);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.grpOperations);
			this.Controls.Add(this.grpSettings);
			this.Controls.Add(this.panelTitle);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "PeriCALM® Patterns™ ActiveX TestBed";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.numHistory)).EndInit();
			this.panelTitle.ResumeLayout(false);
			this.panelTitle.PerformLayout();
			this.grpSettings.ResumeLayout(false);
			this.grpSettings.PerformLayout();
			this.grpOperations.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numTimeoutDlg)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCRStage2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCRStage1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCRLimit)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numCRWindow)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numURLRefresh)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

		private System.Windows.Forms.NumericUpDown numHistory;
		private System.Windows.Forms.OpenFileDialog ofdSelectFiles;
        private System.ComponentModel.BackgroundWorker workerCtl;
        private System.Windows.Forms.Button btnPlugUnplug;
        private System.Windows.Forms.Panel panelTitle;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblFileToProcess;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.GroupBox grpSettings;
		private System.Windows.Forms.GroupBox grpOperations;
		private System.Windows.Forms.Label lblHistory;
		private System.Windows.Forms.Label lblOutOf;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox txtPatientMRN;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtPatientLastName;
		private System.Windows.Forms.TextBox txtPatientFirstName;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.DateTimePicker dtpPatientEDD;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Button btnUpdate;
		private System.Windows.Forms.TextBox txtPatientFetus;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.ComboBox txtURLUserPermission;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtURLUserName;
		private System.Windows.Forms.TextBox txtURLUserID;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown numURLRefresh;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.ComboBox cboViewerBanner;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Button btnRecovery;
		private System.Windows.Forms.Button btnDisconnect;
		private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Button btnPowerOnOff;
		private System.Windows.Forms.Button btnSelectFile;
		private System.Windows.Forms.Button btnStartStop;
		private System.Windows.Forms.Button btnDemo;
		private System.Windows.Forms.Button btnOpenURL;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown numCRStage2;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.NumericUpDown numCRStage1;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.NumericUpDown numCRLimit;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.NumericUpDown numCRWindow;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.DateTimePicker dtpPatientReset;
		private System.Windows.Forms.NumericUpDown numTimeoutDlg;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Button btnLateRealtimeOnOff;
    }
}

