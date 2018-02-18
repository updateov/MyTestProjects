namespace Read2Array
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxLoad = new System.Windows.Forms.TextBox();
            this.buttonBrowseLoad = new System.Windows.Forms.Button();
            this.buttonBrowseSave = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxSave = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxLoaddUP = new System.Windows.Forms.TextBox();
            this.buttonBrowseUP = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxArchive = new System.Windows.Forms.TextBox();
            this.buttonBrowseArchive = new System.Windows.Forms.Button();
            this.radioButtonText = new System.Windows.Forms.RadioButton();
            this.radioButtonDecode = new System.Windows.Forms.RadioButton();
            this.radioButtonSingleReq = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.textBoxSingleReqPath = new System.Windows.Forms.TextBox();
            this.buttonBrowseSingleReq = new System.Windows.Forms.Button();
            this.radioButtonResults = new System.Windows.Forms.RadioButton();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxResultsPath = new System.Windows.Forms.TextBox();
            this.buttonResultsPath = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(51, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "FHR Path:";
            // 
            // textBoxLoad
            // 
            this.textBoxLoad.Location = new System.Drawing.Point(141, 30);
            this.textBoxLoad.Name = "textBoxLoad";
            this.textBoxLoad.Size = new System.Drawing.Size(380, 20);
            this.textBoxLoad.TabIndex = 1;
            // 
            // buttonBrowseLoad
            // 
            this.buttonBrowseLoad.Location = new System.Drawing.Point(527, 28);
            this.buttonBrowseLoad.Name = "buttonBrowseLoad";
            this.buttonBrowseLoad.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseLoad.TabIndex = 2;
            this.buttonBrowseLoad.Text = "...";
            this.buttonBrowseLoad.UseVisualStyleBackColor = true;
            this.buttonBrowseLoad.Click += new System.EventHandler(this.buttonBrowseLoad_Click);
            // 
            // buttonBrowseSave
            // 
            this.buttonBrowseSave.Location = new System.Drawing.Point(527, 297);
            this.buttonBrowseSave.Name = "buttonBrowseSave";
            this.buttonBrowseSave.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseSave.TabIndex = 2;
            this.buttonBrowseSave.Text = "...";
            this.buttonBrowseSave.UseVisualStyleBackColor = true;
            this.buttonBrowseSave.Click += new System.EventHandler(this.buttonBrowseSave_Click);
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(478, 367);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(75, 23);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(397, 367);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 2;
            this.buttonOK.Text = "Run";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 302);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Save Path:";
            // 
            // textBoxSave
            // 
            this.textBoxSave.Location = new System.Drawing.Point(141, 299);
            this.textBoxSave.Name = "textBoxSave";
            this.textBoxSave.Size = new System.Drawing.Size(380, 20);
            this.textBoxSave.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(51, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "UP Path:";
            // 
            // textBoxLoaddUP
            // 
            this.textBoxLoaddUP.Location = new System.Drawing.Point(141, 59);
            this.textBoxLoaddUP.Name = "textBoxLoaddUP";
            this.textBoxLoaddUP.Size = new System.Drawing.Size(380, 20);
            this.textBoxLoaddUP.TabIndex = 1;
            // 
            // buttonBrowseUP
            // 
            this.buttonBrowseUP.Location = new System.Drawing.Point(527, 57);
            this.buttonBrowseUP.Name = "buttonBrowseUP";
            this.buttonBrowseUP.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseUP.TabIndex = 2;
            this.buttonBrowseUP.Text = "...";
            this.buttonBrowseUP.UseVisualStyleBackColor = true;
            this.buttonBrowseUP.Click += new System.EventHandler(this.buttonBrowseLoadUP_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(51, 112);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Archive Path:";
            // 
            // textBoxArchive
            // 
            this.textBoxArchive.Location = new System.Drawing.Point(141, 109);
            this.textBoxArchive.Name = "textBoxArchive";
            this.textBoxArchive.Size = new System.Drawing.Size(380, 20);
            this.textBoxArchive.TabIndex = 1;
            // 
            // buttonBrowseArchive
            // 
            this.buttonBrowseArchive.Location = new System.Drawing.Point(527, 107);
            this.buttonBrowseArchive.Name = "buttonBrowseArchive";
            this.buttonBrowseArchive.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseArchive.TabIndex = 2;
            this.buttonBrowseArchive.Text = "...";
            this.buttonBrowseArchive.UseVisualStyleBackColor = true;
            this.buttonBrowseArchive.Click += new System.EventHandler(this.buttonBrowseLoadArchive_Click);
            // 
            // radioButtonText
            // 
            this.radioButtonText.AutoSize = true;
            this.radioButtonText.Checked = true;
            this.radioButtonText.Location = new System.Drawing.Point(13, 13);
            this.radioButtonText.Name = "radioButtonText";
            this.radioButtonText.Size = new System.Drawing.Size(46, 17);
            this.radioButtonText.TabIndex = 3;
            this.radioButtonText.TabStop = true;
            this.radioButtonText.Text = "Text";
            this.radioButtonText.UseVisualStyleBackColor = true;
            this.radioButtonText.CheckedChanged += new System.EventHandler(this.radioButtonText_CheckedChanged);
            // 
            // radioButtonDecode
            // 
            this.radioButtonDecode.AutoSize = true;
            this.radioButtonDecode.Location = new System.Drawing.Point(13, 92);
            this.radioButtonDecode.Name = "radioButtonDecode";
            this.radioButtonDecode.Size = new System.Drawing.Size(63, 17);
            this.radioButtonDecode.TabIndex = 4;
            this.radioButtonDecode.Text = "Decode";
            this.radioButtonDecode.UseVisualStyleBackColor = true;
            this.radioButtonDecode.CheckedChanged += new System.EventHandler(this.radioButtonDecode_CheckedChanged);
            // 
            // radioButtonSingleReq
            // 
            this.radioButtonSingleReq.AutoSize = true;
            this.radioButtonSingleReq.Location = new System.Drawing.Point(12, 152);
            this.radioButtonSingleReq.Name = "radioButtonSingleReq";
            this.radioButtonSingleReq.Size = new System.Drawing.Size(97, 17);
            this.radioButtonSingleReq.TabIndex = 5;
            this.radioButtonSingleReq.TabStop = true;
            this.radioButtonSingleReq.Text = "Single Request";
            this.radioButtonSingleReq.UseVisualStyleBackColor = true;
            this.radioButtonSingleReq.CheckedChanged += new System.EventHandler(this.radioButtonSingleReq_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(51, 172);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Request Path:";
            // 
            // textBoxSingleReqPath
            // 
            this.textBoxSingleReqPath.Location = new System.Drawing.Point(141, 169);
            this.textBoxSingleReqPath.Name = "textBoxSingleReqPath";
            this.textBoxSingleReqPath.Size = new System.Drawing.Size(380, 20);
            this.textBoxSingleReqPath.TabIndex = 1;
            // 
            // buttonBrowseSingleReq
            // 
            this.buttonBrowseSingleReq.Location = new System.Drawing.Point(527, 167);
            this.buttonBrowseSingleReq.Name = "buttonBrowseSingleReq";
            this.buttonBrowseSingleReq.Size = new System.Drawing.Size(26, 23);
            this.buttonBrowseSingleReq.TabIndex = 2;
            this.buttonBrowseSingleReq.Text = "...";
            this.buttonBrowseSingleReq.UseVisualStyleBackColor = true;
            this.buttonBrowseSingleReq.Click += new System.EventHandler(this.buttonBrowseLoadSingleReq_Click);
            // 
            // radioButtonResults
            // 
            this.radioButtonResults.AutoSize = true;
            this.radioButtonResults.Location = new System.Drawing.Point(12, 207);
            this.radioButtonResults.Name = "radioButtonResults";
            this.radioButtonResults.Size = new System.Drawing.Size(100, 17);
            this.radioButtonResults.TabIndex = 6;
            this.radioButtonResults.TabStop = true;
            this.radioButtonResults.Text = "Analyze Results";
            this.radioButtonResults.UseVisualStyleBackColor = true;
            this.radioButtonResults.CheckedChanged += new System.EventHandler(this.radioButtonResults_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(51, 227);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Results Path:";
            // 
            // textBoxResultsPath
            // 
            this.textBoxResultsPath.Location = new System.Drawing.Point(141, 224);
            this.textBoxResultsPath.Name = "textBoxResultsPath";
            this.textBoxResultsPath.Size = new System.Drawing.Size(380, 20);
            this.textBoxResultsPath.TabIndex = 1;
            // 
            // buttonResultsPath
            // 
            this.buttonResultsPath.Location = new System.Drawing.Point(527, 222);
            this.buttonResultsPath.Name = "buttonResultsPath";
            this.buttonResultsPath.Size = new System.Drawing.Size(26, 23);
            this.buttonResultsPath.TabIndex = 2;
            this.buttonResultsPath.Text = "...";
            this.buttonResultsPath.UseVisualStyleBackColor = true;
            this.buttonResultsPath.Click += new System.EventHandler(this.buttonResultsPath_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(217, 367);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(565, 402);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.radioButtonResults);
            this.Controls.Add(this.radioButtonSingleReq);
            this.Controls.Add(this.radioButtonDecode);
            this.Controls.Add(this.radioButtonText);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonClose);
            this.Controls.Add(this.buttonBrowseSave);
            this.Controls.Add(this.buttonResultsPath);
            this.Controls.Add(this.buttonBrowseSingleReq);
            this.Controls.Add(this.buttonBrowseArchive);
            this.Controls.Add(this.buttonBrowseUP);
            this.Controls.Add(this.buttonBrowseLoad);
            this.Controls.Add(this.textBoxResultsPath);
            this.Controls.Add(this.textBoxSingleReqPath);
            this.Controls.Add(this.textBoxSave);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxArchive);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBoxLoaddUP);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxLoad);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxLoad;
        private System.Windows.Forms.Button buttonBrowseLoad;
        private System.Windows.Forms.Button buttonBrowseSave;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxSave;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxLoaddUP;
        private System.Windows.Forms.Button buttonBrowseUP;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxArchive;
        private System.Windows.Forms.Button buttonBrowseArchive;
        private System.Windows.Forms.RadioButton radioButtonText;
        private System.Windows.Forms.RadioButton radioButtonDecode;
        private System.Windows.Forms.RadioButton radioButtonSingleReq;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBoxSingleReqPath;
        private System.Windows.Forms.Button buttonBrowseSingleReq;
        private System.Windows.Forms.RadioButton radioButtonResults;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxResultsPath;
        private System.Windows.Forms.Button buttonResultsPath;
        private System.Windows.Forms.Button button1;
    }
}

