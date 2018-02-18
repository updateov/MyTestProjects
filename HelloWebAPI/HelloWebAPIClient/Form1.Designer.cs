namespace HelloWebAPIClient
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
            this.textBoxURI = new System.Windows.Forms.TextBox();
            this.listBoxResults = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.buttonListAllProducts = new System.Windows.Forms.Button();
            this.button2Params = new System.Windows.Forms.Button();
            this.button2Ints = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Host:";
            // 
            // textBoxURI
            // 
            this.textBoxURI.Location = new System.Drawing.Point(15, 25);
            this.textBoxURI.Name = "textBoxURI";
            this.textBoxURI.Size = new System.Drawing.Size(243, 20);
            this.textBoxURI.TabIndex = 1;
            // 
            // listBoxResults
            // 
            this.listBoxResults.FormattingEnabled = true;
            this.listBoxResults.Location = new System.Drawing.Point(12, 117);
            this.listBoxResults.MultiColumn = true;
            this.listBoxResults.Name = "listBoxResults";
            this.listBoxResults.Size = new System.Drawing.Size(330, 134);
            this.listBoxResults.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 101);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Result;";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(270, 23);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "Connect";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // buttonListAllProducts
            // 
            this.buttonListAllProducts.Location = new System.Drawing.Point(15, 52);
            this.buttonListAllProducts.Name = "buttonListAllProducts";
            this.buttonListAllProducts.Size = new System.Drawing.Size(79, 23);
            this.buttonListAllProducts.TabIndex = 5;
            this.buttonListAllProducts.Text = "List Products";
            this.buttonListAllProducts.UseVisualStyleBackColor = true;
            this.buttonListAllProducts.Click += new System.EventHandler(this.buttonListAllProducts_Click);
            // 
            // button2Params
            // 
            this.button2Params.Location = new System.Drawing.Point(132, 51);
            this.button2Params.Name = "button2Params";
            this.button2Params.Size = new System.Drawing.Size(75, 23);
            this.button2Params.TabIndex = 6;
            this.button2Params.Text = "2 Params";
            this.button2Params.UseVisualStyleBackColor = true;
            this.button2Params.Click += new System.EventHandler(this.button2Params_Click);
            // 
            // button2Ints
            // 
            this.button2Ints.Location = new System.Drawing.Point(213, 51);
            this.button2Ints.Name = "button2Ints";
            this.button2Ints.Size = new System.Drawing.Size(75, 23);
            this.button2Ints.TabIndex = 6;
            this.button2Ints.Text = "2 Ints";
            this.button2Ints.UseVisualStyleBackColor = true;
            this.button2Ints.Click += new System.EventHandler(this.button2Ints_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 302);
            this.Controls.Add(this.button2Ints);
            this.Controls.Add(this.button2Params);
            this.Controls.Add(this.buttonListAllProducts);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.listBoxResults);
            this.Controls.Add(this.textBoxURI);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Web API Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxURI;
        private System.Windows.Forms.ListBox listBoxResults;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button buttonListAllProducts;
        private System.Windows.Forms.Button button2Params;
        private System.Windows.Forms.Button button2Ints;
    }
}

