namespace PeriGen.Patterns.Settings.Tool
{
	partial class frmEdit
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmEdit));
			this.grpValues = new System.Windows.Forms.GroupBox();
			this.numEditor = new System.Windows.Forms.NumericUpDown();
			this.cmbBoolean = new System.Windows.Forms.ComboBox();
			this.lblCommentDetails = new System.Windows.Forms.Label();
			this.txtValueDetails = new System.Windows.Forms.TextBox();
			this.lblValueDetails = new System.Windows.Forms.Label();
			this.btnReset = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.grpValues.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numEditor)).BeginInit();
			this.SuspendLayout();
			// 
			// grpValues
			// 
			this.grpValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.grpValues.Controls.Add(this.numEditor);
			this.grpValues.Controls.Add(this.cmbBoolean);
			this.grpValues.Controls.Add(this.lblCommentDetails);
			this.grpValues.Controls.Add(this.txtValueDetails);
			this.grpValues.Controls.Add(this.lblValueDetails);
			this.grpValues.Location = new System.Drawing.Point(12, 12);
			this.grpValues.Name = "grpValues";
			this.grpValues.Size = new System.Drawing.Size(400, 191);
			this.grpValues.TabIndex = 8;
			this.grpValues.TabStop = false;
			// 
			// numEditor
			// 
			this.numEditor.Font = new System.Drawing.Font("Consolas", 9F);
			this.numEditor.Location = new System.Drawing.Point(8, 32);
			this.numEditor.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.numEditor.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
			this.numEditor.Name = "numEditor";
			this.numEditor.Size = new System.Drawing.Size(385, 22);
			this.numEditor.TabIndex = 5;
			this.numEditor.Visible = false;
			// 
			// cmbBoolean
			// 
			this.cmbBoolean.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbBoolean.Font = new System.Drawing.Font("Consolas", 9F);
			this.cmbBoolean.FormattingEnabled = true;
			this.cmbBoolean.Location = new System.Drawing.Point(8, 32);
			this.cmbBoolean.Name = "cmbBoolean";
			this.cmbBoolean.Size = new System.Drawing.Size(385, 22);
			this.cmbBoolean.TabIndex = 4;
			this.cmbBoolean.Visible = false;
			// 
			// lblCommentDetails
			// 
			this.lblCommentDetails.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblCommentDetails.ForeColor = System.Drawing.Color.DarkSlateGray;
			this.lblCommentDetails.Location = new System.Drawing.Point(9, 62);
			this.lblCommentDetails.Margin = new System.Windows.Forms.Padding(0);
			this.lblCommentDetails.Name = "lblCommentDetails";
			this.lblCommentDetails.Size = new System.Drawing.Size(381, 114);
			this.lblCommentDetails.TabIndex = 3;
			this.lblCommentDetails.Text = "Comment";
			// 
			// txtValueDetails
			// 
			this.txtValueDetails.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtValueDetails.Location = new System.Drawing.Point(9, 32);
			this.txtValueDetails.Name = "txtValueDetails";
			this.txtValueDetails.Size = new System.Drawing.Size(381, 22);
			this.txtValueDetails.TabIndex = 1;
			this.txtValueDetails.Visible = false;
			// 
			// lblValueDetails
			// 
			this.lblValueDetails.AutoSize = true;
			this.lblValueDetails.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblValueDetails.Location = new System.Drawing.Point(6, 16);
			this.lblValueDetails.Name = "lblValueDetails";
			this.lblValueDetails.Size = new System.Drawing.Size(42, 14);
			this.lblValueDetails.TabIndex = 0;
			this.lblValueDetails.Text = "Value";
			// 
			// btnReset
			// 
			this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnReset.Location = new System.Drawing.Point(12, 209);
			this.btnReset.Name = "btnReset";
			this.btnReset.Size = new System.Drawing.Size(75, 25);
			this.btnReset.TabIndex = 9;
			this.btnReset.Text = "Reset";
			this.btnReset.UseVisualStyleBackColor = true;
			this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(256, 209);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 25);
			this.btnOk.TabIndex = 10;
			this.btnOk.Text = "Ok";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(337, 209);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 25);
			this.btnCancel.TabIndex = 11;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// frmEdit
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.AliceBlue;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(427, 241);
			this.ControlBox = false;
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnReset);
			this.Controls.Add(this.grpValues);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmEdit";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "PeriCALM® Patterns™ Settings Editor";
			this.grpValues.ResumeLayout(false);
			this.grpValues.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numEditor)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox grpValues;
		private System.Windows.Forms.Label lblCommentDetails;
		private System.Windows.Forms.TextBox txtValueDetails;
		private System.Windows.Forms.Label lblValueDetails;
		private System.Windows.Forms.Button btnReset;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.ComboBox cmbBoolean;
		private System.Windows.Forms.NumericUpDown numEditor;

	}
}