using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BuildRunnerClient
{
    public partial class MainForm : Form
    {
        public String Host { get; set; }
        public DataTable AwaitingBuildsTbl { get; set; }
        public DataTable CompletedBuildsTbl { get; set; }
        private Timer m_gridRefreshTimer;

        public MainForm()
        {
            InitializeComponent();
            AwaitingBuildsTbl = new DataTable();
            CompletedBuildsTbl = new DataTable();
            CreateAwaitingDataTable();
            CreateCompletedDataTable();
            dataGridViewAwaitingBuilds.DataSource = AwaitingBuildsTbl;
            dataGridViewCompleted.DataSource = CompletedBuildsTbl;
            m_gridRefreshTimer = new Timer();
            m_gridRefreshTimer.Interval = 20000;
            m_gridRefreshTimer.Tick += m_gridRefreshTimer_Tick;
        }

        private void CreateAwaitingDataTable()
        {
            DataColumn col = new DataColumn("ProjectName");
            col.DataType = Type.GetType("System.String");
            col.ReadOnly = true;
            col.Unique = true;
            col.AutoIncrement = false;
            col.Caption = "Project Name";
            AwaitingBuildsTbl.Columns.Add(col);

            col = new DataColumn("RequestTime");
            col.DataType = Type.GetType("System.String");
            col.ReadOnly = true;
            col.Unique = true;
            col.AutoIncrement = false;
            col.Caption = "Request Time";
            AwaitingBuildsTbl.Columns.Add(col);
        }

        private void CreateCompletedDataTable()
        {
            DataColumn col = new DataColumn("ProjectName");
            col.DataType = Type.GetType("System.String");
            col.ReadOnly = true;
            col.Unique = false;
            col.AutoIncrement = false;
            col.Caption = "Project Name";
            CompletedBuildsTbl.Columns.Add(col);

            col = new DataColumn("BuildLog");
            col.DataType = Type.GetType("System.String");
            col.ReadOnly = true;
            col.Unique = false;
            col.AutoIncrement = false;
            col.Caption = "Build Log";
            CompletedBuildsTbl.Columns.Add(col);
        }

        void m_gridRefreshTimer_Tick(object sender, EventArgs e)
        {
            GetAwaitingBuilds();
            GetCompletedBuilds();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            Host = "http://" + comboBoxHost.SelectedItem.ToString() + ":8190/builderservice";
            GetAvailableBuilds();
            GetAwaitingBuilds();
            GetCompletedBuilds();
            m_gridRefreshTimer.Start();
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void GetAvailableBuilds()
        {
            var client = new RestClient(Host);
            var requestBuilds = new RestRequest("/Builds", Method.GET) { Timeout = 3000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(requestBuilds);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                        select c;

            comboBoxAvailableBuilds.Items.Clear();
            FillBuildsCombo(elems);
            comboBoxAvailableBuilds.Enabled = true;
        }

        private void FillBuildsCombo(IEnumerable<XElement> elems)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => { FillBuildsCombo(elems); }));
            else
            {
                foreach (var item in elems)
                {
                    var toIns = item.Elements().First().Value;
                    comboBoxAvailableBuilds.Items.Add(toIns);
                }
            }
        }

        private void GetAwaitingBuilds()
        {
            var client = new RestClient(Host);
            var requestBuilds = new RestRequest("/Run", Method.GET) { Timeout = 3000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(requestBuilds);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                        select c;

            UpdateAwaitingGrid(elems);
        }

        private void GetCompletedBuilds()
        {
            var client = new RestClient(Host);
            var requestBuilds = new RestRequest("/Completed", Method.GET) { Timeout = 3000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(requestBuilds);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                        select c;

            UpdateCompletedGrid(elems);
        }

        private void UpdateAwaitingGrid(IEnumerable<XElement> elems)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => { UpdateAwaitingGrid(elems); }));
            else
            {
                AwaitingBuildsTbl.Rows.Clear();
                DataRow row;
                foreach (var item in elems)
                {
                    row = AwaitingBuildsTbl.NewRow();
                    row["ProjectName"] = item.Elements().ElementAt(0).Value;
                    row["RequestTime"] = item.Elements().ElementAt(1).Value;
                    AwaitingBuildsTbl.Rows.Add(row);
                }
            }
        }

        private void UpdateCompletedGrid(IEnumerable<XElement> elems)
        {
            if (InvokeRequired)
                Invoke(new MethodInvoker(() => { UpdateCompletedGrid(elems); }));
            else
            {
                CompletedBuildsTbl.Rows.Clear();
                DataRow row;
                foreach (var item in elems)
                {
                    row = CompletedBuildsTbl.NewRow();
                    row["ProjectName"] = item.Elements().ElementAt(1).Value;
                    row["BuildLog"] = item.Elements().ElementAt(0).Value;
                    CompletedBuildsTbl.Rows.Add(row);
                }
            }
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            GetAwaitingBuilds();
            GetCompletedBuilds();
        }

        private void comboBoxAvailableBuilds_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonBuild.Enabled = comboBoxAvailableBuilds.SelectedIndex > -1;

        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            comboBoxHost.Items.Clear();
            foreach (var item in ConfigurationManager.AppSettings.AllKeys)
            {
                comboBoxHost.Items.Add(ConfigurationManager.AppSettings[item]);
            }
        }

        private void comboBoxHost_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonConnect.Enabled = comboBoxHost.Items.Count > 0 && comboBoxHost.SelectedIndex > -1;
        }

        private void buttonBuild_Click(object sender, EventArgs e)
        {
            var client = new RestClient(Host);
            var buildName = comboBoxAvailableBuilds.SelectedItem.ToString();
            if (ExistsInGrid(buildName))
                return;

            var requestBuilds = new RestRequest("/Run/" + buildName, Method.POST) { Timeout = 3000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(requestBuilds);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                        select c;

            UpdateAwaitingGrid(elems);
        }

        private void dataGridViewCompleted_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowInd = e.RowIndex;
            if (rowInd < 0)
                return;

            var row = CompletedBuildsTbl.Rows[rowInd];
            var client = new RestClient(Host);
            var requestBuilds = new RestRequest("/Completed/" + row["ProjectName"] + "/" + row["BuildLog"], Method.GET) { Timeout = 3000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(requestBuilds);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                        select c;

            var logStr = elems.Count() > 0 ? elems.ElementAt(0).Value : String.Empty;
            if (logStr.Equals(String.Empty))
                return;

            var bytes = Convert.FromBase64String(logStr);
            File.WriteAllBytes("tmp.log", bytes);
            Process.Start("tmp.log");
        }

        private void dataGridViewAwaitingBuilds_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowNum = e.RowIndex;
            if (rowNum != 0)
                return;

            var client = new RestClient(Host);
            var requestBuilds = new RestRequest("/Run/Current", Method.GET) { Timeout = 3000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(requestBuilds);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var contentElement = XElement.Parse(content);
            var elems = from c in contentElement.Elements()
                        select c;

            var logStr = elems.Count() > 0 ? elems.ElementAt(0).Value : String.Empty;
            if (logStr.Equals(String.Empty))
            {
                MessageBox.Show("Current log not available");
                return;
            }

            var bytes = Convert.FromBase64String(logStr);
            File.WriteAllBytes("tmp.log", bytes);
            Process.Start("tmp.log");
        }

        private bool ExistsInGrid(String buildName)
        {
            var build = from c in AwaitingBuildsTbl.AsEnumerable()
                    where c["ProjectName"].Equals(buildName)
                    select buildName;

            if (build == null || build.Count() <= 0)
                return false;

            return true;
        }
    }
}
