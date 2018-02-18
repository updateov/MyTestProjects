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
using CommonLogger;
using MMSInterfaces;
using System.Diagnostics;

namespace Export.Plugin
{
    public class PluginTask : IPluginTask
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
                var laborer = PluginLaborer.Instance; 

                //TODO: create pluginuri with out using magic "/"
                Uri baseUri = new Uri(PluginsDataFeed);
                Uri pluginUri = new Uri(PluginsDataFeed + "/" + Name());
                m_host = new WebServiceHost(typeof(PluginWebHost), pluginUri);

                Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "ExportPluginTask", "Web host initialized");
                StartTime = DateTime.MinValue;
                bRes = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Critical, "ExportPluginTask", "Web host failed to initialize, please verify a valid url address in settings tool", ex);
                throw ex;
            }

            return bRes;
        }

        public bool Start()
        {
            m_host.Open();

            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "ExportPluginTask", "Export web host started");
            StartTime = DateTime.UtcNow;
            return true;
        }

        public bool Stop()
        {
            m_host.Close();

            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "ExportPluginTask", "Web host stopped");
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
                case "pluginaction":
                    ApplyPluginAction(request, response);
                    break;
                default:
                    break;
            }

            return true;
        }

        public void UpdateDowntime(bool isInDowntime, bool isAfterDowntime)
        {
            PluginLaborer.Instance.UpdateEpisodesDowntime(isInDowntime, isAfterDowntime);
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
                {
                    req.SetAttributeValue("interval", "-1");
                    req.SetAttributeValue("export", "-1");
                }
                else
                {
                    req.SetAttributeValue("interval", node.Attribute2Int("interval"));
                    req.SetAttributeValue("export", node.Attribute2Int("export"));
                }
            }

            if (response != null && response.Elements() != null)
            {
                PluginLaborer.Instance.UpdateEpisodesTracingStatus(response);
                PluginLaborer.Instance.UpdateEpisodesData(response, request);
                PluginLaborer.Instance.UpdateEpisodesParameters(response);
            }
        }

        private void ApplyAction(XElement request, XElement response)
        {
            PluginLaborer.Instance.PerformActionRecalculation(request);
        }

        private void ApplyPatients(XElement request, XElement response)
        {
            PluginLaborer.Instance.UpdateEpisodesList(response);
        }

        private void ApplyEpisodes(XElement request, XElement response)
        {
            PluginLaborer.Instance.UpdateEpisodesXML(request);
        }
       
        private void ApplyPluginAction(XElement request, XElement response)
        {
            PluginLaborer.Instance.PerformPluginAction(request, response);
        }

        public string Name()
        {
            return "ExportPlugin";
        }

        public bool IsEnabled()
        {
            bool toRet = true;
            try
            {
                if (PluginSettings.Instance.CALMServicesEnabled)
                {
                    NetTcpBinding bind = new NetTcpBinding(SecurityMode.Transport);
                    String url = PluginSettings.Instance.CALMServerLink;
                    EndpointAddress addr = new EndpointAddress(url);
                    ChannelFactory<IMMService> chn = new ChannelFactory<IMMService>(bind, addr);
                    IMMService conn = chn.CreateChannel();
                    MMSBooleanResponse res = conn.GetExportPluginEnabled();
                    if (res == null || res.ResponseCode != MMSResponseCode.Success || !res.BooleanValue)
                        toRet = false;
                    else
                        toRet = true;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "ExportPluginTask", "Error Server connection", ex);
            }

            return toRet;
        }
    }
}

