using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using System.ServiceModel;
using PatternsAddOnManager;
using PeriGenSettingsManager;
using CommonLogger;

namespace PatternsAddOnService
{
    public class AddOnTask
    {
        public AddOnTask()
        {
            String uriStr = "http://localhost:" + Settings_WebServicePort.ToString() + "/PatternsAddOnService";
            Host = new WebServiceHost(typeof(PatternsService), new Uri(uriStr));
            var bind = new WebHttpBinding();
            bind.MaxReceivedMessageSize = 2147483647;
            bind.ReaderQuotas.MaxArrayLength = 2147483647;
            bind.ReaderQuotas.MaxStringContentLength = 2147483647;
            Host.AddServiceEndpoint(typeof(IPatternsService), bind, Settings_WebServiceEndpoint);
            StartTime = DateTime.MinValue;
            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "Patterns Add On Service, Add On Task, Service startup", "Service started");
        }

        public void StartTask()
        {
            Host.Open();
            StartTime = DateTime.UtcNow;
        }

        public void StopTask()
        {
            var guidHndlr = GUIDHandler.Init();
            guidHndlr.PurgeAll();
            Host.Close();
            Logger.WriteLogEntry(System.Diagnostics.TraceEventType.Information, "Patterns Add On Service, Add On Task, Service stop", "Service stopped");
        }

        static int Settings_WebServicePort = AppSettingsMngr.GetSettingsIntValue("WebServicePort");
        static String Settings_WebServiceEndpoint = AppSettingsMngr.GetSettingsStrValue("WebServiceEndpoint");
        public static DateTime StartTime { get; private set; }
        private WebServiceHost Host { get; set; }
    }
}
