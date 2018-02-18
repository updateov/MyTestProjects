using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PatternsAddOnManager;
using RestSharp;
using System.IO;
using System.Xml.Linq;

namespace PatternsAddOnWinformTestApp
{
    public partial class GetResultsCtrl : PatternsCtrlAbs
    {
        public GetResultsCtrl()
        {
            InitializeComponent();
            dataGridViewOutput.DataSource = ResultsTable;
        }

        private void checkBoxFlush_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxArcive.Enabled = buttonBrowseRes.Enabled = textBoxResult.Enabled = checkBoxFlush.Checked;
        }

        private void buttonBrowseRes_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = "DAT Files (*.dat)|*.dat";
            dlg.FilterIndex = 1;
            if (dlg.ShowDialog() == DialogResult.OK)
                textBoxResult.Text = dlg.FileName;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            ResultsTable.Clear();
            buttonGetResults.Enabled = true;
        }

        private void buttonGetResults_Click(object sender, EventArgs e)
        {
            GetResults();
            if (checkBoxFlush.Checked)
            {
                if (checkBoxArcive.Checked)
                {
                    var storage = PatternsAddOnTestClient.Instance.Tracings;
                    XMLWriter.FlushToArchiveFormat(textBoxResult.Text, AbsoluteStart, ResultsDAT, storage.UpsList, storage.FhrList);
                }

                using (StreamWriter sw = new StreamWriter(textBoxResult.Text))
                {
                    foreach (var item in ResultsDAT)
                    {
                        sw.WriteLine(item);
                    }
                }
            }

            buttonGetResults.Enabled = false;
        }

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
    }
}
