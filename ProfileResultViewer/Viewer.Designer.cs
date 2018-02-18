namespace ProfileResultViewer
{
    partial class Viewer
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
            this.treeView = new System.Windows.Forms.TreeView();
            this.btnOpen = new System.Windows.Forms.Button();
            this.radioBtnCallers = new System.Windows.Forms.RadioButton();
            this.radioBtnCallees = new System.Windows.Forms.RadioButton();
            this.radioBtnRoots = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView.Location = new System.Drawing.Point(12, 12);
            this.treeView.Name = "treeView";
            this.treeView.Size = new System.Drawing.Size(538, 233);
            this.treeView.TabIndex = 0;
            this.treeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeView_BeforeExpand);
            // 
            // btnOpen
            // 
            this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpen.Location = new System.Drawing.Point(487, 252);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(62, 23);
            this.btnOpen.TabIndex = 1;
            this.btnOpen.Text = "Open...";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // radioBtnCallers
            // 
            this.radioBtnCallers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioBtnCallers.AutoSize = true;
            this.radioBtnCallers.Location = new System.Drawing.Point(13, 252);
            this.radioBtnCallers.Name = "radioBtnCallers";
            this.radioBtnCallers.Size = new System.Drawing.Size(56, 17);
            this.radioBtnCallers.TabIndex = 2;
            this.radioBtnCallers.Text = "Callers";
            this.radioBtnCallers.UseVisualStyleBackColor = true;
            this.radioBtnCallers.CheckedChanged += new System.EventHandler(this.radioBtn_CheckedChanged);
            // 
            // radioBtnCallees
            // 
            this.radioBtnCallees.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioBtnCallees.AutoSize = true;
            this.radioBtnCallees.Location = new System.Drawing.Point(75, 252);
            this.radioBtnCallees.Name = "radioBtnCallees";
            this.radioBtnCallees.Size = new System.Drawing.Size(59, 17);
            this.radioBtnCallees.TabIndex = 3;
            this.radioBtnCallees.Text = "Callees";
            this.radioBtnCallees.UseVisualStyleBackColor = true;
            this.radioBtnCallees.CheckedChanged += new System.EventHandler(this.radioBtn_CheckedChanged);
            // 
            // radioBtnRoots
            // 
            this.radioBtnRoots.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioBtnRoots.AutoSize = true;
            this.radioBtnRoots.Checked = true;
            this.radioBtnRoots.Location = new System.Drawing.Point(140, 252);
            this.radioBtnRoots.Name = "radioBtnRoots";
            this.radioBtnRoots.Size = new System.Drawing.Size(53, 17);
            this.radioBtnRoots.TabIndex = 4;
            this.radioBtnRoots.TabStop = true;
            this.radioBtnRoots.Text = "Roots";
            this.radioBtnRoots.UseVisualStyleBackColor = true;
            this.radioBtnRoots.CheckedChanged += new System.EventHandler(this.radioBtn_CheckedChanged);
            // 
            // Viewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(562, 279);
            this.Controls.Add(this.radioBtnRoots);
            this.Controls.Add(this.radioBtnCallees);
            this.Controls.Add(this.radioBtnCallers);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.treeView);
            this.Name = "Viewer";
            this.Text = "Profile Result Viewer";
            this.Shown += new System.EventHandler(this.Viewer_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.RadioButton radioBtnCallers;
        private System.Windows.Forms.RadioButton radioBtnCallees;
        private System.Windows.Forms.RadioButton radioBtnRoots;
    }
}

