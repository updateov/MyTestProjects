using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Base64ToByteArray
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            Byte[] bytes = Convert.FromBase64String(textBoxBase64.Text);
            String res = String.Empty;
            listBoxBytes.Items.Clear();
            for (int i = 0; i < bytes.Length; i++ )
            {
                if (bytes[i] == 255)
                    listBoxBytes.Items.Add(bytes[i].ToString() + " is in position " + i.ToString());
            }

            labelSamples.Text = "Total samples: " + bytes.Length;
            labelTotal.Text = "Found " + listBoxBytes.Items.Count + " samples of 255's in the request.";
        }

    }
}
