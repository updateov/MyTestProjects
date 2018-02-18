using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using PeriGen.Patterns.Engine;
using PeriGen.Patterns.Engine.Data;
using PeriGen.Patterns.Helper;
using System.Reflection;
using System.IO;

namespace PeriGen.Patterns.BatchProcessor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            m_absoluteStart = DateTime.Now;
            ResultsDAT = new List<String>();
            BW = new BackgroundWorker();
            BW.WorkerReportsProgress = true;
            BW.WorkerSupportsCancellation = true;
            BW.DoWork += new DoWorkEventHandler(BW_DoWork);
            BW.ProgressChanged += new ProgressChangedEventHandler(BW_ProgressChanged);
            BW.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BW_RunWorkerCompleted);
        }

        void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            String[] files = Directory.GetFiles(textBoxInputFilePath.Text);
            List<String> filesToProcess = new List<String>();
            if (checkBoxIn.Checked)
                AddFilesToProcess(files, filesToProcess, "in");

            if (checkBoxV01.Checked)
                AddFilesToProcess(files, filesToProcess, "v01");

            if (checkBoxXML.Checked)
                AddFilesToProcess(files, filesToProcess, "xml");

            int numOfFilesToProc = filesToProcess.Count;
            int cnt = 0;

            foreach (var item in filesToProcess)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                List<byte> UPList = new List<byte>();
                List<byte> FHRList = new List<byte>();

                String ext = item.Remove(0, item.LastIndexOf(".") + 1).ToLower();
                if (ext.Equals("xml"))
                {
                    XElement XML = XMLHelper.LoadTracingsFromXML(item, UPList, FHRList);
                }
                else if (ext.Equals("in"))
                {
                    var blocks = TracingFileReader.ReadInFile(item);
                    FillTracingsLists(UPList, FHRList, blocks, ref m_absoluteStart);
                }
                else if (ext.Equals("v01"))
                {
                    var blocks = TracingFileReader.ReadV01File(item);
                    FillTracingsLists(UPList, FHRList, blocks, ref m_absoluteStart);
                }
                else //Error
                {
                    System.Diagnostics.Trace.WriteLine("Unknown file type");
                    continue;
                }

                ++cnt;
                worker.ReportProgress((int)((double)cnt / numOfFilesToProc) * 100, String.Format("Processing file {0} of {1}", cnt, numOfFilesToProc));
                //labelProcessing.Text = String.Format("Processing file {0} of {1}", cnt, numOfFilesToProc);

                if (UPList.Count * 4 != FHRList.Count)
                    continue;

                var engine = new PatternsEngineWrapper(m_absoluteStart);
                byte[] ups = UPList.ToArray();
                byte[] fhrs = FHRList.ToArray();
                var results = engine.Process(fhrs, ups, 0, ups.Length);

                AppendResultsDAT(results);

                FlushToArchiveFormat(UPList, FHRList, item);
                //System.Diagnostics.Trace.WriteLine("Finished at: " + DateTime.Now.ToString());
            }
        }

        void BW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labelProcessing.Text = e.UserState.ToString();
        }

        void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
                MessageBox.Show("Batch process completed successfully", "Patterns Calculator", MessageBoxButtons.OK);

            buttonClose.Enabled = buttonBrowseInput.Enabled = buttonBrowseOutput.Enabled = buttonRun.Enabled = true;
            buttonClose.Text = "Close";
            labelProcessing.Text = e.Cancelled ? "Canceled" : "Done!";
        }

       private void buttonBrowseInput_Click(object sender, EventArgs e)
        {
            var flBrDlg = new FolderBrowserDialog();
            if (flBrDlg.ShowDialog() == DialogResult.OK)
            {
                textBoxInputFilePath.Text = flBrDlg.SelectedPath;
                String[] files = Directory.GetFiles(flBrDlg.SelectedPath);
            }
        }

        private void buttonBrowseOutput_Click(object sender, EventArgs e)
        {
            var flBrDlg = new FolderBrowserDialog();
            if (flBrDlg.ShowDialog() == DialogResult.OK)
            {
                textBoxOutFilePath.Text = flBrDlg.SelectedPath;
                String[] files = Directory.GetFiles(flBrDlg.SelectedPath);
            }
        }

        private void textBoxFilePath_TextChanged(object sender, EventArgs e)
        {
            buttonRun.Enabled = !textBoxInputFilePath.Text.Equals(String.Empty) && !textBoxOutFilePath.Text.Equals(String.Empty) &&
                                (checkBoxIn.Checked || checkBoxV01.Checked || checkBoxXML.Checked);

            if (!textBoxInputFilePath.Text.Equals(String.Empty))
                checkBoxXML.Enabled = checkBoxV01.Enabled = checkBoxIn.Enabled = true;
            else
                checkBoxIn.Enabled = checkBoxIn.Checked = checkBoxV01.Enabled = checkBoxV01.Checked = checkBoxXML.Enabled = checkBoxXML.Checked = false;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            if (BW.IsBusy)
            {
                BW.CancelAsync();
                buttonClose.Enabled = false;
                labelProcessing.Text = "Canceling...";
                return;
            }

            Close();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            buttonRun.Enabled = buttonBrowseInput.Enabled = buttonBrowseOutput.Enabled = false;
            buttonClose.Text = "Cancel";
            BW.RunWorkerAsync();
        }

        private static void AddFilesToProcess(String[] files, List<String> filesToProcess, String extension)
        {
            var filesToAdd = from c in files
                             where c.Remove(0, c.LastIndexOf(".") + 1).ToLower().Equals(extension)
                             select c;
         
            filesToProcess.AddRange(filesToAdd);
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            buttonRun.Enabled = !textBoxInputFilePath.Text.Equals(String.Empty) && !textBoxOutFilePath.Text.Equals(String.Empty) &&
                                (checkBoxIn.Checked || checkBoxV01.Checked || checkBoxXML.Checked);
        }

        private static void FillTracingsLists(List<byte> UPList, List<byte> FHRList, List<TracingBlock> blocks, ref DateTime absoluteStart)
        {
            DateTime absoluteEnd = blocks[blocks.Count - 1].End;
            DateTime lastAmountOfHoursStart = absoluteEnd.AddHours(-TracingBlock.LastHours);

            var lastHours = from c in blocks
                            where c.Start > lastAmountOfHoursStart
                            select c;

            absoluteStart = lastHours.ElementAt(0).Start;
            DateTime prevBlockEnd = DateTime.MinValue;
            foreach (var item in lastHours)
            {
                if (prevBlockEnd == DateTime.MinValue)
                    prevBlockEnd = item.End;
                else if ((item.Start - prevBlockEnd).Seconds > 1)
                    FillGap(UPList, FHRList, prevBlockEnd, item.Start);

                prevBlockEnd = item.End;
                UPList.AddRange(item.UPs);
                FHRList.AddRange(item.HRs);
            }
        }

        private static void FillGap(List<byte> UPList, List<byte> FHRList, DateTime prevBlockEnd, DateTime blockStart)
        {
            while ((blockStart - prevBlockEnd).TotalSeconds > 1)
            {
                UPList.Add(TracingBlock.NoData);
                FHRList.Add(TracingBlock.NoData);
                FHRList.Add(TracingBlock.NoData);
                FHRList.Add(TracingBlock.NoData);
                FHRList.Add(TracingBlock.NoData);
                prevBlockEnd = prevBlockEnd.AddSeconds(1);
            }
        }

        private void AppendResultsDAT(IEnumerable<DetectedObject> results)
        {
            ResultsDAT.Clear();
            foreach (var item in results)
            {
                String toAdd = item.WriteDAT(m_absoluteStart);
                ResultsDAT.Add(toAdd);
            }
        }

        public void FlushToArchiveFormat(List<byte> UPList, List<byte> FHRList, String fileName)
        {
            String up = Convert.ToBase64String(UPList.ToArray());
            String fhr = Convert.ToBase64String(FHRList.ToArray());
            String resultsDATStr = String.Empty;
            using (StreamWriter sw = new StreamWriter(fileName + ".dat"))
            {
                foreach (var item in ResultsDAT)
                {
                    sw.WriteLine(item);
                    resultsDATStr += "\r\n" + item;
                }
            }

            byte[] resultsArray = Encoding.ASCII.GetBytes(resultsDATStr.Trim());
            String bytesStr = String.Empty;
            foreach (var item in resultsArray)
            {
                bytesStr += item.ToString() + "\r\n";
            }

            String artifacts = Convert.ToBase64String(resultsArray);
            DateTime now = m_absoluteStart;
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

            String outPath = textBoxOutFilePath.Text + fileName.Remove(0, fileName.LastIndexOf("\\"));
            outPath = outPath.Remove(outPath.LastIndexOf("."));
            outPath += "-result.xml";
            outXML.Save(outPath);
        }

        private DateTime m_absoluteStart;
        public List<String> ResultsDAT { get; private set; }
        public BackgroundWorker BW { get; private set; }

    }
}
