using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using PatternsAddOnManager;
using System.Threading;
using RestSharp;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Globalization;
using System.Reflection;

namespace PatternsAddOnWinformTestApp
{
    public partial class WCFRunnerCtrl : UserControl
    {
        #region Constuction and Init

        public WCFRunnerCtrl()
        {
            InitializeComponent();
            InitGrid();
            InitBGWorkers();
            ResultTimerToStop = false;
            ResultTimer = new System.Windows.Forms.Timer();
            ResultTimer.Tick += new EventHandler(ResultTimer_Tick);
            dataGridViewOutput.DataSource = ResultsTable;
            LastDetected = DateTime.Now;
            AbsoluteStart = DateTime.MinValue;
            ResultsDAT = new List<String>();
            Running = false;
        }

        private void InitGrid()
        {
            ResultsTable = new DataTable();
            DataColumn column;
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Category";
            column.Caption = "Category";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "StartTime";
            column.Caption = "StartTime";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "EndTime";
            column.Caption = "EndTime";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Value";
            column.Caption = "Value";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Variability";
            column.Caption = "Variability";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "PeakTime";
            column.Caption = "PeakTime";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);
        }

        private void InitBGWorkers()
        {
            Worker = new BackgroundWorker();
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += new DoWorkEventHandler(Worker_DoWork);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(Worker_RunWorkerCompleted);
            ResultsWorker = new BackgroundWorker();
            ResultsWorker.WorkerSupportsCancellation = true;
            ResultsWorker.DoWork += new DoWorkEventHandler(ResultsWorker_DoWork);
            ResultsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ResultsWorker_RunWorkerCompleted);
        }

        #endregion

        #region BackgroundWorker

        void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            XElement XML = null;
            List<byte> UPList = new List<byte>();
            List<byte> FHRList = new List<byte>();
            XML = LoadTracingsFromXML(textBoxInPath.Text, UPList, FHRList);

            int nShift = 0;
            DateTime time = DateTime.Now;
            AbsoluteStart = time;
            var token = MainForm.CurrentToken;
            int nSlice = checkBoxSliceData.Checked ? (int)numericUpDownSliceInterval.Value : UPList.Count;

            while (nShift < UPList.Count)
            {
                var sublistSize = checkBoxSliceData.Checked ? Math.Min(UPList.Count - nShift, nSlice) : UPList.Count;
                var upSubList = Sublist(UPList, nShift, sublistSize);
                var fhrSubList = Sublist(FHRList, nShift * 4, Math.Min(sublistSize * 4, FHRList.Count - (nShift * 4)));

                var dataStr = PrepareXML(upSubList, fhrSubList, time.AddSeconds(nShift), LastDetected);

                var client = new RestClient(MainForm.CurrentHost);
                var request = new RestRequest("Sessions/" + token, Method.POST) { Timeout = 6000, RequestFormat = DataFormat.Xml };
                request.AddParameter("text/xml", dataStr, ParameterType.RequestBody);

                var response = client.Execute(request);
                System.Diagnostics.Trace.WriteLine("Response code: " + response.ResponseStatus.ToString());
                Process proc = Process.GetCurrentProcess();
                var mem = proc.PrivateMemorySize64;
                Trace.WriteLine("Memory usage: " + mem.ToString());

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted && response.StatusCode != System.Net.HttpStatusCode.MethodNotAllowed)
                {
                    break;
                }

                var content = response.Content; // raw content as string
                if (content.Equals(String.Empty))
                    return;

                var contentElement = XElement.Parse(content);
                var tokenIds = from c in contentElement.Elements()
                               select c;

                var tokenId = tokenIds.Count() > 1 ? tokenIds.ElementAt(1).Value : String.Empty;
                if (tokenId.Equals(String.Empty))
                    return;

                //Thread.Sleep(1000);

                nShift += nSlice;
                if (nShift >= UPList.Count)
                    break;
                
                if (checkBoxSliceData.Checked)
                {
                    int sleep = checkBoxRTInterval.Checked ? (int)numericUpDownSliceInterval.Value * 1000 : 1500;
                    Thread.Sleep(sleep);
                }
                else
                    Thread.Sleep(1500);

                //if (nShift >= UPList.Count)
                //{
                //    time = time.AddSeconds(nShift);
                //    nShift = 0;
                //}
            }

            GC.Collect();
        }

        void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!ResultsWorker.IsBusy)
            {
                ResultTimerToStop = true;
                ResultsWorker.RunWorkerAsync();
            }
        }

        void ResultsWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var session = new PatternsSessionData();
            List<Artifact> Results = new List<Artifact>();
            int nClrTmp = 20;
            int nResInd = 0;
            var token = MainForm.CurrentToken;
            var client = new RestClient(MainForm.CurrentHost);
            var request = new RestRequest("Sessions/" + token + "/Artifacts", Method.GET) { Timeout = 6000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(request);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var sr = new StringReader(content);
            var contentElement = XElement.Load(sr);
            contentElement = RemoveAllNamespaces(contentElement);
            AppendResultsArtifacts(contentElement);
            var newResults = from c in contentElement.Elements()
                             select c;

            if (newResults.Count() > 0)
            {
                var newDeserializedResults = DeserializeResults(newResults);
                AppendResults(Results, newDeserializedResults);
                if (checkBoxFlush.Checked)
                {
                    if (checkBoxArcive.Checked || textBoxResult.Text.ToLower().IndexOf(".dat") > 0)
                        AppendResultsDAT(Results);
                 
                    if (textBoxResult.Text.ToLower().IndexOf(".xml") > 0)
                        AppendResultsXML(Results);
                }

                AppendToGrid(Results, nResInd);
                nResInd = Results.Count;
                LastDetected = UpdateLastDetectedTime(Results);
            }

            if (--nClrTmp < 0)
            {
                nClrTmp = 20;
                Results.Clear();
            }
        }

        void ResultsWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Trace.WriteLine("Results received.");
            if (!ResultTimer.Enabled)
            {
                MessageBox.Show("Work complete");
                buttonRun.Text = "Run";
                if (checkBoxFlush.Checked)
                {
                    if (textBoxResult.Text.ToLower().IndexOf(".dat") > 0)
                    {
                        using (StreamWriter sw = new StreamWriter(textBoxResult.Text))
                        {
                            foreach (var item in ResultsDAT)
                            {
                                sw.WriteLine(item);
                            }
                        }
                    }
                    else
                    {
                        var xml = (from c in ResultsXML.Elements()
                                   select c);

                        var xmlArr = xml.ToArray();
                        ResultsXML.Elements().Remove();
                        ResultsXML.Add(xmlArr.Distinct());
                        XElement XML = XElement.Load(textBoxInPath.Text);
                        if (XML.Elements("fhr-sample").Count() > 0 && XML.Elements("up-sample").Count() > 0)
                        {
                            var FHRElems = from b in XML.Descendants("fhr-sample")
                                           select b;

                            var UPElems = from c in XML.Descendants("up-sample")
                                          select c;

                            ResultsXML.Add(FHRElems);
                            ResultsXML.Add(UPElems);
                        }
                        else if (XML.Elements("visit").Count() > 0)
                        {
                            List<byte> FHRList = new List<byte>();
                            List<byte> UPList = new List<byte>();

                            LoadRawTracings(UPList, FHRList, XML);
                            foreach (var item in UPList)
                            {
                                ResultsXML.Add(new XElement("up-sample", new XAttribute("value", item)));
                            }

                            foreach (var item in FHRList)
                            {
                                ResultsXML.Add(new XElement("fhr-sample", new XAttribute("value", item)));
                            }
                        }


                        ResultsXML.Save(textBoxResult.Text);
                    }

                    if (checkBoxArcive.Enabled && checkBoxArcive.Checked)
                    {
                        FlushToArchiveFormat();
                    }
                }

                Running = false;
            }
        }

        #endregion

        #region Event handlers

        void ResultTimer_Tick(object sender, EventArgs e)
        {
            if (!Worker.IsBusy && !ResultsWorker.IsBusy)
                ResultTimer.Stop();

            if (ResultTimerToStop)
                ResultTimer.Stop();

            if (!ResultsWorker.IsBusy)
                ResultsWorker.RunWorkerAsync();
            else
                return;

        }

        private void checkBoxSliceData_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRTInterval.Enabled = numericUpDownSliceInterval.Enabled = checkBoxSliceData.Checked;
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            if (!Running)
            {
                if (!Worker.IsBusy)
                {
                    Worker.RunWorkerAsync();
                    buttonRun.Text = "Stop";
                    Running = true;
                }

                ResultTimer.Interval = 20000;
                ResultTimer.Start();
            }
            else
            {
                if (Worker.IsBusy)
                {
                    Running = false;
                    Worker.CancelAsync();
                    buttonRun.Text = "Run";
                }

                ResultTimer.Stop();
            }
        }

        private void buttonBrowseInput_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxInPath.Text = dlg.FileName;
                ResultsTable.Clear();
                buttonRun.Enabled = true;
            }
        }

        private void textBoxInPath_TextChanged(object sender, EventArgs e)
        {
            buttonRun.Enabled = File.Exists(textBoxInPath.Text);
        }

        private void checkBoxFlush_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxArcive.Enabled = buttonBrowseRes.Enabled = textBoxResult.Enabled = checkBoxFlush.Checked;
        }

        private void buttonBrowseResult_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml|DAT Files (*.dat)|*.dat";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxResult.Text = dlg.FileName;
        }

        #endregion

        #region Static methods

        //Core recursion function
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

        private static XElement GetXMLElementByLocalName(XElement xml, String localName)
        {
            var toRet = from c in xml.Elements()
                        where c.Name.LocalName.Equals(localName)
                        select c;

            if (toRet == null || toRet.Count() <= 0)
                return null;

            return toRet.ElementAt(0);
        }

        private static String GetXMLValByLocalName(XElement xml, String localName)
        {
            var toRet = from c in xml.Elements()
                        where c.Name.LocalName.Equals(localName)
                        select c.Value;

            if (toRet == null || toRet.Count() <= 0)
                return String.Empty;

            return toRet.ElementAt(0);
        }

        private void FlushToArchiveFormat()
        {
            XElement XML = null;
            List<byte> UPList = new List<byte>();
            List<byte> FHRList = new List<byte>();
            XML = LoadTracingsFromXML(textBoxInPath.Text, UPList, FHRList);
            String up = Convert.ToBase64String(UPList.ToArray());
            String fhr = Convert.ToBase64String(FHRList.ToArray());
            String resultsDATStr = String.Empty;
            foreach (var item in ResultsDAT)
            {
                resultsDATStr += "\r\n" + item;
            }
                        
            byte[] resultsArray = Encoding.ASCII.GetBytes(resultsDATStr.Trim());
            String bytesStr = String.Empty;
            foreach (var item in resultsArray)
            {
                bytesStr += item.ToString() + "\r\n";
            }

            String artifacts = Convert.ToBase64String(resultsArray);
            DateTime now = DateTime.UtcNow;
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            String verStr = String.Format("{0:00}.{1:00}.{2:00}", ver.Major, ver.Minor, ver.Build);
            XElement outXML = new XElement("patternsarchive",
                                            new XAttribute("patternengine", verStr),
                                            new XAttribute("backupengine", "1.0"),
                                            new XAttribute("configuration", String.Empty),
                                        new XElement("visit",
                                                        new XAttribute("dischargetime", String.Empty),
                                                        new XAttribute("archivetime", now.Year.ToString() + "-" + now.Month.ToString() + "-" + now.Day.ToString() + " " + now.Hour.ToString() + ":" + now.Minute.ToString() + ":" + now.Second.ToString()),
                                                        new XAttribute("dob", String.Empty),
                                                        new XAttribute("age", String.Empty),
                                                        new XAttribute("edd", now.Year.ToString() + "-" + now.Month.ToString() + "-" + now.Day.ToString()),
                                                        new XAttribute("ga", String.Empty),
                                                        new XAttribute("fetuscount", "1"),
                                                    new XElement("patient",
                                                                    new XAttribute("patientid", "123456"),
                                                                    new XAttribute("accountno", "n/a"),
                                                                    new XAttribute("lastname", "n/a"),
                                                                    new XAttribute("firstname", "n/a")),
                                                    new XElement("data",
                                                                new XElement("tracings", new XAttribute("starttime", now.Year.ToString() + "-" + now.Month.ToString() + "-" + now.Day.ToString() + " " + now.Hour.ToString() + ":" + now.Minute.ToString() + ":" + now.Second.ToString()),
                                                                            new XElement("tracing", new XAttribute("type", "up"),
                                                                                        new XElement("segment",
                                                                                                        new XAttribute("start", "0"),
                                                                                                        new XAttribute("compress", "False"),
                                                                                                        new XAttribute("data", up))),
                                                                            new XElement("tracing", new XAttribute("type", "fhr1"),
                                                                                        new XElement("segment",
                                                                                                        new XAttribute("start", "0"),
                                                                                                        new XAttribute("compress", "False"),
                                                                                                        new XAttribute("data", fhr)))),
                                                                new XElement("patterns",
                                                                                new XAttribute("compress", "False"),
                                                                                new XAttribute("data", artifacts)))));

            String outPath = textBoxResult.Text;
            outPath = outPath.Remove(outPath.Length - 4);
            outPath += "-archive.xml";
            outXML.Save(outPath);
        }

        public static bool SameArtifactExists(List<Artifact> data, Artifact newData)
        {
            foreach (var item in data)
            {
                if (item.StartTime.Equals(newData.StartTime) &&
                    item.EndTime.Equals(newData.EndTime) &&
                    item.Category == newData.Category)
                    return true;
            }

            return false;
        }

        private static DateTime GetDateTime(String dateTime)
        {
            return DateTime.Parse(dateTime);
        }

        private static DateTime? GetRefDateTime(String dateTime)
        {
            if (dateTime.Equals(String.Empty))
                return null;

            return DateTime.Parse(dateTime);
        }

        private static List<byte> Sublist(List<byte> data, int index, int length)
        {
            List<byte> toRet = new List<byte>();
            if (index >= data.Count)
                return toRet;

            for (int i = index; i < index + length && i < data.Count; i++)
            {
                toRet.Add(data[i]);
            }

            return toRet;
        }

        #endregion

        #region Patterns Results Methods

        private void AppendResultsArtifacts(XElement contentElement)
        {
            if (ResultsArtifacts == null)
                ResultsArtifacts = new XElement("Artifacts");

            //XNamespace i = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            //XNamespace defaultNamespace = XNamespace.Get("http://schemas.datacontract.org/2004/07/PatternsAddOnManager");

            ResultsArtifacts.Add(contentElement);
        }

        private List<Artifact> DeserializeResults(IEnumerable<XElement> newResults)
        {
            var toRet = new List<Artifact>();
            foreach (var item in newResults)
            {
                var curArt = new Artifact();// { Category = item.Descendants("Category").ElementAt(0).Value };//, StartTime = GetDateTime(item.Element("StartTime").Value), EndTime = GetDateTime(item.Element("EndTime").Value) };
                String curVal = GetXMLValByLocalName(item, "StartTime");
                curArt.StartTime = GetDateTime(curVal);
                curVal = GetXMLValByLocalName(item, "EndTime");
                curArt.EndTime = GetDateTime(curVal);
                curVal = GetXMLValByLocalName(item, "Category");
                curArt.Category = curVal;
                var artData = GetXMLElementByLocalName(item, "ArtifactData");
                if (artData == null)
                    continue;

                switch (curVal)
                {
                    case "Baseline":
                        curArt.ArtifactData = CreateBaselineData(artData);
                        break;
                    case "Contraction":
                        curArt.ArtifactData = CreateContractionData(artData);
                        break;
                    case "Acceleration":
                        curArt.ArtifactData = CreateAccelerationData(artData);
                        break;
                    case "Deceleration":
                        curArt.ArtifactData = CreateDecelerationData(artData);
                        break;
                    default:
                        break;
                }

                toRet.Add(curArt);
            }

            return toRet;
        }

        private ArtifactType CreateBaselineData(XElement artData)
        {
            var toRet = new PatternsAddOnManager.Baseline() { Category = "Baseline" };
            double nVal;
            if (!Double.TryParse(artData.Element("BaselineVariability").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.BaselineVariability = nVal;

            if (!Double.TryParse(artData.Element("Y1").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Y1 = nVal;

            if (!Double.TryParse(artData.Element("Y2").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Y2 = nVal;
            return toRet;
        }

        private ArtifactType CreateContractionData(XElement artData)
        {
            var toRet = new PatternsAddOnManager.Contraction() { Category = "Contraction" };
            toRet.PeakTime = GetDateTime(artData.Element("PeakTime").Value);
            int nId;
            if (!Int32.TryParse(artData.Element("Id").Value, out nId) || nId < 0)
                nId = 0;

            toRet.Id = nId;
            return toRet;
        }

        private ArtifactType CreateAccelerationData(XElement artData)
        {
            var toRet = new PatternsAddOnManager.Acceleration() { Category = "Acceleration" };
            CreateAccelerationDataInternal(artData, toRet);
            return toRet;
        }

        private void CreateAccelerationDataInternal(XElement artData, PatternsAddOnManager.Acceleration toRet)
        {
            double nVal;
            if (!Double.TryParse(artData.Element("Confidence").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Confidence = nVal;

            if (!Double.TryParse(artData.Element("Height").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Height = nVal;
            toRet.IsNonInterpretable = artData.Element("IsNonInterpretable").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.PeakTime = GetDateTime(artData.Element("PeakTime").Value);
            if (!Double.TryParse(artData.Element("PeakValue").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.PeakValue = nVal;

            if (!Double.TryParse(artData.Element("Repair").Value, out nVal) || nVal < 0)
                nVal = 0;

            toRet.Repair = nVal;
        }

        private ArtifactType CreateDecelerationData(XElement artData)
        {
            var toRet = new PatternsAddOnManager.Deceleration() { Category = "Deceleration" };
            CreateAccelerationDataInternal(artData, toRet);
            toRet.ContractionStart = GetRefDateTime(artData.Element("ContractionStart").Value);
            toRet.DecelerationCategory = artData.Element("DecelerationCategory").Value;
            toRet.HasSixtiesNonReassuringFeature = artData.Element("HasSixtiesNonReassuringFeature").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.IsEarlyDeceleration = artData.Element("IsEarlyDeceleration").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.IsLateDeceleration = artData.Element("IsLateDeceleration").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.IsNonAssociatedDeceleration = artData.Element("IsNonAssociatedDeceleration").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.IsVariableDeceleration = artData.Element("IsVariableDeceleration").Value.ToUpper().Equals("TRUE") ? true : false;
            toRet.NonReassuringFeatures = artData.Element("NonReassuringFeatures").Value;

            return toRet;
        }

        #endregion

        #region Methods

        public void StopTimer()
        {
            ResultTimer.Stop();
        }

        private String PrepareXML(List<byte> upSubList, List<byte> fhrSubList, DateTime dateTime, DateTime lastDetected)
        {
            var upString = Convert.ToBase64String(upSubList.ToArray());
            var fhrString = Convert.ToBase64String(fhrSubList.ToArray());

            XNamespace i = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            XNamespace defaultNamespace = XNamespace.Get("http://schemas.datacontract.org/2004/07/PatternsAddOnManager");
            XElement xml = new XElement(defaultNamespace + "TracingData",
                            new XAttribute(XNamespace.Xmlns + "i", i.NamespaceName),
                        new XElement("Fhr", fhrString),
                        new XElement("PreviousDetectededEndTime", lastDetected),
                        new XElement("StartTime", dateTime),
                        new XElement("Up", upString));

            var toRet = xml.ToString();
            toRet = toRet.Replace("xmlns=\"\"", "");
            return toRet;
        }

        private void AppendResults(List<Artifact> Results, IEnumerable<Artifact> newResults)
        {
            foreach (var item in newResults)
            {
                if (SameArtifactExists(Results, item))
                    continue;

                Results.Add(item);
            }

        }

        private void AppendResultsXML(List<Artifact> Results)
        {
            if (ResultsXML == null)
                ResultsXML = new XElement("lms-patterns-fetus", new XAttribute("fhr-sample-rate", "4"), new XAttribute("up-sample-rate", "1"));

            foreach (var item in Results)
            {
                XElement toAdd = null;
                switch (item.ArtifactData.Category)
                {
                    case "Baseline":
                        toAdd = WriteBaselineXML(item);
                        break;
                    case "Contraction":
                        toAdd = WriteContractionXML(item);
                        break;
                    case "Acceleration":
                        toAdd = WriteAccelerationXML(item);
                        break;
                    case "Deceleration":
                        toAdd = WriteDecelerationXML(item);
                        break;
                    default:
                        break;
                }

                ResultsXML.Add(toAdd);
            }
        }

        private XElement WriteBaselineXML(Artifact item)
        {
            Baseline blItem = item.ArtifactData as Baseline;
            var value = new XElement("event",
                            new XAttribute("start", ((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("peak", "-1"),
                            new XAttribute("end", ((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("contraction", "-1"),
                            new XAttribute("type", "9"),
                            new XAttribute("y1", blItem.Y1.ToString("0.000000", CultureInfo.InvariantCulture)),
                            new XAttribute("y2", blItem.Y2.ToString("0.000000", CultureInfo.InvariantCulture)));

            return value;
        }

        private XElement WriteContractionXML(Artifact item)
        {
            Contraction ctrItem = item.ArtifactData as Contraction;
            XElement value = new XElement("contraction",
                                new XAttribute("start", ((int)((item.StartTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("peak", ((int)((ctrItem.PeakTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture)),
                                new XAttribute("end", ((int)((item.EndTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture)));

            return value;
        }

        private XElement WriteAccelerationXML(Artifact item)
        {
            Acceleration acItem = item.ArtifactData as Acceleration;
            var value = new XElement("event",
                            new XAttribute("start", ((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("peak", ((int)((acItem.PeakTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("end", ((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("contraction", "-1"),
                            new XAttribute("type", "1"),
                            new XAttribute("y1", "0.000"),
                            new XAttribute("y2", "0.000"));

            return value;
        }

        private XElement WriteDecelerationXML(Artifact item)
        {
            Deceleration decItem = item.ArtifactData as Deceleration;
            int decelType = GetDecelType(item);
            var value = new XElement("event",
                            new XAttribute("start", ((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("peak", ((int)((decItem.PeakTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("end", ((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture)),
                            new XAttribute("contraction", "-1"),
                            new XAttribute("type", decelType.ToString()),
                            new XAttribute("y1", "0.000"),
                            new XAttribute("y2", "0.000"));

            return value;
        }

        void AppendResultsDAT(List<Artifact> Results)
        {
            foreach (var item in Results)
            {
                String toAdd = String.Empty;
                switch (item.ArtifactData.Category)
                {
                    case "Baseline":
                        toAdd = WriteBaselineDAT(item);
                        break;
                    case "Contraction":
                        toAdd = WriteContractionDAT(item);
                        break;
                    case "Acceleration":
                        toAdd = WriteAccelerationDAT(item);
                        break;
                    case "Deceleration":
                        toAdd = WriteDecelerationDAT(item);
                        break;
                    default:
                        break;
                }

                ResultsDAT.Add(toAdd);
            }
        }

        private string WriteBaselineDAT(Artifact item)
        {
            Baseline blItem = item.ArtifactData as Baseline;
            StringBuilder value = new StringBuilder(255);

            value.Append("EVT|");

            /* 00 */
            value.Append("9");  // event::tbaseline
            value.Append("|");
            /* 01 */
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 02 */
            value.Append(string.Empty); // Peak time
            value.Append("|");
            /* 03 */
            value.Append(((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 04 */
            value.Append(blItem.Y1.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 05 */
            value.Append(blItem.Y2.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 06 */
            value.Append(string.Empty); // Contraction start
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(string.Empty); // Strikeout
            value.Append("|");
            /* 09 */
            value.Append(string.Empty); // Confidence
            value.Append("|");
            /* 10 */
            value.Append(string.Empty); // Repair
            value.Append("|");
            /* 11 */
            value.Append(string.Empty); // Height
            value.Append("|");
            /* 12 */
            value.Append(blItem.BaselineVariability.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 13 */
            value.Append(string.Empty); // Peak value
            value.Append("|");
            /* 14 */
            value.Append(string.Empty); // Non reassuring features ?
            value.Append("|");
            /* 15 */
            value.Append(string.Empty); // Variable decel
            value.Append("|");
            /* 16 */
            value.Append(string.Empty); // Lag
            value.Append("|");
            /* 17 */
            value.Append(string.Empty); // Non reassuring features
            value.Append("|");
            /* 18 */
            value.Append(string.Empty); // Non Interpretable
            value.Append("|");
            /* 19 */
            value.Append(string.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.ArtifactData.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        private string WriteContractionDAT(Artifact item)
        {
            Contraction ctrItem = item.ArtifactData as Contraction;
            StringBuilder value = new StringBuilder(255);

            value.Append("CTR|");
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            value.Append(((int)((ctrItem.PeakTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            value.Append(((int)((item.EndTime - AbsoluteStart).TotalSeconds)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            value.Append("y"); // Is final
            value.Append("|");
            value.Append("n"); // Is Strikeout
            value.Append("|");
            value.Append(item.ArtifactData.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        private string WriteAccelerationDAT(Artifact item)
        {
            Acceleration acItem = item.ArtifactData as Acceleration;
            StringBuilder value = new StringBuilder(255);

            value.Append("EVT|");

            /* 00 */
            value.Append("1"); // event::tacceleration
            value.Append("|");
            /* 01 */
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 02 */
            value.Append(((int)((acItem.PeakTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 03 */
            value.Append(((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 04 */
            value.Append(string.Empty); // Y1
            value.Append("|");
            /* 05 */
            value.Append(string.Empty); // Y2
            value.Append("|");
            /* 06 */
            value.Append(string.Empty); // Contraction start
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(string.Empty); // Strikeout
            value.Append("|");
            /* 09 */
            value.Append(acItem.Confidence.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 10 */
            value.Append(acItem.Repair.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 11 */
            value.Append(acItem.Height.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 12 */
            value.Append(string.Empty); // Baseline variability
            value.Append("|");
            /* 13 */
            value.Append(acItem.PeakValue.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 14 */
            value.Append(string.Empty); // Non reassuring features ?
            value.Append("|");
            /* 15 */
            value.Append(string.Empty); // Variable decel
            value.Append("|");
            /* 16 */
            value.Append(string.Empty); // Lag
            value.Append("|");
            /* 17 */
            value.Append(string.Empty); // Non reassuring features
            value.Append("|");
            /* 18 */
            if (acItem.IsNonInterpretable) value.Append("y");
            value.Append("|");
            /* 19 */
            value.Append(string.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.ArtifactData.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        private string WriteDecelerationDAT(Artifact item)
        {
            Deceleration decItem = item.ArtifactData as Deceleration;
            StringBuilder value = new StringBuilder(255);

            value.Append("EVT|");

            /* 00 */
            value.Append(GetDecelType(item).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 01 */
            value.Append(((int)((item.StartTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 02 */
            value.Append(((int)((decItem.PeakTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 03 */
            value.Append(((int)((item.EndTime - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 04 */
            value.Append(string.Empty); // Y1
            value.Append("|");
            /* 05 */
            value.Append(string.Empty); // Y2
            value.Append("|");
            /* 06 */
            value.Append(decItem.ContractionStart.HasValue ? ((int)((decItem.ContractionStart.Value - AbsoluteStart).TotalSeconds * 4)).ToString(CultureInfo.InvariantCulture) : String.Empty);
            value.Append("|");
            /* 07 */
            value.Append("y"); // Final
            value.Append("|");
            /* 08 */
            value.Append(string.Empty); // Strikeout
            value.Append("|");
            /* 09 */
            value.Append(decItem.Confidence.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 10 */
            value.Append(decItem.Repair.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 11 */
            value.Append(decItem.Height.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 12 */
            value.Append(string.Empty); // Baseline variability
            value.Append("|");
            /* 13 */
            value.Append(decItem.PeakValue.ToString("0.000000", CultureInfo.InvariantCulture));
            value.Append("|");
            /* 14 */
            value.Append("|");
            /* 15 */
            if (decItem.DecelerationCategory.Equals("Variable"))
                value.Append("y");
            value.Append("|");
            /* 16 */
            value.Append("-1"); // Lag
            value.Append("|");
            /* 17 */
            value.Append(GetDecelNonReassuring(item).ToString(CultureInfo.InvariantCulture));
            value.Append("|");
            /* 18 */
            if (decItem.IsNonInterpretable)
                value.Append("y");
            value.Append("|");
            /* 19 */
            value.Append(string.Empty); // Confirmed
            value.Append("|");
            /* 20 */
            value.Append(item.ArtifactData.Id.ToString(CultureInfo.InvariantCulture));

            return value.ToString();
        }

        private int GetDecelType(Artifact item)
        {
            switch ((item.ArtifactData as Deceleration).DecelerationCategory)
            {
                case "NonAssociated":
                    return 7;
                case "Early":
                    return 3;
                case "Late":
                    return 6;
                case "Variable":
                    var tmp = (item.ArtifactData as Deceleration).NonReassuringFeatures;
                    if (tmp.Equals("Prolonged"))
                        return 14;  // this.IsVariableDeceleration && (this.NonReassuringFeatures != AtypicalCharacteristics.None);
                    else if (tmp.Equals("None"))
                        return 4;
                    return 5;
                default:
                    return 5;
            }
        }

        private int GetDecelNonReassuring(Artifact item)
        {
            String valStr = (item.ArtifactData as Deceleration).NonReassuringFeatures;
            int atypicalValue = 0;
            if (valStr.Equals("Biphasic"))
            {
                atypicalValue |= 1;
            }
            if (valStr.Equals("LossRise"))
            {
                atypicalValue |= 2;
            }
            if (valStr.Equals("LossVariability"))
            {
                atypicalValue |= 4;
            }
            if (valStr.Equals("LowerBaseline"))
            {
                atypicalValue |= 8;
            }
            if (valStr.Equals("ProlongedSecondRise"))
            {
                atypicalValue |= 16;
            }
            if (valStr.Equals("Sixties") || (valStr.Equals("Prolonged") && (item.ArtifactData as Deceleration).HasSixtiesNonReassuringFeature))
            {
                atypicalValue |= 32;
            }
            if (valStr.Equals("SlowReturn"))
            {
                atypicalValue |= 64;
            }

            return atypicalValue;
        }

        private DateTime UpdateLastDetectedTime(List<Artifact> Results)
        {
            var last = (from b in Results
                        select b.EndTime).Max();

            return last;
        }

        private void AppendToGrid(List<Artifact> Results, int nResIndex)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { AppendToGrid(Results, nResIndex); }));
            }
            else
            {
                for (int i = nResIndex; i < Results.Count; i++)
                {
                    DataRow row = ResultsTable.NewRow();
                    if (Results[i].Category.Equals("Acceleration"))
                    {
                        row["Category"] = "Acceleration";
                        row["Value"] = (Results[i].ArtifactData as PatternsAddOnManager.Acceleration).PeakValue.ToString();
                        row["PeakTime"] = (Results[i].ArtifactData as PatternsAddOnManager.Acceleration).PeakTime.ToString();
                    }
                    else if (Results[i].Category.Equals("Baseline"))
                    {
                        row["Category"] = "Baseline";
                        row["Variability"] = (Results[i].ArtifactData as PatternsAddOnManager.Baseline).BaselineVariability.ToString();
                        row["Value"] = (Results[i].ArtifactData as PatternsAddOnManager.Baseline).Y1.ToString();
                    }
                    else if (Results[i].Category.Equals("Contraction"))
                    {
                        row["Category"] = "Contraction";
                        row["PeakTime"] = (Results[i].ArtifactData as PatternsAddOnManager.Contraction).PeakTime.ToString();
                    }
                    else if (Results[i].Category.Equals("Deceleration"))
                    {
                        row["Category"] = "Deceleration";
                        row["Value"] = (Results[i].ArtifactData as PatternsAddOnManager.Deceleration).PeakValue.ToString();
                        row["PeakTime"] = (Results[i].ArtifactData as PatternsAddOnManager.Deceleration).PeakTime.ToString();
                    }
                    else
                        row["Category"] = "None";

                    row["StartTime"] = Results[i].StartTime.ToString();
                    row["EndTime"] = Results[i].EndTime.ToString();
                    ResultsTable.Rows.Add(row);
                }

                if (!checkBoxSliceData.Checked && Results.Count > 0)
                    ResultTimer.Stop();
            }
        }

        public XElement LoadTracingsFromXML(String xmlPath, List<byte> UPList, List<byte> FHRList)
        {
            XElement XML = XElement.Load(xmlPath);
            LoadTracingsFromXMLInternal(UPList, FHRList, XML);
            return XML;
        }

        public XElement LoadTracingsFromXML(int num, List<byte> UPList, List<byte> FHRList, ref XElement XML)
        {
            switch (num)
            {
                case 1:
                    XML = XElement.Load(@"Tracings\1.xml");
                    break;
                case 2:
                    XML = XElement.Load(@"Tracings\2.xml");
                    break;
                case 3:
                    XML = XElement.Load(@"Tracings\3.xml");
                    break;
                case 4:
                    XML = XElement.Load(@"Tracings\4.xml");
                    break;
                case 5:
                    XML = XElement.Load(@"Tracings\5.xml");
                    break;
                case 6:
                    XML = XElement.Load(@"Tracings\6.xml");
                    break;
                default:
                    break;
            }

            LoadTracingsFromXMLInternal(UPList, FHRList, XML);
            return XML;
        }

        private void LoadTracingsFromXMLInternal(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            if (XML.Elements("fhr-sample").Count() > 0 && XML.Elements("up-sample").Count() > 0)
                LoadFamous6(UPList, FHRList, XML);

            if (XML.Elements("visit").Count() > 0)
                LoadRawTracings(UPList, FHRList, XML);
        }

        private static void LoadFamous6(List<byte> UPList, List<byte> FHRList, XElement XML)
        {
            var FHRElems = from b in XML.Descendants("fhr-sample")
                           select b.Attribute("value").Value;

            var UPElems = from c in XML.Descendants("up-sample")
                          select c.Attribute("value").Value;

            foreach (var item in UPElems)
            {
                var intPart = item.Remove(item.IndexOf("."));
                byte val;
                if (!Byte.TryParse(intPart, out val))
                    continue;

                UPList.Add(val);
            }

            foreach (var item in FHRElems)
            {
                var intPart = item.Remove(item.IndexOf("."));
                byte val;
                if (!Byte.TryParse(intPart, out val))
                    continue;

                FHRList.Add(val);
            }
        }

        private static void LoadRawTracings(List<byte> UPList, List<byte> FHRList, XElement XML)
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

        #endregion

        #region Properties

        private bool Running { get; set; }
        public MainForm MainForm { get; set; }
        private DataTable ResultsTable { get; set; }
        private BackgroundWorker Worker { get; set; }
        private BackgroundWorker ResultsWorker { get; set; }
        private System.Windows.Forms.Timer ResultTimer { get; set; }
        private DateTime LastDetected { get; set; }
        private DateTime AbsoluteStart { get; set; }
        public List<String> ResultsDAT { get; set; }
        public XElement ResultsXML { get; set; }
        public bool ResultTimerToStop { get; set; }
        public XElement ResultsArtifacts { get; set; }

        #endregion

    }

}
