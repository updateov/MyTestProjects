namespace MainServerApplication
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
            this.listBoxClients = new System.Windows.Forms.ListBox();
            this.buttonCreateChild = new System.Windows.Forms.Button();
            this.buttonRemoveSelected = new System.Windows.Forms.Button();
            this.buttonRemoveAll = new System.Windows.Forms.Button();
            this.labelChildrenCount = new System.Windows.Forms.Label();
            this.textBoxStartIndex = new System.Windows.Forms.TextBox();
            this.textBoxLength = new System.Windows.Forms.TextBox();
            this.textBoxValue = new System.Windows.Forms.TextBox();
            this.buttonFill = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.listBoxResult = new System.Windows.Forms.ListBox();
            this.textBoxGetStartInd = new System.Windows.Forms.TextBox();
            this.textBoxGetLength = new System.Windows.Forms.TextBox();
            this.buttonGet = new System.Windows.Forms.Button();
            this.textBoxBuffer = new System.Windows.Forms.TextBox();
            this.buttonGetBuffer = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listBoxClients
            // 
            this.listBoxClients.FormattingEnabled = true;
            this.listBoxClients.Location = new System.Drawing.Point(13, 11);
            this.listBoxClients.Name = "listBoxClients";
            this.listBoxClients.Size = new System.Drawing.Size(238, 290);
            this.listBoxClients.TabIndex = 0;
            // 
            // buttonCreateChild
            // 
            this.buttonCreateChild.Location = new System.Drawing.Point(257, 12);
            this.buttonCreateChild.Name = "buttonCreateChild";
            this.buttonCreateChild.Size = new System.Drawing.Size(101, 23);
            this.buttonCreateChild.TabIndex = 1;
            this.buttonCreateChild.Text = "Create Child";
            this.buttonCreateChild.UseVisualStyleBackColor = true;
            this.buttonCreateChild.Click += new System.EventHandler(this.buttonCreateChild_Click);
            // 
            // buttonRemoveSelected
            // 
            this.buttonRemoveSelected.Location = new System.Drawing.Point(257, 41);
            this.buttonRemoveSelected.Name = "buttonRemoveSelected";
            this.buttonRemoveSelected.Size = new System.Drawing.Size(101, 23);
            this.buttonRemoveSelected.TabIndex = 2;
            this.buttonRemoveSelected.Text = "Remove Selected";
            this.buttonRemoveSelected.UseVisualStyleBackColor = true;
            this.buttonRemoveSelected.Click += new System.EventHandler(this.buttonRemoveSelected_Click);
            // 
            // buttonRemoveAll
            // 
            this.buttonRemoveAll.Location = new System.Drawing.Point(257, 70);
            this.buttonRemoveAll.Name = "buttonRemoveAll";
            this.buttonRemoveAll.Size = new System.Drawing.Size(101, 23);
            this.buttonRemoveAll.TabIndex = 2;
            this.buttonRemoveAll.Text = "Remove All";
            this.buttonRemoveAll.UseVisualStyleBackColor = true;
            this.buttonRemoveAll.Click += new System.EventHandler(this.buttonRemoveAll_Click);
            // 
            // labelChildrenCount
            // 
            this.labelChildrenCount.AutoSize = true;
            this.labelChildrenCount.Location = new System.Drawing.Point(13, 308);
            this.labelChildrenCount.Name = "labelChildrenCount";
            this.labelChildrenCount.Size = new System.Drawing.Size(0, 13);
            this.labelChildrenCount.TabIndex = 3;
            // 
            // textBoxStartIndex
            // 
            this.textBoxStartIndex.Location = new System.Drawing.Point(465, 14);
            this.textBoxStartIndex.Name = "textBoxStartIndex";
            this.textBoxStartIndex.Size = new System.Drawing.Size(160, 20);
            this.textBoxStartIndex.TabIndex = 4;
            // 
            // textBoxLength
            // 
            this.textBoxLength.Location = new System.Drawing.Point(465, 40);
            this.textBoxLength.Name = "textBoxLength";
            this.textBoxLength.Size = new System.Drawing.Size(160, 20);
            this.textBoxLength.TabIndex = 4;
            // 
            // textBoxValue
            // 
            this.textBoxValue.Location = new System.Drawing.Point(465, 66);
            this.textBoxValue.Name = "textBoxValue";
            this.textBoxValue.Size = new System.Drawing.Size(160, 20);
            this.textBoxValue.TabIndex = 4;
            // 
            // buttonFill
            // 
            this.buttonFill.Location = new System.Drawing.Point(550, 92);
            this.buttonFill.Name = "buttonFill";
            this.buttonFill.Size = new System.Drawing.Size(75, 23);
            this.buttonFill.TabIndex = 5;
            this.buttonFill.Text = "Fill";
            this.buttonFill.UseVisualStyleBackColor = true;
            this.buttonFill.Click += new System.EventHandler(this.buttonFill_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(398, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Start Index:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(398, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Length:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(398, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Value:";
            // 
            // listBoxResult
            // 
            this.listBoxResult.FormattingEnabled = true;
            this.listBoxResult.Location = new System.Drawing.Point(257, 125);
            this.listBoxResult.Name = "listBoxResult";
            this.listBoxResult.Size = new System.Drawing.Size(98, 95);
            this.listBoxResult.TabIndex = 7;
            // 
            // textBoxGetStartInd
            // 
            this.textBoxGetStartInd.Location = new System.Drawing.Point(257, 99);
            this.textBoxGetStartInd.Name = "textBoxGetStartInd";
            this.textBoxGetStartInd.Size = new System.Drawing.Size(44, 20);
            this.textBoxGetStartInd.TabIndex = 8;
            // 
            // textBoxGetLength
            // 
            this.textBoxGetLength.Location = new System.Drawing.Point(311, 99);
            this.textBoxGetLength.Name = "textBoxGetLength";
            this.textBoxGetLength.Size = new System.Drawing.Size(44, 20);
            this.textBoxGetLength.TabIndex = 8;
            // 
            // buttonGet
            // 
            this.buttonGet.Location = new System.Drawing.Point(412, 97);
            this.buttonGet.Name = "buttonGet";
            this.buttonGet.Size = new System.Drawing.Size(75, 23);
            this.buttonGet.TabIndex = 9;
            this.buttonGet.Text = "Get Values";
            this.buttonGet.UseVisualStyleBackColor = true;
            this.buttonGet.Click += new System.EventHandler(this.buttonGet_Click);
            // 
            // textBoxBuffer
            // 
            this.textBoxBuffer.Location = new System.Drawing.Point(389, 199);
            this.textBoxBuffer.Name = "textBoxBuffer";
            this.textBoxBuffer.Size = new System.Drawing.Size(151, 20);
            this.textBoxBuffer.TabIndex = 10;
            // 
            // buttonGetBuffer
            // 
            this.buttonGetBuffer.Location = new System.Drawing.Point(556, 197);
            this.buttonGetBuffer.Name = "buttonGetBuffer";
            this.buttonGetBuffer.Size = new System.Drawing.Size(75, 23);
            this.buttonGetBuffer.TabIndex = 11;
            this.buttonGetBuffer.Text = "Get Buffer";
            this.buttonGetBuffer.UseVisualStyleBackColor = true;
            this.buttonGetBuffer.Click += new System.EventHandler(this.buttonGetBuffer_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(643, 327);
            this.Controls.Add(this.buttonGetBuffer);
            this.Controls.Add(this.textBoxBuffer);
            this.Controls.Add(this.buttonGet);
            this.Controls.Add(this.textBoxGetLength);
            this.Controls.Add(this.textBoxGetStartInd);
            this.Controls.Add(this.listBoxResult);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonFill);
            this.Controls.Add(this.textBoxValue);
            this.Controls.Add(this.textBoxLength);
            this.Controls.Add(this.textBoxStartIndex);
            this.Controls.Add(this.labelChildrenCount);
            this.Controls.Add(this.buttonRemoveAll);
            this.Controls.Add(this.buttonRemoveSelected);
            this.Controls.Add(this.buttonCreateChild);
            this.Controls.Add(this.listBoxClients);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxClients;
        private System.Windows.Forms.Button buttonCreateChild;
        private System.Windows.Forms.Button buttonRemoveSelected;
        private System.Windows.Forms.Button buttonRemoveAll;
        private System.Windows.Forms.Label labelChildrenCount;
        private System.Windows.Forms.TextBox textBoxStartIndex;
        private System.Windows.Forms.TextBox textBoxLength;
        private System.Windows.Forms.TextBox textBoxValue;
        private System.Windows.Forms.Button buttonFill;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox listBoxResult;
        private System.Windows.Forms.TextBox textBoxGetStartInd;
        private System.Windows.Forms.TextBox textBoxGetLength;
        private System.Windows.Forms.Button buttonGet;
        private System.Windows.Forms.TextBox textBoxBuffer;
        private System.Windows.Forms.Button buttonGetBuffer;
    }
}

