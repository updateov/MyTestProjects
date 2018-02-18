//Review: 27/12/2015
//Review: 19/11/2017
using Export.Algorithm;
using Export.Entities;
using PatternsEntities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using PatternsPluginsCommon;
using Export.PluginDataModel;
using System.Diagnostics;
using CommonLogger;

namespace Export.Plugin
{
    public class PluginEpisode : Episode
    {
        #region Properties & Members

        private object m_lock = new object();
        public IntervalsCalculator IntervalsCalculator { get; private set; }
        public List<Interval> ExportedIntervals { get; private set; }
        public int LastSavedIntervalId { get; set; }
        public int LastSavedExportId { get; set; }
        public String MotherId { get; set; }

        public int CurrentIntervalDuration
        {
            get
            {
                return IntervalsCalculator.DrawingWindowSize;
            }
        }

        public int LastIntervalId
        {
            get
            {
                return IntervalsCalculator.CalculatedIntervalsList.Count > 0 ? IntervalsCalculator.CalculatedIntervalsList.Max(c => c.IntervalID) : -1;
            }
        }

        public int LastExportId
        {
            get
            {
                return IntervalsCalculator.CalculatedIntervalsList.Count > 0 ? IntervalsCalculator.CalculatedIntervalsList.Max(c => c.ExportID) : -1;
            }
        }

        public DateTime LastIntervalEnd
        {
            get
            {
                return LastIntervalId > -1 ? IntervalsCalculator.CalculatedIntervalsList[LastIntervalId].EndTime : DateTime.MinValue;
            }
        }

        internal bool CacheNewIntervalsWithoutSave { get; set; }

        private List<int> m_NewIntervalsCache = null;
        internal List<int> NewIntervalsCache
        {
            get
            {
                return m_NewIntervalsCache;
            }
        }

        internal bool HasNewIntervalsCached
        {
            get
            {
                return m_NewIntervalsCache != null;
            }
        }

        #endregion

        public PluginEpisode(int episodeID)
            : base(episodeID)
        {
            LastSavedIntervalId = -1;
            LastSavedExportId = -1;
            ExportedIntervals = new List<Interval>();
            IntervalsCalculator = new IntervalsCalculator();
            IntervalsCalculator.IntervalsUpdated += OnIntervalsUpdated;
            IntervalsCalculator.SetIntervalDuration(PluginSettings.Instance.IntervalDuration);
        }


        #region Events & Delegates

        private void OnIntervalsUpdated(object sender, IntervalUpdatedEventArgs e)
        {
            PluginLaborer.Instance.UpdateEpisodeIntervals(MotherId, EpisodeId, e.IntervalID, e.IntervalEndTime);
        }

        private void AddNewIntervalsToCache(List<int> dirtyIntervalIds)
        {
            if (CacheNewIntervalsWithoutSave)
            {
                if (m_NewIntervalsCache == null)
                    m_NewIntervalsCache = new List<int>();

                m_NewIntervalsCache = m_NewIntervalsCache.Union(dirtyIntervalIds).ToList();
            }
        }

        #endregion

        public void UpdateExportableIntervals(int intervalId)
        {
            List<int> dirtyIntervalIds = new List<int>();
            if (intervalId < 0)
            {
                dirtyIntervalIds = IntervalsCalculator.CalculatedIntervalsList.Where(i => i.IntervalID > LastSavedIntervalId).Select(i => i.IntervalID).ToList();
            }
            else
            {
                dirtyIntervalIds.Add(intervalId);
            }

            if (CacheNewIntervalsWithoutSave)
            {
                AddNewIntervalsToCache(dirtyIntervalIds);
            }
            else
            {
                bool bRes = SaveEpisode(dirtyIntervalIds);
            }
        }

        public bool UpdateCurrentIntervalDuration(int intervalDuration)
        {
            bool bRes = false;

            IntervalsCalculator.SetIntervalDuration(intervalDuration);
            bRes = SaveEpisode(new List<int>());

            return bRes;
        }

        public Interval GetIntervalForExport(int intervalId)
        {
            Interval intervalForExport = null;

            var calculatedIntervalsList = IntervalsCalculator.CalculatedIntervalsList;
            var calculatedInterval = calculatedIntervalsList.FirstOrDefault(e => e.IntervalID == intervalId);

            intervalForExport = PluginFactory.Instance.CreateInterval(calculatedInterval);

            return intervalForExport;
        }

        public bool SaveExportedInterval(Interval exportedInterval, bool externalUse)
        {
            bool bRes = false;

            var calculatedIntervalsList = IntervalsCalculator.CalculatedIntervalsList;
            var calculatedInterval = calculatedIntervalsList.FirstOrDefault(e => e.IntervalID == exportedInterval.IntervalId);
            //currently we do not support re-export therefore we only save once.
            if (calculatedInterval != null && calculatedInterval.ExportID == -1)
            {
                //save to objectivity
                bRes = externalUse ? true : ExportManager.Instance.SaveInterval(VisitKey, exportedInterval);

                if (bRes)
                {
                    int lastExportId = calculatedIntervalsList.Count > 0 ? calculatedIntervalsList.Max(c => c.ExportID) : -1;
                    lastExportId++;

                    exportedInterval.ExportId = lastExportId;
                    ExportedIntervals.Add(exportedInterval);

                    bRes = SaveEpisode(new List<int>());

                    if (bRes)
                    {
                        calculatedInterval.ExportID = lastExportId;
                    }
                }
            }

            return bRes;
        }

        private bool SaveEpisode(List<int> dirtyIntervalIds)
        {
            bool bRes = false;

            try
            {
                var episodeModel = PluginFactory.Instance.CreateEpisodeModel(this, dirtyIntervalIds);
                List<EpisodeModel> episodesModels = new List<EpisodeModel>();
                episodesModels.Add(episodeModel);

                bRes = PluginDBManager.Instance.SaveEpisodes(episodesModels);

                if (bRes)
                {
                    int maxDirtyIntervalId = dirtyIntervalIds.Count() > 0 ? dirtyIntervalIds.Max() : -1;
                    LastSavedIntervalId = Math.Max(LastSavedIntervalId, maxDirtyIntervalId);
                    LastSavedExportId = IntervalsCalculator.CalculatedIntervalsList.Count > 0 ? 
                                        IntervalsCalculator.CalculatedIntervalsList.Max(i => i.ExportID) : 
                                        LastSavedExportId;
                    DateUpdatedUTC = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "ExportPluginEpisode", "Error occurred in SaveEpisode.", ex);
            }

            return bRes;
        }

        internal static bool SaveNewCachedIntervals(List<PluginEpisode> episodes)
        {
            bool bRes = false;

            try
            {
                List<EpisodeModel> episodesModels = new List<EpisodeModel>();
                foreach (PluginEpisode episode in episodes)
                {
                    EpisodeModel episodeModel = PluginFactory.Instance.CreateEpisodeModel(episode, episode.NewIntervalsCache);
                    episodesModels.Add(episodeModel);
                }

                bRes = PluginDBManager.Instance.SaveEpisodes(episodesModels);

                if (bRes)
                {
                    DateTime utcNow = DateTime.UtcNow;
                    foreach (PluginEpisode episode in episodes)
                    {
                        int maxDirtyIntervalId = episode.NewIntervalsCache.Count() > 0 ? episode.NewIntervalsCache.Max() : -1;
                        episode.LastSavedIntervalId = Math.Max(episode.LastSavedIntervalId, maxDirtyIntervalId);
                        episode.LastSavedExportId = episode.IntervalsCalculator.CalculatedIntervalsList.Count > 0 ?
                                            episode.IntervalsCalculator.CalculatedIntervalsList.Max(i => i.ExportID) :
                                            episode.LastSavedExportId;
                        episode.DateUpdatedUTC = utcNow;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "ExportPluginEpisode", "Error occurred in SaveNewCachedIntervals.", ex);
            }

            return bRes;
        }

        public XElement CreateIntervalsForActiveX(int clientIntervalId, int clientExportId)
        {
            XElement toRet = null;
            var calculatedIntervalsList = IntervalsCalculator.CalculatedIntervalsList;
            if (calculatedIntervalsList.Count > 0)
            {
                toRet = new XElement("intervals");
                long baseEpochTime = calculatedIntervalsList[0].StartTime.ToEpochTime();
                toRet.SetAttributeValue("basetime", baseEpochTime);

                var intervals = calculatedIntervalsList.Where(e => e.IntervalID > clientIntervalId || e.ExportID > clientExportId).OrderBy(i => i.IntervalID);

                foreach (var interval in intervals)
                {
                    String toXml = String.Format(CultureInfo.InvariantCulture, "INT|{0}|{1}|{2}|{3}|{4}",
                    interval.IntervalID,
                        interval.StartTime.ToEpochTime() - baseEpochTime,
                        interval.EndTime.ToEpochTime() - baseEpochTime,
                        interval.IntervalDuration,
                    interval.ExportID);
                    toRet.Add(new XElement("interval", new XAttribute("data", toXml)));
                }
            }

            return toRet;
        }

        internal void ClearNewIntervalsCache()
        {
            m_NewIntervalsCache = null;
        }

        #region merge

        public void ResetEpisode()
        {
            lock (m_lock)
            {
                int currentIntervalDuration = IntervalsCalculator.m_intervalDuration;
                DBManager.Instance.DeleteEpisodeData(EpisodeId);
                base.Reset();
                IntervalsCalculator = new IntervalsCalculator();
                IntervalsCalculator.SetIntervalDuration(currentIntervalDuration);
                LastSavedIntervalId = -1;
                LastSavedExportId = -1;
                m_NewIntervalsCache = null;
            }
        }

        public void UpdateIntervalsAfterMerge()
        {
            lock (m_lock)
            {
                List<int> dirtyIntervals = new List<int>();
                if (IntervalsCalculator.CalculatedIntervalsList.Count > 0)
                {
                    dirtyIntervals = (from i in IntervalsCalculator.CalculatedIntervalsList
                                      where i.IntervalID >= 0
                                      select i.IntervalID).ToList();
                }
                   
                if (ExportedIntervals.Count > 0)
                {
                    List<Interval> intervalsExportedBeforeMerge = new List<Interval>(ExportedIntervals);
                    ExportedIntervals = new List<Interval>();

                    foreach (var oldInterval in intervalsExportedBeforeMerge)
                    {
                        CalculatedInterval interval = IntervalsCalculator.CalculatedIntervalsList.FirstOrDefault(c => c.StartTime == oldInterval.StartTime && c.EndTime == oldInterval.EndTime);
                        if (interval != null)
                        {
                            interval.ExportID = oldInterval.ExportId;

                            Interval newInterval = new Interval()
                            {
                                StartTime = interval.StartTime,
                                EndTime = interval.EndTime,
                                IntervalId = interval.IntervalID,
                                LoginName = oldInterval.LoginName,
                                Concepts = new List<BaseConcept>(oldInterval.Concepts),
                                IntervalDuration = oldInterval.IntervalDuration,
                                ExportId = oldInterval.ExportId
                            };

                            ExportedIntervals.Add(newInterval);
                        }
                    }
                }

                SaveEpisode(dirtyIntervals);
            }
        }
        #endregion
    }
}
