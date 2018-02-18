using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Diagnostics;
using PatternsAddOnManager;
using RestSharp;
using System.Threading;
using System.Globalization;

namespace PatternsAddOnWinformTestApp
{
    public partial class PatternsRunnerCtrl : PatternsCtrlAbs
    {
        #region Construction

        public PatternsRunnerCtrl()
        {
            InitializeComponent();
            InitBGWorkers();
            ResultTimerToStop = false;
            ResultTimer = new System.Windows.Forms.Timer();
            ResultTimer.Tick += new EventHandler(ResultTimer_Tick);
            dataGridViewOutput.DataSource = ResultsTable;
            Running = false;
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
            XML = XMLHelper.LoadTracingsFromXML(textBoxInPath.Text, UPList, FHRList);

            int nShift = 0;
            DateTime time = DateTime.Now.AddSeconds(-(int)numericUpDownSliceInterval.Value);
            AbsoluteStart = time;
            var token = PatternsAddOnTestClient.Instance.CurrentToken;
            int nSlice = checkBoxSliceData.Checked ? (int)numericUpDownSliceInterval.Value : UPList.Count;

            while (nShift < UPList.Count)
            {
                var sublistSize = checkBoxSliceData.Checked ? Math.Min(UPList.Count - nShift, nSlice) : UPList.Count;
                var upSubList = CommonHelper.Sublist(UPList, nShift, sublistSize);
                var fhrSubList = CommonHelper.Sublist(FHRList, nShift * 4, Math.Min(sublistSize * 4, FHRList.Count - (nShift * 4)));

                LastTracingTimeStamp = time.AddSeconds(nShift);
                var dataStr = XMLHelper.PrepareXML(upSubList, fhrSubList, time.AddSeconds(nShift), LastDetected);
                var client = new RestClient(PatternsAddOnTestClient.Instance.CurrentHost);
                var request = new RestRequest("Sessions/" + token, Method.POST) { Timeout = 6000, RequestFormat = DataFormat.Xml };
                request.AddParameter("text/xml", dataStr, ParameterType.RequestBody);

                var response = client.Execute(request);
                System.Diagnostics.Trace.WriteLine("Response code: " + response.ResponseStatus.ToString());
                Process proc = Process.GetCurrentProcess();
                var mem = proc.PrivateMemorySize64;
                Trace.WriteLine("Memory usage: " + mem.ToString());

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted && response.StatusCode != System.Net.HttpStatusCode.MethodNotAllowed)
                    break;

                var content = response.Content; // raw content as string
                if (content.Equals(String.Empty))
                    return;

                var contentElement = XElement.Parse(content);
                var tokenIds = from c in contentElement.Elements()
                               select c;

                var tokenId = tokenIds.Count() > 1 ? tokenIds.ElementAt(1).Value : String.Empty;
                if (tokenId.Equals(String.Empty))
                    return;

                nShift += nSlice;
                if (nShift >= UPList.Count)
                    break;

                if (checkBoxSliceData.Checked)
                {
                    int sleep = 1500;
                    if (checkBoxRTInterval.Checked)
                        sleep = (int)numericUpDownSliceInterval.Value * 1000;
                    else
                        sleep = numericUpDownDelay.Visible ? (int)numericUpDownDelay.Value : 1500;

                    Thread.Sleep(sleep);
                }
                else
                    Thread.Sleep(1500);
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
            GetResults();
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

                            XMLHelper.LoadRawTracings(UPList, FHRList, XML);
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
                        XMLWriter.FlushToArchiveFormat(textBoxInPath.Text, textBoxResult.Text, ResultsDAT, AbsoluteStart);
                }

                Running = false;
            }
        }

        #endregion

        #region Event Handlers

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

        private void buttonBrowseRes_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "XML Files (*.xml)|*.xml|DAT Files (*.dat)|*.dat";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxResult.Text = dlg.FileName;
        }

        private void checkBoxSliceData_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRTInterval.Enabled = numericUpDownSliceInterval.Enabled = checkBoxSliceData.Checked;
            labelDelay.Visible = numericUpDownDelay.Visible = !checkBoxRTInterval.Checked & checkBoxSliceData.Checked;
        }

        private void checkBoxFlush_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxArcive.Enabled = buttonBrowseRes.Enabled = textBoxResult.Enabled = checkBoxFlush.Checked;
        }

        private void textBoxInPath_TextChanged(object sender, EventArgs e)
        {
            buttonRun.Enabled = File.Exists(textBoxInPath.Text);
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

        private void checkBoxRTInterval_CheckedChanged(object sender, EventArgs e)
        {
            labelDelay.Visible = numericUpDownDelay.Visible = !checkBoxRTInterval.Checked & checkBoxSliceData.Checked;
        }

        #endregion 

        #region Methods

        public void StopTimer()
        {
            ResultTimer.Stop();
        }

        protected override void AppendToGrid(List<Artifact> Results, int nResIndex)
        {
            base.AppendToGrid(Results, nResIndex);
            if (!checkBoxSliceData.Checked && Results.Count > 0)
                ResultTimer.Stop();
        }

        #endregion

        #region Properties

        private bool Running { get; set; }
        public bool ResultTimerToStop { get; set; }
        private System.Windows.Forms.Timer ResultTimer { get; set; }
        private BackgroundWorker Worker { get; set; }
        private BackgroundWorker ResultsWorker { get; set; }

        protected override CheckBox CheckBoxFlush
        {
            get { return checkBoxFlush; }
        }

        protected override CheckBox CheckBoxArcive
        {
            get { return checkBoxArcive; }
        }

        protected override TextBox TextBoxResult
        {
            get { return textBoxResult; }
        }

        #endregion
    }
}
