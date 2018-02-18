using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ListViewTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listBox1.DataSource = DS;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DS.Add(n++);
            //listBox1.DataSource = null;
            //listBox1.Items.Clear();
            //listBox1.DataSource = DS;
        }

        private int n = 0;
        public BindingList<int> DS = new BindingList<int>();
    }
}
