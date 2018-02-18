//Review: 23/03/15
using CommonLogger;
using PatternsPluginsCommon;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PatternsPluginsManager
{
    public class PluginsDataFeedService : IPluginsDataFeed
    {
        private static PluginsManagerSettings m_settings = new PluginsManagerSettings();

        #region ICurveDataFeed Methods

        public XElement GetCurveData(XElement param)
        {
            XElement result = null;
            try
            {
                var response = SendRestRequest("curve", Method.POST, param);
                result = XElement.Parse(response.Content);
                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on GetCurveData", ex);
            }

            return result;
        }

        public XElement UpdateFields(XElement param)
        {
            XElement result = null;
            try
            {
                var response = SendRestRequest("updatefields", Method.POST, param);
                result = XElement.Parse(response.Content);
                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on UpdateFields", ex);
            }

            return result;
        }

        public XElement GetSnapshotsList(XElement param)
        {
            XElement result = null;
            try
            {
                var response = SendRestRequest("snapshots", Method.POST, param);
                result = XElement.Parse(response.Content);
                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on GetSnapshotsList", ex);
            }

            return result;
        }

        public XElement GetDecisionSupportInformation(XElement param)
        {
            XElement result = null;
            try
            {
                var response = SendRestRequest("decisionSupport", Method.POST, param);
                result = XElement.Parse(response.Content);
                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on GetDecisionSupportInformation", ex);
            }

            return result;
        }

        public XElement GetPatternsData(XElement param)
        {
            XElement result = null;
            try
            {
                var response = SendRestRequest("data", Method.POST, param);
                result = XElement.Parse(response.Content);

                PatternsManager.Instance.UpdatePlugins(param, "data", result);
                result.SetAttributeValue("MeanBaselineFHRRoundingTo", PluginsManagerSettings.Instance.MeanBaselineFHRRoundingTo);
                result.SetAttributeValue("MVURoundingTo", PluginsManagerSettings.Instance.MVURoundingTo);
                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on GetPatternsData", ex);
            }

            return result;
        }

        public void PerformUserAction(XElement param)
        {
            try
            {
                var response = SendRestRequest("useraction", Method.POST, param);

                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
                PatternsManager.Instance.UpdatePlugins(param, "action", null);
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on PerformUserAction", ex);
            }

            return;
        }

        public XElement GetPatientList()
        {
            XElement result = null;
            try
            {
                var response = SendRestRequest("patients", Method.GET);
                result = XElement.Parse(response.Content);
                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on GetPatientList", ex);
            }

            return result;
        }

        public void ClosePatient(string id)
        {
            try
            {
                var response = SendRestRequest("close", Method.POST, id);
                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on PerformUserAction", ex);
            }

            return;
        }

        #endregion

        #region IPluginsDataFeed Methods

        public void PerformPluginAction(XElement param)
        {
            try
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
                PatternsManager.Instance.UpdatePlugins(param, "pluginaction", null);
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on PerformUserAction", ex);
            }

            return;
        }

        public XElement GetActivePlugins()
        {
            XElement xElemPlugins = new XElement("Plugins");

            var pluginsNames = PatternsManager.Instance.GetActivePluginsNames();
            foreach (var pluginName in pluginsNames)
	        {
		        XElement xElemPlugin = new XElement("Plugin");
                xElemPlugin.SetAttributeValue("Name", pluginName);

                xElemPlugins.Add(xElemPlugin);
	        }

            return xElemPlugins;
        }

        public XElement GetProductInformation()
        {
            XElement result = null;
            try
            {
                var response = SendRestRequest("productinfo", Method.GET);
                result = XElement.Parse(response.Content);
                WebOperationContext.Current.OutgoingResponse.StatusCode = response.StatusCode;
            }
            catch (Exception ex)
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                Logger.WriteLogEntry(TraceEventType.Error, "PatternsPluginsManager", "Error on GetProductInformation", ex);
            }

            return result;
        }
        #endregion

        private IRestResponse SendRestRequest(string requestName, Method method)
        {
            var client = new RestClient(m_settings.PatternsDataFeed);

            var request = new RestRequest(requestName, method);
            request.Timeout = m_settings.PatternsRequestTimeOut;

            // execute the request
            var response = client.Execute(request);

            return response;
        }

        private IRestResponse SendRestRequest(string requestName, Method method, XElement param)
        {
            var client = new RestClient(m_settings.PatternsDataFeed);

            var request = new RestRequest(requestName, method);
            request.Timeout = m_settings.PatternsRequestTimeOut;
            request.AddParameter("text/xml", param.ToString(), ParameterType.RequestBody);

            // execute the request
            var response = client.Execute(request);

            return response;
        }

        private IRestResponse SendRestRequest(string requestName, Method method, string token)
        {
            var client = new RestClient(m_settings.PatternsDataFeed);

            var request = new RestRequest(requestName + "/" + token, method);
            request.Timeout = m_settings.PatternsRequestTimeOut;

            // execute the request
            var response = client.Execute(request);

            return response;
        }
    }
}
