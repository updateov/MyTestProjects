using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PatternsAddOnWinformTestApp
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Icon = Resource.app;
            var cores = Environment.ProcessorCount;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            wcfInitCtrl.MainForm = this;
            wcfRunnerCtrl.MainForm = this;
            wcfRunnerCtrl.Visible = false;
            wcfInitCtrl.Visible = true;
            wcfInitCtrl.Dock = DockStyle.Fill;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            wcfRunnerCtrl.StopTimer();
            Close();
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            if (buttonNext.Text.Equals("Next"))
            {
                wcfInitCtrl.Dock = DockStyle.None;
                wcfInitCtrl.Visible = false;
                wcfRunnerCtrl.Visible = true;
                wcfRunnerCtrl.Dock = DockStyle.Fill;
                buttonNext.Text = "Back";
            }
            else if (buttonNext.Text.Equals("Back"))
            {
                wcfRunnerCtrl.Visible = false;
                wcfRunnerCtrl.Dock = DockStyle.None;
                wcfInitCtrl.Dock = DockStyle.Fill;
                wcfInitCtrl.Visible = true;
                buttonNext.Text = "Next";
            }
        }

        public String CurrentToken { get { return wcfInitCtrl.TokenID; } }
        public String CurrentHost { get { return wcfInitCtrl.Host; } }
    }
}
