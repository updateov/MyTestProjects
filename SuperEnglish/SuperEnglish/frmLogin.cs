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
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            ShowMDIChildForm(new frmMenu(),this);
            
            //((frmMain)Common.Parent).ShowMDIChildForm(new frmMenu());
            //this.Close();
        }

        private void ShowMDIChildForm(Form child, Form curr)
        {
            curr.Close();
            child.FormBorderStyle = FormBorderStyle.None;
            child.WindowState = FormWindowState.Normal;
            child.ControlBox = false;
            child.MdiParent = Common.Parent;
            Common.Current = child;
            child.Dock = DockStyle.Fill;
            child.Show();
        }
    }
}
