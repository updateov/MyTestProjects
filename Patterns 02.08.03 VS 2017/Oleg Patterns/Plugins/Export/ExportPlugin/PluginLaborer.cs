//Review: 27/12/2015
//Review: 19/11/2017
//Review: 07/12/17

using Export.Algorithm;
using PatternsEntities;
using PatternsPluginsCommon;
using PluginsAlgorithms;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Xml.Linq;
using System.Linq;
using PatternsPluginsManager;
using CommonLogger;
using System.Threading.Tasks;

namespace Export.Plugin
{
    public class PluginLaborer
    {
        private Object m_lock = new Object();
        public ConcurrentDictionary<int, PluginEpisode> Episodes { get; set; }

        private const byte PEN_UP = 255;

        public ConcurrentDictionary<String, List<EpisodeIdentifier>> MotherId2SiblingsList { get; set; }

        private Timer m_checkIntervalComplete = new Timer();

        #region Singleton functionality

        private static PluginLaborer s_pluginLaborer = null;
        private static Object s_lockObject = new Object();

        private PluginLaborer()
        {
            Episodes = new ConcurrentDictionary<int, PluginEpisode>();
            MotherId2SiblingsList = new ConcurrentDictionary<String, List<EpisodeIdentifier>>();

            InitLaborer();
        }

        private void InitLaborer()
        {
            lock (m_lock)
            {
                try
                {
                    List<PluginEpisode> episodes = PluginDBManager.Instance.LoadEpisodes();
                    foreach (PluginEpisode episode in episodes)
                    {
                        bool bSucc = Episodes.TryAdd(episode.EpisodeId, episode);

                    }

                    m_checkIntervalComplete.Interval = PluginSettings.Instance.CheckIntervalCompleteInterval; // each 2 minutes we'll check whether it's OK to close the export interval
                    m_checkIntervalComplete.Elapsed += OnCheckIntervalCompleteTimerElapsed;
                    m_checkIntervalComplete.Enabled = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "StartLaboring", "Error occurred in StartLaboring.", ex);
                }
            }
        }

        public static PluginLaborer Instance
        {
            get
            {
                if (s_pluginLaborer == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_pluginLaborer == null)
                        {
                            s_pluginLaborer = new PluginLaborer();
                        }
                    }
                }

                return s_pluginLaborer;
            }
        }

        #endregion

        #region Events & Delegates

        //TODO: Oleg update check downtime of tracing\patterns\podi
        private void OnCheckIntervalCompleteTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (m_lock)
            {
                foreach (var item in Episodes)
                {
                    item.Value.IntervalsCalculator.UpdateClosedIntervals();
                }
            }
        }

        #endregion

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
                    DateTime startCtr = GetIntervalStartTime(contraction.PeakTime);
                    startData = startCtr.ToEpochTime();
                    endData = startData + 1799;
                    break;
                case ArtifactType.Acceleration:
                case ArtifactType.Deceleration:
                    var accel = artifactObj as PluginAcceleration;
                    DateTime startAcc = GetIntervalStartTime(accel.PeakTime);
                    startData = startAcc.ToEpochTime();
                    endData = startData + 1799;
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

            IntervalsCalculator calc = episode.IntervalsCalculator;
            List<UPTracingsBlockIndicator> compressedUPTracings;
            long upStartTime;
            EventsCalculator.CollectUPTracings(tracings, out compressedUPTracings, out upStartTime);
            List<PluginDetectionArtifact> surroundingObjects = XMLHelpers.DeserializeDetectedArtifacts(absoluteStart, artifactsList);

            int updatedIntervalId;
            calc.RecalculateInterval(artifactObj, surroundingObjects, compressedUPTracings, actionId, actionType, out updatedIntervalId);
        }

        private DateTime GetIntervalStartTime(long eventPeak)
        {
            DateTime peakTime = eventPeak.ToDateTime();
            int start = (peakTime.Minute / 30) * 30;
            DateTime startTime = new DateTime(peakTime.Year, peakTime.Month, peakTime.Day, peakTime.Hour, start, 0, 0, peakTime.Kind);
            return startTime;
        }

        public void UpdateEpisodesList(XElement response)
        {
            lock (m_lock)
            {
                List<int> episodesKeysToHistory = Episodes.Keys.ToList();

                var xElemEpisodes = response.Elements(Resource.IDS_EPISODE_ELEM);
                foreach (XElement xElemEpisode in xElemEpisodes)
                {
                    var xElemPatient = xElemEpisode.Element(Resource.IDS_PATIENT_ELEM);
                    int episodeID = xElemEpisode.Attribute2Int(Resource.IDS_PATIENT_UNIQUE_ID_ATTR);
                    if (!Episodes.ContainsKey(episodeID))
                    {                        
                        PluginEpisode pluginEpisode = new PluginEpisode(episodeID);
                        Episodes[episodeID] = pluginEpisode;
                    }

                    string episodeStatus = xElemEpisode.Attribute2String(Resource.IDS_EPISODE_STATUS_ATTR);
                    if (episodeStatus == "Admitted" || episodeStatus == "New")
                    {
                        Episodes[episodeID].EpisodeStatus = EpisodeStatus.Admitted;
                        Episodes[episodeID].VisitKey = xElemPatient.Attribute2String(Resource.IDS_VISIT_KEY_ATTR);
                        Episodes[episodeID].MotherId = xElemPatient.Attribute2String(Resource.IDS_MOTHER_ID_ATTR);

                        var xmlMultiplesElem = xElemPatient.Element(Resource.IDS_MULTIPLES_ELEM);
                        if (xmlMultiplesElem != null)
                        {
                            var motherIdStr = xmlMultiplesElem.Attribute2String(Resource.IDS_MOTHER_ID_ATTR);
                            if (motherIdStr.Equals(String.Empty))
                                motherIdStr = xElemPatient.Attribute2String(Resource.IDS_VISIT_KEY_ATTR);

                            if (!MotherId2SiblingsList.ContainsKey(motherIdStr))
                            {
                                List<EpisodeIdentifier> siblings = new List<EpisodeIdentifier>();
                                var siblingList = xmlMultiplesElem.Elements(Resource.IDS_SIBLING_ELEM);
                                if (siblingList != null)
                                {
                                    foreach (var sibling in siblingList)
                                    {
                                        siblings.Add(new EpisodeIdentifier(sibling.Attribute2Int(Resource.IDS_PATIENT_UNIQUE_ID_ATTR), sibling.Attribute2String(Resource.IDS_VISIT_KEY_ATTR), -1, DateTime.MinValue));
                                    }
                                }

                                if (siblings.Count <= 0)
                                    siblings.Add(new EpisodeIdentifier(xElemPatient.Attribute2Int(Resource.IDS_PATIENT_UNIQUE_ID_ATTR), xElemPatient.Attribute2String(Resource.IDS_VISIT_KEY_ATTR), -1, DateTime.MinValue));

                                MotherId2SiblingsList[motherIdStr] = siblings; }
                        }

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
                    }
                }

                DischargeEpisodes();
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
                            xElemRequest.SetAttributeValue("interval", episode.LastIntervalId);
                            xElemRequest.SetAttributeValue("export", episode.LastExportId);
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
                            xElemRequest.SetAttributeValue("interval", episode.LastIntervalId);
                            xElemRequest.SetAttributeValue("export", episode.LastExportId);
                        }
                        else
                        {
                            episode.IsIncremental = true;
                        }

                        xElemRequest.SetAttributeValue("requestedBy", Resource.IDS_EXPORT_PLUGIN_LABORER);
                        xElemPatients.Add(xElemRequest);
                    }
                }
            }
        }

        public void UpdateEpisodesTracingStatus(XElement response)
        {
            lock (m_lock)
            {
                var xElemPatients = response.Elements();
                foreach (var xElemPatient in xElemPatients)
                {
                    var xElemRequest = xElemPatient.Element("request");
                    int episodeID = xElemRequest.Attribute2Int("id");

                    //get episode
                    PluginEpisode episode = GetAdmittedEpisode(episodeID);
                    if (episode == null)
                        continue;

                    //update episode parameters of patient
                    //TODO consider use of 'as'
                    episode.TracingStatus = (TracingStatus)xElemPatient.Attribute2Int("status");
                }
            }
        }

        public void UpdateEpisodesData(XElement response, XElement clientCall = null)
        {
            lock (m_lock)
            {
                try
                {
                    List<PluginEpisode> processedEpisodes = new List<PluginEpisode>();
                    List<KeyValuePair<XElement, XElement>> episodesIntervalsData = new List<KeyValuePair<XElement, XElement>>();
                    List<KeyValuePair<PluginEpisode, XElement>> episodesRequestData = new List<KeyValuePair<PluginEpisode, XElement>>();

                    var patients = response.Elements();
                    foreach (var patient in patients)
                    {
                        XElement artifacts, tracings, request;
                        int episodeID;
                        XMLHelpers.CollectPatientData(patient, out artifacts, out tracings, out request, out episodeID);

                        try
                        {
                            //get episode
                            PluginEpisode episode = GetAdmittedEpisode(episodeID);
                            if (episode == null || episode.EpisodeStatus != EpisodeStatus.Admitted)
                                continue;

                            processedEpisodes.Add(episode);

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
                            // END Client specific data

                            //TODO: move to PluginEpisode
                            IntervalsCalculator calc = episode.IntervalsCalculator;


                            CollectTracings(tracings, calc);

                            bool merge = false;
                            // Calculation core
                            if (artifacts != null)
                            {
                                var startTime = artifacts.Attribute("basetime").Value;
                                Logger.WriteLogEntry(TraceEventType.Verbose, "ExportAlgorithm", "basetime: " + startTime);
                                long absoluteStart;
                                Int64.TryParse(startTime, out absoluteStart);
                                var artifactsList = artifacts.Elements();

                                List<PluginDetectionArtifact> detectedObjects = XMLHelpers.DeserializeDetectedArtifacts(absoluteStart, artifactsList);
                                episode.LastMergeId = requestLastMegeID;
                                episode.LastMergeTime = requestMergeTime;

                                List<PastIntervalDuration> intervalDurationsHistory = null;
                                if (requestLastMegeID > episodeLastMergeID)
                                {
                                    intervalDurationsHistory = episode.IntervalsCalculator.GetIntervalsDurationHistory();
                                    episode.ResetEpisode();
                                    merge = true;
                                }
                                if (merge)
                                {
                                    calc = episode.IntervalsCalculator;
                                    calc.CalculateExportDataAfterMerge(detectedObjects, absoluteStart, intervalDurationsHistory);
                                    episode.UpdateIntervalsAfterMerge();

                                }
                                else
                                {
                                    try
                                    {
                                        episode.CacheNewIntervalsWithoutSave = true;
                                        calc.CalculateExportData(detectedObjects, absoluteStart, bIncremental, episode.IsAfterDownTime);
                                    }
                                    finally
                                    {
                                        episode.CacheNewIntervalsWithoutSave = false;
                                    }
                                }
                            }


                            //create interval to xml;
                            int clientIntervalId = merge? request.Attribute2Int("interval") : -1;
                            int clientExportId = merge? request.Attribute2Int("export") : -1;
                            XElement intervals = episode.CreateIntervalsForActiveX(clientIntervalId, clientExportId);
                            episodesIntervalsData.Add(new KeyValuePair<XElement, XElement>(patient, intervals));

                            //set xml attributes
                            episodesRequestData.Add(new KeyValuePair<PluginEpisode, XElement>(episode, request));
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteLogEntry(TraceEventType.Critical, Resource.IDS_EXPORT_PLUGIN_LABORER, "Failed to update episode data: " + episodeID, ex);
                        }
                    }

                    SaveNewCachedIntervals(processedEpisodes);
                    SetResponseIntervalsData(episodesIntervalsData);
                    SetRequestAttributes(episodesRequestData);
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, Resource.IDS_EXPORT_PLUGIN_LABORER, "Error occurred in UpdateEpisodesData.", ex);
                }
            }
        }

        private void SaveNewCachedIntervals(List<PluginEpisode> processedEpisodes)
        {
            List<PluginEpisode> episodesToSave = new List<PluginEpisode>();
            foreach (PluginEpisode episode in processedEpisodes)
                if (episode.HasNewIntervalsCached)
                    episodesToSave.Add(episode);

            if (episodesToSave.Count > 0)
            {
                bool success = PluginEpisode.SaveNewCachedIntervals(episodesToSave);
                if (success)
                {
                    foreach (PluginEpisode episode in episodesToSave)
                        episode.ClearNewIntervalsCache();
                }
            }
        }

        private void SetResponseIntervalsData(List<KeyValuePair<XElement, XElement>> episodesIntervalsData)
        {
            if (episodesIntervalsData != null && episodesIntervalsData.Count > 0)
                foreach (KeyValuePair<XElement, XElement> kv in episodesIntervalsData)
                {
                    XElement episodeResponseNode = kv.Key;
                    XElement intervals = kv.Value;

                    episodeResponseNode.Add(intervals);
                }
        }

        private void SetRequestAttributes(List<KeyValuePair<PluginEpisode, XElement>> episodesRequestData)
        {
            if (episodesRequestData != null && episodesRequestData.Count > 0)
                foreach (KeyValuePair<PluginEpisode, XElement> kv in episodesRequestData)
                {
                    XElement episodeRequestNode = kv.Value;
                    PluginEpisode episode = kv.Key;

                    episodeRequestNode.SetAttributeValue("intervalDuration", episode.CurrentIntervalDuration);
                    episodeRequestNode.SetAttributeValue("interval", episode.LastIntervalId);
                    episodeRequestNode.SetAttributeValue("export", episode.LastExportId);
                }
        }

        public void PerformPluginAction(XElement request, XElement response)
        {
            lock (m_lock)
            {
                var xElemActions = request.Elements("pluginAction");
                foreach (var xElemAction in xElemActions)
                {
                    int type = xElemAction.Attribute2Int("type");
                    PluginsAction action = (PluginsAction)type;

                    switch (action)
                    {
                        case PluginsAction.UpdateIntervalDuration:
                            UpdateIntervalDuration(xElemAction);
                            break;
                        default:
                            break;
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

        public void UpdateEpisodesParameters(XElement response)
        {
            lock (m_lock)
            {
                var xElemPatients = response.Elements();
                foreach (var xElemPatient in xElemPatients)
                {
                    var xElemRequest = xElemPatient.Element("request");
                    int episodeID = xElemRequest.Attribute2Int("id");

                    //get episode
                    PluginEpisode episode = GetAdmittedEpisode(episodeID);
                    if (episode == null)
                        continue;

                    //update episode parameters of request
                    episode.VisitKey = xElemRequest.Attribute2String("key");
                    episode.Serveruid = xElemRequest.Attribute2String("serveruid");
                    episode.Tracing = xElemRequest.Attribute2Int("tracing");
                    episode.Artifact = xElemRequest.Attribute2Int("artifact");
                    episode.Action = xElemRequest.Attribute2Int("action");                    
                }
            }
        }

        private void UpdateIntervalDuration(XElement xElemAction)
        {
            int episodeID = xElemAction.Attribute2Int("patient");

            PluginEpisode episode = GetAdmittedEpisode(episodeID);
            if (episode == null)
            {
                return;
            }

            int intervalDuration = xElemAction.Attribute2Int("intervalduration");
            if (intervalDuration == -1)
            {
                return;
            }

            episode.UpdateCurrentIntervalDuration(intervalDuration);
        }

        private PluginEpisode GetAdmittedEpisode(int episodeID)
        {
            PluginEpisode episode;
            bool bSucc = Episodes.TryGetValue(episodeID, out episode);
            if (!bSucc)
            {
                Logger.WriteLogEntry(TraceEventType.Verbose, Resource.IDS_EXPORT_PLUGIN_LABORER, String.Format("GetAdmittedEpisode, could not find episodeID {0} in the list", episodeID));
                return null;
            }

            if (episode.EpisodeStatus != EpisodeStatus.Admitted)
            {
                Logger.WriteLogEntry(TraceEventType.Verbose, Resource.IDS_EXPORT_PLUGIN_LABORER, String.Format("GetAdmittedEpisode, episodeID {0} is discharged", episodeID));
                return null;
            }

            return episode;
        }

        private void DischargeEpisodes()
        {
            var episodesForDischarge = Episodes.Values.Where(e => e.EpisodeStatus == EpisodeStatus.ToHistory);

            if (episodesForDischarge.Count() > 0)
            {
                var episodesModels = PluginFactory.Instance.CreateEpisodesModels(episodesForDischarge).ToList();

                foreach (var episode in episodesModels)
                {
                    bool bRes = PluginDBManager.Instance.DischargeEpisodes(episode);

                    if (bRes)
                    {
                        PluginEpisode removedEpisode = null;
                        bool isRemoved = Episodes.TryRemove(episode.EpisodeId, out removedEpisode);
                        if (isRemoved)
                        {
                            Logger.WriteLogEntry(TraceEventType.Verbose, Resource.IDS_EXPORT_PLUGIN_LABORER, String.Format(Resource.IDS_EPISODE_REMOVE_SUCCESS, episode.EpisodeId));
                        }
                        else
                        {
                            Logger.WriteLogEntry(TraceEventType.Error, Resource.IDS_EXPORT_PLUGIN_LABORER, String.Format(Resource.IDS_EPISODE_REMOVE_FAILED, episode.EpisodeId));
                        }

                        String motherId = removedEpisode.MotherId;
                        String visitKey = removedEpisode.VisitKey;
                        if (!motherId.Equals(String.Empty))
                        {
                            var removedSiblings = new List<EpisodeIdentifier>();
                            bool bSucc = MotherId2SiblingsList.TryGetValue(motherId, out removedSiblings);
                            int nRet = removedSiblings.RemoveAll(c => c.VisitKey.Equals(visitKey));
                            Logger.WriteLogEntry(bSucc ?
                                                    TraceEventType.Verbose :
                                                    TraceEventType.Error,
                                                Resource.IDS_EXPORT_PLUGIN_LABORER,
                                                nRet == 1 ?
                                                    String.Format(Resource.IDS_MOTHER_ID_REMOVE_SUCCESS, visitKey) :
                                                    String.Format(Resource.IDS_MOTHER_ID_REMOVE_FAILED, visitKey));
                            if(bSucc && removedSiblings.Count == 0)
                            {
                                List<EpisodeIdentifier> removedList;
                                bSucc = MotherId2SiblingsList.TryRemove(motherId, out removedList);
                                Logger.WriteLogEntry(bSucc ?
                                    TraceEventType.Verbose :
                                    TraceEventType.Error,
                                    Resource.IDS_EXPORT_PLUGIN_LABORER,
                                    bSucc?
                                        String.Format(Resource.IDS_MOTHER_ID_REMOVE_SUCCESS, motherId) :
                                        String.Format(Resource.IDS_MOTHER_ID_REMOVE_FAILED, motherId));
                            }
                        }
                    }
                }
            }

        }

        public void UpdateEpisodeIntervals(String motherId, int episodeId, int intervalID, DateTime intervalEndTime)
        {
            lock (m_lock)
            {
                var multiples = new List<EpisodeIdentifier>();
                bool bSucc = MotherId2SiblingsList.TryGetValue(motherId, out multiples);
                if (!bSucc || multiples.Count <= 0)
                    return;

                bool bUpdate = false;
                DateTime earliestInterval = multiples[0].LastIntervalTime;
                foreach (var item in multiples)
                {
                    if (item.EpisodeId == episodeId)
                    {
                        if (item.LastIntervalId == -1 || item.LastIntervalId < intervalID)
                        {
                            item.LastIntervalId = intervalID;
                            item.LastIntervalTime = intervalEndTime;
                            bUpdate = true;
                        }
                    }

                    if (earliestInterval < item.LastIntervalTime)
                        earliestInterval = item.LastIntervalTime;
                }

                if (bUpdate)
                {
                    Task.Factory.StartNew(() => UpdateExportableEpisodeIntervals(motherId, earliestInterval));
                }
            }
        }

        private void UpdateExportableEpisodeIntervals(string motherId, DateTime earliestInterval)
        {
            lock (m_lock)
            {
                var multiples = new List<EpisodeIdentifier>();
                bool bSucc = MotherId2SiblingsList.TryGetValue(motherId, out multiples);
                if (!bSucc || multiples.Count <= 0)
                    return;

                foreach (var sibling in multiples)
                {
                    PluginEpisode episode;
                    bool bRet = Episodes.TryGetValue(sibling.EpisodeId, out episode);
                    if (episode.LastIntervalEnd >= earliestInterval)
                        return;

                    episode.UpdateExportableIntervals(sibling.LastIntervalId);
                }
            }
        }

        /// <summary>
        /// Accumulates UP and FHR tracings in regular live (or retrospective mode)
        /// </summary>
        /// <param name="tracings"></param>
        /// <param name="calc"></param>
        private void CollectTracings(XElement tracings, IntervalsCalculator calc)
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

                    var upData = tracing.Attribute("up").Value;
                    var hrData = tracing.Attribute("hr1").Value;
                    if (upData == null || hrData == null || upData.Equals(String.Empty) || hrData.Equals(String.Empty))
                        continue;

                    var upDataList = Convert.FromBase64String(upData).ToList();
                    calc.AppendUPTracings(start, upDataList);

                    var hrDataArray = Convert.FromBase64String(hrData);
                    var curStart = start;
                    var toAppendList = new List<byte>();
                    int penupStreak = 0;
                    for (int i = 0; i < hrDataArray.Length; i++)
                    {
                        if (toAppendList.Count <= 0)
                        {
                            curStart = start + (i / 4);
                        }

                        if (hrDataArray[i] < PEN_UP)
                        {
                            while (penupStreak % 4 != 0)
                            {
                                --penupStreak;
                                toAppendList.Add(PEN_UP);
                            }

                            penupStreak = 0;
                            toAppendList.Add(hrDataArray[i]);
                        }
                        else
                        {
                            if (++penupStreak >= 4 && toAppendList.Count > 0)
                            {
                                while (toAppendList.Count % 4 != 0)
                                {
                                    toAppendList.Add(PEN_UP);
                                    --penupStreak;
                                }

                                calc.AppendFHRTracings(curStart, toAppendList);
                                toAppendList = new List<byte>();
                            }
                        }
                    }

                    if (toAppendList.Count > 0)
                    {
                        while (toAppendList.Count % 4 != 0)
                        {
                            toAppendList.Add(PEN_UP);
                        }

                        calc.AppendFHRTracings(curStart, toAppendList);
                    }
                }
            }
        }
    }
}
