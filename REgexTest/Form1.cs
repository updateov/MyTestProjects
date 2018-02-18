using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace REgexTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBoxTest_TextChanged(object sender, EventArgs e)
        {
            String pattern = @"^[A-Z0-9]+(_[A-Z0-9]+)*$";
            var rgx = new Regex(pattern);
            if (rgx.IsMatch(textBoxTest.Text))
                textBoxValid.Text = "Valid";
            else
                textBoxValid.Text = "INVALID";
        }
    }
}
