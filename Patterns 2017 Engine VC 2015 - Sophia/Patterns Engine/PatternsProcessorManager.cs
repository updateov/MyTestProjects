using MessagingInterfaces;
using MessagingResponses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Timers;

namespace PeriGen.Patterns.Engine
{
    public sealed class PatternsProcessorManager : IDisposable
    {
        public int PatternsProcessorMaxVisits { get; set; }

        private const string PatternsProcessorChildApp = "Perigen.Patterns.Processor.exe";
        private const string PatternsProcessorPipeName = "net.pipe://localhost/Child_{0}";
        private const int PatternsProcessorSyncTimeSec = 60;

        private Dictionary<string, string> m_VisitProcess = new Dictionary<string, string>();
        private Dictionary<string, int> m_ProcessAvailableSlots = new Dictionary<string, int>();
        private List<string> m_InvalidProcesses = new List<string>();
        private static object s_lock = new object();

        private System.Timers.Timer m_SyncChildProcessCollectionsTimer;

        #region Singleton functionality

        private static volatile PatternsProcessorManager s_instance;
        private static object s_lockObject = new object();

        private PatternsProcessorManager()
        {
            m_SyncChildProcessCollectionsTimer = new System.Timers.Timer(PatternsProcessorSyncTimeSec * 1000);
            m_SyncChildProcessCollectionsTimer.Elapsed += SyncChildProcessCollectionsTimer_Elapsed;
            m_SyncChildProcessCollectionsTimer.Enabled = true;
        }

        public static PatternsProcessorManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                            s_instance = new PatternsProcessorManager();
                    }
                }
                return s_instance;
            }
        }

        #endregion

        public bool InitializeVisit(string visitKey)
        {
            if (VisitHasProcessAssigned(visitKey))
                return true;

            string processGUID = FindFirstAvailableProcess();

            if (string.IsNullOrEmpty(processGUID))
                processGUID = CreateNewProcess();

            bool toRet = AssignVisitToProcess(visitKey, processGUID);

            LogProcessorsInfo();

            return toRet;
        }

        public bool ProcessVisit(string visitKey, byte[] ups, byte[] hrs, int startIndex, int length, ref StringBuilder data)
        {
            string processGUID = GetAssignedProcess(visitKey);
            using (ChannelFactory<IPatternsProcessorMessages> factory = CreateChannelFactory(processGUID))
            {
                IPatternsProcessorMessages channel = factory.CreateChannel();
                try
                {
                    ResultsEngineResponse response = channel.EngineProcessData(visitKey, ups, hrs, startIndex, length);
                    if (response.ResponseCode != EngineResponseCode.Success)
                        throw new Exception(response.ErrorMessage);

                    data = response.ResultsData;
                    return response.MoreData;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }

        internal void UninitializeVisit(string visitKey)
        {
            string processGUID = GetAssignedProcess(visitKey);
            if (string.IsNullOrEmpty(processGUID))
                return;

            bool processIsValid = IsValidProcess(processGUID);

            bool episodeRemoved = true;
            if (processIsValid)
                episodeRemoved = ExecuteRemoveEpisode(visitKey, processGUID);
            if (episodeRemoved)
            {
                bool terminateProcess = false;

                lock (s_lock)
                {
                    m_VisitProcess.Remove(visitKey);

                    m_ProcessAvailableSlots[processGUID]++;
                    if (m_ProcessAvailableSlots[processGUID] == PatternsProcessorMaxVisits)
                    {
                        m_ProcessAvailableSlots.Remove(processGUID);
                        terminateProcess = true;
                    }
                }

                LogProcessorsInfo();

                if (processIsValid && terminateProcess)
                {
                    // last visit removed from process => terminate process
                    ExecuteTerminateProcess(processGUID);
                }
            }
        }

        public bool AssignedProcessIsAlive(string visitKey)
        {
            string processGUID = GetAssignedProcess(visitKey);
            return ProcessIsAlive(processGUID);
        }

        private string CreateNewProcess()
        {
            string newProcessGUID = Guid.NewGuid().ToString();
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            string fileName = Path.Combine(path, PatternsProcessorChildApp);
            Process newProcess = Process.Start(fileName, newProcessGUID.ToString());
            int timeoutMilisec = 2 * 60 * 1000;
            newProcess.WaitForInputIdle(timeoutMilisec);

            // retry again for 2 minutes, every second
            int retryCount = 0;
            bool newProcessIsAlive = false;
            do
            {
                retryCount++;
                newProcessIsAlive = ProcessIsAlive(newProcessGUID);
                if (newProcessIsAlive)
                    break;
                else
                    Thread.Sleep(1000);

            } while (retryCount <= 2 * 60);

            if (!newProcessIsAlive)
            {
                string error = "Could not start Patterns Processor child process";
                PatternsProcessorWrapper.Source.TraceEvent(TraceEventType.Error, 100, error);
                throw new Exception(error);
            }

            lock (s_lock)
            {
                m_ProcessAvailableSlots.Add(newProcessGUID, PatternsProcessorMaxVisits);
            }

            return newProcessGUID;
        }

        private bool ProcessIsAlive(string processGUID)
        {
            try
            {
                ExecuteGetStatus(processGUID);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetAssignedProcess(string visitKey)
        {
            lock (s_lock)
            {
                string processGUID = string.Empty;
                if (m_VisitProcess.TryGetValue(visitKey, out processGUID))
                    return processGUID;
                else
                    return string.Empty;
            }
        }

        public void MarkProcessAsInvalid(string visitKey)
        {
            string processGUID = GetAssignedProcess(visitKey);

            lock (s_lock)
            {
                if (!string.IsNullOrEmpty(processGUID) && !m_InvalidProcesses.Contains(processGUID))
                    m_InvalidProcesses.Add(processGUID);
            }
        }

        private bool IsValidProcess(string processGUID)
        {
            lock (s_lock)
            {
                return !m_InvalidProcesses.Contains(processGUID);
            }
        }

        private string FindFirstAvailableProcess()
        {
            lock (s_lock)
            {
                KeyValuePair<string,int> processAvailableSlots = m_ProcessAvailableSlots.FirstOrDefault(item => item.Value > 0 && !m_InvalidProcesses.Contains(item.Key));
                if (!string.IsNullOrEmpty(processAvailableSlots.Key) && processAvailableSlots.Value > 0)
                    return processAvailableSlots.Key;
                else
                    return string.Empty;
            }
        }

        public bool VisitHasProcessAssigned(string visitKey)
        {
            string processGUID = GetAssignedProcess(visitKey);
            return string.IsNullOrEmpty(processGUID) ? false : true;
        }

        private bool AssignVisitToProcess(string visitKey, string processGUID)
        {
            bool success = ExecuteAddEpisode(visitKey, processGUID);

            if (success)
            {
                lock (s_lock)
                {
                    m_VisitProcess.Add(visitKey, processGUID);

                    m_ProcessAvailableSlots[processGUID]--;
                }

                return true;
            }
            else
                return false;
        }

        private void LogProcessorsInfo()
        {
            lock (s_lock)
            {
                List<string> allProcessGuids = m_VisitProcess.Select(item => item.Value).Distinct().ToList();

                StringBuilder message = new StringBuilder();
                if (allProcessGuids.Count > 0)
                {
                    foreach (string processGuid in allProcessGuids)
                    {
                        List<string> processVisits = m_VisitProcess.Where(visitProcess => visitProcess.Value == processGuid).Select(item => item.Key).ToList();
                        int noOfVisits = processVisits != null && processVisits.Count > 0 ? processVisits.Count : 0;
                        string visitsString = processVisits != null && processVisits.Count > 0 ? string.Join(";", processVisits) : string.Empty;
                        message.AppendFormat("Patterns Processor GUID: {0}. \r\n No of visits: {1} \r\n Visits: {2}. \r\n Available slots: {3}", 
                            processGuid, noOfVisits, visitsString, m_ProcessAvailableSlots[processGuid]);
                        message.AppendLine();
                    }
                }
                else
                    message.Append("No Patterns Processor information to log.");
                PatternsProcessorWrapper.Source.TraceEvent(TraceEventType.Verbose, 105, message.ToString());
            }
        }

        public void TerminateAllProcesses()
        {
            List<string> allProcessGuids = new List<string>();
            lock (s_lock)
            {
                allProcessGuids = m_VisitProcess.Select(item => item.Value).Where(processGUID => IsValidProcess(processGUID)).Distinct().ToList();
                m_InvalidProcesses.Clear();
                m_VisitProcess.Clear();
                m_ProcessAvailableSlots.Clear();
            }

            foreach (string processGuid in allProcessGuids)
            {
                try
                {
                    StatusEngineResponse response = ExecuteGetStatus(processGuid);
                    if (response.ResponseCode == EngineResponseCode.Success)
                    {
                        ExecuteRemoveAllEpisodes(processGuid);
                        Process p = Process.GetProcessById(response.ProcessId);
                        p.Kill();
                    }
                }
                catch (Exception)
                {
                    PatternsProcessorWrapper.Source.TraceEvent(TraceEventType.Warning, 110, "The Patterns Processor (GUID={0}) could not be stopped. It will automatically stop after the configured period of time.", processGuid);
                }
            }
        }

        private bool ExecuteAddEpisode(string visitKey, string processGUID)
        {
            using (ChannelFactory<IPatternsProcessorMessages> factory = CreateChannelFactory(processGUID))
            {
                IPatternsProcessorMessages channel = factory.CreateChannel();
                try
                {
                    EngineResponseBase response = channel.AddEpisode(visitKey);
                    if (response.ResponseCode != EngineResponseCode.Success)
                        throw new Exception(response.ErrorMessage);

                    return true;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }

        private bool ExecuteRemoveEpisode(string visitKey, string processGUID)
        {
            using (ChannelFactory<IPatternsProcessorMessages> factory = CreateChannelFactory(processGUID))
            {
                IPatternsProcessorMessages channel = factory.CreateChannel();
                try
                {
                    EngineResponseBase response = channel.RemoveEpisode(visitKey);
                    if (response.ResponseCode != EngineResponseCode.Success)
                        throw new Exception(response.ErrorMessage);

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }

        private bool ExecuteRemoveAllEpisodes(string processGUID)
        {
            using (ChannelFactory<IPatternsProcessorMessages> factory = CreateChannelFactory(processGUID))
            {
                IPatternsProcessorMessages channel = factory.CreateChannel();
                try
                {
                    EngineResponseBase response = channel.RemoveAllEpisodes();
                    if (response.ResponseCode != EngineResponseCode.Success)
                        throw new Exception(response.ErrorMessage);

                    return true;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }

        private StatusEngineResponse ExecuteGetStatus(string processGUID)
        {
            using (ChannelFactory<IPatternsProcessorMessages> factory = CreateChannelFactory(processGUID))
            {
                IPatternsProcessorMessages channel = factory.CreateChannel();
                try
                {
                    StatusEngineResponse response = channel.GetStatus();
                    if (response.ResponseCode != EngineResponseCode.Success)
                        throw new Exception(response.ErrorMessage);

                    return response;
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }

        private void ExecuteTerminateProcess(string processGUID)
        {
            using (ChannelFactory<IPatternsProcessorMessages> factory = CreateChannelFactory(processGUID))
            {
                IPatternsProcessorMessages channel = factory.CreateChannel();
                try
                {
                    channel.TerminateProcess();
                }
                catch (Exception ex)
                {
                    throw;
                }
                finally
                {
                    CloseChannel((ICommunicationObject)channel);
                }
            }
        }

        private ChannelFactory<IPatternsProcessorMessages> CreateChannelFactory(string processGUID)
        {
            return new ChannelFactory<IPatternsProcessorMessages>(new NetNamedPipeBinding(), new EndpointAddress(string.Format(PatternsProcessorPipeName, processGUID/*"e6be380e-b21a-4596-a1a2-c83241c09097"*/)));
        }

        private void CloseChannel(ICommunicationObject channel)
        {
            try
            {
                channel.Close();
            }
            catch (Exception)
            {
            }
            finally
            {
                channel.Abort();
            }
        }

        private void SyncChildProcessCollectionsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PatternsProcessorWrapper.Source.TraceEvent(TraceEventType.Verbose, 110, "Timer to send message to keep Patterns Processes alive was called");
            SendMessageToKeepChildsAlive();
        }

        private void SendMessageToKeepChildsAlive()
        {
            // send message to all child processes so that they are not automatically terminated
            List<string> allProcessGuids = new List<string>();
            lock (s_lock)
            {
                allProcessGuids = m_VisitProcess.Select(item => item.Value).Distinct().ToList();
            }

            foreach (string guid in allProcessGuids)
            {
                try
                {
                    ExecuteGetStatus(guid);
                }
                catch (Exception)
                {
                }
            }
        }

        public void Dispose()
        {
            m_SyncChildProcessCollectionsTimer.Dispose();
        }
    }
}
