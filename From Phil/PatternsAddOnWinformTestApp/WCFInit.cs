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

namespace PatternsAddOnWinformTestApp
{
    public partial class WCFInit : UserControl
    {
        public WCFInit()
        {
            InitializeComponent();
            InitComboDictionary();
        }

        private void InitComboDictionary()
        {
            ComboVal2String = new Dictionary<int, String>();
            ComboVal2String[0] = Resource.IDS_PATTERNS_ADD_ON_REST_PATH;
        }

        private void textBoxHost_TextChanged(object sender, EventArgs e)
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

        private void buttonInit_Click(object sender, EventArgs e)
        {
            //var client = new RestClient("http://localhost:8000/PatternsAddOnService/PatternsService/");
            Host = Resource.IDS_HTTP + textBoxHost.Text + ":" + textBoxPort.Text + "/" + ComboVal2String[comboBoxRESTPath.SelectedIndex];
            var client = new RestClient(Host);
            //var client = new RestClient(textBoxHost.Text);
            var request = new RestRequest("Sessions/" + textBoxGA.Text, Method.PUT) { Timeout = 3000 };
            var response = client.Execute(request);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var tokenIds = from c in contentElement.Elements()
                           select c;

            var tokenId = tokenIds.Count() > 1 ? tokenIds.ElementAt(2).Value : String.Empty;
            var validId = tokenIds.Count() > 1 ? tokenIds.ElementAt(0).Value : String.Empty;
            if (tokenId.Equals(String.Empty))
                return;

            listBoxToken.Items.Clear();
            listBoxToken.Items.Add(tokenId);
            listBoxValid.Items.Clear();
            listBoxValid.Items.Add(validId);
            TokenID = tokenId;
            //Host = textBoxHost.Text;
        }

        public MainForm MainForm { get; set; }
        public String TokenID { get; set; }
        public String Host { get; private set; }
        private Dictionary<int, String> ComboVal2String { get; set; }
    }
}
