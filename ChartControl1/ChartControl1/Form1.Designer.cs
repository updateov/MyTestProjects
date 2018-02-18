namespace ChartControl1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.buttonStartStop = new System.Windows.Forms.Button();
            this.chartCtrl1 = new ChartControl1.ChartCtrl();
            this.SuspendLayout();
            // 
            // buttonStartStop
            // 
            this.buttonStartStop.Location = new System.Drawing.Point(735, 432);
            this.buttonStartStop.Name = "buttonStartStop";
            this.buttonStartStop.Size = new System.Drawing.Size(75, 23);
            this.buttonStartStop.TabIndex = 2;
            this.buttonStartStop.Text = "Start";
            this.buttonStartStop.UseVisualStyleBackColor = true;
            this.buttonStartStop.Click += new System.EventHandler(this.buttonStartStop_Click);
            // 
            // chartCtrl1
            // 
            this.chartCtrl1.AbsoluteStart = ((long)(1458464248));
            this.chartCtrl1.AbsoluteStartTime = new System.DateTime(2016, 3, 20, 8, 57, 28, 901);
            this.chartCtrl1.BackColor = System.Drawing.Color.White;
            this.chartCtrl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.chartCtrl1.Location = new System.Drawing.Point(0, 0);
            this.chartCtrl1.MinimumSize = new System.Drawing.Size(700, 0);
            this.chartCtrl1.Name = "chartCtrl1";
            this.chartCtrl1.Size = new System.Drawing.Size(822, 426);
            this.chartCtrl1.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(822, 467);
            this.Controls.Add(this.chartCtrl1);
            this.Controls.Add(this.buttonStartStop);
            this.MinimumSize = new System.Drawing.Size(727, 39);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private ChartCtrl chartCtrl1;
        private System.Windows.Forms.Button buttonStartStop;
    }
}

