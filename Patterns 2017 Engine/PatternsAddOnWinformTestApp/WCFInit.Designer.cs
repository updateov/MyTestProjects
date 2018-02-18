namespace PatternsAddOnWinformTestApp
{
    partial class WCFInit
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxHost = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listBoxToken = new System.Windows.Forms.ListBox();
            this.buttonInit = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.comboBoxRESTPath = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.listBoxValid = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxGA = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
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
            // textBoxHost
            // 
            this.textBoxHost.Location = new System.Drawing.Point(70, 19);
            this.textBoxHost.Name = "textBoxHost";
            this.textBoxHost.Size = new System.Drawing.Size(215, 20);
            this.textBoxHost.TabIndex = 0;
            this.textBoxHost.TextChanged += new System.EventHandler(this.textBoxHost_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 170);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Token ID:";
            // 
            // listBoxToken
            // 
            this.listBoxToken.FormattingEnabled = true;
            this.listBoxToken.IntegralHeight = false;
            this.listBoxToken.Location = new System.Drawing.Point(70, 165);
            this.listBoxToken.Name = "listBoxToken";
            this.listBoxToken.Size = new System.Drawing.Size(218, 20);
            this.listBoxToken.TabIndex = 0;
            // 
            // buttonInit
            // 
            this.buttonInit.Enabled = false;
            this.buttonInit.Location = new System.Drawing.Point(210, 124);
            this.buttonInit.Name = "buttonInit";
            this.buttonInit.Size = new System.Drawing.Size(75, 23);
            this.buttonInit.TabIndex = 3;
            this.buttonInit.Text = "Initialize";
            this.buttonInit.UseVisualStyleBackColor = true;
            this.buttonInit.Click += new System.EventHandler(this.buttonInit_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textBoxGA);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.comboBoxRESTPath);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.buttonInit);
            this.groupBox1.Controls.Add(this.textBoxPort);
            this.groupBox1.Controls.Add(this.textBoxHost);
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(291, 156);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Service Initialize";
            // 
            // comboBoxRESTPath
            // 
            this.comboBoxRESTPath.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxRESTPath.FormattingEnabled = true;
            this.comboBoxRESTPath.Items.AddRange(new object[] {
            "Patterns Add-On"});
            this.comboBoxRESTPath.Location = new System.Drawing.Point(70, 71);
            this.comboBoxRESTPath.Name = "comboBoxRESTPath";
            this.comboBoxRESTPath.Size = new System.Drawing.Size(215, 21);
            this.comboBoxRESTPath.TabIndex = 4;
            this.comboBoxRESTPath.SelectedIndexChanged += new System.EventHandler(this.comboBoxRESTPath_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 48);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Port:";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(70, 45);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(215, 20);
            this.textBoxPort.TabIndex = 1;
            this.textBoxPort.TextChanged += new System.EventHandler(this.textBoxHost_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 196);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Valid token:";
            // 
            // listBoxValid
            // 
            this.listBoxValid.FormattingEnabled = true;
            this.listBoxValid.IntegralHeight = false;
            this.listBoxValid.Location = new System.Drawing.Point(70, 191);
            this.listBoxValid.Name = "listBoxValid";
            this.listBoxValid.Size = new System.Drawing.Size(218, 20);
            this.listBoxValid.TabIndex = 0;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 101);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Gestational Age:";
            // 
            // textBoxGA
            // 
            this.textBoxGA.Location = new System.Drawing.Point(145, 98);
            this.textBoxGA.Name = "textBoxGA";
            this.textBoxGA.Size = new System.Drawing.Size(140, 20);
            this.textBoxGA.TabIndex = 6;
            this.textBoxGA.TextChanged += new System.EventHandler(this.textBoxGA_TextChanged);
            // 
            // WCFInit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listBoxValid);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.listBoxToken);
            this.Controls.Add(this.label2);
            this.Name = "WCFInit";
            this.Size = new System.Drawing.Size(588, 309);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxHost;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBoxToken;
        private System.Windows.Forms.Button buttonInit;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.ComboBox comboBoxRESTPath;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox listBoxValid;
        private System.Windows.Forms.TextBox textBoxGA;
        private System.Windows.Forms.Label label6;
    }
}
