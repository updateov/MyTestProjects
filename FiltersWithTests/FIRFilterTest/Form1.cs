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

namespace FIRFilterTest
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            Samples = new List<int>();
            CoefsList = new List<double>();
            Results = new List<double>();
        }

        private void buttonBrowseInput_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "In Files (*.in) | *.in";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxInputFile.Text = dlg.FileName;
                String line;
                using (StreamReader sr = new StreamReader(dlg.FileName))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        var splittedLine = line.Split(' ');
                        if (splittedLine.Length < 15)
                            continue;

                        for (int i = 0; i < 12; i++)
                        {
                            int toIns;
                            if (Int32.TryParse(splittedLine[i], out toIns))
                                Samples.Add(toIns);
                            else
                                Samples.Add(0);
                        }
                    }
                }
            }
        }

        private void buttonBrowseCoeffs_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Text Files (*.txt) | *.txt";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxCoeffs.Text = dlg.FileName;
                String val = File.ReadAllText(dlg.FileName);
                val = val.Remove(0, val.IndexOf("{") + 1);
                val = val.Replace("};", "").Trim();
                val = val.Replace("\r\n", "");
                var strList = val.Split(',').ToList();
                foreach (var item in strList)
                {
                    double toIns;
                    if (Double.TryParse(item, out toIns))
                        CoefsList.Add(toIns);
                }
            }
        }

        private void buttonBrowseOutput_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "Comma Separated Values Files (*.csv) | *.csv";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBoxOutput.Text = dlg.FileName;
            }
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {
            buttonRun.Enabled = false;
            List<int> curSamples = new List<int>();
            if (Results.Count <= 0)
            {
                int[] initArr = new int[123];
                curSamples.AddRange(initArr.ToList());
                curSamples.AddRange(Samples);
            }

            while (curSamples.Count >= CoefsList.Count)
            {
                double toInsRes = 0f;
                for (int i = 0; i < CoefsList.Count; i++)
                {
                    toInsRes += (curSamples[i] * CoefsList[i]);
                }

                toInsRes = curSamples[123] - toInsRes;
                Results.Add(toInsRes);
                curSamples.RemoveAt(0);
            }

            using (var sw = new StreamWriter(textBoxOutput.Text))
            {
                for (int i = 0; i < 10000; i++)
                {
                    String val = String.Empty;
                    for (int j = 0; j < 21; j++)
                    {
                        val += j == 0 ? Samples[j * i].ToString() : "," + Samples[i * j];
                        if (i * j < Results.Count)
                            val += "," + Results[i * j].ToString();

                    }

                    sw.WriteLine(val);
                }
                
                sw.Close();
            }

            buttonRun.Enabled = true;
        }

        List<double> CoefsList { get; set; }
        List<int> Samples { get; set; }
        List<double> Results { get; set; }
    }
}
