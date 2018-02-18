//Review 07/12/17

using PluginsAlgorithms;
using PatternsPluginsCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Diagnostics;
using CommonLogger;

namespace Export.Algorithm
{
    public class IntervalsCalculator : EventsCalculator
    {
        protected Object m_lock = new Object();
        protected Timer m_checkIntervalComplete;

        #region Configuration

        protected int MINIMAL_DATA_PERCENTAGE_TO_QUALIFY = AlgorithmSettings.Instance.MinimalAmountOfDataInQualificationWindow;

        private const int MAGIC_NUMBER = 3; // Contraction is usually detected within 2 minutes, so for confidence we wait at least 3 minutes in case there's no FHR data

        public int m_intervalDuration = 0;
        public override int DrawingWindowSize
        {
            get { return m_intervalDuration; } // TODO: set m_drawingWindowSize according to 15/30 min choice
            set { m_intervalDuration = value; }
        }

        #endregion

        public List<CalculatedInterval> CalculatedIntervalsList { get; protected set; }
        public int IntervalId { get; set; }

        private Dictionary<long, List<byte>> UPTracings { get; set; }
        private Dictionary<long, List<byte>> FHRTracings { get; set; }
        protected List<PluginContraction> m_detectedContractions = new List<PluginContraction>();

        protected int m_montevideoUnitsRateLast;
        protected int m_earlyDecelsRateLast;
        protected int m_variableDecelsRateLast;
        protected int m_otherDecelsRateLast;

        protected long m_lastExportedBlock;
        public long LastExportedBlock { set { m_lastExportedBlock = value; } }

        public IntervalsCalculator()
        {
            m_montevideoUnitsRateLast = 0;
            m_earlyDecelsRateLast = 0;
            m_variableDecelsRateLast = 0;
            m_otherDecelsRateLast = 0;

            IntervalId = 0;

            ResetEventsRates();
            CalculatedIntervalsList = new List<CalculatedInterval>();
            UPTracings = new Dictionary<long, List<byte>>();
            FHRTracings = new Dictionary<long, List<byte>>();
        }

        /// <summary>
        /// Updates ID's of closed intervals that are ready to be saved to DB
        /// </summary>
        public void UpdateClosedIntervals()
        {
            DateTime now = DateTime.UtcNow;
            if (CalculatedIntervalsList.Count <= 0)
                ResetExportData(now.ToEpochTime());

            var openIntervals = from c in CalculatedIntervalsList
                                where c.IntervalID == -1
                                orderby c.EndTime
                                select c;

            if (openIntervals.Count() <= 0)
            {
                var lastEndTime = CalculatedIntervalsList.Max(c => c.EndTime);
                CreateExportData(lastEndTime.AddMinutes(2));
                return;
            }

            if (m_detectedArtifacts.Count <= 0)
                return;

            int curLastIntervalID = IntervalId;
            foreach (var item in openIntervals)
            {
                var diff = now - item.EndTime;
                if (diff.TotalMinutes < MAGIC_NUMBER)
                    continue;

                long start = item.StartTime.ToEpochTime();
                long end = item.EndTime.ToEpochTime();
                var lastFHRBlockStart = (from c in FHRTracings
                                         where c.Key < end
                                         select c).Max(c => c.Key);

                var lastFHRBlock = FHRTracings[lastFHRBlockStart];
                long nBlockLenth = lastFHRBlock.Count / 4; // we want length in seconds, FHR is 4Hz
                if ((lastFHRBlockStart + nBlockLenth) > end) // we handle it in other place
                    continue;
                else
                {
                    if (m_detectedArtifacts.Last().EndTime >= (lastFHRBlockStart + nBlockLenth - 4))
                        item.IntervalID = IntervalId++;
                }
            }

            CleanCollectedFHR();

            CalculateReadyIntervalsValues(curLastIntervalID);
            if (IntervalId != curLastIntervalID)
                FireIntervalsUpdated();

        }

        private void CleanCollectedFHR()
        {
            lock (m_lock)
            {
                int curLastIntervalID = IntervalId;
                var lastInterval = (from c in CalculatedIntervalsList
                                    where c.IntervalID == curLastIntervalID - 1
                                    select c).FirstOrDefault();

                long lastIntervalEnd = lastInterval.EndTime.ToEpochTime();
                var listOfBlockStarts = (from c in FHRTracings
                                         where c.Key < lastIntervalEnd
                                         orderby c.Key
                                         select c.Key).ToList();

                listOfBlockStarts.RemoveAt(listOfBlockStarts.Count - 1);

                foreach (var item in listOfBlockStarts)
                {
                    FHRTracings.Remove(item);
                }
            }
        }

        protected override void ResetEventsRatesInternal()
        {
            m_montevideoUnitsRateLast = 0;
            m_earlyDecelsRateLast = 0;
            m_variableDecelsRateLast = 0;
            m_otherDecelsRateLast = 0;
        }

        public void CalculateExportData(List<PluginDetectionArtifact> detectedObjects, long absoluteStart, bool bIncremental, bool bAfterDownTime)
        {
            lock (m_lock)
            {
                bool bReseted = false;
                if (CalculatedIntervalsList.Count <= 0)
                {
                    ResetExportData(absoluteStart);
                    bReseted = true;
                }

                if (!bReseted && !bIncremental)
                {
                    if (bAfterDownTime)
                    {
                        FillIntervalsGap(DateTime.MaxValue);

                        DateTime lastIntervalEnd = absoluteStart.ToDateTime();
                        var openIntervals = (from c in CalculatedIntervalsList
                                             where c.IntervalID > -1
                                             select c).ToList();

                        if (openIntervals.Count > 0)
                            lastIntervalEnd = openIntervals.Max(d => d.EndTime);

                        var newDetectedObjects = (from c in detectedObjects
                                                  where c.EndTime.ToDateTime() > lastIntervalEnd
                                                  select c).ToList();

                        var newCtrs = (from c in newDetectedObjects
                                       where c.EventType == ArtifactType.Contraction && c.EndTime.ToDateTime() > lastIntervalEnd
                                       orderby c.EndTime
                                       select c as PluginContraction).ToList();

                        var newDistinctCtrs = newCtrs.Except(m_detectedContractions).ToList();
                        m_detectedContractions.AddRange(newDistinctCtrs);
                        UpdateContractionsRates(newDetectedObjects);

                        var newEvents = (from c in newDetectedObjects
                                         where c.EventType != ArtifactType.Contraction && c.EndTime.ToDateTime() > lastIntervalEnd
                                         orderby c.EndTime
                                         select c).ToList();

                        var newDistinctEvents = newEvents.Except(m_detectedArtifacts).ToList();
                        m_detectedArtifacts.AddRange(newDistinctEvents);
                        UpdateEventsRates(newEvents);
                        UpdateClosedIntervals();
                    }

                    return;
                }

                FillIntervalsGap(DateTime.MaxValue);
                var ctrs = (from c in detectedObjects
                            where c.EventType == ArtifactType.Contraction && c.Id > m_lastCalculatedContractionID
                            orderby c.EndTime
                            select c as PluginContraction).ToList();

                var distinctCtrs = ctrs.Except(m_detectedContractions).ToList();
                m_detectedContractions.AddRange(distinctCtrs);

                UpdateContractionsRates(detectedObjects);
                var toAdd = (from c in detectedObjects
                             where c.EventType != ArtifactType.Contraction && c.Id > m_lastCalculatedEventID
                             orderby c.EndTime
                             select c).ToList();

                if (toAdd.Count() <= 0)
                    return;

                var toAddDistinct = toAdd.Except(m_detectedArtifacts).ToList();
                m_detectedArtifacts.AddRange(toAddDistinct);

                UpdateEventsRates(toAdd);

                // After all calculations we update the interval ID's
                PerformPostIntervalCalculation();
            }
        }

        public void RecalculateInterval(PluginDetectionArtifact artifactObj, 
                                        List<PluginDetectionArtifact> surroundingObjects,
                                        List<UPTracingsBlockIndicator> compressedUPTracings, 
                                        int actionId, 
                                        ActionTypes actionType,
                                        out int updatedIntervalId)
        {
            updatedIntervalId = -1;
            if (CalculatedIntervalsList.Count <= 0)
                return;

            DateTime peakTime = (artifactObj as PluginDeleteableDetectionArtifact).PeakTime.ToDateTime();
            CalculatedInterval interval = (from c in CalculatedIntervalsList
                                           where c.StartTime <= peakTime && c.EndTime > peakTime
                                           select c).FirstOrDefault();

            if (interval == null)
                return;

            var artifactType = artifactObj.EventType;

            switch (actionType)
            {
                case ActionTypes.StrikeoutContraction:
                    StrikeoutContraction(interval, artifactObj, true);
                    break;
                case ActionTypes.UndoStrikeoutContraction:
                    StrikeoutContraction(interval, artifactObj, false);
                    break;
                case ActionTypes.StrikeoutEvent:
                    StrikeoutEvent(interval, artifactObj, true);
                    break;
                case ActionTypes.UndoStrikeoutEvent:
                    StrikeoutEvent(interval, artifactObj, false);
                    break;
                default:
                    return;
            }

            if (interval.IntervalID > -1)
                FireIntervalsUpdated(interval.IntervalID, interval.EndTime);
        }

        private void StrikeoutContraction(CalculatedInterval interval, PluginDetectionArtifact artifactObj, bool bStrikeout)
        {
            if (bStrikeout)
                interval.TotalNumOfContractions--;
            else
                interval.TotalNumOfContractions++;

            if (artifactObj.EndTime - artifactObj.StartTime > LongContractionMinLength)
            {
                if (bStrikeout)
                    interval.NumOfLongContractions--;
                else
                    interval.NumOfLongContractions++;
            }
        }

        private void StrikeoutEvent(CalculatedInterval interval, PluginDetectionArtifact artifactObj, bool bStrikeout)
        {
            if (artifactObj.EventType != ArtifactType.Acceleration && artifactObj.EventType != ArtifactType.Deceleration)
                return;

            if (artifactObj.EventType == ArtifactType.Acceleration)
            {
                if (bStrikeout)
                    interval.NumOfAccelerations--;
                else
                    interval.NumOfAccelerations++;
            }
            else if (artifactObj.EventType == ArtifactType.Deceleration)
            {
                PluginDeceleration decelItem = artifactObj as PluginDeceleration;
                if (bStrikeout)
                    interval.NumOfDecelerations--;
                else
                    interval.NumOfDecelerations++;

                if (decelItem.DecelerationCategory == DecelerationCategories.Late)
                {
                    if (bStrikeout)
                        interval.NumOfLateDecelerations--;
                    else
                        interval.NumOfLateDecelerations++;
                }
                else if (decelItem.DecelerationCategory == DecelerationCategories.Prolonged)
                {
                    if (bStrikeout)
                        interval.NumOfProlongedDecelerations--;
                    else
                        interval.NumOfProlongedDecelerations++;
                }
                else if (decelItem.DecelerationCategory == DecelerationCategories.Variable)
                {
                    if (bStrikeout)
                        interval.NumOfVariableDecelerations--;
                    else
                        interval.NumOfVariableDecelerations++;
                }
                else if (decelItem.DecelerationCategory == DecelerationCategories.Early)
                {
                    if (bStrikeout)
                        interval.NumOfEarlyDecelerations--;
                    else
                        interval.NumOfEarlyDecelerations++;
                }
                else
                {
                    if (bStrikeout)
                        interval.NumOfOtherDecelerations--;
                    else
                        interval.NumOfOtherDecelerations++;
                }
            }
        }
        
        private void FillIntervalsGap(DateTime untilTime)
        {
            var endNow = (untilTime != DateTime.MaxValue)? untilTime : DateTime.UtcNow;
            var lastEndTime = CalculatedIntervalsList.Max(c => c.EndTime);
            while (lastEndTime < endNow)
            {
                CreateExportData(lastEndTime.AddMinutes(2));
                lastEndTime = CalculatedIntervalsList.Max(c => c.EndTime);
            }
        }

        private void PerformPostIntervalCalculation()
        {
            int curLastIntervalID = IntervalId;
            UpdateIntervalIDs();
            CalculateReadyIntervalsValues(curLastIntervalID);
            var lastEventEnd = m_detectedArtifacts.Max(d => d.EndTime);
            m_detectedArtifacts.RemoveAll(c => c.EndTime < lastEventEnd - 3600); // one hour in seconds
            m_detectedContractions.RemoveAll(c => c.EndTime < lastEventEnd - 3600); // one hour in seconds
            if (IntervalId != curLastIntervalID)
                FireIntervalsUpdated();
        }

        /// <summary>
        /// Sets new interval duration in minutes on last open interval
        /// </summary>
        /// <param name="newInterval"></param>
        public void SetIntervalDuration(int newInterval)
        {
            lock (m_lock)
            {
                m_intervalDuration = newInterval;
                DateTime now = DateTime.UtcNow;
                var openIntervals = from c in CalculatedIntervalsList // here we consider only the intervals that end in the future (EndTime > time of the request)
                                    where c.EndTime >= now
                                    select c;

                if (openIntervals == null || openIntervals.Count() <= 0)
                    return;

                int newIntervalInSeconds = newInterval * 60;
                var toChange = openIntervals.ElementAt(0);
                if (toChange.IntervalDuration <= newInterval)
                {
                    if (toChange.StartTime.Minute % 2 != 0)
                        return;

                    toChange.IntervalDuration = newInterval;
                    toChange.EndTime = toChange.StartTime.AddSeconds(newIntervalInSeconds - 1);
                    return;
                }

                int ind = CalculatedIntervalsList.IndexOf(toChange);
                var startTime = toChange.StartTime;
                CalculatedIntervalsList.RemoveAll(c => (c.IntervalID == -1 && c.EndTime >= now));

                RecalculateIntervalCounters(startTime);
                int curLastIntervalID = IntervalId;
                if (CalculatedIntervalsList.Count > 0 && m_detectedArtifacts.Count > 0)
                    UpdateIntervalIDs();

                CalculateReadyIntervalsValues(curLastIntervalID);
                if (IntervalId != curLastIntervalID)
                    FireIntervalsUpdated();
            }
        }

        private void FireIntervalsUpdated(int intervalID = -1, DateTime? intervalEndTime = null)
        {
            var handler = IntervalsUpdated;
            if (handler != null)
                handler(this, new IntervalUpdatedEventArgs(intervalID, intervalEndTime.HasValue ? intervalEndTime.Value : DateTime.MinValue));
        }

        // called on interval changed
        private void RecalculateIntervalCounters(DateTime startTime)
        {
            List<PluginDetectionArtifact> contractions = (from c in m_detectedContractions
                                                          where c.PeakTime >= startTime.ToEpochTime()
                                                          select c as PluginDetectionArtifact).ToList();

            ComputeContractionRate(contractions);
            var events = (from c in m_detectedArtifacts
                          where c.EventType != ArtifactType.Baseline && c.StartTime > startTime.ToEpochTime()
                          select c).ToList();

            ComputeEventRatesNow(events);
        }

        private void CalculateReadyIntervalsValues(int curLastIntervalID)
        {
            var intervalsToCalc = from c in CalculatedIntervalsList
                                  where c.IntervalID >= curLastIntervalID
                                  select c;

            var baselines = (from c in m_detectedArtifacts
                             where c.EventType == ArtifactType.Baseline
                             select c as PluginBaseline).ToList();

            foreach (var item in intervalsToCalc)
            {
                long start = item.StartTime.ToEpochTime();
                long end = item.EndTime.ToEpochTime();
                int meanBaselineVariability = -1;
                double meanBaseline = -1f;
                if (QualifyData(start, end))
                    GetMeanBaseline(baselines, start, end, ref meanBaseline, ref meanBaselineVariability);

                item.MeanBaseline = (int)meanBaseline;
                item.MeanBaselineVariability = meanBaselineVariability;
                double mvUnits = GetMeanMontevideoUnits(m_detectedContractions, start, end);
                item.MeanMontevideoUnits = (int)mvUnits;
                double meanContractionInterval = GetMeanContractionInterval(m_detectedContractions, start, end);
                item.MeanContractionInterval = Math.Round(meanContractionInterval, 1);
                String contractionIntervalRange = GetContractionIntervalRange(m_detectedContractions, start, end);
                item.ContractionIntervalRange = contractionIntervalRange;
                List<String> contractionDurationRangeList = GetContractionDurationRange(m_detectedContractions, start, end);
                if (contractionDurationRangeList != null && contractionDurationRangeList.Count == 2)
                {
                    item.OriginalContractionDurationRange = contractionDurationRangeList[0];
                    item.ContractionDurationRange = contractionDurationRangeList[1];
                }
                else
                {
                    item.OriginalContractionDurationRange = String.Empty;
                    item.ContractionDurationRange = String.Empty;
                }
                List<String> contractionIntensityRangeList = GetContractionIntensityRange(m_detectedContractions, start, end);
                if (contractionIntensityRangeList != null && contractionIntensityRangeList.Count == 2)
                {
                    item.OriginalContractionIntensityRange = contractionIntensityRangeList[0];
                    item.ContractionIntensityRange = contractionIntensityRangeList[1];
                }
                else
                {
                    item.OriginalContractionIntensityRange = String.Empty;
                    item.ContractionIntensityRange = String.Empty;
                }
            }
        }

        private bool QualifyData(long start, long end)
        {
            double windowSizeInSec = end - start;
            var artifacts = (from c in m_detectedArtifacts
                             where c.EventType != ArtifactType.Contraction && c.EndTime > start && c.StartTime < end
                             select c).Distinct().ToList();

            long qualifyDatalength = 0;
            foreach (var item in artifacts)
            {
                long tmp = Math.Min(end, item.EndTime) - Math.Max(start, item.StartTime);
                qualifyDatalength += tmp;
            }

            Logger.WriteLogEntry(TraceEventType.Verbose, "Export.Calulator::QualifyData", "qualifyDatalength = " + qualifyDatalength.ToString());
            bool dataQualify = qualifyDatalength / windowSizeInSec * 100 > MINIMAL_DATA_PERCENTAGE_TO_QUALIFY;
            Logger.WriteLogEntry(TraceEventType.Verbose, "Export.Calulator::QualifyFHR", "qualifyDataPercentage = " + (qualifyDatalength / windowSizeInSec * 100).ToString());
            return dataQualify;
        }

        private void UpdateIntervalIDs()
        {
            var toSetIDs = from c in CalculatedIntervalsList
                           where c.IntervalID < 0
                           orderby c.StartTime
                           select c;

            var lastEventTime = m_detectedArtifacts.Max(c => c.EndTime).ToDateTime();
            foreach (var item in toSetIDs)
            {
                if (item.EndTime < lastEventTime)
                    item.IntervalID = IntervalId++;
            }

            CleanCollectedFHR();
        }

        private double GetMeanMontevideoUnits(List<PluginContraction> contractions, long ileft, long iright)
        {
            double toRet = -1f;
            long totalPeak = 0;

            foreach (var item in contractions)
            {
                if (item.IsStrikedOut)
                    continue;

                if (item.PeakTime <= iright && item.PeakTime>= ileft)
                {
                    long curHeight = GetContractionHeight(item);
                    totalPeak += curHeight;
                }
            }

            toRet = (totalPeak * 10) / m_intervalDuration;

            return toRet;
        }

        private double GetMeanContractionInterval(List<PluginContraction> contractions, long ileft, long iright)
        {
            double toRet = 0f;
            double intervalDuration = (iright - ileft) + 1;
            int numOfContractionsInCurInterval = 0;
            foreach (var item in contractions)
            {
                if (item.IsStrikedOut)
                    continue;

                if (item.PeakTime <= iright && item.PeakTime > ileft)
                {
                    numOfContractionsInCurInterval++;
                }
            }

            toRet = (numOfContractionsInCurInterval > 0) ? intervalDuration / (60 * numOfContractionsInCurInterval) : -1f;

            return toRet;
        }

        private long GetContractionHeight(PluginContraction ctr)
        {
            if (UPTracings.Count <= 0)
                return 0;

            long toRet = 0;
            var UPs = from c in UPTracings
                      where c.Key < ctr.EndTime && c.Key + c.Value.Count >= ctr.StartTime
                      orderby c.Key
                      select c;

            if (UPs.Count() <= 0)
                return 0;

            long nextStart = UPs.ElementAt(0).Key;
            long absStart = UPs.ElementAt(0).Key;
            List<byte> upsList = new List<byte>();
            foreach (var item in UPs)
            {
                if (nextStart < item.Key)
                    FillGap(upsList, ref nextStart, item.Key);

                if (nextStart > item.Key)
                    RemoveOverlap(upsList, ref nextStart, item.Key);

                upsList.AddRange(item.Value);
                nextStart += item.Value.Count;
            }

            if (upsList.Count <= 0)
                return 0;
            
            int start = (int)(ctr.StartTime - absStart);
            int end = (int)(ctr.EndTime - absStart) - 1;
            int peak = (int)(ctr.PeakTime - absStart);

            SetInRange(ref start, upsList.Count);
            SetInRange(ref end, upsList.Count);
            SetInRange(ref peak, upsList.Count);

            long vs = upsList[start];
            long ve = upsList[end];
            long vp = upsList[peak];

            toRet = vp - Math.Min(vs, ve);
            return toRet;
        }

        protected String GetContractionIntervalRange(List<PluginContraction> contractions, long ileft, long iright)
        {
            long prevIntervalStart = ileft - (m_intervalDuration * 60);
            var lastPrevCtrs = (from c in contractions
                                where c.PeakTime >= prevIntervalStart && c.PeakTime < ileft
                                select c).ToList();

            long lastPrevCtrPeak = -1;
            if (lastPrevCtrs.Count > 0)
                lastPrevCtrPeak = lastPrevCtrs.Max(c => c.PeakTime);

            var intervalContractions = (from c in contractions
                                        where c.PeakTime >= ileft && c.PeakTime <= iright
                                        orderby c.PeakTime
                                        select c.PeakTime).ToList();

            if (intervalContractions.Count <= 0)
                return String.Empty;

            long minInterval = -1, maxInterval = -1;
            if (lastPrevCtrPeak > -1)
                minInterval = maxInterval = intervalContractions[0] - lastPrevCtrPeak;

            for (int i = 1; i < intervalContractions.Count; i++)
            {
                long curCtrInterval = intervalContractions[i] - intervalContractions[i - 1];
                if (curCtrInterval > maxInterval || maxInterval == -1)
                    maxInterval = curCtrInterval;

                if (curCtrInterval < minInterval || minInterval == -1)
                    minInterval = curCtrInterval;
            }

            long roundedMinInterval = (long)Math.Round(minInterval / 60f);
            long roundedMaxInterval = (long)Math.Round(maxInterval / 60f);

            String toRet = String.Empty;
            if (minInterval == -1 || maxInterval == -1)
                return toRet;
            else if (roundedMinInterval == roundedMaxInterval)
                toRet = roundedMinInterval.ToString();
            else
            {
                String maxMin = roundedMaxInterval.ToString();
                String minMin = roundedMinInterval.ToString();
                toRet = minMin + " - " + maxMin;
            }

            return toRet;
        }

        protected List<String> GetContractionDurationRange(List<PluginContraction> contractions, long ileft, long iright)
        {
            List<String> result = new List<String>();
            var intervalContractions = (from c in contractions
                                        where c.PeakTime >= ileft && c.PeakTime <= iright
                                        orderby c.PeakTime
                                        select c).ToList();

            if (intervalContractions.Count <= 0)
            {
                result.Add(String.Empty);
                result.Add(String.Empty);
                return result;
                //return String.Empty;
            }

            long minDuration = -1, maxDuration = -1;
            foreach (var item in intervalContractions)
            {
                if (item.IsStrikedOut)
                    continue;

                long curDur = item.EndTime - item.StartTime;
                if (curDur < minDuration || minDuration <= -1)
                    minDuration = curDur;

                if (curDur > maxDuration || maxDuration <= -1)
                    maxDuration = curDur;
            }

            String toRet = String.Empty;
            String toRetOriginal = String.Empty;
            if (minDuration == -1 || maxDuration == -1)
            {
                result.Add(toRetOriginal);
                result.Add(toRet);                
                return result;

                //return toRet;
            }

            if (minDuration == maxDuration)
                toRetOriginal = minDuration.ToString();
            else
                toRetOriginal = minDuration.ToString() + " - " + maxDuration.ToString();            

            double durationRounding = AlgorithmSettings.Instance.CTRDurationRoundingTo;
            minDuration = (long)(Math.Round(minDuration / durationRounding, MidpointRounding.AwayFromZero) * durationRounding);
            maxDuration = (long)(Math.Round(maxDuration / durationRounding, MidpointRounding.AwayFromZero) * durationRounding);
            if (minDuration == maxDuration)
                toRet = minDuration.ToString();
            else
                toRet = minDuration.ToString() + " - " + maxDuration.ToString();

            result.Add(toRetOriginal);
            result.Add(toRet);
            return result;
        }

        protected List<String> GetContractionIntensityRange(List<PluginContraction> contractions, long ileft, long iright)
        {
            List<String> result = new List<String>();

            var intervalContractions = (from c in contractions
                                        where c.PeakTime >= ileft && c.PeakTime <= iright
                                        orderby c.PeakTime
                                        select c).ToList();

            if (intervalContractions.Count <= 0)
            {
                result.Add(String.Empty);
                result.Add(String.Empty);
                return result;
                //return String.Empty;
            }

            long minHeight = -1, maxHeight = -1;
            foreach (var item in intervalContractions)
            {
                if (item.IsStrikedOut)
                    continue;

                long curHeight = GetContractionHeight(item);
                if (curHeight < minHeight || minHeight <= -1)
                    minHeight = curHeight;

                if (curHeight > maxHeight || maxHeight <= -1)
                    maxHeight = curHeight;
            }


            String toRet = String.Empty;
            String toRetOriginal = String.Empty;
            if (minHeight == -1 || maxHeight == -1)
            {
                result.Add(toRetOriginal);
                result.Add(toRet);
                return result;
                //return toRet;
            }

            if (minHeight == maxHeight)
                toRetOriginal = minHeight.ToString();
            else
                toRetOriginal = minHeight.ToString() + " - " + maxHeight.ToString();

            double intensityRounding = AlgorithmSettings.Instance.CTRIntesityRoundingTo;
            minHeight = (long)(Math.Round(minHeight / intensityRounding, MidpointRounding.AwayFromZero) * intensityRounding);
            maxHeight = (long)(Math.Round(maxHeight / intensityRounding, MidpointRounding.AwayFromZero) * intensityRounding);
            if (minHeight == maxHeight)
                toRet = minHeight.ToString();
            else
                toRet = minHeight.ToString() + " - " + maxHeight.ToString();

            result.Add(toRetOriginal);
            result.Add(toRet);
            return result;
        }

        private void SetInRange(ref int index, int size)
        {
            if (index < 0)
                index = 0;

            if (index >= size)
                index = size - 1;            
        }

        #region Events & Delegates

        public EventHandler<IntervalUpdatedEventArgs> IntervalsUpdated;

        #endregion

        #region Export data methods

        private void ResetExportData(long time)
        {
            CalculatedIntervalsList.Clear();
            DateTime dt = time.ToDateTime();
            int intervalInSeconds = m_intervalDuration * 60;
            int startMinute = (dt.Minute / m_intervalDuration) * m_intervalDuration;
            DateTime startTime = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, startMinute, 0, 0, dt.Kind);
            CalculatedInterval data = new CalculatedInterval(m_intervalDuration)
            {
                StartTime = startTime,
                EndTime = startTime.AddSeconds(intervalInSeconds - 1)
            };

            CalculatedIntervalsList.Add(data);
            m_lastExportedBlock = 0;
        }

        private CalculatedInterval CreateExportData(DateTime peakTime)
        {
            int start = (peakTime.Minute / m_intervalDuration) * m_intervalDuration;
            int curIntervalDuration = start % 2 == 0 ? m_intervalDuration : 15;
            int intervalInSeconds = curIntervalDuration * 60;
            CalculatedInterval data = new CalculatedInterval(curIntervalDuration);
            DateTime startTime = new DateTime(peakTime.Year, peakTime.Month, peakTime.Day, peakTime.Hour, start, 0, 0, peakTime.Kind);
            data.StartTime = startTime;
            data.EndTime = startTime.AddSeconds(intervalInSeconds - 1);

            if (CalculatedIntervalsList.Count > 0 && peakTime > CalculatedIntervalsList.Max(c => c.EndTime))
                CalculatedIntervalsList.Add(data);
            else
            {
                CalculatedInterval item = (from c in CalculatedIntervalsList
                                           where peakTime < c.StartTime
                                           select c).FirstOrDefault();
                if (item == null)
                    CalculatedIntervalsList.Add(data);
                else
                {
                    int ind = CalculatedIntervalsList.IndexOf(item);
                    CalculatedIntervalsList.Insert(ind, data);
                }
            }

            return data;
        }

        #endregion

        #region Tracings operations

        private void RemoveOverlap(List<byte> upsList, ref long nextStart, long tracingStart)
        {
            while (nextStart-- > tracingStart)
            {
                upsList.RemoveAt(upsList.Count - 1);
            }
        }

        private void FillGap(List<byte> upsList, ref long nextStart, long tracingStart)
        {
            while (nextStart++ < tracingStart)
            {
                upsList.Add(255);
            }
        }

        public void AppendUPTracings(long start, List<byte> ups)
        {
            UPTracings[start] = ups;
        }

        public void AppendFHRTracings(long start, List<byte> hrs)
        {
            FHRTracings[start] = hrs;
        }

        #endregion

        #region Overrides

        public override void AppendContractionsRates(long windowSize, long contractionPeakTime, bool bStrikedOut)
        {
            DateTime peakTime = contractionPeakTime.ToDateTime();
            CalculatedInterval data = (from c in CalculatedIntervalsList
                                       where c.StartTime < peakTime && peakTime < c.EndTime
                                       select c).FirstOrDefault();

            if (data == null)
                data = CreateExportData(peakTime);

            data.TotalNumOfContractions++;
        }

        public override void AppendContractions120SecRates(long windowSize, long contractionPeakTime, bool bStrikedOut)
        {
            //The long contraction is a subset of generic contraction, so no need to create ExportData since it was created by Contraction
            DateTime peakTime = contractionPeakTime.ToDateTime();
            CalculatedInterval data = null;
            data = (from c in CalculatedIntervalsList
                    where c.StartTime < peakTime && peakTime < c.EndTime
                    select c).FirstOrDefault();

            if (data == null)
                data = CreateExportData(peakTime);

            data.NumOfLongContractions++;
        }

        protected override void AppendDeceleration(PluginDeceleration decelItem, long fhrWindow)
        {
            AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.Deceleration, decelItem.IsStrikedOut);

            if (decelItem.DecelerationCategory == DecelerationCategories.Late)
            {
                AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.LateDeceleration, decelItem.IsStrikedOut);
            }
            else if (decelItem.DecelerationCategory == DecelerationCategories.Prolonged)
            {
                AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.ProlongedDeceleration, decelItem.IsStrikedOut);
            }
            else if (decelItem.DecelerationCategory == DecelerationCategories.Variable)
            {
                AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.VariableDeceleration, decelItem.IsStrikedOut);
            }
            else if (decelItem.DecelerationCategory == DecelerationCategories.Early)
            {
                AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.EarlyDeceleration, decelItem.IsStrikedOut);
            }
            else
            {
                AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.OtherDecels, decelItem.IsStrikedOut);
            }
        }

        protected override void AppendEventRate(long windowSize, long eventPeakTime, EventClassification eventType, bool bStrikedOut)
        {
            DateTime peakTime = eventPeakTime.ToDateTime();
            CalculatedInterval data = (from c in CalculatedIntervalsList
                                       where c.StartTime < peakTime && peakTime < c.EndTime
                                       select c).FirstOrDefault();

            if (data == null)
                data = CreateExportData(peakTime);

            switch (eventType)
            {
                case EventClassification.Acceleration:
                    data.NumOfAccelerations++;
                    break;
                case EventClassification.Deceleration:
                    data.NumOfDecelerations++;
                    break;
                case EventClassification.EarlyDeceleration:
                    data.NumOfEarlyDecelerations++;
                    break;
                case EventClassification.LateDeceleration:
                    data.NumOfLateDecelerations++;
                    break;
                case EventClassification.ProlongedDeceleration:
                    data.NumOfProlongedDecelerations++;
                    break;
                case EventClassification.VariableDeceleration:
                    data.NumOfVariableDecelerations++;
                    break;
                case EventClassification.LongAndLargeDeceleration:
                case EventClassification.OtherDecels:
                    data.NumOfOtherDecelerations++;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region merge

        public List<PastIntervalDuration> GetIntervalsDurationHistory()
        {
            List<PastIntervalDuration> durationsHistory = new List<PastIntervalDuration>();
            if (CalculatedIntervalsList.Count > 0)
            {
                CalculatedIntervalsList.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));       
             

                foreach (var interval in CalculatedIntervalsList)
                {
                    if (durationsHistory.Count == 0 || durationsHistory.Last().IntervalDuration != interval.IntervalDuration)
                    {
                        PastIntervalDuration pastDuration = new PastIntervalDuration()
                        {
                            StartTime = interval.StartTime,
                            EndTime = interval.EndTime,
                            IntervalDuration = interval.IntervalDuration
                        };
                        durationsHistory.Add(pastDuration);
                    }
                    else
                    {
                        durationsHistory.Last().EndTime = interval.EndTime;
                    } 
                }

            }
            if (durationsHistory.Count == 0 || durationsHistory.Last().IntervalDuration != this.m_intervalDuration)
            {
                PastIntervalDuration pastDuration = new PastIntervalDuration()
                {
                    StartTime = (durationsHistory.Count == 0) ? DateTime.MinValue : durationsHistory.Last().EndTime,
                    EndTime = DateTime.MaxValue,
                    IntervalDuration = this.m_intervalDuration
                };
                durationsHistory.Add(pastDuration);

            }
            else 
            {
                durationsHistory.Last().EndTime = DateTime.MaxValue;
            }
            
            return durationsHistory;
        }

        public void CalculateExportDataAfterMerge(List<PluginDetectionArtifact> detectedObjects, long absoluteStart, List<PastIntervalDuration> intervalDurationsHistory)
        {
            lock (m_lock)
            {
                int currentIntervalDuration = m_intervalDuration;
                if (intervalDurationsHistory.Count > 0)
                    m_intervalDuration = intervalDurationsHistory.First().IntervalDuration;
                ResetExportData(absoluteStart);                
                foreach (var pastIntervalDuration in intervalDurationsHistory)
                {
                    //m_intervalDuration = pastIntervalDuration.IntervalDuration;
                    SetIntervalDuration(pastIntervalDuration.IntervalDuration);
                    FillIntervalsGap(pastIntervalDuration.EndTime);
                    var ctrs = (from c in detectedObjects
                                where c.EventType == ArtifactType.Contraction && c.Id > m_lastCalculatedContractionID
                                orderby c.EndTime
                                select c as PluginContraction).ToList();

                    var distinctCtrs = ctrs.Except(m_detectedContractions).ToList();
                    m_detectedContractions.AddRange(distinctCtrs);

                    UpdateContractionsRates(detectedObjects);
                    var toAdd = (from c in detectedObjects
                                 where c.EventType != ArtifactType.Contraction && c.Id > m_lastCalculatedEventID
                                 orderby c.EndTime
                                 select c).ToList();

                    if (toAdd.Count() <= 0)
                        break;

                    var toAddDistinct = toAdd.Except(m_detectedArtifacts).ToList();
                    m_detectedArtifacts.AddRange(toAddDistinct);

                    UpdateEventsRates(toAdd);                   
                    UpdateClosedIntervals();
                }

                SetIntervalDuration(currentIntervalDuration);
                // After all calculations we update the interval ID's
                PerformPostIntervalCalculation();
            }
        }

    

        #endregion 
    }

    public class IntervalUpdatedEventArgs : EventArgs
    {
        public int IntervalID { get; set; }
        public DateTime IntervalEndTime { get; set; }

        public IntervalUpdatedEventArgs(int intervalID, DateTime intervalEndTime)
        {
            IntervalID = intervalID;
            IntervalEndTime = intervalEndTime;
        }
    }
}
