namespace PatternsAddOnWinformTestApp
{
    partial class RequestCtrl
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
            this.buttonSend = new System.Windows.Forms.Button();
            this.richTextBoxData = new System.Windows.Forms.RichTextBox();
            this.groupBoxRequest = new System.Windows.Forms.GroupBox();
            this.groupBoxResponse = new System.Windows.Forms.GroupBox();
            this.labelStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxResponse = new System.Windows.Forms.TextBox();
            this.groupBoxRequest.SuspendLayout();
            this.groupBoxResponse.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonSend
            // 
            this.buttonSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSend.Location = new System.Drawing.Point(567, 187);
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new System.Drawing.Size(75, 23);
            this.buttonSend.TabIndex = 0;
            this.buttonSend.Text = "Send";
            this.buttonSend.UseVisualStyleBackColor = true;
            this.buttonSend.Click += new System.EventHandler(this.buttonSend_Click);
            // 
            // richTextBoxData
            // 
            this.richTextBoxData.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBoxData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBoxData.Location = new System.Drawing.Point(6, 19);
            this.richTextBoxData.Name = "richTextBoxData";
            this.richTextBoxData.Size = new System.Drawing.Size(636, 162);
            this.richTextBoxData.TabIndex = 1;
            this.richTextBoxData.Text = "";
            // 
            // groupBoxRequest
            // 
            this.groupBoxRequest.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxRequest.Controls.Add(this.richTextBoxData);
            this.groupBoxRequest.Controls.Add(this.buttonSend);
            this.groupBoxRequest.Location = new System.Drawing.Point(3, 3);
            this.groupBoxRequest.Name = "groupBoxRequest";
            this.groupBoxRequest.Size = new System.Drawing.Size(648, 216);
            this.groupBoxRequest.TabIndex = 2;
            this.groupBoxRequest.TabStop = false;
            this.groupBoxRequest.Text = "Request";
            // 
            // groupBoxResponse
            // 
            this.groupBoxResponse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxResponse.Controls.Add(this.labelStatus);
            this.groupBoxResponse.Controls.Add(this.label1);
            this.groupBoxResponse.Controls.Add(this.textBoxResponse);
            this.groupBoxResponse.Location = new System.Drawing.Point(4, 225);
            this.groupBoxResponse.Name = "groupBoxResponse";
            this.groupBoxResponse.Size = new System.Drawing.Size(647, 164);
            this.groupBoxResponse.TabIndex = 3;
            this.groupBoxResponse.TabStop = false;
            this.groupBoxResponse.Text = "Response";
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelStatus.Location = new System.Drawing.Point(54, 20);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(11, 13);
            this.labelStatus.TabIndex = 2;
            this.labelStatus.Text = " ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Status:";
            // 
            // textBoxResponse
            // 
            this.textBoxResponse.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxResponse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxResponse.Enabled = false;
            this.textBoxResponse.Location = new System.Drawing.Point(6, 36);
            this.textBoxResponse.Multiline = true;
            this.textBoxResponse.Name = "textBoxResponse";
            this.textBoxResponse.Size = new System.Drawing.Size(635, 122);
            this.textBoxResponse.TabIndex = 0;
            // 
            // RequestCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBoxResponse);
            this.Controls.Add(this.groupBoxRequest);
            this.Name = "RequestCtrl";
            this.Size = new System.Drawing.Size(654, 392);
            this.groupBoxRequest.ResumeLayout(false);
            this.groupBoxResponse.ResumeLayout(false);
            this.groupBoxResponse.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonSend;
        private System.Windows.Forms.RichTextBox richTextBoxData;
        private System.Windows.Forms.GroupBox groupBoxRequest;
        private System.Windows.Forms.GroupBox groupBoxResponse;
        private System.Windows.Forms.TextBox textBoxResponse;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label label1;
    }
}
