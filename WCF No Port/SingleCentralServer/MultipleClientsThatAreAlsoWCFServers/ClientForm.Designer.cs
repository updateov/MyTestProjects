namespace MultipleClientsThatAreAlsoWCFServers
{
    partial class ClientForm
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
            this.Register_btn = new System.Windows.Forms.Button();
            this.textToSend_tb = new System.Windows.Forms.TextBox();
            this.SendText_btn = new System.Windows.Forms.Button();
            this.lastMessage_tb = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.sendAnonymously_btn = new System.Windows.Forms.Button();
            this.getLastAnon_btn = new System.Windows.Forms.Button();
            this.lastAnon_tb = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Register_btn
            // 
            this.Register_btn.Location = new System.Drawing.Point(12, 12);
            this.Register_btn.Name = "Register_btn";
            this.Register_btn.Size = new System.Drawing.Size(155, 23);
            this.Register_btn.TabIndex = 0;
            this.Register_btn.Text = "Register With Server";
            this.Register_btn.UseVisualStyleBackColor = true;
            this.Register_btn.Click += new System.EventHandler(this.Register_btn_Click);
            // 
            // textToSend_tb
            // 
            this.textToSend_tb.Location = new System.Drawing.Point(12, 41);
            this.textToSend_tb.Name = "textToSend_tb";
            this.textToSend_tb.Size = new System.Drawing.Size(155, 20);
            this.textToSend_tb.TabIndex = 1;
            // 
            // SendText_btn
            // 
            this.SendText_btn.Location = new System.Drawing.Point(173, 39);
            this.SendText_btn.Name = "SendText_btn";
            this.SendText_btn.Size = new System.Drawing.Size(123, 23);
            this.SendText_btn.TabIndex = 0;
            this.SendText_btn.Text = "Send Text to server";
            this.SendText_btn.UseVisualStyleBackColor = true;
            this.SendText_btn.Click += new System.EventHandler(this.SendText_btn_Click);
            // 
            // lastMessage_tb
            // 
            this.lastMessage_tb.Location = new System.Drawing.Point(151, 135);
            this.lastMessage_tb.Name = "lastMessage_tb";
            this.lastMessage_tb.Size = new System.Drawing.Size(184, 20);
            this.lastMessage_tb.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 138);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(133, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Last Message From Server";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(12, 193);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(167, 23);
            this.button3.TabIndex = 4;
            this.button3.Text = "Unregister (can\'t do this!)";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.unregister_Click);
            // 
            // sendAnonymously_btn
            // 
            this.sendAnonymously_btn.Location = new System.Drawing.Point(173, 68);
            this.sendAnonymously_btn.Name = "sendAnonymously_btn";
            this.sendAnonymously_btn.Size = new System.Drawing.Size(147, 23);
            this.sendAnonymously_btn.TabIndex = 0;
            this.sendAnonymously_btn.Text = "Send Text anonymously";
            this.sendAnonymously_btn.UseVisualStyleBackColor = true;
            this.sendAnonymously_btn.Click += new System.EventHandler(this.sendAnonymously_btn_Click);
            // 
            // getLastAnon_btn
            // 
            this.getLastAnon_btn.Location = new System.Drawing.Point(255, 209);
            this.getLastAnon_btn.Name = "getLastAnon_btn";
            this.getLastAnon_btn.Size = new System.Drawing.Size(127, 23);
            this.getLastAnon_btn.TabIndex = 5;
            this.getLastAnon_btn.Text = "GetLastAnonMessage";
            this.getLastAnon_btn.UseVisualStyleBackColor = true;
            this.getLastAnon_btn.Click += new System.EventHandler(this.getLastAnon_btn_Click);
            // 
            // lastAnon_tb
            // 
            this.lastAnon_tb.Location = new System.Drawing.Point(255, 238);
            this.lastAnon_tb.Name = "lastAnon_tb";
            this.lastAnon_tb.Size = new System.Drawing.Size(100, 20);
            this.lastAnon_tb.TabIndex = 6;
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 264);
            this.Controls.Add(this.lastAnon_tb);
            this.Controls.Add(this.getLastAnon_btn);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lastMessage_tb);
            this.Controls.Add(this.textToSend_tb);
            this.Controls.Add(this.sendAnonymously_btn);
            this.Controls.Add(this.SendText_btn);
            this.Controls.Add(this.Register_btn);
            this.Name = "ClientForm";
            this.Text = "Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Register_btn;
        private System.Windows.Forms.TextBox textToSend_tb;
        private System.Windows.Forms.Button SendText_btn;
        private System.Windows.Forms.TextBox lastMessage_tb;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button sendAnonymously_btn;
        private System.Windows.Forms.Button getLastAnon_btn;
        private System.Windows.Forms.TextBox lastAnon_tb;
    }
}

