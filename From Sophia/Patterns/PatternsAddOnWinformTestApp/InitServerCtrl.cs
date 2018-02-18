using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RestSharp;
using System.Xml.Linq;
using System.IO;
using System.Diagnostics;

namespace PatternsAddOnWinformTestApp
{
    public partial class InitServerCtrl : UserControl
    {
        public InitServerCtrl()
        {
            InitializeComponent();
            InitComboDictionary();
            comboBoxRESTPath.SelectedIndex = 0;
        }

        private void InitComboDictionary()
        {
            ComboVal2String = new Dictionary<int, String>();
            ComboVal2String[0] = Resource.IDS_PATTERNS_ADD_ON_REST_PATH;
        }

        #region Initialize

        private void buttonInit_Click(object sender, EventArgs e)
        {
            Host = Resource.IDS_HTTP + textBoxHost.Text + ":" + textBoxPort.Text + "/" + ComboVal2String[comboBoxRESTPath.SelectedIndex];
            var client = new RestClient(Host);
            var request = new RestRequest("Sessions/" + textBoxGA.Text, Method.PUT) { Timeout = 3000 };
            var response = client.Execute(request);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var tokenIds = from c in contentElement.Elements()
                           select c;

            var tokenId = tokenIds.Count() > 1 ? tokenIds.ElementAt(2).Value : String.Empty;
            var ga = tokenIds.Count() > 1 ? tokenIds.ElementAt(1).Value : String.Empty;
            if (tokenId.Equals(String.Empty))
                return;

            //listBoxToken.Items.Clear();
            textBoxTokenId.Text = tokenId;
            textBoxGARet.Text = ga;
            TokenID = tokenId;
            buttonGetSession.Enabled = buttonGetStatus.Enabled = !Host.Equals(String.Empty);
            if (radioButtonFromFile.Checked)
                PatternsAddOnTestClient.Instance.AddTabs(false);
            else if (radioButtonManual.Checked)
                PatternsAddOnTestClient.Instance.AddTabs(true);
            else
                throw new ArgumentException("Unknown mode");
        }

        private void textBoxHost_TextChanged(object sender, EventArgs e)
        {
            EnableInitializeButton();
        }

        private void textBoxPort_TextChanged(object sender, EventArgs e)
        {
            EnableInitializeButton();
        }

        private void comboBoxRESTPath_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableInitializeButton();
        }

        private void textBoxGA_TextChanged(object sender, EventArgs e)
        {
            EnableInitializeButton();
        }

        private void EnableInitializeButton()
        {
            buttonInit.Enabled = !textBoxHost.Text.Equals(String.Empty) &&
                                    !textBoxPort.Text.Equals(String.Empty) &&
                                    comboBoxRESTPath.SelectedIndex > -1 &&
                                    !textBoxGA.Text.Equals(String.Empty);
        }

        #endregion

        #region Status

        private void buttonGetStatus_Click(object sender, EventArgs e)
        {
            var client = new RestClient(Host);
            var request = new RestRequest("", Method.GET) { Timeout = 3000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(request);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                           select c;

            var serviceVersion = elems.Count() > 0 ? elems.ElementAt(0).Value : String.Empty;
            var startTime = elems.Count() > 1 ? elems.ElementAt(1).Value : String.Empty;
            if (startTime.Equals(String.Empty))
                return;

            listBoxStatus.Items.Clear();
            listBoxStatus.Items.Add("Service Version: " + serviceVersion);
            listBoxStatus.Items.Add("Start Time: " + startTime);
        }

        #endregion

        #region About

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            var client = new RestClient(Host);
            var request = new RestRequest("/About", Method.GET) { Timeout = 5000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(request);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                        select c;

            var aboutStr = elems.Count() > 0 ? elems.ElementAt(0).Value : String.Empty;
            if (aboutStr.Equals(String.Empty))
                return;

            var bytes = Convert.FromBase64String(aboutStr);
            File.WriteAllBytes("tmp.png", bytes);
            Process.Start("tmp.png");
        }

        #endregion

        #region Sessions

        private void buttonGetSession_Click(object sender, EventArgs e)
        {
            var client = new RestClient(Host);
            var request = new RestRequest("/Sessions", Method.GET) { Timeout = 3000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(request);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                        select c;

            listViewSessions.Items.Clear();
            foreach (var item in elems)
            {
                var tokenId = item.Elements().Count() > 2 ? item.Elements().ElementAt(2).Value : String.Empty;
                var ga = item.Elements().Count() > 1 ? item.Elements().ElementAt(1).Value : String.Empty;
                var listItem = new ListViewItem(tokenId);
                listItem.SubItems.Add(ga);
                listViewSessions.Items.Add(listItem);
            }
        }

        #endregion

        public String TokenID { get; set; }
        public String Host { get; private set; }
        private Dictionary<int, String> ComboVal2String { get; set; }
    }
}
