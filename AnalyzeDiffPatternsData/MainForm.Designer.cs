namespace AnalyzeDiffPatternsData
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
            this.labelfileToLoad = new System.Windows.Forms.Label();
            this.textBoxLoafFilePath = new System.Windows.Forms.TextBox();
            this.buttonBrowseLoad = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxSaveFolder = new System.Windows.Forms.TextBox();
            this.buttonBrowseSaveFolder = new System.Windows.Forms.Button();
            this.listBoxAllPatients = new System.Windows.Forms.ListBox();
            this.listBoxSelectedPatients = new System.Windows.Forms.ListBox();
            this.buttonRun = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.moveButtonRemove = new CALMDevTool.MoveButton();
            this.moveButtonAdd = new CALMDevTool.MoveButton();
            this.SuspendLayout();
            // 
            // labelfileToLoad
            // 
            this.labelfileToLoad.AutoSize = true;
            this.labelfileToLoad.Location = new System.Drawing.Point(12, 15);
            this.labelfileToLoad.Name = "labelfileToLoad";
            this.labelfileToLoad.Size = new System.Drawing.Size(69, 13);
            this.labelfileToLoad.TabIndex = 0;
            this.labelfileToLoad.Text = "File To Load:";
            // 
            // textBoxLoafFilePath
            // 
            this.textBoxLoafFilePath.Location = new System.Drawing.Point(87, 12);
            this.textBoxLoafFilePath.Name = "textBoxLoafFilePath";
            this.textBoxLoafFilePath.ReadOnly = true;
            this.textBoxLoafFilePath.Size = new System.Drawing.Size(449, 20);
            this.textBoxLoafFilePath.TabIndex = 1;
            this.textBoxLoafFilePath.TabStop = false;
            // 
            // buttonBrowseLoad
            // 
            this.buttonBrowseLoad.Location = new System.Drawing.Point(542, 10);
            this.buttonBrowseLoad.Name = "buttonBrowseLoad";
            this.buttonBrowseLoad.Size = new System.Drawing.Size(25, 23);
            this.buttonBrowseLoad.TabIndex = 2;
            this.buttonBrowseLoad.Text = "...";
            this.buttonBrowseLoad.UseVisualStyleBackColor = true;
            this.buttonBrowseLoad.Click += new System.EventHandler(this.buttonBrowseLoad_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Save To:";
            // 
            // textBoxSaveFolder
            // 
            this.textBoxSaveFolder.Location = new System.Drawing.Point(87, 41);
            this.textBoxSaveFolder.Name = "textBoxSaveFolder";
            this.textBoxSaveFolder.ReadOnly = true;
            this.textBoxSaveFolder.Size = new System.Drawing.Size(449, 20);
            this.textBoxSaveFolder.TabIndex = 4;
            this.textBoxSaveFolder.TabStop = false;
            // 
            // buttonBrowseSaveFolder
            // 
            this.buttonBrowseSaveFolder.Location = new System.Drawing.Point(542, 39);
            this.buttonBrowseSaveFolder.Name = "buttonBrowseSaveFolder";
            this.buttonBrowseSaveFolder.Size = new System.Drawing.Size(25, 23);
            this.buttonBrowseSaveFolder.TabIndex = 5;
            this.buttonBrowseSaveFolder.Text = "...";
            this.buttonBrowseSaveFolder.UseVisualStyleBackColor = true;
            this.buttonBrowseSaveFolder.Click += new System.EventHandler(this.buttonBrowseSaveFolder_Click);
            // 
            // listBoxAllPatients
            // 
            this.listBoxAllPatients.FormattingEnabled = true;
            this.listBoxAllPatients.Location = new System.Drawing.Point(101, 89);
            this.listBoxAllPatients.Name = "listBoxAllPatients";
            this.listBoxAllPatients.Size = new System.Drawing.Size(214, 95);
            this.listBoxAllPatients.TabIndex = 6;
            // 
            // listBoxSelectedPatients
            // 
            this.listBoxSelectedPatients.FormattingEnabled = true;
            this.listBoxSelectedPatients.Location = new System.Drawing.Point(352, 89);
            this.listBoxSelectedPatients.Name = "listBoxSelectedPatients";
            this.listBoxSelectedPatients.Size = new System.Drawing.Size(214, 95);
            this.listBoxSelectedPatients.TabIndex = 6;
            // 
            // buttonRun
            // 
            this.buttonRun.Enabled = false;
            this.buttonRun.Location = new System.Drawing.Point(410, 196);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 8;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(491, 196);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 8;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 89);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(87, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Choose Patients:";
            // 
            // moveButtonRemove
            // 
            this.moveButtonRemove.ButtonDirection = CALMDevTool.MoveButton.MoveDirection.MovePrev;
            this.moveButtonRemove.Enabled = false;
            this.moveButtonRemove.Location = new System.Drawing.Point(321, 136);
            this.moveButtonRemove.Name = "moveButtonRemove";
            this.moveButtonRemove.Size = new System.Drawing.Size(25, 25);
            this.moveButtonRemove.TabIndex = 7;
            this.moveButtonRemove.UseVisualStyleBackColor = true;
            this.moveButtonRemove.Click += new System.EventHandler(this.moveButtonRemove_Click);
            // 
            // moveButtonAdd
            // 
            this.moveButtonAdd.ButtonDirection = CALMDevTool.MoveButton.MoveDirection.MoveNext;
            this.moveButtonAdd.Enabled = false;
            this.moveButtonAdd.Location = new System.Drawing.Point(321, 105);
            this.moveButtonAdd.Name = "moveButtonAdd";
            this.moveButtonAdd.Size = new System.Drawing.Size(25, 25);
            this.moveButtonAdd.TabIndex = 7;
            this.moveButtonAdd.UseVisualStyleBackColor = true;
            this.moveButtonAdd.Click += new System.EventHandler(this.moveButtonAdd_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(579, 231);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonRun);
            this.Controls.Add(this.moveButtonRemove);
            this.Controls.Add(this.moveButtonAdd);
            this.Controls.Add(this.listBoxSelectedPatients);
            this.Controls.Add(this.listBoxAllPatients);
            this.Controls.Add(this.buttonBrowseSaveFolder);
            this.Controls.Add(this.textBoxSaveFolder);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonBrowseLoad);
            this.Controls.Add(this.textBoxLoafFilePath);
            this.Controls.Add(this.labelfileToLoad);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.Text = "Analyze Patterns™ Engine Input";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelfileToLoad;
        private System.Windows.Forms.TextBox textBoxLoafFilePath;
        private System.Windows.Forms.Button buttonBrowseLoad;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxSaveFolder;
        private System.Windows.Forms.Button buttonBrowseSaveFolder;
        private System.Windows.Forms.ListBox listBoxAllPatients;
        private System.Windows.Forms.ListBox listBoxSelectedPatients;
        private CALMDevTool.MoveButton moveButtonAdd;
        private CALMDevTool.MoveButton moveButtonRemove;
        private System.Windows.Forms.Button buttonRun;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Label label2;
    }
}

