using CommonLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PluginsAlgorithms
{
    public enum EventClassification
    {
        Acceleration,
        Deceleration,
        EarlyDeceleration,
        LateDeceleration,
        LongAndLargeDeceleration,
        ProlongedDeceleration,
        VariableDeceleration,
        OtherDecels
    }

    public enum ActionTypes
    {
        None = 0,
        StrikeoutContraction = 1,
        StrikeoutEvent = 2,
        ConfirmEvent = 3,
        UndoStrikeoutEvent = 4,
        UndoStrikeoutContraction = 5,
    }

    public abstract class EventsCalculator
    {
        #region Definitions

        public int m_drawingWindowSize = 30; // 30 minutes window
        protected const int UPSampleRate = 1; // 1 Hz UP smple rate
        protected const int FHRSampleRate = 1; //  1 Hz FHR smple rate - at this point we look only at events with depenency on tracings so we calculate in full seconds
        protected const int UPPenUp = 127;
        protected const long LongContractionMinLength = 120; // seconds

        public virtual int DrawingWindowSize
        {
            get { return m_drawingWindowSize; }
            set { m_drawingWindowSize = value; }
        }

        #endregion

        protected List<PluginDetectionArtifact> m_detectedArtifacts = new List<PluginDetectionArtifact>();

        public List<long> ContractionsRate { get; protected set; }
        public List<long> ContractionsRateIndex { get; protected set; }
        public List<long> Contractions120SecRate { get; protected set; }
        public List<long> Contractions120SecRateIndex { get; protected set; }

        public List<PluginEventsRate> EventsRate { get; protected set; }
        public List<long> EventsRateIndex { get; protected set; }
        
        protected int m_contractionsRateLast;
        protected int m_contractions120SecRateLast;

        protected int m_accelerationRateLast;
        protected int m_lateDecelRateLast;
        protected int m_prolongedDecelRateLast;

        protected int m_lastCalculatedContractionID;
        protected int m_lastCalculatedEventID;

        public Dictionary<int, int> ActionId2ContractilityId { get; set; }
        
        public EventsCalculator()
        {
            m_contractionsRateLast = 0;
            m_contractions120SecRateLast = 0;

            m_accelerationRateLast = 0;
            m_lateDecelRateLast = 0;
            m_prolongedDecelRateLast = 0;

            m_lastCalculatedContractionID = 0;
            m_lastCalculatedEventID = 0;

            ContractionsRate = new List<long>();
            ContractionsRateIndex = new List<long>();
            Contractions120SecRate = new List<long>();
            Contractions120SecRateIndex = new List<long>();
            ResetContractionsRates();

            EventsRateIndex = new List<long>();

            ActionId2ContractilityId = new Dictionary<int, int>();
        }

        #region Contractions related calculations

        public void ResetContractionsRates()
        {
            ClearContractionsRate();
            ClearContractionsRateIndex();
            ClearContractions120SecRate();
            ClearContractions120SecRateIndex();
            m_contractionsRateLast = 0;
            m_contractions120SecRateLast = 0;
        }

        private void ClearContractionsRate()
        {
            ContractionsRate.Clear();
            ContractionsRate.Add(0);
        }

        private void ClearContractionsRateIndex()
        {
            ContractionsRateIndex.Clear();
            ContractionsRateIndex.Add(0);
        }

        private void ClearContractions120SecRate()
        {
            Contractions120SecRate.Clear();
            Contractions120SecRate.Add(0);
        }

        private void ClearContractions120SecRateIndex()
        {
            Contractions120SecRateIndex.Clear();
            Contractions120SecRateIndex.Add(0);
        }

        protected void UpdateContractionsRates(List<PluginDetectionArtifact> detectedObjects)
        {
            var contractions = (from c in detectedObjects
                                where c.EventType == ArtifactType.Contraction && c.Id > m_lastCalculatedContractionID
                                orderby c.Id
                                select c).ToList();

            ComputeContractionRate(contractions);
            int lastCTRId = m_lastCalculatedContractionID;
            if (contractions.Count() > 0)
                lastCTRId = contractions.Last().Id;

            m_lastCalculatedContractionID = lastCTRId;
        }

        public void ComputeContractionRate(List<PluginDetectionArtifact> contractions)
        {
            foreach (var item in contractions)
            {
                PluginContraction contractionItem = item as PluginContraction;
                long windowSize = DrawingWindowSize;
                AppendContractionsRates(windowSize, contractionItem.PeakTime, contractionItem.IsStrikedOut);

                if (contractionItem.EndTime - contractionItem.StartTime > LongContractionMinLength)
                {
                    AppendContractions120SecRates(windowSize, contractionItem.PeakTime, contractionItem.IsStrikedOut);
                }
            }
        }

        public abstract void AppendContractionsRates(long windowSize, long contractionPeakTime, bool bStrikedOut);
        public abstract void AppendContractions120SecRates(long windowSize, long contractionPeakTime, bool bStrikedOut);
        
        #endregion

        #region Events related calculations

        protected virtual void ResetEventsRates()
        {
            m_accelerationRateLast = 0;
            m_lateDecelRateLast = 0;
            m_prolongedDecelRateLast = 0;

            EventsRateIndex.Clear();
            EventsRateIndex.Add(0);
            ResetEventsRatesInternal();
        }

        protected virtual void ResetEventsRatesInternal()
        {
            PluginEventsRate toInsert = new PluginEventsRate();
            EventsRate.Clear();
            EventsRate.Add(toInsert);
        }

        protected void UpdateEventsRates(List<PluginDetectionArtifact> artifacts)
        {
            var events = (from c in artifacts
                          where c.EventType != ArtifactType.Baseline && c.Id > m_lastCalculatedEventID
                          select c).ToList();

            ComputeEventRatesNow(events);
            int lastEventId = m_lastCalculatedEventID;
            if (events.Count() > 0)
                lastEventId = events.Last().Id;

            m_lastCalculatedEventID = lastEventId;
        }

        protected void ComputeEventRatesNow(List<PluginDetectionArtifact> events)
        {
            foreach (var item in events)
            {
                PluginAcceleration accelItem = item as PluginAcceleration;
                long fhrWindow = DrawingWindowSize;
                ComputeEventRatesNowInternal(accelItem, fhrWindow);
            }
        }

        protected virtual void ComputeEventRatesNowInternal(PluginAcceleration accelItem, long fhrWindow)
        {
                if (accelItem.EventType == ArtifactType.Acceleration)
                {
                    AppendAcceleration(accelItem, fhrWindow);
                }
                else if (accelItem.EventType == ArtifactType.Deceleration)
                {
                    PluginDeceleration decelItem = accelItem as PluginDeceleration;
                    AppendDeceleration(decelItem, fhrWindow);
                }
                else
                    return;
        }

        protected virtual void AppendAcceleration(PluginAcceleration accelItem, long fhrWindow)
        {
            AppendEventRate(fhrWindow, accelItem.PeakTime, EventClassification.Acceleration, accelItem.IsStrikedOut);
        }

        protected abstract void AppendDeceleration(PluginDeceleration accelItem, long fhrWindow);
        protected abstract void AppendEventRate(long windowSize, long eventPeakTime, EventClassification eventType, bool bStrikedOut);

        #endregion

        // =================================================================================================================
        //    Look for index in m_<event>Rate for given fhr index. We first look at the last returned index and, if the given fhr index is
        //    not close, that is if it does not fall between ratesIndex[rateLast - 1] and ratesIndex[rateLast + 1], we go for
        //    the binary search.
        // ===================================================================================================================
        public int FindEventRateIndex(long index, ref int rateLast, List<long> ratesIndex)
        {
            if (ratesIndex.Count == 0)
            {
                return -1;
            }

            int a = rateLast;
            int b = rateLast;
            int n = ratesIndex.Count;

            if (index < 0)
            {
                index = 0;
            }

            if (index >= ratesIndex[ratesIndex.Count - 1])
            {
                rateLast = n - 1;
            }
            else if (rateLast > 0 && index < ratesIndex[rateLast - 1])
            {
                a = 0;
            }
            else if (index < ratesIndex[rateLast])
            {
                rateLast--;
            }
            else if (rateLast + 2 < n && index > ratesIndex[rateLast + 2])
            {
                b = n;
            }
            else if (rateLast + 2 < n && index == ratesIndex[rateLast + 2])
            {
                rateLast += 2;
            }
            else if (rateLast + 1 < n && index >= ratesIndex[rateLast + 1])
            {
                rateLast++;
            }

            if (a != b)
            {
                while (a < b - 1)
                {
                    if (index == ratesIndex[(a + b) / 2])
                    {
                        a = b = (a + b) / 2;
                    }
                    else if (index < ratesIndex[(a + b) / 2])
                    {
                        b = (a + b) / 2;
                    }
                    else
                    {
                        a = (a + b) / 2;
                    }
                }

                rateLast = a;
            }

            return rateLast;
        }

        protected void GetMeanBaseline(IEnumerable<PluginBaseline> baselines, long ileft, long iright, ref double meanBaseline, ref int meanVar)
        {
            double totalBL = 0f;
            double totalBLVar = 0f;
            double meanBL = -1f;
            double meanBLVar = -1f;
            long totalSamples = 0, totalSamplesVar = 0;
            long minSamples = 2 * 60 * FHRSampleRate;
            long minBaselineLength = 0;
            long totalBaselineTrim = 0;

            // 30/03/2015: Copied from PatternsEngine and adapted to work at 1 Hz
            // NOTE 27/05/08 : until have mechanism to access signal processing config without needing the signal
            // * processing engine code, we hardcode these params so that they are equivalent whether running Patterns in
            // * client or server mode
            minBaselineLength = 15;			// (30 / 2) -> need half of 30 second segment at 1 Hz
            totalBaselineTrim = 10;			// 2 * 5 -> 5 seconds at 1 Hz on each side of baseline are trimmed for variability calculations

            foreach (var item in baselines)
            {
                if (item.StartTime <= iright && item.EndTime >= ileft)
                {
                    long baselineLength = item.EndTime - item.StartTime + 1;
                    if (baselineLength - totalBaselineTrim >= minBaselineLength) // baselines that are too short will not have variability calculated
                    {
                        if (item.StartTime < ileft)
                        {	// starts before window - don't consider samples outside of window
                            baselineLength -= (ileft - item.StartTime);
                        }

                        if (item.EndTime > iright)
                        {	// end after window - don't consider samples outside window
                            baselineLength -= (item.EndTime - iright);
                        }

                        totalSamples += baselineLength;
                        totalBL += (double)baselineLength * ((item.Y1 + item.Y2) / 2f); // take mean of y1 and y2 as level for individual baseline
                        if (item.BaselineVariability > 0)
                        {
                            totalSamplesVar += baselineLength;
                            totalBLVar += (double)baselineLength * (item.BaselineVariability);		// var as calculated in engine
                        }
                    }
                }
            }

            if (totalSamples > minSamples)	// enough baseline to make valid estimate
            {
                meanBL = (double)totalBL / totalSamples;
            }

            if (totalSamplesVar > minSamples)
            {
                meanBLVar = (double)totalBLVar / totalSamples;
            }

            meanBaseline = meanBL;
            meanVar = (int)Math.Floor(meanBLVar);
        }

        protected virtual long GetAccelerationRateAt(long fhrIndex)
        {
            return EventsRate[FindEventRateIndex(fhrIndex, ref m_accelerationRateLast, EventsRateIndex)].AccelerationRate;
        }

        protected virtual long GetLateDecelRateAt(long fhrIndex)
        {
            return EventsRate[FindEventRateIndex(fhrIndex, ref m_lateDecelRateLast, EventsRateIndex)].LateDecelRate;
        }

        protected virtual long GetProlongedDecelRateAt(long fhrIndex)
        {
            return EventsRate[FindEventRateIndex(fhrIndex, ref m_prolongedDecelRateLast, EventsRateIndex)].ProlongedDecelRate;
        }

        /// <summary>
        /// Collects UP tracings once after action (strikeout or undo strikeout)
        /// </summary>
        /// <param name="tracings"></param>
        /// <param name="bIncremental"></param>
        /// <param name="calc"></param>
        public static void CollectUPTracings(XElement tracings, out List<UPTracingsBlockIndicator> compressedUPTracings, out long upStartTime)
        {
            compressedUPTracings = null;
            upStartTime = -1;
            if (tracings == null)
                return;

            var tracingsList = tracings.Elements();
            if (tracingsList == null || tracingsList.Count() <= 0)
                return;

            var tracing = tracingsList.ElementAt(0);
            if (tracing == null)
                return;

            var startTime = tracing.Attribute("start").Value;
            if (!Int64.TryParse(startTime, out upStartTime))
            {
                Logger.WriteLogEntry(TraceEventType.Verbose, "Tracings Data", "start time: " + startTime);
                return;
            }

            var upTracingBase64Str = tracing.Attribute("up").Value;
            if (upTracingBase64Str == null || upTracingBase64Str.Equals(String.Empty))
                return;

            var upTracings = Convert.FromBase64String(upTracingBase64Str).ToList();
            if (upTracings == null)
                return;

            compressedUPTracings = new List<UPTracingsBlockIndicator>() { new UPTracingsBlockIndicator() { ValidDataEnd = upStartTime, ValidDataStart = upStartTime } };
            EventsCalculator.CompressUPTracings(upStartTime, upTracings, compressedUPTracings);
        }

        public static void CompressUPTracings(long start, List<byte> ups, List<UPTracingsBlockIndicator> compressedUPTracings)
        {
            int numOfUPsBefore = compressedUPTracings.Count;
            long curTimeStamp = start;
            bool bLastValid = true;
            if (compressedUPTracings.Count <= 0)
                compressedUPTracings.Add(new UPTracingsBlockIndicator() { ValidDataEnd = start, ValidDataStart = start });

            foreach (var item in ups)
            {
                if (item < UPPenUp)
                {
                    if (!bLastValid)
                        compressedUPTracings.Add(new UPTracingsBlockIndicator() { ValidDataStart = curTimeStamp, ValidDataEnd = curTimeStamp });

                    compressedUPTracings.Last().ValidDataEnd = curTimeStamp;
                    bLastValid = true;
                }
                else
                {
                    bLastValid = false;
                }

                curTimeStamp++;
            }

            int numOfUPsAfter = compressedUPTracings.Count;
            //Logger.WriteLogEntry(TraceEventType.Warning, "CRICalculator", "Number of compressed UP elements: before = " + numOfUPsBefore.ToString() + ", after = " + numOfUPsAfter.ToString());
        }

    }
}
