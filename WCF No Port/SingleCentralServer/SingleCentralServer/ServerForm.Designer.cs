namespace SingleCentralServer
{
    partial class ServerForm
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
            this.clients_lb = new System.Windows.Forms.ListBox();
            this.broadcast_btn = new System.Windows.Forms.Button();
            this.textToSendToClient_tb = new System.Windows.Forms.TextBox();
            this.selectClientSend_btn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.anon_tb = new System.Windows.Forms.TextBox();
            this.unregisterClient_btn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // clients_lb
            // 
            this.clients_lb.FormattingEnabled = true;
            this.clients_lb.Location = new System.Drawing.Point(12, 12);
            this.clients_lb.Name = "clients_lb";
            this.clients_lb.Size = new System.Drawing.Size(480, 212);
            this.clients_lb.TabIndex = 0;
            // 
            // broadcast_btn
            // 
            this.broadcast_btn.Location = new System.Drawing.Point(118, 261);
            this.broadcast_btn.Name = "broadcast_btn";
            this.broadcast_btn.Size = new System.Drawing.Size(75, 23);
            this.broadcast_btn.TabIndex = 1;
            this.broadcast_btn.Text = "Broadcast";
            this.broadcast_btn.UseVisualStyleBackColor = true;
            this.broadcast_btn.Click += new System.EventHandler(this.broadcast_btn_Click);
            // 
            // textToSendToClient_tb
            // 
            this.textToSendToClient_tb.Location = new System.Drawing.Point(12, 263);
            this.textToSendToClient_tb.Name = "textToSendToClient_tb";
            this.textToSendToClient_tb.Size = new System.Drawing.Size(100, 20);
            this.textToSendToClient_tb.TabIndex = 2;
            // 
            // selectClientSend_btn
            // 
            this.selectClientSend_btn.Location = new System.Drawing.Point(213, 261);
            this.selectClientSend_btn.Name = "selectClientSend_btn";
            this.selectClientSend_btn.Size = new System.Drawing.Size(215, 23);
            this.selectClientSend_btn.TabIndex = 3;
            this.selectClientSend_btn.Text = "Send to selected client";
            this.selectClientSend_btn.UseVisualStyleBackColor = true;
            this.selectClientSend_btn.Click += new System.EventHandler(this.selectClientSend_btn_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(297, 305);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(34, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "anon:";
            // 
            // anon_tb
            // 
            this.anon_tb.Location = new System.Drawing.Point(337, 302);
            this.anon_tb.Name = "anon_tb";
            this.anon_tb.Size = new System.Drawing.Size(155, 20);
            this.anon_tb.TabIndex = 2;
            // 
            // unregisterClient_btn
            // 
            this.unregisterClient_btn.Location = new System.Drawing.Point(277, 232);
            this.unregisterClient_btn.Name = "unregisterClient_btn";
            this.unregisterClient_btn.Size = new System.Drawing.Size(215, 23);
            this.unregisterClient_btn.TabIndex = 3;
            this.unregisterClient_btn.Text = "Unregister selected client";
            this.unregisterClient_btn.UseVisualStyleBackColor = true;
            this.unregisterClient_btn.Click += new System.EventHandler(this.unregisterClient_btn_Click);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(504, 332);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.unregisterClient_btn);
            this.Controls.Add(this.selectClientSend_btn);
            this.Controls.Add(this.anon_tb);
            this.Controls.Add(this.textToSendToClient_tb);
            this.Controls.Add(this.broadcast_btn);
            this.Controls.Add(this.clients_lb);
            this.Name = "ServerForm";
            this.Text = "Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox clients_lb;
        private System.Windows.Forms.Button broadcast_btn;
        private System.Windows.Forms.TextBox textToSendToClient_tb;
        private System.Windows.Forms.Button selectClientSend_btn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox anon_tb;
        private System.Windows.Forms.Button unregisterClient_btn;
    }
}

