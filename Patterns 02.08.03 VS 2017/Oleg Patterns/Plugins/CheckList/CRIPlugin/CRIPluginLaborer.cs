//Review: 23/03/15
using CRIAlgorithm;
using RestSharp;
using PatternsPluginsCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;
using PluginsAlgorithms;
using CRIEntities;
using PatternsEntities;
using PatternsPluginsManager;
using CommonLogger;
using System.ServiceModel;
using MMSInterfaces;

namespace CRIPlugin
{
    public class CRIPluginLaborer
    {
        private const string SYSTEM_USER_NAME = "SYSTEM";
        private Object m_lock = new Object();
        private XElement m_CRIAlgorithmSettings = null;

        public ConcurrentDictionary<int, CRIPluginEpisode> Episodes { get; set; }
        private List<ConceptInfo> m_saveConceptsInfo = new List<ConceptInfo>();
        private List<int> m_conceptsIds = new List<int>();

        #region Singleton functionality

        private static CRIPluginLaborer s_criPluginLaborer = null;
        private static Object s_lockObject = new Object();

        private int m_CRIStateConceptId = CRIPluginSettings.Instance.CRIStateConceptID;
        private CRIPluginLaborer()
        {
            m_conceptsIds.Add(m_CRIStateConceptId);
            if (m_CRIStateConceptId != 0)
                InitConceptsInfoList();

            Episodes = new ConcurrentDictionary<int, CRIPluginEpisode>();
            InitCRIAlgorithmSettings();
            InitLaborer();
        }

        private void InitConceptsInfoList()
        {
            if (CRIPluginSettings.Instance.CRIStateConceptID == 0)
                return;

            try
            {
                NetTcpBinding bind = new NetTcpBinding(SecurityMode.Transport);
                String url = CRIPluginSettings.Instance.CALMServerLink;
                EndpointAddress addr = new EndpointAddress(url);
                ChannelFactory<IMMService> chn = new ChannelFactory<IMMService>(bind, addr);
                IMMService conn = chn.CreateChannel();
                MMSConceptsInfoResponse res = conn.GetConceptsInfo(m_conceptsIds);
                m_saveConceptsInfo = res.ConceptsInfo;
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CRIPLuginLaborer", "Error getting concepts info", ex);
            }
        }

        private void InitCRIAlgorithmSettings()
        {
            m_CRIAlgorithmSettings = new XElement("criAlgorithmSettings",
                    new XAttribute("minBaselineVariability", CRIAlgorithmSettings.Instance.MinimalBaselineVariability),
                    new XAttribute("accelsRate", CRIAlgorithmSettings.Instance.MinimalAccelerationsAmount),
                    new XAttribute("lateDecelConfidence", CRIAlgorithmSettings.Instance.MinimalLateDecelConfidence),
                    new XAttribute("lateDecelsRate", CRIAlgorithmSettings.Instance.MinimalLateDecelAmount),
                    new XAttribute("prolongedDecelHeight", CRIAlgorithmSettings.Instance.MinimalProlongedDecelHeight),
                    new XAttribute("lateAndProlongedDecelsRate", CRIAlgorithmSettings.Instance.MinimalLateAndProlongedDecelAmount),
                    new XAttribute("lateAndLargeAndLongDecelRate", CRIAlgorithmSettings.Instance.MinimalLateAndLargeAndLongDecelAmount),
                    new XAttribute("largeAndLongDecelRate", CRIAlgorithmSettings.Instance.MinimalLargeAndLongDecelAmount),
                    new XAttribute("contractionRate", CRIAlgorithmSettings.Instance.MinimalContractionsAmount),
                    new XAttribute("longContractionRate", CRIAlgorithmSettings.Instance.MinimalLongContractionsAmount),
                    new XAttribute("minDataWindowToQualify", CRIAlgorithmSettings.Instance.CRIStateQualificationWindowSize),
                    new XAttribute("minAmountOfDataInWindow", CRIAlgorithmSettings.Instance.MinimalAmountOfDataInQualificationWindow));
        }

        private void InitLaborer()
        {
            lock (m_lock)
            {
                try
                {
                    List<CRIPluginEpisode> episodes = CRIPluginDBManager.Instance.LoadEpisodes();
                    foreach (CRIPluginEpisode episode in episodes)
                    {
                        if (Episodes.TryAdd(episode.EpisodeId, episode) && m_CRIStateConceptId != 0)
                        {
                            //Task.Factory.StartNew(() => SaveCRIState(episode.VisitKey, episode.CurrentCRIState));
                            episode.CRIStateChanged += Episode_CRIStateChanged;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "StartLaboring", "Error occurred in StartLaboring.", ex);
                }
            }
        }

        private void SaveCRIState(String visitKey, ExposedCRIState episodeState)
        {
            if (CRIPluginSettings.Instance.CRIStateConceptID == 0)
            {
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginLaborer", "CRIStateConceptID = 0!!!");
                return;
            }

            if (m_saveConceptsInfo == null || m_saveConceptsInfo.Count <= 0)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginLaborer", "m_saveConceptsInfo is null or empty, probably MMS is down or not installed");
                return;
            }

            try
            {
                List<BaseConcept> tosave = new List<BaseConcept>();
                StringConcept toAdd = new StringConcept()
                {
                    ConceptTime = DateTime.Now,
                    Id = CRIPluginSettings.Instance.CRIStateConceptID,
                    Name = m_saveConceptsInfo[0].Caption,
                    OOC = 1,
                    Value = episodeState.ToString()
                };

                tosave.Add(toAdd);
                NetTcpBinding bind = new NetTcpBinding(SecurityMode.Transport);
                String url = CRIPluginSettings.Instance.CALMServerLink;
                EndpointAddress addr = new EndpointAddress(url);
                ChannelFactory<IMMService> chn = new ChannelFactory<IMMService>(bind, addr);
                IMMService conn = chn.CreateChannel();
                conn.SaveObservations(visitKey, tosave, m_saveConceptsInfo, "\\CALMLINK");
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginLaborer", "Error saving observation", ex);
            }
        }

        private void Episode_CRIStateChanged(object sender, CRIStateChangedEventArgs e)
        {
            SaveCRIState(e.VisitKey, e.State);
        }

        public static CRIPluginLaborer Instance
        {
            get
            {
                if (s_criPluginLaborer == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_criPluginLaborer == null)
                        {
                            s_criPluginLaborer = new CRIPluginLaborer();
                        }
                    }
                }

                return s_criPluginLaborer;
            }
        }

        #endregion

        public static long CurrentMemoryUsage()
        {
            long currentMemoryUsage;
            string nameToUseForMemory = Process.GetCurrentProcess().ProcessName;

            using (var procPerfCounter = new PerformanceCounter("Process", "Working Set - Private", nameToUseForMemory))
            {
                currentMemoryUsage = procPerfCounter.RawValue / 1024; //in KB
            }

            return currentMemoryUsage;
        }

        public static void LogCurrentMemoryUsage(string logDescription)
        {
            if (DateTime.Now.Minute % 10 == 0 && DateTime.Now.Second < 15)
            {
                long currentMemoryUsage = CurrentMemoryUsage();
                string logMessage = logDescription + "  Working Set = " + currentMemoryUsage.ToString();

                Logger.WriteLogEntry(TraceEventType.Information, "CRIPluginLaborer", logMessage);
            }
        }

        public void UpdateEpisodesList(XElement param)
        {
            lock (m_lock)
            {
                List<int> episodesKeysToHistory = Episodes.Keys.ToList();

                var xElemEpisodes = param.Elements("Episode");
                foreach (XElement xElemEpisode in xElemEpisodes)
                {
                    var xElemPatient = xElemEpisode.Element("Patient");
                    int episodeID = xElemEpisode.Attribute2Int("PatientUniqueId");
                    if (!Episodes.ContainsKey(episodeID))
                    {
                        CRIPluginEpisode pluginEpisode = new CRIPluginEpisode(episodeID);
                        Episodes[episodeID] = pluginEpisode;
                        pluginEpisode.CRIStateChanged += Episode_CRIStateChanged;
                    }

                    string episodeStatus = xElemEpisode.Attribute2String("EpisodeStatus");
                    if (episodeStatus == "Admitted" || episodeStatus == "New")
                    {
                        Episodes[episodeID].EpisodeStatus = EpisodeStatus.Admitted;

                        Episodes[episodeID].VisitKey = xElemPatient.Attribute2String("Key");
                        Episodes[episodeID].Fetuses = xElemPatient.Attribute2Int("Fetuses");
                        Episodes[episodeID].GA = xElemPatient.Attribute2String("GA");

                        episodesKeysToHistory.Remove(episodeID);
                    }
                    else if (episodeStatus == "Discharged")
                    {
                        Episodes[episodeID].EpisodeStatus = EpisodeStatus.Discharged;

                        episodesKeysToHistory.Remove(episodeID);
                    }
                }

                if (episodesKeysToHistory.Count > 0)
                {
                    foreach (var episodeKey in episodesKeysToHistory)
                    {
                        Episodes[episodeKey].EpisodeStatus = EpisodeStatus.ToHistory;
                        Episodes[episodeKey].SetPersistenciesToAck(SYSTEM_USER_NAME, DateTime.UtcNow);
                    }
                }

                DischargeEpisodes();
            }
        }

        public void UpdateEpisodesTracingStatus(XElement data)
        {
            lock (m_lock)
            {
                try
                {
                    var xElemPatients = data.Elements();
                    foreach (var xElemPatient in xElemPatients)
                    {
                        var xElemRequest = xElemPatient.Element("request");
                        int episodeID = xElemRequest.Attribute2Int("id");

                        //get episode
                        CRIPluginEpisode episode = GetAdmittedEpisode(episodeID);
                        if (episode == null)
                            continue;

                        //update episode parameters of patient
                        episode.TracingStatus = (TracingStatus)xElemPatient.Attribute2Int("status");
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginLaborer", "Error occurred in UpdateEpisodesTracingStatus.", ex);
                }
            }
        }

        public void UpdateEpisodesData(XElement data, XElement clientCall = null)
        {
            lock (m_lock)
            {
                try
                {
                    var patients = data.Elements();

                    List<CRIPluginEpisode> episodesToSave = new List<CRIPluginEpisode>();
                    List<KeyValuePair<XElement, int>> requestsMaxIds = new List<KeyValuePair<XElement, int>>();
                    foreach (var patient in patients)
                    {
                        XElement artifacts, tracings, request;
                        int episodeID;
                        XMLHelpers.CollectPatientData(patient, out artifacts, out tracings, out request, out episodeID);

                        int clientCallAction = -1;
                        if (clientCall != null)
                        {
                            var requests = clientCall.Elements();
                            var clientRequest = (from c in clientCall.Elements()
                                                 where c.Attribute2String("key").Equals(request.Attribute2String("key"))
                                                 select c).First();

                            clientCallAction = clientRequest.Attribute2Int("action");
                        }

                        CRIPluginEpisode episode = GetAdmittedEpisode(episodeID);
                        if (episode == null || (episode.EpisodeStatus != EpisodeStatus.Admitted))
                            continue;

                        try
                        {
                            bool isValid = XMLHelpers.ValidatePatient(patient);

                            // Client specific data
                            bool bIncremental = false;
                            int requestLastMegeID = -1;
                            int episodeLastMergeID = episode.LastMergeId;
                            DateTime requestMergeTime = DateTime.MinValue;
                            if (request != null)
                            {
                                bIncremental = request.Attribute("incremental") == null ||
                                        request.Attribute("incremental") != null && !request.Attribute("incremental").Value.Equals("0");
                                if (request.Attribute("merge") != null)
                                {
                                    requestLastMegeID = Int32.Parse(request.Attribute("merge").Value ?? "-1", System.Globalization.CultureInfo.InvariantCulture);

                                    if (request.Attribute("mergetime") != null)
                                    {
                                        var mergeTime = request.Attribute("mergetime").Value;
                                        long epochMergeTime;
                                        if (mergeTime != "-1" && Int64.TryParse(mergeTime, out epochMergeTime))
                                        {
                                            requestMergeTime = epochMergeTime.ToDateTime();
                                        }
                                    }
                                }

                            }
                            episode.LastMergeId = requestLastMegeID;
                            episode.LastMergeTime = requestMergeTime;

                            bool merge = false;
                            if (requestLastMegeID > episodeLastMergeID)
                            {
                                episode.ResetEpisode();                               
                                bIncremental = false;
                                merge = true;
                            }

                            int contractility = (!merge && request.Attribute("contractility") != null) ? request.Attribute2Int("contractility") : episode.Contractility;
                            int actionId = request.Attribute2Int("action");
                            // END Client specific data

                            //Logger.WriteLogEntry(TraceEventType.Information, "CRILaborer::UpdateEpisodesData",
                            //                        "data.action: " + request.Attribute("action").Value);

                            CRICalculator calc = episode.ContractilitiesCalculator;
                            CollectUPTracings(tracings, bIncremental, calc);

                            var newActions = (from c in calc.ActionId2ContractilityId
                                              where c.Key > clientCallAction
                                              select c).ToList();

                            XElement contractilities = null;
                            int startId = -1;
                            List<Contractility> contractilitiesToEncode = null;

                            //Logger.WriteLogEntry(TraceEventType.Information, "CRILaborer::UpdateEpisodesData",
                            //                        "ActionId2ContractilityId: " + calc.ActionId2ContractilityId.Count().ToString());

                            if ((newActions != null && newActions.Count() > 0) || (clientCallAction < actionId && calc.ActionId2ContractilityId.Count > 0))
                            {
                                startId = newActions.Min(c => c.Value);
                                contractilitiesToEncode = (from c in calc.Contractilities
                                                           where c.Id >= startId
                                                           orderby c.StartTime
                                                           select c).ToList();

                                if (contractilitiesToEncode != null && contractilitiesToEncode.Count() > 0)
                                {
                                    long firstContractilityTime = contractilitiesToEncode.ElementAt(0).StartTime;
                                    if (artifacts == null)
                                    {
                                        contractilities = EncodeContractilitiesForActivex(ref firstContractilityTime, contractilitiesToEncode, startId - 1, isValid);
                                        patient.Add(contractilities);
                                    }
                                }
                            }

                            // Calculation core
                            if (artifacts != null)
                            {
                                var startTime = artifacts.Attribute("basetime").Value;
                                Logger.WriteLogEntry(TraceEventType.Verbose, "Algorithm", "basetime: " + startTime);
                                long absoluteStart;
                                Int64.TryParse(startTime, out absoluteStart);
                                var artifactsList = artifacts.Elements();

                                List<PluginDetectionArtifact> detectedObjects = XMLHelpers.DeserializeDetectedArtifacts(absoluteStart, artifactsList);
                                calc.CalculateContractility(detectedObjects, contractility, absoluteStart, bIncremental);

                                if (merge || episode.IsAfterDownTime)
                                    PerformCalculationsAfterDownTime(episode, calc, detectedObjects);

                                // Prepare return data to clients
                                if (newActions == null || newActions.Count() <= 0 || !bIncremental)
                                    contractilities = EncodeContractilitiesForActivex(ref absoluteStart, calc.Contractilities, contractility, isValid);
                                else
                                {
                                    contractilities = EncodeContractilitiesForActivex(ref absoluteStart, contractilitiesToEncode, startId - 1, isValid);
                                }

                                if (contractilities != null)
                                {
                                    patient.Add(contractilities);
                                }

                                if (!bIncremental)
                                {
                                    patient.Add(m_CRIAlgorithmSettings);
                                }

                                if (bIncremental)
                                {
                                    episode.UpdatePersistenceHistory(-1, false);
                                    episodesToSave.Add(episode);
                                }

                                if (!merge && episode.IsAfterDownTime)
                                    episode.AcknowledgeOldPositivePersistences();

                            } //END if (artifacts != null)


                            if (request != null)
                            {
                                int maxId = calc.Contractilities.Count > 0 ? calc.Contractilities.Max(a => a.Id) : -1;
                                requestsMaxIds.Add(new KeyValuePair<XElement, int>(request, maxId));
                            }

                            if (contractilities != null)
                            {
                                if (bIncremental)
                                    Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginLaborer", "XML to client:\r\n" + patient.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginLaborer", "Failed to update episode data: " + episodeID, ex);
                        }
                    } // END foreach (var patient in patients)

                    bool success = CRIPluginEpisode.SaveEpisodes(episodesToSave);
                    if (success)
                        SetRequestContractilityIds(requestsMaxIds);
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginLaborer", "Error occurred in UpdateEpisodesData.", ex);
                }
            }
        }

        private void SetRequestContractilityIds(List<KeyValuePair<XElement, int>> requestsMaxIds)
        {
            if (requestsMaxIds != null && requestsMaxIds.Count > 0)
                foreach (KeyValuePair<XElement, int> kv in requestsMaxIds)
                    kv.Key.SetAttributeValue("contractility", kv.Value.ToString());
        }

        public void UpdateEpisodesParameters(XElement data)
        {
            lock (m_lock)
            {
                var xElemPatients = data.Elements();
                foreach (var xElemPatient in xElemPatients)
                {
                    var xElemRequest = xElemPatient.Element("request");
                    int episodeID = xElemRequest.Attribute2Int("id");

                    //get episode
                    CRIPluginEpisode episode = GetAdmittedEpisode(episodeID);
                    if (episode == null)
                        continue;

                    //update episode parameters of request
                    episode.VisitKey = xElemRequest.Attribute2String("key");
                    episode.Serveruid = xElemRequest.Attribute2String("serveruid");
                    episode.Tracing = xElemRequest.Attribute2Int("tracing");
                    episode.Artifact = xElemRequest.Attribute2Int("artifact");
                    episode.Contractility = xElemRequest.Attribute2Int("contractility");
                    episode.Action = xElemRequest.Attribute2Int("action");
                    episode.LastMergeId = Int32.Parse(xElemRequest.Attribute("merge").Value ?? "-1", CultureInfo.InvariantCulture);
                    episode.LastMergeTime = DateTime.MinValue;
                    if (xElemRequest.Attribute("mergetime") != null)
                    {
                        var mergeTime = xElemRequest.Attribute("mergetime").Value;
                        long epochMergeTime;
                        if (mergeTime != "-1" && Int64.TryParse(mergeTime, out epochMergeTime))
                        {
                            episode.LastMergeTime = epochMergeTime.ToDateTime();
                        }
                    }
                }
            }
        }

        public void UpdateEpisodesXML(XElement xElemPatients)
        {
            lock (m_lock)
            {
                var requests = xElemPatients.Elements("request");

                foreach (var episode in Episodes.Values)
                {
                    if (episode.EpisodeStatus != EpisodeStatus.Admitted)
                    {
                        continue;
                    }

                    bool bFound = false;
                    foreach (var xElemRequest in requests)
                    {
                        string key = xElemRequest.Attribute2String("key");
                        if (episode.VisitKey.Equals(key))
                        {
                            xElemRequest.SetAttributeValue("contractility", episode.Contractility);
                            bFound = true;
                            break;
                        }
                    }

                    if (!bFound)
                    {
                        XElement xElemRequest = new XElement("request");
                        xElemRequest.SetAttributeValue("key", episode.VisitKey);

                        if (episode.IsIncremental)
                        {
                            xElemRequest.SetAttributeValue("id", episode.EpisodeId);
                            xElemRequest.SetAttributeValue("tracing", episode.Tracing);
                            xElemRequest.SetAttributeValue("artifact", episode.Artifact);
                            xElemRequest.SetAttributeValue("action", episode.Action);
                            xElemRequest.SetAttributeValue("incremental", episode.IsIncremental);
                            xElemRequest.SetAttributeValue("serveruid", episode.Serveruid);
                            xElemRequest.SetAttributeValue("contractility", episode.Contractility);
                        }
                        else
                        {
                            episode.IsIncremental = true;
                        }

                        xElemRequest.SetAttributeValue("requestedBy", "ExportPluginLaborer");
                        xElemPatients.Add(xElemRequest);
                    }
                }
            }
        }

        public void UpdateEpisodesDowntime(bool isInDownTime, bool isAfterDownTime)
        {
            lock (m_lock)
            {
                if (isAfterDownTime)
                {
                    foreach (var episode in Episodes.Values)
                    {
                        episode.IsAfterDownTime = true;
                        episode.IsIncremental = false;
                    }
                }

                if (isInDownTime)
                {
                    foreach (var episode in Episodes.Values)
                    {
                        episode.TracingStatus = TracingStatus.Error;
                    }
                }
            }
        }

        private CRIPluginEpisode GetAdmittedEpisode(int episodeID)
        {
            CRIPluginEpisode episode;
            bool bSucc = Episodes.TryGetValue(episodeID, out episode);
            if (!bSucc)
            {
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginLaborer", String.Format("GetAdmittedEpisode, could not find episodeID {0} in the list", episodeID));
                return null;
            }

            if (episode.EpisodeStatus != EpisodeStatus.Admitted)
            {
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginLaborer", String.Format("GetAdmittedEpisode, episodeID {0} is discharged", episodeID));
                return null;
            }

            return episode;
        }

        /// <summary>
        /// Accumulates UP tracings in regular live (or retrospective mode)
        /// </summary>
        /// <param name="tracings"></param>
        /// <param name="bIncremental"></param>
        /// <param name="calc"></param>
        private void CollectUPTracings(XElement tracings, bool bIncremental, CRICalculator calc)
        {
            if (tracings != null)
            {
                var tracingsList = tracings.Elements();
                foreach (var tracing in tracingsList)
                {
                    var startTime = tracing.Attribute("start").Value;
                    long start;
                    if (!Int64.TryParse(startTime, out start))
                    {
                        Logger.WriteLogEntry(TraceEventType.Verbose, "Tracings Data", "start time: " + startTime);
                        continue;
                    }

                    var upTRacing = tracing.Attribute("up").Value;
                    if (upTRacing == null || upTRacing.Equals(String.Empty))
                        continue;

                    var ups = Convert.FromBase64String(upTRacing).ToList();
                    calc.AppendUPTracings(bIncremental, start, ups);
                }
            }
        }

        private void PerformCalculationsAfterDownTime(CRIPluginEpisode episode, CRICalculator calc, List<PluginDetectionArtifact> detectedObjects)
        {
            var calcContractilities = episode.ContractilitiesCalculator.Contractilities;
            if (calcContractilities.Count > 0)
            {
                int lastContractilityId = calcContractilities.Max(c => c.Id);
                long lastContractilityEndTime = calcContractilities.FirstOrDefault(c => c.Id == lastContractilityId).EndTime;
                var nonCalculatedDetectedObjects = detectedObjects.Where(d => d.StartTime >= lastContractilityEndTime).ToList();
                if (nonCalculatedDetectedObjects.Count > 0)
                {
                    long minStartTime = nonCalculatedDetectedObjects.Min(d => d.StartTime);
                    calc.CalculateContractility(nonCalculatedDetectedObjects, lastContractilityId, minStartTime, true);
                }
            }
        }

        public void PerformActionRecalculation(XElement param)
        {
            lock (m_lock)
            {
                PerformActionRecalculationInternal(param);
            }
        }

        private void PerformActionRecalculationInternal(XElement param)
        {
            // Get the artifact that was un/striked out from Patterns
            XElement responseArtifactData = PluginsLaborer.Instance.PerformActionCallToPatterns(param, "data/artifact");
            if (responseArtifactData == null)
                return;

            var patient = responseArtifactData.Elements().First();
            if (patient == null)
                return;

            var artifactsElement = patient.Element("artifacts");
            if (artifactsElement == null)
                return;

            String baseTimeStr = artifactsElement.Attribute("basetime").Value;
            long baseTime;
            if (!Int64.TryParse(baseTimeStr, out baseTime) || baseTime < 0)
                return;

            var artifactsList = artifactsElement.Elements();
            if (artifactsList.Count() <= 0)
                return;

            var artifact = artifactsList.ElementAt(0);
            if (artifact == null)
                return;

            String artifactData = artifact.Attribute("data").Value;
            PluginDetectionArtifact artifactObj = DATFileReader.ReadDAT(artifactData, baseTime);
            long startData = -1;
            long endData = -1;
            switch (artifactObj.EventType)
            {
                case ArtifactType.Contraction:
                    var contraction = artifactObj as PluginContraction;
                    startData = contraction.PeakTime - CRIAlgorithmSettings.Instance.CRIStateQualificationWindowSize;
                    endData = contraction.PeakTime + CRIAlgorithmSettings.Instance.CRIStateQualificationWindowSize;
                    break;
                case ArtifactType.Acceleration:
                case ArtifactType.Deceleration:
                    var accel = artifactObj as PluginAcceleration;
                    startData = accel.PeakTime - CRIAlgorithmSettings.Instance.CRIStateQualificationWindowSize;
                    endData = accel.PeakTime + CRIAlgorithmSettings.Instance.CRIStateQualificationWindowSize;
                    break;
                default:
                    return;
            }

            param.SetAttributeValue("startTime", startData.ToString());
            param.SetAttributeValue("endTime", endData.ToString());

            // Get all Patterns data for QualificationWindow before and QualificationWindow after the artifact peak time 
            XElement artifactsForRecalculation = PluginsLaborer.Instance.PerformActionCallToPatterns(param, "data/period");
            if (artifactsForRecalculation == null)
                return;

            patient = artifactsForRecalculation.Element("patient");

            XElement artifacts, request, tracings;
            int episodeID;
            XMLHelpers.CollectPatientData(patient, out artifacts, out tracings, out request, out episodeID);
            var episode = GetAdmittedEpisode(episodeID);
            if (episode == null)
                return;

            int actionId = request.Attribute("action") != null ? request.Attribute2Int("action") : episode.Action;
            ActionTypes actionType;
            String actionTypeStr = param.Attribute("type").Value;
            if (!Enum.TryParse<ActionTypes>(actionTypeStr, out actionType) || actionType == ActionTypes.ConfirmEvent || actionType == ActionTypes.None)
                return;

            // Calculation core
            if (artifacts == null)
                return;

            var startTime = artifacts.Attribute("basetime").Value;
            Logger.WriteLogEntry(TraceEventType.Verbose, "Algorithm", "basetime: " + startTime);
            long absoluteStart;
            Int64.TryParse(startTime, out absoluteStart);
            artifactsList = artifacts.Elements();

            CRICalculator calc = episode.ContractilitiesCalculator;
            List<UPTracingsBlockIndicator> compressedUPTracings;
            long upStartTime;
            EventsCalculator.CollectUPTracings(tracings, out compressedUPTracings, out upStartTime);
            List<PluginDetectionArtifact> surroundingObjects = XMLHelpers.DeserializeDetectedArtifacts(absoluteStart, artifactsList);

            int startId;
            calc.RecalculateContractilities(artifactObj, surroundingObjects, compressedUPTracings, actionId, actionType, out startId);

            var updatedContractilities = (from c in calc.Contractilities
                                          where c.Id >= startId
                                          orderby c.StartTime
                                          select c);
            if (updatedContractilities != null && updatedContractilities.Count() > 0)
            {
                List<Contractility> recalcultedContractilities = updatedContractilities.ToList();
                long minimalContractilityStartTime = recalcultedContractilities.First().StartTime;
                episode.UpdatePersistenceHistory(minimalContractilityStartTime, true);
            }
        }

        private XElement EncodeContractilitiesForActivex(ref long absoluteStart, IEnumerable<Contractility> contractilities, int lastReceivedCRI, bool isValid)
        {
            XElement toRet = new XElement("contractilities");
            if (contractilities != null)
            {
                var CRIsToRet = (from c in contractilities
                                 where c.Id > lastReceivedCRI
                                 orderby c.Id
                                 select c).ToList();

                if (CRIsToRet == null || CRIsToRet.Count() == 0)
                    return null;

                if (CRIsToRet.ElementAt(0).StartTime != absoluteStart)
                    absoluteStart = CRIsToRet.First().StartTime;

                toRet.SetAttributeValue("basetime", absoluteStart);

                foreach (var item in CRIsToRet)
                {
                    if (item == null)
                        continue;

                    toRet.Add(EncodeContractilityForActivex(item, absoluteStart, item.Id, isValid));
                }
            }

            return toRet;
        }

        private XElement EncodeContractilityForActivex(Contractility item, long absoluteStart, int criId, bool isValid)
        {
            String toXml = String.Format(CultureInfo.InvariantCulture, "CRI|{0}|{1}|{2}|{3}",
                                                                        isValid ? (int)item.CRIClassification : (int)ContractilityClassification.Unknown,
                                                                        item.StartTime - absoluteStart,
                                                                        item.EndTime - absoluteStart,
                                                                        criId);


            Logger.WriteLogEntry(TraceEventType.Verbose, "Algorithm", "AbsoluteStart: " + absoluteStart.ToString() + " | " + toXml);
            return new XElement("contractility", new XAttribute("data", toXml));
        }

        private void DischargeEpisodes()
        {
            var episodesForDischarge = Episodes.Values.Where(e => e.EpisodeStatus == EpisodeStatus.ToHistory);

            if (episodesForDischarge.Count() > 0)
            {
                bool bRes = CRIPluginDBManager.Instance.DischargeEpisodes(episodesForDischarge);

                if (bRes)
                {
                    foreach (var episode in episodesForDischarge)
                    {
                        CRIPluginEpisode removedEpisode = null;
                        bool isRemoved = Episodes.TryRemove(episode.EpisodeId, out removedEpisode);
                        if (isRemoved)
                        {
                            Logger.WriteLogEntry(TraceEventType.Verbose, "CRIPluginLaborer", String.Format("Succeed to remove episode {0} from Episodes list", episode.EpisodeId));
                        }
                        else
                        {
                            Logger.WriteLogEntry(TraceEventType.Error, "CRIPluginLaborer", String.Format("Failed to remove episode {0} from Episodes list", episode.EpisodeId));
                        }
                    }
                }
            }
        }
    }
}
