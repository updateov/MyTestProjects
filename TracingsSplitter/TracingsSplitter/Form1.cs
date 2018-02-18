using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;

namespace TracingsSplitter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void checkBoxAddGap_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDownGapSize.Enabled = numericUpDownCapReoccurance.Enabled = checkBoxAddGap.Checked;
        }

        private void textBoxInput_TextChanged(object sender, EventArgs e)
        {
            groupBox1.Enabled = !textBoxInput.Text.Equals(String.Empty);
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxInput.Text = dlg.FileName;
        }

        private void buttonBrowseOut_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxOutput.Text = dlg.FileName;
        }

        private void buttonBrowseFolderOut_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxFolderPath.Text = dlg.SelectedPath;
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            buttonRun.Enabled = false;
            var xml = XElement.Load(textBoxInput.Text);
            XElement toOut = new XElement("requests");
            String up = GetValue(xml, "up");
            var upArr = Convert.FromBase64String(up);
            String fhr = GetValue(xml, "fhr1");
            var fhrArr = Convert.FromBase64String(fhr);
            DateTime startTime = DateTime.UtcNow;
            DateTime prevEndTime = DateTime.MinValue;
            int i = 0, reqNum = 0;
            int chunk = (int)numericUpDownChunkLength.Value;
            while (i < upArr.Length && i * 4 < fhrArr.Length)
            {
                XElement req = CreateRequest(upArr.SubArray(i, chunk), fhrArr.SubArray(i * 4, chunk * 4), i, startTime, prevEndTime);
                i += chunk;
                if (radioButtonToFile.Checked)
                    toOut.Add(req);

                if (radioButtonOutputFolder.Checked)
                {
                    //req.Save(textBoxFolderPath.Text + "\\Request" + ++reqNum + ".xml");
                    var toRet = req.ToString();
                    toRet = toRet.Replace("xmlns=\"\"", "");
                    using (var sr = new StreamWriter(textBoxFolderPath.Text + "\\Request" + ++reqNum + ".xml"))
                    {
                        sr.Write(toRet);
                    }
                }
            }

            if (radioButtonToFile.Checked)
            {
                var toRet = toOut.ToString();
                toRet = toRet.Replace("xmlns=\"\"", "");
                var sr = new StreamWriter(textBoxOutput.Text);
                sr.Write(toRet);
            }

            MessageBox.Show("Done");
            buttonRun.Enabled = true;
        }

        private static XElement CreateRequest(byte[] upArr, byte[] fhrArr, int ind, DateTime startTime, DateTime prevEndTime)
        {
            var up = Convert.ToBase64String(upArr);
            var fhr = Convert.ToBase64String(fhrArr);
            var start = startTime.AddSeconds(ind);
            XNamespace i = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace defaultNamespace = XNamespace.Get("http://schemas.datacontract.org/2004/07/PatternsAddOnManager");
            XElement toRet = new XElement(defaultNamespace + "TracingData",
                            new XAttribute(XNamespace.Xmlns + "i", i.NamespaceName),
                        new XElement("Fhr", fhr),
                        new XElement("PreviousDetectededEndTime", prevEndTime),
                        new XElement("StartTime", start),
                        new XElement("Up", up));

            return toRet;
        }

        private static String GetValue(XElement xml, String name)
        {
            //var nXml = RemoveAllNamespaces(xml);
            var upElem = from b in xml.Descendants("tracing")
                         where b.Attribute("type").Value.Equals(name)
                         select b.Element("segment");

            String toRet = upElem.Attributes("data").ElementAt(0).Value;
            return toRet;
        }

        private static XElement RemoveAllNamespaces(XElement xmlDocument)
        {
            if (!xmlDocument.HasElements)
            {
                XElement xElement = new XElement(xmlDocument.Name.LocalName);
                xElement.Value = xmlDocument.Value;

                foreach (XAttribute attribute in xmlDocument.Attributes())
                    xElement.Add(attribute);

                return xElement;
            }

            return new XElement(xmlDocument.Name.LocalName, xmlDocument.Elements().Select(el => RemoveAllNamespaces(el)));
        }

        private void radioButtonOutputFolder_CheckedChanged(object sender, EventArgs e)
        {
            textBoxOutput.Enabled = buttonBrowseOut.Enabled = !radioButtonOutputFolder.Checked;
            textBoxFolderPath.Enabled = buttonBrowseFolderOut.Enabled = radioButtonOutputFolder.Checked;
        }
    }

    public static class Exts
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            if (data.Length - index < 0)
                return new T[1];

            int size = data.Length - index > length ? length : data.Length - index;
            T[] result = new T[size];
            Array.Copy(data, index, result, 0, size);
            return result;
        }

    }
}
