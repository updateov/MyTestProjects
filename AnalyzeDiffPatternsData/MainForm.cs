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

namespace AnalyzeDiffPatternsData
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            EpisodeKey2Data = new Dictionary<String, List<String>>();
        }

        private void buttonBrowseLoad_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            textBoxLoafFilePath.Text = dlg.FileName;
            Cursor = Cursors.WaitCursor;
            var log = LoadFile();
            FillDictionary(log);
            FillListBox();
            Cursor = Cursors.Default;
        }

        private void buttonBrowseSaveFolder_Click(object sender, EventArgs e)
        {
            var dlg = new FolderBrowserDialog();
            dlg.Description = "Select Output Folder";
            dlg.ShowDialog();
            textBoxSaveFolder.Text = dlg.SelectedPath;
        }

        private void moveButtonAdd_Click(object sender, EventArgs e)
        {
            var sel = listBoxAllPatients.SelectedItem.ToString();
            if (listBoxSelectedPatients.Items.Count <= 0 || !listBoxSelectedPatients.Items.Contains(sel))
            {
                listBoxSelectedPatients.Items.Add(sel);
            }

            if (listBoxSelectedPatients.Items.Count == 1)
                listBoxSelectedPatients.SelectedIndex = 0;

            UpdateButtonsActivity();
        }

        private void moveButtonRemove_Click(object sender, EventArgs e)
        {
            listBoxSelectedPatients.Items.RemoveAt(listBoxSelectedPatients.SelectedIndex);
            if (listBoxSelectedPatients.Items.Count > 0)
                listBoxSelectedPatients.SelectedIndex = listBoxSelectedPatients.Items.Count - 1;

            UpdateButtonsActivity();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            foreach (var item in listBoxSelectedPatients.Items)
            {
                var str = item.ToString();
                using(StreamWriter sw = new StreamWriter(textBoxSaveFolder.Text + "\\" + str + ".log"))
                {
                    foreach (var episodeItem in EpisodeKey2Data[str])
                    {
                        sw.WriteLine(episodeItem);
                    }

                    sw.WriteLine();
                }
            }
        }

        private void UpdateButtonsActivity()
        {
            buttonRun.Enabled = moveButtonRemove.Enabled = listBoxSelectedPatients.Items.Count > 0;
        }

        private String LoadFile()
        {
            String log = String.Empty;
            using (StreamReader sr = new StreamReader(textBoxLoafFilePath.Text))
            {
                log = sr.ReadToEnd();
                sr.Close();
            }

            return log;
        }

        private void FillDictionary(String log)
        {
            log = log.Replace("Log file for date: 7/15/2015", String.Empty).Trim();
            String[] separ = new String[] { "Data sent to engine from processing" };
            List<String> listLog = log.Split(separ, StringSplitOptions.RemoveEmptyEntries).ToList();
            while (listLog.Count > 0)
            {
                String key = listLog[0].Remove(0, listLog[0].IndexOf("episode key: ") + 13);
                key = key.Remove(key.IndexOf("\r\n"));
                var data = from c in listLog
                           where c.IndexOf(key) > -1
                           select c;

                if (data == null || data.Count() <= 0)
                    continue;

                EpisodeKey2Data[key] = new List<String>(data);
                listLog.RemoveAll(c => c.IndexOf(key) > -1);
            }
        }

        private void FillListBox()
        {
            listBoxAllPatients.Items.Clear();
            listBoxSelectedPatients.Items.Clear();
            moveButtonAdd.Enabled = moveButtonRemove.Enabled = false;
            var items = (from c in EpisodeKey2Data
                         select c.Key).ToArray();

            listBoxAllPatients.Items.AddRange(items);
            if (listBoxAllPatients.Items.Count > 0)
            {
                moveButtonAdd.Enabled = true;
                listBoxAllPatients.SelectedIndex = 0;
            }
        }

        public Dictionary<String, List<String>> EpisodeKey2Data { get; private set; }
    }
}
