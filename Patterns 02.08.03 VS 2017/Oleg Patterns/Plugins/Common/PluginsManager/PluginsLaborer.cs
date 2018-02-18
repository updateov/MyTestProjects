using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using RestSharp;
using PatternsPluginsCommon;
using PluginsAlgorithms;
using CommonLogger;

namespace PatternsPluginsManager
{
    public class PluginsLaborer
    {
        private Object m_lock = new Object();

        private Timer m_updateEpisodesTimer;
        private bool m_isInDownTime;

        #region Singleton functionality

        private static PluginsLaborer s_pluginLaborer = null;
        private static Object s_lockObject = new Object();

        private PluginsLaborer()
        {
            m_updateEpisodesTimer = new Timer();
            m_isInDownTime = true;
            Task.Factory.StartNew(() => StartLaboring());
        }

        public async Task StartLaboring()
        {
            try
            {
                await Task.Factory.StartNew(() => DoWork());

                m_updateEpisodesTimer.Interval = PluginsManagerSettings.Instance.PatternsRequestInterval;
                m_updateEpisodesTimer.Elapsed += OnUpdateEpisodesTimerElapsed;
                m_updateEpisodesTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "StartLaboring", "Error occurred in StartLaboring.", ex);
            }
        }

        public static PluginsLaborer Instance
        {
            get
            {
                if (s_pluginLaborer == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_pluginLaborer == null)
                        {
                            s_pluginLaborer = new PluginsLaborer();
                        }
                    }
                }

                return s_pluginLaborer;
            }
        }

        #endregion

        private async void OnUpdateEpisodesTimerElapsed(object sender, ElapsedEventArgs e)
        {
            m_updateEpisodesTimer.Stop();
            await Task.Factory.StartNew(() => DoWork());
            m_updateEpisodesTimer.Start();
        }

        private async Task DoWork()
        {
            try
            {
                m_updateEpisodesTimer.Enabled = false;
                var watch = Stopwatch.StartNew();
                XElement patients = RequestPatientsList();

                bool isPatternsOffilne = patients.Attribute2Bool("IsOffline");
                bool isAfterDownTime = m_isInDownTime && !isPatternsOffilne;
                m_isInDownTime = isPatternsOffilne;

                if (!m_isInDownTime)
                {
                    PatternsManager.Instance.UpdatePluginsDowntime(m_isInDownTime, isAfterDownTime);
                    PatternsManager.Instance.UpdatePlugins(null, "patients", patients);

                    XElement request = GetEpisodesFromAllPlugins();
                    PatternsManager.Instance.UpdatePlugins(request, "episodes", null);

                    XElement response = await SendSplittedRequest(request);
                    //XElement response = RequestTracingData(request);
                    PatternsManager.Instance.UpdatePlugins(request, "data", response);
                }
                else
                {
                    PatternsManager.Instance.UpdatePluginsDowntime(m_isInDownTime, isAfterDownTime);
                }
                Logger.WriteLogEntry(TraceEventType.Verbose, 9996, "DoWork", string.Format("DoWork ended - {0} ms", watch.ElapsedMilliseconds));
            }
            catch (Exception ex)
            {
                m_isInDownTime = true;
                PatternsManager.Instance.UpdatePluginsDowntime(m_isInDownTime, false);
                Logger.WriteLogEntry(TraceEventType.Critical, "PluginsLaborer", "Error occurred in DoWork.", ex);
            }
            finally
            {
                m_updateEpisodesTimer.Enabled = true;
            }
        }

        private async Task<XElement> SendSplittedRequest(XElement request)
        {
            List<Task<XElement>> splittedRequests = new List<Task<XElement>>();
            XElement patientsElem = new XElement("patients", new XAttribute("user", "CALMLINK"));
            var reqElements = (from c in request.Elements("request")
                               select c).ToList();

            XElement partialRequest = null;
            int splitCount = 0;
            foreach (var item in reqElements)
            {
                if (splitCount <= 0 || partialRequest == null)
                {
                    splitCount = 0;
                    partialRequest = new XElement(patientsElem);
                }

                partialRequest.Add(item);
                if (++splitCount >= 3)
                {
                    splittedRequests.Add(RequestTracingData(partialRequest));
                    splitCount = 0;
                }
            }

            if (splitCount > 0)
                splittedRequests.Add(RequestTracingData(partialRequest));

            await Task.WhenAll(splittedRequests);

            XElement toRet = null;
            foreach (var item in splittedRequests)
            {
                if (toRet == null)
                {
                    toRet = new XElement(item.Result);
                }
                else
                {
                    var toAdd = (from c in item.Result.Elements("patient")
                                 select c).ToArray();

                    toRet.Add(toAdd);
                }
            }

            return toRet;
        }

        private XElement RequestPatientsList()
        {
            var client = new RestClient(PluginsManagerSettings.Instance.PatternsDataFeed);
            var request = new RestRequest("patients", Method.GET);
            request.Timeout = PluginsManagerSettings.Instance.PatternsRequestTimeOut;
            var response = client.Execute(request);

            XElement xElemEpisodes = XElement.Parse(response.Content);

            return xElemEpisodes;
        }

        private async Task<XElement> RequestTracingData(XElement xElemPatients)
        {
            XElement xElemResponse = null;
            await Task.Factory.StartNew(() =>
            {
                var client = new RestClient(PluginsManagerSettings.Instance.PatternsDataFeed);
                var request = new RestRequest("data", Method.POST);
                request.Timeout = PluginsManagerSettings.Instance.PatternsRequestTimeOut;
                request.AddParameter("text/xml", xElemPatients.ToString(), ParameterType.RequestBody);
                var response = client.Execute(request);

                xElemResponse = XElement.Parse(response.Content);
            });

            return xElemResponse;
        }

        public XElement PerformActionCallToPatterns(XElement param, String requestUri)
        {
            var client = new RestClient(PluginsManagerSettings.Instance.PatternsDataFeed);
            var request = new RestRequest(requestUri, Method.POST);
            request.Timeout = PluginsManagerSettings.Instance.PatternsRequestTimeOut;
            request.AddParameter("text/xml", param.ToString(), ParameterType.RequestBody);
            var response = client.Execute(request);

            // Get artifact's peak time
            if (response == null || response.Content == null || response.Content.Equals(String.Empty))
                return null;

            XElement responseArtifactData = XElement.Parse(response.Content);
            return responseArtifactData;
        }

        private XElement GetEpisodesFromAllPlugins()
        {
            XElement xElemPatients = new XElement("patients");
            xElemPatients.SetAttributeValue("version", PluginsManagerSettings.Instance.PatternsVersion);
            xElemPatients.SetAttributeValue("user", "CALMLINK");

            return xElemPatients;
        }
    }
}
