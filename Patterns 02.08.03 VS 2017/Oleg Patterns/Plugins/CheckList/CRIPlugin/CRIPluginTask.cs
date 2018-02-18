//Review: 17/02/15
//Review: 23/03/15
using CommonLogger;
using PatternsPluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CRIPlugin
{
    public class CRIPluginTask : IPluginTask
    {
        #region Properties & Members

        public static DateTime StartTime { get; private set; }
        private WebServiceHost m_host;

        #endregion

        //D.A. Note: creating webHost in plugin does not create addition Windows service (which is good)
        public bool Init(string PluginsDataFeed)
        {
            bool bRes = false;
            try
            {
                var lab = CRIPluginLaborer.Instance;

                //TODO: create pluginuri with out using magic "/"
                Uri baseUri = new Uri(PluginsDataFeed);
                Uri pluginUri = new Uri(PluginsDataFeed + "/" + Name());
                m_host = new WebServiceHost(typeof(CRIPluginHost), pluginUri);

                Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "CRIPluginTask", "Web host initialized");
                StartTime = DateTime.MinValue;

                bRes = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Critical, "CRIPluginTask", "Web host failed to initialize, please verify a valid url address in settings tool", ex);
                throw ex;
            }

            return bRes;
        }

        public bool Start()
        {
            m_host.Open();

            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "CRIPluginTask", "CRI web host started");
            StartTime = DateTime.UtcNow;
            return true;
        }

        public bool Stop()
        {
            m_host.Close();

            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "CRIPluginTask", "Web host stopped");
            StartTime = DateTime.MinValue;
            return true;
        }

        public bool Apply(XElement request, string requestName, XElement response)
        {
            switch (requestName.ToLower())
            {
                case "data":
                    ApplyData(request, response);
                    break;
                case "action":
                    ApplyAction(request, response);
                    break;
                case "patients":
                    ApplyPatients(request, response);
                    break;
                case "episodes":
                    ApplyEpisodes(request, response);
                    break;
                default:
                    break;
            }

            return true;
        }

        private void ApplyData(XElement request, XElement response)
        {
            foreach (var node in request.Descendants("request"))
            {
                if (node.Attribute("key") == null)
                    continue;

                var req = (from c in response.Descendants("request")
                           where c.Attribute("key").Value.Equals(node.Attribute("key").Value)
                           select c).FirstOrDefault();

                if (req == null)
                    continue;

                if (node.Attribute("incremental") != null && node.Attribute("incremental").Value.Equals("0"))
                    req.SetAttributeValue("contractility", "-1");
                else
                    req.SetAttributeValue("contractility", node.Attribute("contractility") != null ? node.Attribute("contractility").Value : "-1");
            }

            if (response != null && response.Elements() != null)
            {
                CRIPluginLaborer.Instance.UpdateEpisodesTracingStatus(response);
                CRIPluginLaborer.Instance.UpdateEpisodesData(response, request);
                CRIPluginLaborer.Instance.UpdateEpisodesParameters(response);
            }
        }

        private void ApplyAction(XElement request, XElement response)
        {
            CRIPluginLaborer.Instance.PerformActionRecalculation(request);
        }

        private void ApplyPatients(XElement request, XElement response)
        {
            CRIPluginLaborer.Instance.UpdateEpisodesList(response);
        }

        private void ApplyEpisodes(XElement request, XElement response)
        {
            CRIPluginLaborer.Instance.UpdateEpisodesXML(request);
        }

        public void UpdateDowntime(bool isInDowntime, bool isAfterDowntime)
        {
            CRIPluginLaborer.Instance.UpdateEpisodesDowntime(isInDowntime, isAfterDowntime);
        }

        public string Name()
        {
            return "CheckListPlugin";
        }


        public bool IsEnabled()
        {
            return true;
        }
    }
}
