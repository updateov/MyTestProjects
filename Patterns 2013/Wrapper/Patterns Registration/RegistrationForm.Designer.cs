namespace PeriGen.Patterns.Engine.Registration
{
	partial class RegistrationForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RegistrationForm));
			this.label1 = new System.Windows.Forms.Label();
			this.txtSerialID = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtActivationCode = new System.Windows.Forms.TextBox();
			this.btnRegister = new System.Windows.Forms.Button();
			this.imageList = new System.Windows.Forms.ImageList(this.components);
			this.panel1 = new System.Windows.Forms.Panel();
			this.lblLicenseInformation = new System.Windows.Forms.Label();
			this.btnClear = new System.Windows.Forms.Button();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(50, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Serial ID:";
			// 
			// txtSerialID
			// 
			this.txtSerialID.BackColor = System.Drawing.SystemColors.Control;
			this.txtSerialID.Location = new System.Drawing.Point(127, 6);
			this.txtSerialID.Name = "txtSerialID";
			this.txtSerialID.ReadOnly = true;
			this.txtSerialID.Size = new System.Drawing.Size(533, 20);
			this.txtSerialID.TabIndex = 0;
			this.txtSerialID.TabStop = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 36);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(85, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Activation Code:";
			// 
			// txtActivationCode
			// 
			this.txtActivationCode.BackColor = System.Drawing.SystemColors.Info;
			this.txtActivationCode.Location = new System.Drawing.Point(127, 32);
			this.txtActivationCode.Name = "txtActivationCode";
			this.txtActivationCode.Size = new System.Drawing.Size(533, 20);
			this.txtActivationCode.TabIndex = 1;
			// 
			// btnRegister
			// 
			this.btnRegister.ImageKey = "check.png";
			this.btnRegister.ImageList = this.imageList;
			this.btnRegister.Location = new System.Drawing.Point(326, 71);
			this.btnRegister.Name = "btnRegister";
			this.btnRegister.Size = new System.Drawing.Size(164, 57);
			this.btnRegister.TabIndex = 2;
			this.btnRegister.Text = "Register";
			this.btnRegister.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.btnRegister.UseVisualStyleBackColor = true;
			this.btnRegister.Click += new System.EventHandler(this.btnRegister_Click);
			// 
			// imageList
			// 
			this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
			this.imageList.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList.Images.SetKeyName(0, "check.png");
			this.imageList.Images.SetKeyName(1, "uncheck.png");
			this.imageList.Images.SetKeyName(2, "demo.png");
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Controls.Add(this.lblLicenseInformation);
			this.panel1.Location = new System.Drawing.Point(12, 71);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(298, 57);
			this.panel1.TabIndex = 6;
			// 
			// lblLicenseInformation
			// 
			this.lblLicenseInformation.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblLicenseInformation.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lblLicenseInformation.ImageKey = "uncheck.png";
			this.lblLicenseInformation.ImageList = this.imageList;
			this.lblLicenseInformation.Location = new System.Drawing.Point(0, 0);
			this.lblLicenseInformation.Name = "lblLicenseInformation";
			this.lblLicenseInformation.Size = new System.Drawing.Size(296, 55);
			this.lblLicenseInformation.TabIndex = 6;
			this.lblLicenseInformation.Text = "Current license information";
			this.lblLicenseInformation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// btnClear
			// 
			this.btnClear.ImageKey = "demo.png";
			this.btnClear.ImageList = this.imageList;
			this.btnClear.Location = new System.Drawing.Point(496, 72);
			this.btnClear.Name = "btnClear";
			this.btnClear.Size = new System.Drawing.Size(164, 57);
			this.btnClear.TabIndex = 7;
			this.btnClear.Text = "Demo mode";
			this.btnClear.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this.btnClear.UseVisualStyleBackColor = true;
			this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
			// 
			// RegistrationForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(670, 140);
			this.Controls.Add(this.btnClear);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.btnRegister);
			this.Controls.Add(this.txtActivationCode);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtSerialID);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "RegistrationForm";
			this.Text = "PeriCALM® Patterns™ Engine Registration";
			this.Load += new System.EventHandler(this.RegistrationForm_Load);
			this.panel1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtSerialID;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtActivationCode;
		private System.Windows.Forms.Button btnRegister;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label lblLicenseInformation;
		private System.Windows.Forms.ImageList imageList;
		private System.Windows.Forms.Button btnClear;

	}
}

