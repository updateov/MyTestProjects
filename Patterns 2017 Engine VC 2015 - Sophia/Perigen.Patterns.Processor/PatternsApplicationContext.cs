using MessagingInterfaces;
using MessagingResponses;
using PatternsEngineBridge;
using System;
using System.Diagnostics;
using System.ServiceModel;
using System.Text;
using System.Windows.Forms;

namespace Perigen.Patterns.Processor
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class PatternsApplicationContext : ApplicationContext, IPatternsProcessorMessages
    {
        ServiceHost Host { get; set; }
        public EngineBridge Bridge { get; private set; }

        Timer m_InactivityTimer;
        DateTime m_LastRequest = DateTime.Now;
        const int PatternsProcessorInactivityTimerSec = 60;

        public PatternsApplicationContext(String guid)
        {
            try
            {
                Bridge = new EngineBridge();

                Host = new ServiceHost(this);
                NetNamedPipeBinding binding = new NetNamedPipeBinding()
                {
                    MaxReceivedMessageSize = 2147483647,
                    CloseTimeout = new TimeSpan(0, 1, 0),
                    OpenTimeout = new TimeSpan(0, 1, 0),
                    ReceiveTimeout = new TimeSpan(0, 3, 0),
                    SendTimeout = new TimeSpan(0, 1, 0)
                };

                Host.AddServiceEndpoint((typeof(IPatternsProcessorMessages)), binding, "net.pipe://localhost/Child_" + guid);
                Host.Open();

                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 210, "Patterns Processor (PID={0}, GUID={1}): ServiceHost initialized", 
                    PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID);

                m_InactivityTimer = new Timer();
                m_InactivityTimer.Interval = PatternsProcessorInactivityTimerSec * 1000;
                m_InactivityTimer.Tick += InactivityTimer_Tick;
                m_InactivityTimer.Enabled = true;
            }
            catch (Exception ex)
            {
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Error, 215, "Patterns Processor (PID={0}, GUID={1}): Process initialization failed. \r\n {2}",
                    PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, ex);
                throw;
            }
        }

        public StatusEngineResponse GetStatus()
        {
            StatusEngineResponse toRet;

            try
            {
                m_LastRequest = DateTime.Now;

                string[] activeVisits = Bridge.ActiveVisits;
                //LogActiveVisits(activeVisits);

                toRet = new StatusEngineResponse()
                {
                    ProcessId = PatternsProcessorSettings.Instance.ProcessId,
                    ActiveVisits = activeVisits,
                    ResponseCode = EngineResponseCode.Success
                };
            }
            catch (Exception ex)
            {
                toRet = new StatusEngineResponse()
                {
                    ResponseCode = EngineResponseCode.Error,
                    ErrorMessage = ex.ToString()
                };
            }

            return toRet;
        }

        public EngineResponseBase AddEpisode(String visitKey)
        {
            EngineResponseBase toRet;

            try
            {
                m_LastRequest = DateTime.Now;

                Bridge.AddEpisode(visitKey);
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 225, "Patterns Processor (PID={0}, GUID={1}): Visit {2} added.", 
                    PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, visitKey);

                toRet = new EngineResponseBase()
                {
                    ResponseCode = EngineResponseCode.Success
                };
            }
            catch (Exception ex)
            {
                toRet = new EngineResponseBase()
                {
                    ResponseCode = EngineResponseCode.Error,
                    ErrorMessage = ex.ToString()
                };
            }

            LogActiveVisits(Bridge.ActiveVisits);

            return toRet;
        }

        public EngineResponseBase RemoveEpisode(String visitKey)
        {
            EngineResponseBase toRet;

            try
            {
                m_LastRequest = DateTime.Now;

                Bridge.RemoveEpisode(visitKey);
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 230, "Patterns Processor (PID={0}, GUID={1}): Visit {2} removed.",
                    PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, visitKey);

                toRet = new EngineResponseBase()
                {
                    ResponseCode = EngineResponseCode.Success
                };
            }
            catch (Exception ex)
            {
                toRet = new EngineResponseBase()
                {
                    ResponseCode = EngineResponseCode.Error,
                    ErrorMessage = ex.ToString()
                };
            }

            LogActiveVisits(Bridge.ActiveVisits);

            return toRet;
        }

        public EngineResponseBase RemoveAllEpisodes()
        {
            EngineResponseBase toRet;

            try
            {
                m_LastRequest = DateTime.Now;

                Bridge.RemoveAllEpisodes();
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 233, "Patterns Processor (PID={0}, GUID={1}): All episodes removed.",
                    PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID);

                toRet = new EngineResponseBase()
                {
                    ResponseCode = EngineResponseCode.Success
                };
            }
            catch (Exception ex)
            {
                toRet = new EngineResponseBase()
                {
                    ResponseCode = EngineResponseCode.Error,
                    ErrorMessage = ex.ToString()
                };
            }

            LogActiveVisits(Bridge.ActiveVisits);

            return toRet;
        }

        public ResultsEngineResponse EngineProcessData(string visitKey, byte[] ups, byte[] hrs, int startIndex, int length)
        {
            ResultsEngineResponse toRet;

            try
            {
                m_LastRequest = DateTime.Now;

                // Process the data AS IS
                int buffer_size = 0;
                bool moreData = false;

                // Simulate a limited seconds feed of 1 day of tracing at once maximum
                int time_frame = 86400;
                int position = startIndex;
                int end = startIndex + length;
                while (position < end)
                {
                    int block_size = Math.Min(time_frame, end - position);
                    buffer_size = Bridge.EngineProcessData(visitKey, ups, position, block_size, hrs, 4 * position, 4 * block_size);
                    position += time_frame;
                }

                StringBuilder data;
                if (buffer_size > 0)
                {
                    data = new StringBuilder(buffer_size);
                    moreData = Bridge.EngineReadResults(visitKey, ref data, buffer_size);
                }
                else
                {
                    data = new StringBuilder();
                    moreData = false;
                }

                //PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 235, "Patterns Processor (PID={0}, GUID={1}): Tracings processed for visit {2}.",
                //   PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, visitKey);

                toRet = new ResultsEngineResponse()
                {
                    ResultsData = data,
                    MoreData = moreData,
                    ResponseCode = EngineResponseCode.Success
                };
            }
            catch (Exception ex)
            {
                toRet = new ResultsEngineResponse()
                {
                    ResponseCode = EngineResponseCode.Error,
                    ErrorMessage = ex.ToString()
                };
            }

            return toRet;
        }

        public void TerminateProcess()
        {
            PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 240, "Patterns Processor (PID={0}, GUID={1}): Terminate process.", 
                PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID);

            ReleaseResources();
            ExitThread();
        }

        private static void LogActiveVisits(string[] activeVisits)
        {
            int noOfActiveVisits = activeVisits != null ? activeVisits.Length : 0;
            string activeVisitsString = activeVisits != null && activeVisits.Length > 0 ? string.Join(";", activeVisits) : string.Empty;
            PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 220, "Patterns Processor (PID={0}, GUID={1}): List of active visits \r\n No of active visits = {2} \r\n Active visits = {3}",
                PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, noOfActiveVisits, activeVisitsString);
        }

        private void ReleaseResources()
        {
            try
            {
                Bridge.RemoveAllEpisodes();

                m_InactivityTimer.Enabled = false;
                m_InactivityTimer.Tick -= InactivityTimer_Tick;
                m_InactivityTimer.Dispose();
                m_InactivityTimer = null;

                Host.Close();
            }
            catch (Exception ex)
            {
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Warning, 245, "Patterns Processor (PID={0}, GUID={1}): Process resources could not be fully released. \r\n {2}",
                        PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, ex);
            }
        }

        private void InactivityTimer_Tick(object sender, EventArgs e)
        {
            if (m_LastRequest.Add(new TimeSpan(0, PatternsProcessorSettings.Instance.PatternsProcessorTerminateTimeout, 0)) < DateTime.Now)
            {
                PatternsProcessorSettings.Source.TraceEvent(TraceEventType.Verbose, 250, "Patterns Processor (PID={0}, GUID={1}): Process not active for {2} minutes. It will be terminated.", 
                    PatternsProcessorSettings.Instance.ProcessId, PatternsProcessorSettings.Instance.ProcessGUID, PatternsProcessorSettings.Instance.PatternsProcessorTerminateTimeout);

                TerminateProcess();
            }
        }
    }
}
