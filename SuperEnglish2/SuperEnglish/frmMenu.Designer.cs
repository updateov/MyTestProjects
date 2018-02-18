namespace SuperEnglish
{
    partial class frmMenu
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
            this.pb_words = new System.Windows.Forms.PictureBox();
            this.pb_reading = new System.Windows.Forms.PictureBox();
            this.pb_listen = new System.Windows.Forms.PictureBox();
            this.pb_exam = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pb_words)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_reading)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_listen)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_exam)).BeginInit();
            this.SuspendLayout();
            // 
            // pb_words
            // 
            this.pb_words.BackColor = System.Drawing.Color.Transparent;
            this.pb_words.Image = global::SuperEnglish.Properties.Resources.words;
            this.pb_words.Location = new System.Drawing.Point(592, 76);
            this.pb_words.Name = "pb_words";
            this.pb_words.Size = new System.Drawing.Size(250, 250);
            this.pb_words.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pb_words.TabIndex = 0;
            this.pb_words.TabStop = false;
            this.pb_words.Click += new System.EventHandler(this.pb_words_Click);
            // 
            // pb_reading
            // 
            this.pb_reading.BackColor = System.Drawing.Color.Transparent;
            this.pb_reading.Image = global::SuperEnglish.Properties.Resources.reading;
            this.pb_reading.Location = new System.Drawing.Point(592, 325);
            this.pb_reading.Name = "pb_reading";
            this.pb_reading.Size = new System.Drawing.Size(250, 250);
            this.pb_reading.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pb_reading.TabIndex = 1;
            this.pb_reading.TabStop = false;
            // 
            // pb_listen
            // 
            this.pb_listen.BackColor = System.Drawing.Color.Transparent;
            this.pb_listen.Image = global::SuperEnglish.Properties.Resources.listen;
            this.pb_listen.Location = new System.Drawing.Point(64, 76);
            this.pb_listen.Name = "pb_listen";
            this.pb_listen.Size = new System.Drawing.Size(250, 250);
            this.pb_listen.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pb_listen.TabIndex = 2;
            this.pb_listen.TabStop = false;
            // 
            // pb_exam
            // 
            this.pb_exam.BackColor = System.Drawing.Color.Transparent;
            this.pb_exam.Image = global::SuperEnglish.Properties.Resources.exam;
            this.pb_exam.Location = new System.Drawing.Point(64, 325);
            this.pb_exam.Name = "pb_exam";
            this.pb_exam.Size = new System.Drawing.Size(250, 250);
            this.pb_exam.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pb_exam.TabIndex = 3;
            this.pb_exam.TabStop = false;
            // 
            // frmMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::SuperEnglish.Properties.Resources.bgimage;
            this.ClientSize = new System.Drawing.Size(900, 580);
            this.Controls.Add(this.pb_exam);
            this.Controls.Add(this.pb_listen);
            this.Controls.Add(this.pb_reading);
            this.Controls.Add(this.pb_words);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "frmMenu";
            this.Text = "frmMenu";
            ((System.ComponentModel.ISupportInitialize)(this.pb_words)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_reading)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_listen)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_exam)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pb_words;
        private System.Windows.Forms.PictureBox pb_reading;
        private System.Windows.Forms.PictureBox pb_listen;
        private System.Windows.Forms.PictureBox pb_exam;

    }
}