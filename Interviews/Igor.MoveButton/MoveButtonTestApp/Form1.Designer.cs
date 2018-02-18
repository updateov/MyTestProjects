namespace MoveButtonTestApp
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
            this._btnDown = new MoveButtonTestApp.BtnArrow();
            this._btnRight = new MoveButtonTestApp.BtnArrow();
            this._btnUp = new MoveButtonTestApp.BtnArrow();
            this._btnLeft = new MoveButtonTestApp.BtnArrow();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // _btnDown
            // 
            this._btnDown.AutoSize = true;
            this._btnDown.Direction = MoveButtonTestApp.BtnDirection.Left;
            this._btnDown.Location = new System.Drawing.Point(88, 144);
            this._btnDown.Margin = new System.Windows.Forms.Padding(0);
            this._btnDown.MinimumSize = new System.Drawing.Size(50, 50);
            this._btnDown.Name = "_btnDown";
            this._btnDown.Size = new System.Drawing.Size(50, 50);
            this._btnDown.TabIndex = 3;
            this._btnDown.Click += new System.EventHandler(this._btn_Click);
            // 
            // _btnRight
            // 
            this._btnRight.AutoSize = true;
            this._btnRight.Direction = MoveButtonTestApp.BtnDirection.Left;
            this._btnRight.Location = new System.Drawing.Point(142, 90);
            this._btnRight.Margin = new System.Windows.Forms.Padding(0);
            this._btnRight.MinimumSize = new System.Drawing.Size(50, 50);
            this._btnRight.Name = "_btnRight";
            this._btnRight.Size = new System.Drawing.Size(50, 50);
            this._btnRight.TabIndex = 2;
            this._btnRight.Click += new System.EventHandler(this._btn_Click);
            // 
            // _btnUp
            // 
            this._btnUp.AutoSize = true;
            this._btnUp.Direction = MoveButtonTestApp.BtnDirection.Left;
            this._btnUp.Location = new System.Drawing.Point(88, 36);
            this._btnUp.Margin = new System.Windows.Forms.Padding(0);
            this._btnUp.MinimumSize = new System.Drawing.Size(50, 50);
            this._btnUp.Name = "_btnUp";
            this._btnUp.Size = new System.Drawing.Size(50, 50);
            this._btnUp.TabIndex = 1;
            this._btnUp.Click += new System.EventHandler(this._btn_Click);
            // 
            // _btnLeft
            // 
            this._btnLeft.AutoSize = true;
            this._btnLeft.Direction = MoveButtonTestApp.BtnDirection.Left;
            this._btnLeft.Location = new System.Drawing.Point(33, 90);
            this._btnLeft.Margin = new System.Windows.Forms.Padding(0);
            this._btnLeft.MinimumSize = new System.Drawing.Size(50, 50);
            this._btnLeft.Name = "_btnLeft";
            this._btnLeft.Size = new System.Drawing.Size(50, 50);
            this._btnLeft.TabIndex = 0;
            this._btnLeft.Click += new System.EventHandler(this._btn_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 217);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(268, 212);
            this.listBox1.TabIndex = 4;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 454);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this._btnDown);
            this.Controls.Add(this._btnRight);
            this.Controls.Add(this._btnUp);
            this.Controls.Add(this._btnLeft);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Form1_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private BtnArrow _btnLeft;
        private BtnArrow _btnUp;
        private BtnArrow _btnRight;
        private BtnArrow _btnDown;
        private System.Windows.Forms.ListBox listBox1;
    }
}

