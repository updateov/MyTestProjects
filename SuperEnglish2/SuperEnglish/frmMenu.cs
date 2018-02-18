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
    public partial class frmMenu : Form
    {
        public frmMenu()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        private void pb_words_Click(object sender, EventArgs e)
        {
            ShowMDIChildForm(new frmWordComp(),this);
            //this.Close();
            //((frmMain)Common.Parent).ShowMDIChildForm(new frmWordComp());
            
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
