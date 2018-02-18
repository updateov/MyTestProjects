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

namespace ArtifactsFromLog
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxInPath.Text = dlg.FileName;
                buttonExtract.Enabled = true;
            }
       }

        private void buttonExtract_Click(object sender, EventArgs e)
        {
            String log = String.Empty;
            using (StreamReader sr = new StreamReader(textBoxInPath.Text))
            {
                log = sr.ReadToEnd();
                sr.Close();
            }

            var lines = log.Split('\n');
            var extracted = from c in lines
                            where c.IndexOf("Unformated Artifact:") > 0 || c.IndexOf(": Artifact:") > 0 || c.IndexOf("Baseline:") > 0
                            select c;

            var sw = new StreamWriter(textBoxInPath.Text.Remove(textBoxInPath.Text.Length - 4) + "Extract.txt");
            foreach (var item in extracted)
            {
                sw.WriteLine(item);
            }

            MessageBox.Show("Done");
            buttonExtract.Enabled = false;
        }
    }
}
