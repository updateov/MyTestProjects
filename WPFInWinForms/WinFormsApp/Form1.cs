using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinFormsApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            userControl11.UpdateTest(textBoxToInsert.Text + "BLABLABLA!!!");
        }

        private void textBoxToInsert_TextChanged(object sender, EventArgs e)
        {
            userControl11.UpdateTest(textBoxToInsert.Text);
        }
    }
}
