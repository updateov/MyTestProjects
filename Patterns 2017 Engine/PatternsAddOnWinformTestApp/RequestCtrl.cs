using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RestSharp;

namespace PatternsAddOnWinformTestApp
{
    public partial class RequestCtrl : UserControl
    {
        public RequestCtrl()
        {
            InitializeComponent();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            String dataStr = richTextBoxData.Text.Trim();
            if (!PatternsAddOnTestClient.Instance.AbsolutStartSet)
                PatternsAddOnTestClient.Instance.SetAbsoluteStart(dataStr);

            PatternsAddOnTestClient.Instance.Tracings.AppendTracings(dataStr);
            var client = new RestClient(PatternsAddOnTestClient.Instance.CurrentHost);
            var request = new RestRequest("Sessions/" + PatternsAddOnTestClient.Instance.CurrentToken, Method.POST) { Timeout = 6000, RequestFormat = DataFormat.Xml };
            request.AddParameter("text/xml", dataStr, ParameterType.RequestBody);

            var response = client.Execute(request);
            textBoxResponse.Text = response.Content;
            labelStatus.Text = response.StatusDescription;
        }
    }
}
