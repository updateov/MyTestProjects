using MainEntry;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UseUnmanagedNuget
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainEntryClass obj = new MainEntryClass();
            String text = textBox1.Text;
            int num;
            if (!Int32.TryParse(text, out num))
                num = -1;

            int toShow = obj.GetNum(num);
            labelResult.Text = toShow.ToString();
        }
    }
}
