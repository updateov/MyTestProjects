using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SplitToFiles
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonBrowseSource_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "XML File (*.xml)|*.xml";
            dlg.ShowDialog();
            textBoxSource.Text = dlg.FileName;
        }

        private void buttonBrowseDest_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = "Select Output Folder";
            dlg.ShowDialog();
            textBoxDest.Text = dlg.SelectedPath;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            var xml = XElement.Load(textBoxSource.Text);
            var reqs = xml.Elements();
            int reqsCount = reqs.Count();
            int nReq = 1;
            foreach (var item in reqs)
            {
                String fileName = String.Empty;
                if (reqsCount >= 100)
                {
                    if (nReq > 99)
                        fileName = String.Format("{1}\\TestRequest{0}.xml", nReq++, textBoxDest.Text);
                    else
                        fileName = nReq < 10 ? String.Format("{1}\\TestRequest00{0}.xml", nReq++, textBoxDest.Text) : String.Format("{1}\\TestRequest0{0}.xml", nReq++, textBoxDest.Text);
                }
                else
                    fileName = nReq < 10 ? String.Format("{1}\\TestRequest0{0}.xml", nReq++, textBoxDest.Text) : String.Format("{1}\\TestRequest{0}.xml", nReq++, textBoxDest.Text);

                item.Save(fileName);
            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
