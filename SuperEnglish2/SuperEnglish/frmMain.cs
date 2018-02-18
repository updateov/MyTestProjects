using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SuperEnglish
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            ShowMDIChildForm(new frmLogin());
        }

        private void ShowMDIChildForm(Form child)
        {
            child.FormBorderStyle = FormBorderStyle.None;
            child.WindowState = FormWindowState.Normal;
            child.ControlBox = false;
            child.MdiParent = this;
            Common.Parent = this;
            Common.Current = child;
            child.Dock = DockStyle.Fill;
            child.Show();
        }
    }
}
