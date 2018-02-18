using CommonLogger;
using CRIEntities;
using PluginsAlgorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRIAlgorithm
{
    public class CRICalculator : EventsCalculator
    {
        protected Object m_lock = new Object();

        #region Configuration

        public override int DrawingWindowSize
        {
            get { return m_drawingWindowSize * 60 * FHRSampleRate; }
        }

        public int MINIMAL_BASELINE_VARIABILITY = CRIAlgorithmSettings.Instance.MinimalBaselineVariability;
        public int ACCEL_RATE_AMOUNT = CRIAlgorithmSettings.Instance.MinimalAccelerationsAmount;
        public int LATE_DECEL_RATE_AMOUNT = CRIAlgorithmSettings.Instance.MinimalLateDecelAmount;
        public int LATE_DECEL_AND_LARGE_AND_LONG_RATE_AMOUNT = CRIAlgorithmSettings.Instance.MinimalLateAndLargeAndLongDecelAmount;
        public int LATE_DECEL_AND_PROLONGED_RATE_AMOUNT = CRIAlgorithmSettings.Instance.MinimalLateAndProlongedDecelAmount;
        public int LARGE_AND_LONG_DECEL_RATE_AMOUNT = CRIAlgorithmSettings.Instance.MinimalLargeAndLongDecelAmount;
        public int CONTRACTIONS_RATE_AMOUNT = CRIAlgorithmSettings.Instance.MinimalContractionsAmount;
        public int LONG_CONTRACTIONS_RATE_AMOUNT = CRIAlgorithmSettings.Instance.MinimalLongContractionsAmount;

        protected float MINIMAL_LATE_DECEL_CONFIDENCE_LEVEL = CRIAlgorithmSettings.Instance.MinimalLateDecelConfidence;
        protected int MINIMAL_PROLONGED_DECEL_HEIGHT = CRIAlgorithmSettings.Instance.MinimalProlongedDecelHeight;

        protected int CONTRACTILITY_QUALIFY_WINDOW_SIZE = CRIAlgorithmSettings.Instance.CRIStateQualificationWindowSize;
        protected int MINIMAL_DATA_PERCENTAGE_TO_QUALIFY = CRIAlgorithmSettings.Instance.MinimalAmountOfDataInQualificationWindow;

        #endregion

        protected double m_lastMeanBaselineVariability;

        public new List<CRIEventsRate> EventsRate { get; protected set; }

        protected int m_longAndLargeDecelsRateLast;

        public List<Contractility> Contractilities { get; protected set; }
        public int ContractilityId { get; set; }
        protected long m_lastCalculatedContractility;
        public long LastCalculatedContractility { set { m_lastCalculatedContractility = value; } }

        protected List<UPTracingsBlockIndicator> m_compressedUPTracings = new List<UPTracingsBlockIndicator>();

        public CRIEvents LastCalculatedEvents
        {
            get
            {
                var toRet = new CRIEvents();
                int nEventsMidIndex = EventsRate.Count > 1 ? EventsRate.Count / 2 : 0;

                long tempVal = ContractionsRate[ContractionsRate.Count > 1 ? ContractionsRate.Count / 2 : 0];
                toRet.Contractions = new EventCounterLong() { Value = tempVal, IsReason = tempVal >= CONTRACTIONS_RATE_AMOUNT };

                tempVal = Contractions120SecRate[Contractions120SecRate.Count > 1 ? Contractions120SecRate.Count / 2 : 0];
                toRet.LongContractions = new EventCounterLong() { Value = tempVal, IsReason = tempVal >= LONG_CONTRACTIONS_RATE_AMOUNT };

                tempVal = EventsRate[nEventsMidIndex].AccelerationRate;
                toRet.Variability = new EventCounterDouble() { Value = m_lastMeanBaselineVariability, IsReason = m_lastMeanBaselineVariability < MINIMAL_BASELINE_VARIABILITY && tempVal <= ACCEL_RATE_AMOUNT };
                toRet.Accels = new EventCounterLong() { Value = tempVal, IsReason = m_lastMeanBaselineVariability < MINIMAL_BASELINE_VARIABILITY && tempVal <= ACCEL_RATE_AMOUNT };

                toRet.LateDecels = new EventCounterLong() { Value = EventsRate[nEventsMidIndex].LateDecelRate };
                toRet.ProlongedDecels = new EventCounterLong() { Value = EventsRate[nEventsMidIndex].ProlongedDecelRate };
                toRet.LargeDeceles = new EventCounterLong() { Value = EventsRate[nEventsMidIndex].LongAndLargeDecelRate };

                toRet.LateDecels.IsReason = toRet.LateDecels.Value >= LATE_DECEL_RATE_AMOUNT || toRet.LateDecels.Value + toRet.LargeDeceles.Value + toRet.ProlongedDecels.Value >= LATE_DECEL_RATE_AMOUNT;
                toRet.ProlongedDecels.IsReason = toRet.LateDecels.Value > 0 && toRet.ProlongedDecels.Value > 0;
                toRet.LargeDeceles.IsReason = (toRet.LateDecels.Value > 0 && toRet.LargeDeceles.Value > 0) || (toRet.LargeDeceles.Value >= LARGE_AND_LONG_DECEL_RATE_AMOUNT);

                return toRet;
            }
        }

        public CRICalculator()
        {
            m_longAndLargeDecelsRateLast = 0;
            m_lastCalculatedContractility = 0;

            m_lastMeanBaselineVariability = -1;

            EventsRate = new List<CRIEventsRate>();
            ResetEventsRates();
            Contractilities = new List<Contractility>();
            ContractilityId = 0;
        }

        public void CalculateContractility(List<PluginDetectionArtifact> detectedObjects, int lastReceivedCRI, long absoluteStart, bool bIncremental)
        {
            lock (m_lock)
            {
                if (Contractilities.Count == 0)
                {
                    ResetContractilities(absoluteStart);
                }

                if (Contractilities.Count > 0 && Contractilities[0].Id > -1)
                {
                    if (lastReceivedCRI < Contractilities.Count)
                    {
                        if (lastReceivedCRI <= 0 && !bIncremental && ContractionsRate.Count == 1 && Contractions120SecRate.Count == 1 && EventsRate.Count == 1)
                        {
                            UpdateContractionsRates(detectedObjects);
                            var oldEvents = (from c in detectedObjects
                                             where c.EventType != ArtifactType.Contraction && c.Id > m_lastCalculatedEventID
                                             orderby c.EndTime
                                             select c).Distinct().ToList();

                            if (oldEvents.Count() <= 0)
                                return;

                            m_detectedArtifacts.AddRange(oldEvents);
                            UpdateEventsRates(oldEvents);
                            CalculateContractilityInternal(detectedObjects, oldEvents);
                        }

                        return;
                    }
                }

                UpdateContractionsRates(detectedObjects);
                if (m_lastCalculatedContractility == 0)
                {
                    m_lastCalculatedContractility = absoluteStart - 1;

                    Contractilities[0].StartTime = absoluteStart;
                    Contractilities[0].EndTime = absoluteStart;
                }

                var toAdd = (from c in detectedObjects
                             where c.EventType != ArtifactType.Contraction && c.Id > m_lastCalculatedEventID
                             orderby c.EndTime
                             select c).ToList();

                var toAddDistinct = toAdd.Except(m_detectedArtifacts).ToList();

                if (toAdd.Count() <= 0 || (toAdd.Count() > 0 && toAdd.Max(c => c.EndTime) <= Contractilities.Max(c => c.EndTime)))
                    return;

                m_detectedArtifacts.AddRange(toAddDistinct);

                UpdateEventsRates(toAdd);
                CalculateContractilityInternal(detectedObjects, toAdd);
            }
        }

        private void CalculateContractilityInternal(List<PluginDetectionArtifact> detectedObjects, List<PluginDetectionArtifact> eventsToAdd)
        {
            var baselines = (from c in m_detectedArtifacts
                             where c.EventType == ArtifactType.Baseline
                             select c as PluginBaseline).ToList();

            long artifactsEnd = m_detectedArtifacts.Max(c => c.EndTime);
            long curContractilityAbsoluteStart = m_lastCalculatedContractility + 1;
            double meanBaseline = 0f;
            int meanBaselineVariability = 0;
            long windowSize = DrawingWindowSize;
            long i = curContractilityAbsoluteStart;
            int lastEventID = eventsToAdd.Max(c => c.Id);

            while (detectedObjects.Count > 0 && i <= artifactsEnd)
            {
                long contractionRate = 0, contractions120SecRate = 0, accelRate = 0, lateDecelRate = 0, prolongedDecelRate = 0, longAndLargeDecelRate = 0;
                CalculateRates(windowSize, FHRSampleRate, i, m_detectedArtifacts, baselines, ref contractionRate, ref contractions120SecRate, ref accelRate, ref lateDecelRate, ref prolongedDecelRate,
                                ref longAndLargeDecelRate, ref meanBaselineVariability, ref meanBaseline);

                var classification = CalculateContractilityClassification(contractionRate, accelRate, meanBaselineVariability,
                                                                            lateDecelRate, prolongedDecelRate, longAndLargeDecelRate, contractions120SecRate);
                bool bQualified = true;

                if (classification == ContractilityClassification.Normal)
                {
                    bQualified = QualifyData(m_detectedArtifacts, i, m_compressedUPTracings);
                    if (!bQualified)
                        classification = ContractilityClassification.Unknown;
                }

                String toLog = String.Format("contractionRate = {0}, contractions120SecRate = {1}, accelRate = {2}, lateDecelRate = {3}, longAndLargeDecelRate = {4}, prolongedDecelRate = {5}, meanBaselineVariability = {6}, bQualified = {7}",
                                            contractionRate, contractions120SecRate, accelRate, lateDecelRate, longAndLargeDecelRate, prolongedDecelRate, meanBaselineVariability, bQualified);
                Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator::CalculateContractility", toLog);
                AppendContractility(baselines, m_detectedArtifacts, i, classification, curContractilityAbsoluteStart);

                // This one should always be the last in loop
                i = i != artifactsEnd && i + 15 > artifactsEnd ? artifactsEnd : i + 15;
            }

            m_lastCalculatedEventID = lastEventID;
            m_lastMeanBaselineVariability = meanBaselineVariability;

            FillEventsCounters(baselines);
            UpdateContractilityIDs();
            m_detectedArtifacts.RemoveAll(c => c.EndTime < m_lastCalculatedContractility - CONTRACTILITY_QUALIFY_WINDOW_SIZE);
        }

        public override void AppendContractionsRates(long windowSize, long contractionPeakTime, bool bStrikedOut)
        {
            int j1 = GetContractionsRateIndexAt(contractionPeakTime);
            int j2 = GetContractionsRateIndexAt(contractionPeakTime + windowSize);
            int pos1 = j1 + 1;
            int pos2 = j2 + 2;

            ContractionsRate.Insert(pos1, bStrikedOut ? ContractionsRate[j1] : ContractionsRate[j1] + 1);
            ContractionsRateIndex.Insert(pos1, contractionPeakTime);

            ContractionsRate.Insert(pos2, ContractionsRate[j2 + 1] == 0 ? 0 : ContractionsRate[j2 + 1] - 1);
            ContractionsRateIndex.Insert(pos2, contractionPeakTime + windowSize);

            if (!bStrikedOut)
                UpdateContractionsRateInRange(pos1, pos2);
        }

        public override void AppendContractions120SecRates(long windowSize, long contractionPeakTime, bool bStrikedOut)
        {
            int j1 = GetContractions120SecRateIndexAt(contractionPeakTime);
            int j2 = GetContractions120SecRateIndexAt(contractionPeakTime + windowSize);
            int pos1 = j1 + 1;
            int pos2 = j2 + 2;

            Contractions120SecRate.Insert(pos1, bStrikedOut ? Contractions120SecRate[j1] : Contractions120SecRate[j1] + 1);
            Contractions120SecRateIndex.Insert(pos1, contractionPeakTime);

            Contractions120SecRate.Insert(pos2, Contractions120SecRate[j2 + 1] == 0 ? 0 : Contractions120SecRate[j2 + 1] - 1);
            Contractions120SecRateIndex.Insert(pos2, contractionPeakTime + windowSize);

            if (!bStrikedOut)
                UpdateContractions120SecRateInRange(pos1, pos2);
        }

        protected void UpdateContractionsRateInRange(int start, int end, bool bInc = true)
        {
            for (int j = start + 1; j < end; ++j)
            {
                if (bInc)
                    ContractionsRate[j]++;
                else
                    ContractionsRate[j]--;
            }
        }

        protected void UpdateContractions120SecRateInRange(int start, int end, bool bInc = true)
        {
            for (int j = start + 1; j < end; ++j)
            {
                if (bInc)
                    Contractions120SecRate[j]++;
                else
                    Contractions120SecRate[j]--;
            }
        }

        protected long GetContractionsRateAt(long upi)
        {
            return ContractionsRate[GetContractionsRateIndexAt(upi)];
        }

        protected int GetContractionsRateIndexAt(long upi)
        {
            return FindEventRateIndex(upi, ref m_contractionsRateLast, ContractionsRateIndex);
        }

        protected long GetContractions120SecRateAt(long upi)
        {
            return Contractions120SecRate[GetContractions120SecRateIndexAt(upi)];
        }

        protected int GetContractions120SecRateIndexAt(long upi)
        {
            return FindEventRateIndex(upi, ref m_contractions120SecRateLast, Contractions120SecRateIndex);
        }

        protected void UpdateEventsRateInRange(EventClassification eventType, int pos1, int pos2, bool bInc = true)
        {
            for (int j = pos1 + 1; j < pos2; ++j)
            {
                if (eventType == EventClassification.Acceleration)
                {
                    if (bInc)
                        EventsRate[j].AccelerationRate++;
                    else
                        EventsRate[j].AccelerationRate--;
                }
                else if (eventType == EventClassification.LateDeceleration)
                {
                    if (bInc)
                        EventsRate[j].LateDecelRate++;
                    else
                        EventsRate[j].LateDecelRate--;
                }
                else if (eventType == EventClassification.ProlongedDeceleration)
                {
                    if (bInc)
                        EventsRate[j].ProlongedDecelRate++;
                    else
                        EventsRate[j].ProlongedDecelRate--;
                }
                else if (eventType == EventClassification.LongAndLargeDeceleration)
                {
                    if (bInc)
                        EventsRate[j].LongAndLargeDecelRate++;
                    else
                        EventsRate[j].LongAndLargeDecelRate--;
                }
            }
        }

        private bool QualifyData(List<PluginDetectionArtifact> detectedArtifacts, long i, List<UPTracingsBlockIndicator> compressedUPTracings)
        {
            bool dataQualify = QualifyFHR(detectedArtifacts, i, CONTRACTILITY_QUALIFY_WINDOW_SIZE);
            bool UPQualify = QualifyUP(compressedUPTracings, i, CONTRACTILITY_QUALIFY_WINDOW_SIZE);
            Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator", "dataQualify = " + dataQualify.ToString() + "UPQualify = " + UPQualify.ToString());
            return UPQualify && dataQualify;
        }

        private bool QualifyFHR(List<PluginDetectionArtifact> detectedArtifacts, long i, int windowSizeInSec)
        {
            long windowStart = i - windowSizeInSec;

            var eventsInWindow = (from c in detectedArtifacts
                                  where c.StartTime < i && c.EndTime > windowStart
                                  select c).ToList();

            double qualifyDatalength = 0;
            foreach (var item in eventsInWindow)
            {
                if (item.StartTime < windowStart)
                    qualifyDatalength += item.EndTime - windowStart;
                else if (item.EndTime > i)
                    qualifyDatalength += i - item.StartTime;
                else
                    qualifyDatalength += item.EndTime - item.StartTime;
            }

            qualifyDatalength += eventsInWindow.Count;
            Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator::QualifyFHR", "qualifyDatalength = " + qualifyDatalength.ToString());
            bool dataQualify = qualifyDatalength / windowSizeInSec * 100 > MINIMAL_DATA_PERCENTAGE_TO_QUALIFY;
            Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator::QualifyFHR", "qualifyDataPercentage = " + (qualifyDatalength / windowSizeInSec * 100).ToString());
            return dataQualify;
        }

        private bool QualifyUP(List<UPTracingsBlockIndicator> compressedUPTracings, long i, int windowSizeInSec)
        {
            var qualiList = (from c in compressedUPTracings
                             where c.ValidDataStart <= i && c.ValidDataEnd >= i - CONTRACTILITY_QUALIFY_WINDOW_SIZE
                             orderby c.ValidDataStart
                             select c).ToList();

            if (qualiList == null || qualiList.Count <= 0)
                return false;

            long gaps = 0;
            if (qualiList.Count == 1)
            {
                if (i - CONTRACTILITY_QUALIFY_WINDOW_SIZE >= qualiList[0].ValidDataStart && i <= qualiList[0].ValidDataEnd) // whole window inside same up block
                {
                    Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator", "i = " + i.ToString() + "\r\ni - CONTRACTILITY_QUALIFY_WINDOW_SIZE >= qualiList[0].ValidDataStart && i <= qualiList[0].ValidDataEnd");
                    return true;
                }
                else if (i <= qualiList[0].ValidDataEnd) // up pen up before up block
                {
                    Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator", "i = " + i.ToString() + "\r\ni <= qualiList[0].ValidDataEnd");
                    gaps = qualiList[0].ValidDataStart - (i - CONTRACTILITY_QUALIFY_WINDOW_SIZE) - 1;
                }
                else if (i - CONTRACTILITY_QUALIFY_WINDOW_SIZE >= qualiList[0].ValidDataStart) // up pen up after up block
                {
                    Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator", "i = " + i.ToString() + "\r\ni - CONTRACTILITY_QUALIFY_WINDOW_SIZE >= qualiList[0].ValidDataStart");
                    gaps = i - qualiList[0].ValidDataEnd;
                }
                else // pen ups surrounding up block
                {
                    gaps = (qualiList[0].ValidDataStart - (i - CONTRACTILITY_QUALIFY_WINDOW_SIZE) - 1) + (i - qualiList[0].ValidDataEnd - 1);
                    Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator", "i = " + i.ToString() + "\r\npen ups surrounding up block");
                }
            }

            if (qualiList.Count > 1) // current window spans over several up blocks
            {
                String qualiListStr = "elem 0: start = " + qualiList[0].ValidDataStart.ToString() + ", end = " + qualiList[0].ValidDataEnd.ToString();
                if (qualiList[0].ValidDataStart >= i - CONTRACTILITY_QUALIFY_WINDOW_SIZE)
                    gaps += (qualiList[0].ValidDataStart - (i - CONTRACTILITY_QUALIFY_WINDOW_SIZE)) - 1;

                for (int ind = 1; ind < qualiList.Count; ind++)
                {
                    gaps += (qualiList[ind].ValidDataStart - qualiList[ind - 1].ValidDataEnd) - 1;
                    qualiListStr += "\r\nelem " + ind.ToString() + ": start = " + qualiList[ind].ValidDataStart.ToString() + ", end = " + qualiList[ind].ValidDataEnd.ToString();
                }

                if (qualiList.Last().ValidDataEnd < i)
                    gaps += i - qualiList.Last().ValidDataEnd;

                Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator", "qualiList is: \r\n" + qualiListStr + "\r\ni = " + i.ToString() + ", gaps = " + gaps.ToString());
            }

            Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator::QualifyUP", "gaps = " + gaps.ToString() + ", CONTRACTILITY_QUALIFY_WINDOW_SIZE - gaps = " + (CONTRACTILITY_QUALIFY_WINDOW_SIZE - gaps).ToString());
            bool UPQualify = ((double)(CONTRACTILITY_QUALIFY_WINDOW_SIZE - gaps)) / CONTRACTILITY_QUALIFY_WINDOW_SIZE * 100 > MINIMAL_DATA_PERCENTAGE_TO_QUALIFY;
            return UPQualify;
        }

        private void UpdateContractilityIDs()
        {
            var toSetIDs = from c in Contractilities
                           where c.Id == -1
                           orderby c.EndTime
                           select c;

            if (toSetIDs == null)
                return;

            foreach (var item in toSetIDs)
            {
                item.Id = ++ContractilityId;
            }
        }

        private void FillEventsCounters(IEnumerable<PluginBaseline> baselines)
        {
            var toSetIDs = from c in Contractilities
                           where c.Id == -1
                           orderby c.EndTime
                           select c;

            if (toSetIDs == null)
                return;

            foreach (var item in toSetIDs)
            {
                long curRate = GetContractionsRateAt(item.EndTime);
                item.EventsCounted.Contractions.Value = curRate;

                curRate = GetContractions120SecRateAt(item.EndTime);
                item.EventsCounted.LongContractions.Value = curRate;

                curRate = GetAccelerationRateAt(item.EndTime);
                item.EventsCounted.Accels.Value = curRate;

                double meanBaseline = 0f; 
                int meanBaselineVariability = 0;
                GetMeanBaseline(baselines, item.EndTime - DrawingWindowSize * 60, item.EndTime, ref meanBaseline, ref meanBaselineVariability);
                item.EventsCounted.Variability.Value = meanBaselineVariability;

                curRate = GetLateDecelRateAt(item.EndTime);
                item.EventsCounted.LateDecels.Value = curRate;

                curRate = GetProlongedDecelRateAt(item.EndTime);
                item.EventsCounted.ProlongedDecels.Value = curRate;

                curRate = GetLongAndLargeDecelRateAt(item.EndTime);
                item.EventsCounted.LargeDeceles.Value = curRate;

                if (item.CRIClassification == ContractilityClassification.Alert || item.CRIClassification == ContractilityClassification.Danger)
                {
                    item.EventsCounted.LongContractions.IsReason = (item.EventsCounted.LongContractions.Value >= LONG_CONTRACTIONS_RATE_AMOUNT);
                    item.EventsCounted.Contractions.IsReason = (item.EventsCounted.Contractions.Value >= CONTRACTIONS_RATE_AMOUNT);
                    item.EventsCounted.Variability.IsReason = item.EventsCounted.Accels.IsReason = item.EventsCounted.Accels.Value <= ACCEL_RATE_AMOUNT &&
                                                                                                   meanBaselineVariability < MINIMAL_BASELINE_VARIABILITY;

                    item.EventsCounted.LateDecels.IsReason = item.EventsCounted.LateDecels.Value >= LATE_DECEL_RATE_AMOUNT ||
                                                             (item.EventsCounted.LateDecels.Value > 0 &&
                                                                    (item.EventsCounted.LateDecels.Value + item.EventsCounted.LargeDeceles.Value >= LATE_DECEL_AND_LARGE_AND_LONG_RATE_AMOUNT ||
                                                                     item.EventsCounted.LargeDeceles.Value + item.EventsCounted.ProlongedDecels.Value >= LATE_DECEL_AND_PROLONGED_RATE_AMOUNT));

                    item.EventsCounted.ProlongedDecels.IsReason = item.EventsCounted.LateDecels.Value > 0 && item.EventsCounted.ProlongedDecels.Value > 0;

                    item.EventsCounted.LargeDeceles.IsReason = (item.EventsCounted.LateDecels.Value > 0 && item.EventsCounted.LargeDeceles.Value > 0) ||
                                                               (item.EventsCounted.LargeDeceles.Value >= LARGE_AND_LONG_DECEL_RATE_AMOUNT);
                }

                String toLog = String.Format("Contractility: Start = {0}, End = {1}, State = {2}, Reason = {3}",
                                                item.StartTime.ToString(), item.EndTime.ToString(), item.CRIClassification.ToString(),
                                                (item.EventsCounted.LongContractions.IsReason ? "Long CTR " : " ") +
                                                (item.EventsCounted.Contractions.IsReason ? "Contractions " : " ") +
                                                ((item.EventsCounted.Variability.IsReason && item.EventsCounted.Accels.IsReason) ? "Variability/Accels " : " ") +
                                                (item.EventsCounted.LateDecels.IsReason ? "Late decels " : " ") +
                                                (item.EventsCounted.ProlongedDecels.IsReason ? "Prolonged decels " : " ") +
                                                (item.EventsCounted.LargeDeceles.IsReason ? "Large decels " : " "));

                Logger.WriteLogEntry(TraceEventType.Verbose, "CRICalulator::FillEventsCounters", toLog.Trim());
            }
        }

        private void CalculateRates(long windowSize, int FHRSampleRate, long ind, List<PluginDetectionArtifact> detectedArtifacts, List<PluginBaseline> baselines,
                                    ref long contractionRate, ref long contractions120SecRate,
                                    ref long accelRate, ref long lateDecelRate,
                                    ref long prolongedDecelRate, ref long longAndLargeDecelRate,
                                    ref int meanBaselineVariability, ref double meanBaseline)
        {
            CalculateRatesInternal(ind, ref contractionRate, ref contractions120SecRate, ref accelRate, ref lateDecelRate, ref prolongedDecelRate, ref longAndLargeDecelRate);
            long iright = ind * FHRSampleRate;
            long ileft = iright - windowSize < 0 ? 0 : iright - windowSize;
            if (!QualifyFHR(detectedArtifacts, ind, CONTRACTILITY_QUALIFY_WINDOW_SIZE))
                meanBaselineVariability = -1;
            else
                GetMeanBaseline(baselines, ileft, iright, ref meanBaseline, ref meanBaselineVariability);
        }

        private void CalculateRatesInternal(long ind, ref long contractionRate, ref long contractions120SecRate, ref long accelRate,
                                    ref long lateDecelRate, ref long prolongedDecelRate, ref long longAndLargeDecelRate)
        {
            contractionRate = GetContractionsRateAt(ind);
            contractions120SecRate = GetContractions120SecRateAt(ind);
            accelRate = GetAccelerationRateAt(ind);
            lateDecelRate = GetLateDecelRateAt(ind);
            prolongedDecelRate = GetProlongedDecelRateAt(ind);
            longAndLargeDecelRate = GetLongAndLargeDecelRateAt(ind);
        }

        private ContractilityClassification CalculateContractilityClassification(long contractionRate, long accelRate, double meanBaselineVariability, long lateDecelRate,
                                                                         long prolongedDecelRate, long longAndLargeDecelRate, long contractions120SecRate)
        {
            if (contractionRate < CONTRACTIONS_RATE_AMOUNT)
            {
                if (IsMatchingCriteria(accelRate, meanBaselineVariability, lateDecelRate, prolongedDecelRate, longAndLargeDecelRate) && contractions120SecRate < LONG_CONTRACTIONS_RATE_AMOUNT)
                    return ContractilityClassification.Normal;
                else
                    return ContractilityClassification.Alert;
            }
            else
            {
                if (IsMatchingCriteria(accelRate, meanBaselineVariability, lateDecelRate, prolongedDecelRate, longAndLargeDecelRate))
                    return ContractilityClassification.Alert;
                else
                    return ContractilityClassification.Danger;
            }

        }

        private bool IsMatchingCriteria(long accelRate, double meanBaselineVariability, long lateDecelRate, long prolongedDecelRate, long longAndLargeDecelRate)
        {
            return
                    (accelRate > ACCEL_RATE_AMOUNT || (meanBaselineVariability < 0 || meanBaselineVariability >= MINIMAL_BASELINE_VARIABILITY)) &&
                    (lateDecelRate <= 0 || lateDecelRate + prolongedDecelRate < LATE_DECEL_AND_PROLONGED_RATE_AMOUNT && lateDecelRate + longAndLargeDecelRate < LATE_DECEL_AND_LARGE_AND_LONG_RATE_AMOUNT) &&
                    lateDecelRate < LATE_DECEL_RATE_AMOUNT &&
                    longAndLargeDecelRate < LARGE_AND_LONG_DECEL_RATE_AMOUNT;
        }

        protected override void AppendDeceleration(PluginDeceleration decelItem, long fhrWindow)
        {
            if (decelItem.DecelerationCategory == DecelerationCategories.Late && decelItem.Confidence > MINIMAL_LATE_DECEL_CONFIDENCE_LEVEL)
            {
                AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.LateDeceleration, decelItem.IsStrikedOut);
            }
            else if (decelItem.DecelerationCategory == DecelerationCategories.Prolonged)
            {
                if (decelItem.LateTiming && decelItem.Confidence > MINIMAL_LATE_DECEL_CONFIDENCE_LEVEL)
                {
                    AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.LateDeceleration, decelItem.IsStrikedOut);
                }
                else if (decelItem.Height > MINIMAL_PROLONGED_DECEL_HEIGHT)
                {
                    AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.ProlongedDeceleration, decelItem.IsStrikedOut);
                }
            }
            else if (decelItem.LongAndLarge)
            {
                AppendEventRate(fhrWindow, decelItem.PeakTime, EventClassification.LongAndLargeDeceleration, decelItem.IsStrikedOut);
            }
        }

        protected override void AppendEventRate(long windowSize, long eventPeakTime, EventClassification eventType, bool bStrikedOut)
        {
            int j1 = 0;
            int j2 = 0;
            CRIEventsRate toAddIn = new CRIEventsRate();
            CRIEventsRate toAddOut = new CRIEventsRate();

            switch (eventType)
            {
                case EventClassification.Acceleration:
                    j1 = FindEventRateIndex(eventPeakTime, ref m_accelerationRateLast, EventsRateIndex);
                    j2 = FindEventRateIndex(eventPeakTime + windowSize, ref m_accelerationRateLast, EventsRateIndex);
                    toAddIn.AccelerationRate = bStrikedOut ? EventsRate[j1].AccelerationRate : EventsRate[j1].AccelerationRate + 1;
                    toAddIn.LateDecelRate = EventsRate[j1].LateDecelRate;
                    toAddIn.ProlongedDecelRate = EventsRate[j1].ProlongedDecelRate;
                    toAddIn.LongAndLargeDecelRate = EventsRate[j1].LongAndLargeDecelRate;
                    EventsRate.Insert(j1 + 1, toAddIn);
                    toAddOut.AccelerationRate = EventsRate[j2 + 1].AccelerationRate == 0 ? 0 : EventsRate[j2 + 1].AccelerationRate - 1;
                    toAddOut.LateDecelRate = EventsRate[j2 + 1].LateDecelRate;
                    toAddOut.ProlongedDecelRate = EventsRate[j2 + 1].ProlongedDecelRate;
                    toAddOut.LongAndLargeDecelRate = EventsRate[j2 + 1].LongAndLargeDecelRate;
                    EventsRate.Insert(j2 + 2, toAddOut);
                    break;
                case EventClassification.LateDeceleration:
                    j1 = FindEventRateIndex(eventPeakTime, ref m_lateDecelRateLast, EventsRateIndex);
                    j2 = FindEventRateIndex(eventPeakTime + windowSize, ref m_lateDecelRateLast, EventsRateIndex);
                    toAddIn.AccelerationRate = EventsRate[j1].AccelerationRate;
                    toAddIn.LateDecelRate = bStrikedOut ? EventsRate[j1].LateDecelRate : EventsRate[j1].LateDecelRate + 1;
                    toAddIn.ProlongedDecelRate = EventsRate[j1].ProlongedDecelRate;
                    toAddIn.LongAndLargeDecelRate = EventsRate[j1].LongAndLargeDecelRate;
                    EventsRate.Insert(j1 + 1, toAddIn);
                    toAddOut.AccelerationRate = EventsRate[j2 + 1].AccelerationRate;
                    toAddOut.LateDecelRate = EventsRate[j2 + 1].LateDecelRate == 0 ? 0 : EventsRate[j2 + 1].LateDecelRate - 1;
                    toAddOut.ProlongedDecelRate = EventsRate[j2 + 1].ProlongedDecelRate;
                    toAddOut.LongAndLargeDecelRate = EventsRate[j2 + 1].LongAndLargeDecelRate;
                    EventsRate.Insert(j2 + 2, toAddOut);
                    break;
                case EventClassification.ProlongedDeceleration:
                    j1 = FindEventRateIndex(eventPeakTime, ref m_prolongedDecelRateLast, EventsRateIndex);
                    j2 = FindEventRateIndex(eventPeakTime + windowSize, ref m_prolongedDecelRateLast, EventsRateIndex);
                    toAddIn.AccelerationRate = EventsRate[j1].AccelerationRate;
                    toAddIn.LateDecelRate = EventsRate[j1].LateDecelRate;
                    toAddIn.ProlongedDecelRate = bStrikedOut ? EventsRate[j1].ProlongedDecelRate : EventsRate[j1].ProlongedDecelRate + 1;
                    toAddIn.LongAndLargeDecelRate = EventsRate[j1].LongAndLargeDecelRate;
                    EventsRate.Insert(j1 + 1, toAddIn);
                    toAddOut.AccelerationRate = EventsRate[j2 + 1].AccelerationRate;
                    toAddOut.LateDecelRate = EventsRate[j2 + 1].LateDecelRate;
                    toAddOut.ProlongedDecelRate = EventsRate[j2 + 1].ProlongedDecelRate == 0 ? 0 : EventsRate[j2 + 1].ProlongedDecelRate - 1;
                    toAddOut.LongAndLargeDecelRate = EventsRate[j2 + 1].LongAndLargeDecelRate;
                    EventsRate.Insert(j2 + 2, toAddOut);
                    break;
                case EventClassification.LongAndLargeDeceleration:
                    j1 = FindEventRateIndex(eventPeakTime, ref m_longAndLargeDecelsRateLast, EventsRateIndex);
                    j2 = FindEventRateIndex(eventPeakTime + windowSize, ref m_longAndLargeDecelsRateLast, EventsRateIndex);
                    toAddIn.AccelerationRate = EventsRate[j1].AccelerationRate;
                    toAddIn.LateDecelRate = EventsRate[j1].LateDecelRate;
                    toAddIn.ProlongedDecelRate = EventsRate[j1].ProlongedDecelRate;
                    toAddIn.LongAndLargeDecelRate = bStrikedOut ? EventsRate[j1].LongAndLargeDecelRate : EventsRate[j1].LongAndLargeDecelRate + 1;
                    EventsRate.Insert(j1 + 1, toAddIn);
                    toAddOut.AccelerationRate = EventsRate[j2 + 1].AccelerationRate;
                    toAddOut.LateDecelRate = EventsRate[j2 + 1].LateDecelRate;
                    toAddOut.ProlongedDecelRate = EventsRate[j2 + 1].ProlongedDecelRate;
                    toAddOut.LongAndLargeDecelRate = EventsRate[j2 + 1].LongAndLargeDecelRate == 0 ? 0 : EventsRate[j2 + 1].LongAndLargeDecelRate - 1;
                    EventsRate.Insert(j2 + 2, toAddOut);
                    break;
                default:
                    return;
            };

            int pos1 = j1 + 1;
            int pos2 = j2 + 2;

            EventsRateIndex.Insert(pos1, eventPeakTime);
            EventsRateIndex.Insert(pos2, eventPeakTime + windowSize);

            if (!bStrikedOut)
                UpdateEventsRateInRange(eventType, pos1, pos2);
        }

        protected override void ResetEventsRatesInternal()
        {
            m_longAndLargeDecelsRateLast = 0;

            CRIEventsRate toInsert = new CRIEventsRate();
            EventsRate.Clear();
            EventsRate.Add(toInsert);
        }

        void ResetContractilities(long time = 0)
        {
            Contractilities.Clear();
            Contractilities.Add(new Contractility(time, time, ContractilityClassification.Unknown));
            m_lastCalculatedContractility = 0;
        }

        protected override long GetAccelerationRateAt(long fhrIndex)
        {
            return EventsRate[FindEventRateIndex(fhrIndex, ref m_accelerationRateLast, EventsRateIndex)].AccelerationRate;
        }

        protected override long GetLateDecelRateAt(long fhrIndex)
        {
            return EventsRate[FindEventRateIndex(fhrIndex, ref m_lateDecelRateLast, EventsRateIndex)].LateDecelRate;
        }

        protected override long GetProlongedDecelRateAt(long fhrIndex)
        {
            return EventsRate[FindEventRateIndex(fhrIndex, ref m_prolongedDecelRateLast, EventsRateIndex)].ProlongedDecelRate;
        }

        protected long GetLongAndLargeDecelRateAt(long fhrIndex)
        {
            return EventsRate[FindEventRateIndex(fhrIndex, ref m_longAndLargeDecelsRateLast, EventsRateIndex)].LongAndLargeDecelRate;
        }

        #region Contractility related methods

        long GetLastContractilityEnd()
        {
            return Contractilities[Contractilities.Count - 1].EndTime;
        }

        public int FindContractilityIndex(long i)
        {
            int numOfContractilities = Contractilities.Count;
            while ((numOfContractilities > 0) && (Contractilities[numOfContractilities - 1].EndTime > i))
            {
                numOfContractilities--;
            }

            return numOfContractilities;
        }

        public Contractility GetContractility(int i)
        {
            int numOfContractilities = Contractilities.Count;
            if (i >= numOfContractilities)
                i = numOfContractilities - 1;

            return i < 0 ? new Contractility() : Contractilities[i];
        }

        public Contractility GetContractilityByIndex(int index)
        {
            return GetContractility(FindContractilityIndex(index));
        }

        public Contractility GetContractilityStarting(long start)
        {
            long numOfContractilities = Contractilities.Count;
            for (int i = 0; i < numOfContractilities; ++i)
            {
                if (Contractilities[i].StartTime == start)
                    return Contractilities[i];
            }

            return null;
        }

        void AppendContractility(List<PluginBaseline> baselines, List<PluginDetectionArtifact> detectedObjects, long end, ContractilityClassification classification, long absoluteStart)
        {
            if (Contractilities[Contractilities.Count - 1].EndTime == Contractilities[Contractilities.Count - 1].StartTime)
            {
                Contractilities[Contractilities.Count - 1].EndTime = end;
                Contractilities[Contractilities.Count - 1].StartTime = absoluteStart;
                Contractilities[Contractilities.Count - 1].CRIClassification = classification;
                m_lastCalculatedContractility = end;
            }
            else if (Contractilities[Contractilities.Count - 1].CRIClassification == classification)
            {
                if (Contractilities[Contractilities.Count - 1].StartTime < absoluteStart)
                {
                    Contractility curContractility = new Contractility(Math.Max(Contractilities[Contractilities.Count - 1].EndTime + 1, absoluteStart), end, classification);
                    AppendContractility(curContractility);
                }

                Contractilities[Contractilities.Count - 1].EndTime = end;
                m_lastCalculatedContractility = end;
            }
            else
            {
                FindContractilityEdge(baselines, detectedObjects, end);
                Contractility curContractility = new Contractility(Contractilities[Contractilities.Count - 1].EndTime + 1, end, classification);
                AppendContractility(curContractility);
            }
        }

        void AppendContractility(Contractility contractility)
        {
            int curContractilityIndex = FindContractilityIndex(contractility.StartTime);
            if (Contractilities.Count == 0 || !GetContractility(curContractilityIndex).Intersects(contractility))
            {
                Contractilities.Insert(curContractilityIndex, contractility);
                m_lastCalculatedContractility = contractility.EndTime;
            }
        }

        void AppendContractilities(List<Contractility> newContractilities)
        {
            if (newContractilities.Count == 0)
                return;

            if (Contractilities.Count == 0)
            {
                // Special case for first injection
                Contractilities.AddRange(newContractilities);
            }
            else
            {
                foreach (var item in newContractilities)
                {
                    AppendContractility(item);
                }
            }
        }

        void MergeContractilities(List<Contractility> newContractilities)
        {
            if (newContractilities.Count == 0)
                return;

            if (Contractilities.Count == 0)
            {
                // Special case for first injection
                Contractilities.AddRange(newContractilities);
            }
            else if (Contractilities[Contractilities.Count - 1].EndTime < newContractilities[0].StartTime) //no intersections with old just append new contractilities
            {
                AppendContractilities(newContractilities);
            }
            else if (newContractilities[0].StartTime <= Contractilities[0].StartTime)
            {
                Contractilities.Clear();
                AppendContractilities(newContractilities);
            }
            else
            {
                int firstIntersectionIndex = FindContractilityIndex(newContractilities[0].StartTime);
                if (firstIntersectionIndex == Contractilities.Count) // no intersections with old just append new contractilities
                {
                    AppendContractilities(newContractilities);
                }
                else // remove contractilities until intersection and append new contractilities
                {
                    int numToErase = Contractilities.Count - firstIntersectionIndex + 1;
                    Contractilities.RemoveRange(firstIntersectionIndex - 1, numToErase);

                    if (Contractilities[Contractilities.Count - 1].Intersects(newContractilities[0])) // new start contractility intersects with old end contractility
                    {
                        AppendContractilitiesIntersectedWithEnd(newContractilities);
                    }
                    else
                    {
                        AppendContractilities(newContractilities);
                    }

                }
            }
            m_lastCalculatedContractility = Contractilities[Contractilities.Count - 1].EndTime;
        }

        void AppendContractilitiesIntersectedWithEnd(List<Contractility> newContractilities)
        {
            int beginIndex = 0;
            Contractility oldEndContractility = Contractilities[Contractilities.Count - 1];
            Contractility newBeginContractility = newContractilities[0];

            if (oldEndContractility.Intersects(newBeginContractility)) // new start contractility intersects with old end contractility
            {
                if (oldEndContractility.CRIClassification == newBeginContractility.CRIClassification) // the same clasification: just update the end
                {
                    if (oldEndContractility.StartTime <= newBeginContractility.StartTime && oldEndContractility.EndTime != newBeginContractility.EndTime)
                    {
                        oldEndContractility.EndTime = newBeginContractility.EndTime;
                        Contractilities[Contractilities.Count - 1].EndTime = newBeginContractility.EndTime;

                        beginIndex++;
                    }
                }
                else // different classification: break old contractility
                {
                    if (oldEndContractility.StartTime < newBeginContractility.StartTime - 1)
                    {
                        Contractilities[Contractilities.Count - 1].EndTime = newBeginContractility.StartTime - 1;
                    }

                }
                if (beginIndex != newContractilities.Count - 1)
                {
                    Contractilities.AddRange(newContractilities.GetRange(beginIndex, newContractilities.Count - beginIndex));
                }
            }
        }

        public ContractilityClassification GetContractilityClassification(int index)
        {
            int curIndex = FindContractilityIndex(index);
            Contractility curContractility = GetContractility(curIndex);
            ContractilityClassification toRet = curContractility.CRIClassification;
            return toRet;
        }

        void FindContractilityEdge(List<PluginBaseline> baselines, List<PluginDetectionArtifact> detectedObjects, long index)
        {
            long end = Contractilities[Contractilities.Count - 1].EndTime;
            long middle = (end + index) / 2;
            if (middle <= end + 1)
                return;

            long windowSize = DrawingWindowSize * 60 * FHRSampleRate;
            long contractionRate = 0, contractions120SecRate = 0, accelRate = 0, lateDecelRate = 0, prolongedDecelRate = 0, longAndLargeDecelRate = 0;
            int meanBaselineVariability = 0;
            double meanBaseline = 0f;
            CalculateRates(windowSize, FHRSampleRate, middle, detectedObjects, baselines, ref contractionRate, ref contractions120SecRate, ref accelRate, ref lateDecelRate, ref prolongedDecelRate,
                            ref longAndLargeDecelRate, ref meanBaselineVariability, ref meanBaseline);

            ContractilityClassification classification = CalculateContractilityClassification(contractionRate, accelRate, meanBaselineVariability,
                                                                                                    lateDecelRate, prolongedDecelRate, longAndLargeDecelRate, contractions120SecRate);

            if (classification == Contractilities[Contractilities.Count - 1].CRIClassification)
            {
                Contractilities[Contractilities.Count - 1].EndTime = middle;
                FindContractilityEdge(baselines, detectedObjects, index);
            }
            else
                FindContractilityEdge(baselines, detectedObjects, middle);
        }

        private void FillContractilityClassification(List<PluginDetectionArtifact> detectedObjects, long curContractilityAbsoluteStartTime, long rightBound,
                                                   ref int meanBaselineVariability, List<PluginBaseline> baselines,
                                                   List<UPTracingsBlockIndicator> compressedUPTracings)
        {
            double meanBaseline = 0;
            long windowSize = DrawingWindowSize * 60 * FHRSampleRate;
            long i = curContractilityAbsoluteStartTime;
            while (detectedObjects.Count > 0 && i <= rightBound)
            {
                long contractionRate = 0, contractions120SecRate = 0, accelRate = 0, lateDecelRate = 0, prolongedDecelRate = 0, longAndLargeDecelRate = 0;
                CalculateRates(windowSize, FHRSampleRate, i, detectedObjects, baselines, ref contractionRate, ref contractions120SecRate, ref accelRate, ref lateDecelRate,
                                ref prolongedDecelRate, ref longAndLargeDecelRate, ref meanBaselineVariability, ref meanBaseline);

                var classification = CalculateContractilityClassification(contractionRate, accelRate, meanBaselineVariability,
                                                                            lateDecelRate, prolongedDecelRate, longAndLargeDecelRate, contractions120SecRate);
                bool bQualified = true;
                if (classification == ContractilityClassification.Normal)
                {
                    bQualified = QualifyData(detectedObjects, i, compressedUPTracings);
                    if (!bQualified)
                        classification = ContractilityClassification.Unknown;
                }

                AppendContractility(baselines, detectedObjects, i, classification, curContractilityAbsoluteStartTime);

                // This one should always be the last in loop
                i = i != rightBound && i + 15 > rightBound ? rightBound : i + 15;
            }
        }

        public void RecalculateContractilities(PluginDetectionArtifact artifactObj, List<PluginDetectionArtifact> surroundingObjects,
                                                List<UPTracingsBlockIndicator> compressedUPTracings, int actionId, ActionTypes actionType, out int lastUnchangedContractilityId)
        {
            lastUnchangedContractilityId = Contractilities.Count() > 0 ? Math.Max(0, Contractilities.Last().Id) : 0;
            if (artifactObj.EventType == ArtifactType.Baseline)
                return;

            if (surroundingObjects == null || surroundingObjects.Count <= 0)
                return;

            long peakTime = (artifactObj as PluginDeleteableDetectionArtifact).PeakTime;
            var artifactType = artifactObj.EventType;

            switch (actionType)
            {
                case ActionTypes.StrikeoutContraction:
                    StrikeoutContraction(artifactObj, peakTime, true);
                    break;
                case ActionTypes.UndoStrikeoutContraction:
                    StrikeoutContraction(artifactObj, peakTime, false);
                    break;
                case ActionTypes.StrikeoutEvent:
                    StrikeoutEvent(artifactObj, peakTime, true);
                    break;
                case ActionTypes.UndoStrikeoutEvent:
                    StrikeoutEvent(artifactObj, peakTime, false);
                    break;
                default:
                    return;
            }

            long rightBound = peakTime + CONTRACTILITY_QUALIFY_WINDOW_SIZE;
            long maxEndTimeSurrounding = surroundingObjects.Max(c => c.EndTime);
            if (maxEndTimeSurrounding < rightBound)
                rightBound = maxEndTimeSurrounding;

            int meanBaselineVariability = 0;
            var beyondWindowContractilities = from c in Contractilities
                                              where c.EndTime > rightBound
                                              orderby c.StartTime
                                              select c;


            List<Contractility> beyondRightBound = null;
            if (beyondWindowContractilities != null && beyondWindowContractilities.Count() > 0)
            {
                beyondRightBound = new List<Contractility>(beyondWindowContractilities);
                beyondRightBound.ForEach(c => c.Id = -1);
            }
            else
                beyondRightBound = new List<Contractility>();

            if (beyondRightBound.Count > 0 && beyondRightBound[0].StartTime <= rightBound)
                beyondRightBound[0].StartTime = rightBound + 1;

            if (Contractilities.Count > 0)
                Contractilities.RemoveAll(c => c.StartTime >= peakTime);

            int nextContractilityId = -1;
            if (Contractilities.Count > 0)
            {
                if (Contractilities.Last().EndTime >= peakTime)
                    Contractilities.Last().EndTime = peakTime - 1;

                // In case last Contractility is short (less than 3 seconds) - remove it and extend prvious contractility until peak time - 1
                if (Contractilities.Last().Length < 3)
                {
                    Contractilities.RemoveAt(Contractilities.Count - 1);
                    Contractilities.Last().EndTime = peakTime - 1;
                }

                nextContractilityId = Contractilities.Last().Id;
            }

            if (nextContractilityId > 0)
                ActionId2ContractilityId[actionId] = nextContractilityId;

            var baselines = (from c in surroundingObjects
                             where c.EventType == ArtifactType.Baseline
                             select c as PluginBaseline).ToList();

            var surroundingEvents = (from c in surroundingObjects
                                     where c.EventType != ArtifactType.Contraction
                                     orderby c.Id
                                     select c).ToList();

            FillContractilityClassification(surroundingEvents, peakTime, rightBound, ref meanBaselineVariability, baselines, compressedUPTracings);
            AppendContractilities(beyondRightBound);
            FillEventsCounters(baselines);
            ContractilityId = Contractilities.Max(c => c.Id);
            lastUnchangedContractilityId = Math.Max(0, ContractilityId);
            UpdateContractilityIDs();
        }

        #endregion

        public void AppendUPTracings(bool bIncremental, long start, List<byte> ups)
        {
            if (ups == null || ups.Count <= 0)
                return;

            if (!bIncremental)
            {
                if (m_compressedUPTracings.Count > 0 && Contractilities.Count > 0 && Contractilities.Max(c => c.Id > 0))
                    return;

                m_compressedUPTracings.Clear();
                m_compressedUPTracings.Add(new UPTracingsBlockIndicator() { ValidDataStart = start, ValidDataEnd = start });
            }

            if (bIncremental)
            {
                if (m_compressedUPTracings.Count > 0)
                {
                    long lastEndTime = m_compressedUPTracings.Max(c => c.ValidDataEnd);

                    // RemoveOverlap
                    while (ups.Count > 0 && lastEndTime + 1 > start)
                    {
                        //Logger.WriteLogEntry(TraceEventType.Warning, "CRICalculator", "AppendUPTracings: remove overlap, UP max time = " + lastEndTime.ToString() + ", start = " + start.ToString());
                        ups.RemoveAt(0);
                        ++start;
                    }
                }
            }

            if (ups.Count <= 0)
                return;

            CompressUPTracings(start, ups, m_compressedUPTracings);
        }

        private void StrikeoutContraction(PluginDetectionArtifact artifactObj, long peakTime, bool bStrikeout)
        {
            int updateStart = GetContractionsRateIndexAt(peakTime) - 1;
            int updateEnd = GetContractionsRateIndexAt(peakTime + CONTRACTILITY_QUALIFY_WINDOW_SIZE);
            UpdateContractionsRateInRange(updateStart, updateEnd, !bStrikeout);
            if (artifactObj.EndTime - artifactObj.StartTime > LongContractionMinLength)
            {
                updateStart = GetContractions120SecRateIndexAt(peakTime) - 1;
                updateEnd = GetContractions120SecRateIndexAt(peakTime + CONTRACTILITY_QUALIFY_WINDOW_SIZE);
                UpdateContractions120SecRateInRange(updateStart, updateEnd, !bStrikeout);
            }
        }

        private void StrikeoutEvent(PluginDetectionArtifact artifactObj, long peakTime, bool bStrikeout)
        {
            if (artifactObj.EventType != ArtifactType.Acceleration && artifactObj.EventType != ArtifactType.Deceleration)
                return;

            int updateStart = 0, updateEnd = 0, rateLast = 0;
            updateStart = FindEventRateIndex(peakTime, ref rateLast, EventsRateIndex);
            updateEnd = FindEventRateIndex(peakTime + CONTRACTILITY_QUALIFY_WINDOW_SIZE, ref rateLast, EventsRateIndex);

            if (artifactObj.EventType == ArtifactType.Acceleration)
            {
                UpdateEventsRateInRange(EventClassification.Acceleration, updateStart - 1, updateEnd, !bStrikeout);
            }
            else if (artifactObj.EventType == ArtifactType.Deceleration)
            {
                PluginDeceleration decelItem = artifactObj as PluginDeceleration;
                if (decelItem.DecelerationCategory == DecelerationCategories.Late && decelItem.Confidence > MINIMAL_LATE_DECEL_CONFIDENCE_LEVEL)
                {
                    UpdateEventsRateInRange(EventClassification.LateDeceleration, updateStart - 1, updateEnd, !bStrikeout);
                }
                else if (decelItem.DecelerationCategory == DecelerationCategories.Prolonged)
                {
                    if (decelItem.LateTiming && decelItem.Confidence > MINIMAL_LATE_DECEL_CONFIDENCE_LEVEL)
                    {
                        UpdateEventsRateInRange(EventClassification.LateDeceleration, updateStart - 1, updateEnd, !bStrikeout);
                    }
                    else if (decelItem.Height > MINIMAL_PROLONGED_DECEL_HEIGHT)
                    {
                        UpdateEventsRateInRange(EventClassification.ProlongedDeceleration, updateStart - 1, updateEnd, !bStrikeout);
                    }
                }
                else if (decelItem.LongAndLarge)
                {
                    UpdateEventsRateInRange(EventClassification.LongAndLargeDeceleration, updateStart - 1, updateEnd, !bStrikeout);
                }
            }
        }
    }
}