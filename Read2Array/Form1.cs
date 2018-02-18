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

namespace Read2Array
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            radioButtonText.Checked = false;
            radioButtonText.Checked = true;
        }

        private void radioButtonText_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonText.Checked)
            {
                textBoxLoad.Enabled = textBoxLoaddUP.Enabled = buttonBrowseLoad.Enabled = buttonBrowseUP.Enabled = true;
                textBoxArchive.Enabled = buttonBrowseArchive.Enabled = false;
                textBoxSingleReqPath.Enabled = buttonBrowseSingleReq.Enabled = false;
                textBoxResultsPath.Enabled = buttonResultsPath.Enabled = false;
            }
        }

        private void radioButtonDecode_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonDecode.Checked)
            {
                textBoxLoad.Enabled = textBoxLoaddUP.Enabled = buttonBrowseLoad.Enabled = buttonBrowseUP.Enabled = false;
                textBoxArchive.Enabled = buttonBrowseArchive.Enabled = true;
                textBoxSingleReqPath.Enabled = buttonBrowseSingleReq.Enabled = false;
                textBoxResultsPath.Enabled = buttonResultsPath.Enabled = false;
            }
        }

        private void radioButtonSingleReq_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSingleReq.Checked)
            {
                textBoxLoad.Enabled = textBoxLoaddUP.Enabled = buttonBrowseLoad.Enabled = buttonBrowseUP.Enabled = false;
                textBoxArchive.Enabled = buttonBrowseArchive.Enabled = false;
                textBoxSingleReqPath.Enabled = buttonBrowseSingleReq.Enabled = true;
                textBoxResultsPath.Enabled = buttonResultsPath.Enabled = false;
            }
        }

        private void radioButtonResults_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonResults.Checked)
            {
                textBoxLoad.Enabled = textBoxLoaddUP.Enabled = buttonBrowseLoad.Enabled = buttonBrowseUP.Enabled = false;
                textBoxArchive.Enabled = buttonBrowseArchive.Enabled = false;
                textBoxSingleReqPath.Enabled = buttonBrowseSingleReq.Enabled = false;
                textBoxResultsPath.Enabled = buttonResultsPath.Enabled = true;
            }
        }

        private void buttonBrowseLoad_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Text File (*.txt)|*.txt";
            dlg.ShowDialog();
            textBoxLoad.Text = dlg.FileName;
        }

        private void buttonBrowseLoadUP_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Text File (*.txt)|*.txt";
            dlg.ShowDialog();
            textBoxLoaddUP.Text = dlg.FileName;
        }

        private void buttonBrowseLoadArchive_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "XML File (*.xml)|*.xml";
            dlg.ShowDialog();
            textBoxArchive.Text = dlg.FileName;
        }

        private void buttonBrowseLoadSingleReq_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "XML File (*.xml)|*.xml";
            dlg.ShowDialog();
            textBoxSingleReqPath.Text = dlg.FileName;
        }

        private void buttonResultsPath_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "XML File (*.xml)|*.xml";
            dlg.ShowDialog();
            textBoxResultsPath.Text = dlg.FileName;
        }

        private void buttonBrowseSave_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "XML File (*.xml)|*.xml|Comma Separated Value File (*.csv)|*.csv";
            dlg.ShowDialog();
            textBoxSave.Text = dlg.FileName;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (radioButtonText.Checked)
            {
                DataFHR = new List<String>();
                DataUP = new List<String>();
                ReadData(DataFHR, textBoxLoad.Text);
                ReadData(DataUP, textBoxLoaddUP.Text);
                SaveData();
            }
            else if (radioButtonDecode.Checked)
            {
                List<byte> upList = new List<byte>();
                List<byte> fhrList = new List<byte>();
                XElement xml = XElement.Load(textBoxArchive.Text);
                LoadRawTracings(upList, fhrList, xml);

                var toOut = new XElement("lms-patterns-fetus", new XAttribute("fhr-sample-rate", "4"), new XAttribute("up-sample-rate", "1"));
                foreach (var item in fhrList)
                {
                    toOut.Add(new XElement("fhr-sample", new XAttribute("value", item.ToString() + ".00")));
                }

                foreach (var item in upList)
                {
                    toOut.Add(new XElement("up-sample", new XAttribute("value", item.ToString() + ".00")));
                }

                toOut.Save(textBoxSave.Text);
            }
            else if (radioButtonSingleReq.Checked)
            {
                XElement xml = XElement.Load(textBoxSingleReqPath.Text);

                var fhr = xml.Elements().ElementAt(0).Value;
                var up = xml.Elements().ElementAt(3).Value;
                var fhrs = Convert.FromBase64String(fhr);
                var ups = Convert.FromBase64String(up);

                var toOut = new XElement("lms-patterns-fetus", new XAttribute("fhr-sample-rate", "4"), new XAttribute("up-sample-rate", "1"));
                foreach (var item in fhrs)
                {
                    toOut.Add(new XElement("fhr-sample", new XAttribute("value", item.ToString() + ".00")));
                }

                foreach (var item in ups)
                {
                    toOut.Add(new XElement("up-sample", new XAttribute("value", item.ToString() + ".00")));
                }

                toOut.Save(textBoxSave.Text);
            }
            else
            {
                XElement xml = XElement.Load(textBoxResultsPath.Text);
                XNamespace df = xml.Name.Namespace;
                List<String> artifacts = new List<String>();
                var arts = from c in xml.Elements()
                           select c;

                foreach (var item in arts)
                {
                    DateTime start, end;
                    DateTime.TryParse(item.Element(df + "StartTime").Value, out start);
                    DateTime.TryParse(item.Element(df + "EndTime").Value, out end);
                    var duration = end - start;
                    artifacts.Add(item.Element(df + "Category").Value + "," + item.Element(df + "StartTime").Value + "," + item.Element(df + "EndTime").Value + "," + duration.ToString());
                }

                using (var sw = new StreamWriter(textBoxSave.Text))
                {
                    foreach (var item in artifacts)
                    {
                        sw.WriteLine(item);
                    }
                }

            }

            MessageBox.Show("Done!");
        }

        private void SaveData()
        {
            var toOut = new XElement("lms-patterns-fetus", new XAttribute("fhr-sample-rate", "4"), new XAttribute("up-sample-rate", "1"));
            foreach (var item in DataFHR)
            {
                toOut.Add(new XElement("fhr-sample", new XAttribute("value", item + ".00")));
            }

            foreach (var item in DataUP)
            {
                toOut.Add(new XElement("up-sample", new XAttribute("value", item + ".00")));
            }

            toOut.Save(textBoxSave.Text);
        }

        private void ReadData(List<String> Data, String path)
        {
            using (var reader = new StreamReader(path))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    Data.Add(line);
                }
            }
        }

        public static void LoadRawTracings(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            var upElem = from b in XML.Descendants("tracing")
                         where b.Attribute("type").Value.Equals("up")
                         select b.Element("segment");

            String upStr = upElem.Attributes("data").ElementAt(0).Value;
            var upArr = Convert.FromBase64String(upStr);
            UPList.AddRange(upArr);

            var fhrElem = from b in XML.Descendants("tracing")
                          where b.Attribute("type").Value.Equals("fhr1")
                          select b.Element("segment");

            String fhrStr = fhrElem.Attributes("data").ElementAt(0).Value;
            var fhrArr = Convert.FromBase64String(fhrStr);
            FHRList.AddRange(fhrArr);
        }

        List<String> DataFHR { get; set; }
        List<String> DataUP { get; set; }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<int> l1 = new List<int>() { 5, 8, 6, 859, 65, 63, 45, 6, 16, 46, 64, 64, 64, 64, 64, 65, 985, 12, 864, 54156, 486 };
            List<int> l2 = new List<int>() { 5, 8, 6, 64, 64, 64, 64, 65, 985, 12, 864, 54156, 486 };
            eqwualize(l1, l2);

        }

        private static void eqwualize(List<int> l1, List<int> l2)
        {
            int diff = (l2.Count * 4) - l1.Count;
            if (diff < 0)
            {
                l1.RemoveRange(l1.Count - (Math.Abs(diff)), (Math.Abs(diff)));
            }
            else
            {
                for (int i = 0; i < diff; i++)
                {
                    l1.Add(255);
                }
            }
        }
    }
}
