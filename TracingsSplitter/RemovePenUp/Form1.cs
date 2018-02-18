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

namespace RemovePenUp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            var xml = XElement.Load(textBoxFileIn.Text);
            var fhr = xml.Descendants().ElementAt(0).Value;
            var up = xml.Descendants().ElementAt(3).Value;
            var startTime = xml.Descendants().ElementAt(2).Value;
            DateTime start;
            DateTime.TryParse(startTime, out start);
            SplitData(fhr, up, start);

            MessageBox.Show("Done!");
        }

        private void SplitData(String fhr, String up, DateTime startTime)
        {
            var fhrList = Convert.FromBase64String(fhr).ToList();
            var upList = Convert.FromBase64String(up).ToList();

            List<List<byte>> chunksFhr = new List<List<byte>>();
            List<List<byte>> chunksUp = new List<List<byte>>();
            List<int> fhrStarts = new List<int>() { 0 };
            List<int> upStarts = new List<int>() { 0 };

            int startData = 0;
            int startLongFHRBreak = 0;
            for (int i = 0; i < fhrList.Count; i++)
            {
                startLongFHRBreak = 0;
                int breakSize = 0;
                if (fhrList[i] == 255)
                {
                    if (i == 0)
                        fhrStarts.Clear();

                    ValidateLength(true, fhrList, ref i, ref startLongFHRBreak, ref breakSize);
                }

                if (startLongFHRBreak > 0)
                {
                    List<byte> toAdd = fhrList.GetRange(startData, startLongFHRBreak);
                    chunksFhr.Add(toAdd);
                    fhrStarts.Add(startLongFHRBreak + breakSize + 1);
                    startData = startLongFHRBreak + breakSize + 1;
                }
            }

            chunksFhr.Add(fhrList.GetRange(startData, fhrList.Count - startData));
            //fhrStarts.Add(startData);

            startData = 0;
            for (int i = 0; i < upList.Count; i++)
            {
                int startLongUPBreak = 0;
                int breakSize = 0;
                if (upList[i] == 255)
                {
                    if (i == 0)
                        upStarts.Clear();

                    ValidateLength(false, upList, ref i, ref startLongUPBreak, ref breakSize);
                }

                if (startLongUPBreak > 0)
                {
                    List<byte> toAdd = upList.GetRange(startData, startLongUPBreak);
                    chunksUp.Add(toAdd);
                    upStarts.Add(startLongUPBreak + breakSize + 1);
                    startData = startLongUPBreak + breakSize + 1;
                }
            }

            chunksUp.Add(upList.GetRange(startData, upList.Count - startData));
            //upStarts.Add(startData);

            if (chunksFhr.Count == chunksUp.Count && fhrStarts.Count == upStarts.Count)
            {
                for (int i = 0; i < chunksUp.Count; i++)
                {
                    var start = startTime.AddSeconds(upStarts[i]);
                    var req = CreateRequest(chunksUp[i].ToArray(), chunksFhr[i].ToArray(), upStarts[i], startTime, DateTime.MinValue);
                    var toRet = req.ToString();
                    toRet = toRet.Replace("xmlns=\"\"", "");
                    using (var sr = new StreamWriter(textBoxFileOut.Text.Replace(".xml", "") + i.ToString() + ".xml"))
                    {
                        sr.Write(toRet);
                    }
                }
            }
        }

        private void ValidateLength(bool bFHR, List<byte> fhrList, ref int index, ref int startLongFHRBreak, ref int breakSize)
        {
            breakSize = 0;
            for (int i = 0; index < fhrList.Count && breakSize < (bFHR ? 1204 : 301); i++, index++, breakSize++)
            {
                if (breakSize > 0 && fhrList[index] < 255)
                    return;
            }

            if (breakSize >= (bFHR ? 1204 : 301))
                startLongFHRBreak = index - breakSize;

            while (++index < fhrList.Count && fhrList[index] == 255)
            {
                breakSize++;
            }
        }

        private void buttonBrowseIn_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxFileIn.Text = dlg.FileName;
        }

        private void buttonBrowseOut_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml";
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxFileOut.Text = dlg.FileName;
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
    }
}
