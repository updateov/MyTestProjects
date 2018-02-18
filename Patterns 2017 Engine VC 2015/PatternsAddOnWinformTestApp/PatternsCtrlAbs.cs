using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PatternsAddOnManager;
using System.Xml.Linq;
using System.Data;
using RestSharp;
using System.IO;

namespace PatternsAddOnWinformTestApp
{
    public class PatternsCtrlAbs : UserControl
    {
        public PatternsCtrlAbs()
        {
            InitGrid();
            ResultsDAT = new List<String>();
            LastDetected = DateTime.Now;
            AbsoluteStart = DateTime.MinValue;
            LastTracingTimeStamp = DateTime.MinValue;
        }


        protected virtual void InitGrid()
        {
            ResultsTable = new DataTable();
            DataColumn column;
            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "SystemTime";
            column.Caption = "SystemTime";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Category";
            column.Caption = "Category";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "StartTime";
            column.Caption = "StartTime";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "EndTime";
            column.Caption = "EndTime";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Y1";
            column.Caption = "Y1";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Y2";
            column.Caption = "Y2";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Confidence";
            column.Caption = "Confidence";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Height";
            column.Caption = "Height";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "DecelerationCategory";
            column.Caption = "DecelerationCategory";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "ContractionStart";
            column.Caption = "ContractionStart";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "PeakValue";
            column.Caption = "PeakValue";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "Variability";
            column.Caption = "Variability";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);

            column = new DataColumn();
            column.DataType = System.Type.GetType("System.String");
            column.ColumnName = "PeakTime";
            column.Caption = "PeakTime";
            column.ReadOnly = true;
            ResultsTable.Columns.Add(column);
        }

        protected virtual void AppendResults(List<Artifact> Results, IEnumerable<Artifact> newResults)
        {
            foreach (var item in newResults)
            {
                if (ArtifactsHelper.SameArtifactExists(Results, item))
                    continue;

                Results.Add(item);
            }
        }

        protected virtual void AppendResultsArtifacts(XElement contentElement)
        {
            if (ResultsArtifacts == null)
                ResultsArtifacts = new XElement("Artifacts");

            ResultsArtifacts.Add(contentElement);
        }

        protected virtual void AppendResultsXML(List<Artifact> Results)
        {
            if (ResultsXML == null)
                ResultsXML = new XElement("lms-patterns-fetus", new XAttribute("fhr-sample-rate", "4"), new XAttribute("up-sample-rate", "1"));

            foreach (var item in Results)
            {
                XElement toAdd = null;
                switch (item.ArtifactData.Category)
                {
                    case "Baseline":
                        toAdd = XMLWriter.WriteBaselineXML(item, AbsoluteStart);
                        break;
                    case "Contraction":
                        toAdd = XMLWriter.WriteContractionXML(item, AbsoluteStart);
                        break;
                    case "Acceleration":
                        toAdd = XMLWriter.WriteAccelerationXML(item, AbsoluteStart);
                        break;
                    case "Deceleration":
                        toAdd = XMLWriter.WriteDecelerationXML(item, AbsoluteStart);
                        break;
                    default:
                        break;
                }

                ResultsXML.Add(toAdd);
            }
        }

        protected virtual void AppendResultsDAT(List<Artifact> Results)
        {
            foreach (var item in Results)
            {
                String toAdd = String.Empty;
                switch (item.ArtifactData.Category)
                {
                    case "Baseline":
                        toAdd = DATWriter.WriteBaselineDAT(item, AbsoluteStart);
                        break;
                    case "Contraction":
                        toAdd = DATWriter.WriteContractionDAT(item, AbsoluteStart);
                        break;
                    case "Acceleration":
                        toAdd = DATWriter.WriteAccelerationDAT(item, AbsoluteStart);
                        break;
                    case "Deceleration":
                        toAdd = DATWriter.WriteDecelerationDAT(item, AbsoluteStart);
                        break;
                    default:
                        break;
                }

                ResultsDAT.Add(toAdd);
            }
        }

        protected virtual void AppendToGrid(List<Artifact> Results, int nResIndex)
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { AppendToGrid(Results, nResIndex); }));
            }
            else
            {
                for (int i = nResIndex; i < Results.Count; i++)
                {
                    DataRow row = ResultsTable.NewRow();
                    row["SystemTime"] = LastTracingTimeStamp.Equals(DateTime.MinValue) ? String.Empty : LastTracingTimeStamp.ToString();

                    if (Results[i].Category.Equals("Acceleration"))
                    {
                        row["Category"] = "Acceleration";
                        row["PeakValue"] = (Results[i].ArtifactData as PatternsAddOnManager.Acceleration).PeakValue.ToString();
                        row["PeakTime"] = (Results[i].ArtifactData as PatternsAddOnManager.Acceleration).PeakTime.ToString();
                        row["Confidence"] = (Results[i].ArtifactData as PatternsAddOnManager.Acceleration).Confidence.ToString();
                        row["Height"] = (Results[i].ArtifactData as PatternsAddOnManager.Acceleration).Height.ToString();
                    }
                    else if (Results[i].Category.Equals("Baseline"))
                    {
                        row["Category"] = "Baseline";
                        row["Variability"] = (Results[i].ArtifactData as PatternsAddOnManager.Baseline).BaselineVariability.ToString();
                        row["Y1"] = (Results[i].ArtifactData as PatternsAddOnManager.Baseline).Y1.ToString();
                        row["Y2"] = (Results[i].ArtifactData as PatternsAddOnManager.Baseline).Y2.ToString();
                    }
                    else if (Results[i].Category.Equals("Contraction"))
                    {
                        row["Category"] = "Contraction";
                        row["PeakTime"] = (Results[i].ArtifactData as PatternsAddOnManager.Contraction).PeakTime.ToString();
                    }
                    else if (Results[i].Category.Equals("Deceleration"))
                    {
                        row["Category"] = "Deceleration";
                        row["PeakValue"] = (Results[i].ArtifactData as PatternsAddOnManager.Deceleration).PeakValue.ToString();
                        row["PeakTime"] = (Results[i].ArtifactData as PatternsAddOnManager.Deceleration).PeakTime.ToString();
                        row["Confidence"] = (Results[i].ArtifactData as PatternsAddOnManager.Deceleration).Confidence.ToString();
                        row["Height"] = (Results[i].ArtifactData as PatternsAddOnManager.Deceleration).Height.ToString();
                        row["DecelerationCategory"] = (Results[i].ArtifactData as PatternsAddOnManager.Deceleration).DecelerationCategory;
                        var cs = (Results[i].ArtifactData as PatternsAddOnManager.Deceleration).ContractionStart;
                        row["ContractionStart"] = cs != null ? cs.Value.ToString() : String.Empty;
                    }
                    else
                        row["Category"] = "None";

                    row["StartTime"] = Results[i].StartTime.ToString();
                    row["EndTime"] = Results[i].EndTime.ToString();

                    if (RowExistsInTable(row))
                        continue;
                    else
                        ResultsTable.Rows.Add(row);
                }

            }
        }

        private bool RowExistsInTable(DataRow row)
        {
            foreach (DataRow item in ResultsTable.Rows)
            {
                if ((item["Category"] as String).Equals(row["Category"] as String) &&
                    (item["StartTime"] as String).Equals(row["StartTime"] as String) &&
                    (item["EndTime"] as String).Equals(row["EndTime"] as String))
                    return true;
            }

            return false;
        }

        protected virtual void GetResults()
        {
            var session = new PatternsSessionData();
            List<Artifact> Results = new List<Artifact>();
            int nClrTmp = 20;
            int nResInd = 0;
            var token = PatternsAddOnTestClient.Instance.CurrentToken;
            var client = new RestClient(PatternsAddOnTestClient.Instance.CurrentHost);
            var request = new RestRequest("Sessions/" + token + "/Artifacts", Method.GET) { Timeout = 6000, RequestFormat = DataFormat.Xml };
            var response = client.Execute(request);
            var content = response.Content; // raw content as string
            if (content.Equals(String.Empty))
                return;

            var sr = new StringReader(content);
            var contentElement = XElement.Load(sr);
            contentElement = XMLHelper.RemoveAllNamespaces(contentElement);
            AppendResultsArtifacts(contentElement);
            var newResults = from c in contentElement.Elements()
                             select c;

            if (newResults.Count() > 0)
            {
                var newDeserializedResults = ArtifactsHelper.DeserializeResults(newResults);
                AppendResults(Results, newDeserializedResults);
                if (CheckBoxFlush.Checked)
                {
                    if (CheckBoxArcive.Checked || TextBoxResult.Text.ToLower().IndexOf(".dat") > 0)
                        AppendResultsDAT(Results);

                    if (TextBoxResult.Text.ToLower().IndexOf(".xml") > 0)
                        AppendResultsXML(Results);
                }

                AppendToGrid(Results, nResInd);
                nResInd = Results.Count;
                LastDetected = UpdateLastDetectedTime(Results);
            }

            if (--nClrTmp < 0)
            {
                nClrTmp = 20;
                Results.Clear();
            }
        }

        protected virtual DateTime UpdateLastDetectedTime(List<Artifact> Results)
        {
            var last = (from b in Results
                        select b.EndTime).Max();

            return last;
        }

        public XElement ResultsXML { get; set; }
        public List<String> ResultsDAT { get; set; }
        public DateTime AbsoluteStart { get; set; }
        protected DateTime LastDetected { get; set; }
        protected DataTable ResultsTable { get; set; }
        public XElement ResultsArtifacts { get; set; }
        protected virtual CheckBox CheckBoxFlush { get { return null; } }
        protected virtual CheckBox CheckBoxArcive { get { return null; } }
        protected virtual TextBox TextBoxResult { get { return null; } }
        public DateTime LastTracingTimeStamp { get; set; }
    }
}
