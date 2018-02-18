using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace PDFSerializationTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonFrom_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.ShowDialog();
            textBoxFrom.Text = fd.FileName;
        }

        private void buttonTo_Click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.ShowDialog();
            textBoxTo.Text = fd.FileName;
        }

        private void buttonOut_Click(object sender, EventArgs e)
        {
            SaveFileDialog fd = new SaveFileDialog();
            fd.ShowDialog();
            textBoxOut.Text = fd.FileName;
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            var bytes = File.ReadAllBytes(textBoxFrom.Text);
            String bs64 = Convert.ToBase64String(bytes);
            var sr = new StringReader(bs64);
            //var contentElement = XElement.Load(sr);
            var xml = new XElement("data", bs64);
            xml.Save(textBoxTo.Text);
        }

        private void buttonDeserialize_Click(object sender, EventArgs e)
        {
            var xml = XElement.Load(textBoxTo.Text);
            var datas = from s in xml.Elements("data")
                        select s;

            var data = xml.Value;
            var bytes = Convert.FromBase64String(data);
            File.WriteAllBytes(textBoxOut.Text, bytes);
        }
    }
}
