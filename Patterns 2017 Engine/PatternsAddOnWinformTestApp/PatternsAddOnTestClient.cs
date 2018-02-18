using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;

namespace PatternsAddOnWinformTestApp
{
    public partial class PatternsAddOnTestClient : Form
    {
        public PatternsAddOnTestClient()
        {
            InitializeComponent();
            Icon = Resource.app;
            s_instance = this;
            AbsolutStartSet = false;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            patternsRunnerCtrl.StopTimer();
            Close();
        }

        public void AddTabs(bool bManual)
        {
            Tracings = null;
            while (tabControl.TabPages.Count > 1)
            {
                tabControl.TabPages.RemoveAt(1);
            }

            if (bManual)
            {
                InitTracings();
                tabControl.Controls.Add(tabPageRequest);
                tabControl.Controls.Add(tabPageGetResults);
            }
            else
                tabControl.Controls.Add(tabPageRunner);
        }

        public void InitTracings()
        {
            Tracings = new ManualStorage();
            getResultsCtrl1.AbsoluteStart = DateTime.MinValue;
            AbsolutStartSet = false;
        }

        public void SetAbsoluteStart(String dataStr)
        {
            var sr = new StringReader(dataStr);
            var contentElement = XElement.Load(sr);
            contentElement = XMLHelper.RemoveAllNamespaces(contentElement);
            var startTime = contentElement.Element("StartTime").Value;
            getResultsCtrl1.AbsoluteStart = ArtifactsHelper.GetDateTime(startTime);
            AbsolutStartSet = true;
        }

        public bool AbsolutStartSet { get; private set; }

        public String CurrentToken { get { return initServerCtrl.TokenID; } }
        public String CurrentHost { get { return initServerCtrl.Host; } }
        public ManualStorage Tracings { get; private set; }

        public static PatternsAddOnTestClient Instance { get { return s_instance; } }
        private static PatternsAddOnTestClient s_instance = null;

    }
}
