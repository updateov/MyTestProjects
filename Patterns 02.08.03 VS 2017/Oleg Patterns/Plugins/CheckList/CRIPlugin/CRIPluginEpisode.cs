//REVIEW: 23/03/15
using CommonLogger;
using CRIAlgorithm;
using CRIEntities;
using PatternsEntities;
using PatternsPluginsCommon;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Timers;
using System.Xml.Linq;

namespace CRIPlugin
{
    public enum ExposedCRIState
    {
        Undefined, // for init purposes only
        NA,
        Unknown,
        Negative,
        Positive
    };

    public class CRIPluginEpisode : Episode
    {
        #region Members & Properties

        private const string SYSTEM_USER_NAME = "SYSTEM";
        private const int SAVE_INTERVAL = 2;

        private Object m_lock = new Object();

        public CRICalculator ContractilitiesCalculator { get; private set; }
        public List<CRIPersistence> PersistenceHistory { get; private set; }
        private ExposedCRIState m_CRIState = ExposedCRIState.Undefined;
        public ExposedCRIState CurrentCRIState { get { return m_CRIState; } }

        public string GA { get; set; }
        public int Fetuses { get; set; }
        public int Contractility { get; set; }       

        private Contractility m_positiveReasonContractility = null;
        private CRIObject m_currentCRIStatus = null;
        public CRIObject CurrentCRIStatus
        {
            get
            {
                lock (m_lock)
                {
                    m_currentCRIStatus = UpdateCurrentCRIStatus();
                    UpdateCRIState();
                    return m_currentCRIStatus;
                }
            }

            private set
            {
                m_currentCRIStatus = value;
            }
        }

        public int LastSavedContractilityId { get; set; }

        #endregion

        #region constructors

        public CRIPluginEpisode(int episodeID)
            : base(episodeID)
        {
            ContractilitiesCalculator = new CRICalculator();
            PersistenceHistory = new List<CRIPersistence>();

            GA = String.Empty;
            Fetuses = -1;
            Contractility = -1;

            LastSavedContractilityId = -1;
        }

        #endregion

        #region merge
        public void ResetEpisode()
        {
            lock (m_lock)
            {
                CRIPluginDBManager.Instance.DeleteEpisodeData(EpisodeId);
                base.Reset();
                ContractilitiesCalculator = new CRICalculator();
                PersistenceHistory = new List<CRIPersistence>();
                Contractility = -1;
                LastSavedContractilityId = -1;
               
            }          

        }

        #endregion

        #region Events and Delegates

        public event EventHandler<CRIStateChangedEventArgs> CRIStateChanged;

        #endregion

        private void UpdateCRIState()
        {
            ExposedCRIState newState = ExposedCRIState.Unknown;
            switch (m_currentCRIStatus.CRIStatus)
            {
                case CRIState.Off:
                    newState = ExposedCRIState.Unknown;
                    break;
                case CRIState.UnknownNotEnoughTime:
                    newState = ExposedCRIState.Unknown;
                    break;
                case CRIState.UnknownGAOrSingletonNotMet:
                    newState = ExposedCRIState.NA;
                    break;
                case CRIState.UnknownGAOrSingletonMissing:
                    newState = ExposedCRIState.Unknown;
                    break;
                case CRIState.PositivePastNotYetReviewed:
                    newState = ExposedCRIState.Negative;
                    break;
                case CRIState.PositiveCurrent:
                    newState = ExposedCRIState.Positive;
                    break;
                case CRIState.PositiveReviewed:
                    newState = ExposedCRIState.Positive;
                    break;
                case CRIState.Negative:
                    newState = ExposedCRIState.Negative;
                    break;
                default:
                    newState = ExposedCRIState.Unknown;
                    break;
            }
            if (newState == m_CRIState)
                return;

            m_CRIState = newState;

            EventHandler<CRIStateChangedEventArgs> tmpHandler = CRIStateChanged;
            if (tmpHandler != null)
            {
                tmpHandler(this, new CRIStateChangedEventArgs(VisitKey, m_CRIState));
            }
        }

        public void UpdatePersistenceHistory(long updatedContractilitiesFirstTime, bool performSave)
        {
            lock (m_lock)
            {
                try
                {
                    var contractilities = ContractilitiesCalculator.Contractilities;

                    if (contractilities == null || contractilities.Count <= 0)
                    {
                        PersistenceHistory = new List<CRIPersistence>();
                        return;
                    }

                    //Contractility currentPositiveReason = null;
                    CalculateContractilityPersistencies(PersistenceHistory, contractilities, updatedContractilitiesFirstTime);
                    m_currentCRIStatus = UpdateCurrentCRIStatus();
                    UpdateCRIState();

                    //a strike out was done, we do not know which contractilities were changed and which did not, need to re save all the data.
                    if(updatedContractilitiesFirstTime >= 0)
                    {
                        LastSavedContractilityId = -1;
                    }

                    if (performSave && DateTime.UtcNow > DateUpdatedUTC.AddMinutes(SAVE_INTERVAL))
                    {
                        SaveEpisode();
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginEpisode", "Error occurred in UpdatePersistenceHistory.", ex);
                }
            }
        }

        private void CalculateContractilityPersistencies(List<CRIPersistence> persistentStates, List<Contractility> contractilities, long updatedContractilitiesFirstTime)
        {

            DateTime absoluteStartTime = contractilities.First().StartTime.ToDateTime();
            List<CRIPersistence> persistenceHistoryToDelete = new List<CRIPersistence>();
            bool recalculateNeeded = false;
            if (updatedContractilitiesFirstTime > 0)
            {
                DateTime startTime = updatedContractilitiesFirstTime.ToDateTime();
                int firstIndexToUpdate = persistentStates.FindIndex(p => p.EndTime >= startTime);


                if (firstIndexToUpdate >= 0)
                {
                    recalculateNeeded = true;
                    UpdateCurrentCRIStatus();
                    CRIPersistence lastOldPercistence = (persistentStates.Count > firstIndexToUpdate) ?
                        persistentStates[firstIndexToUpdate] : null;
                    int countToRemove = persistentStates.Count - firstIndexToUpdate;
                    persistenceHistoryToDelete = persistentStates.GetRange(firstIndexToUpdate, countToRemove);
                    persistentStates.RemoveRange(firstIndexToUpdate, countToRemove);
                    if (lastOldPercistence != null && lastOldPercistence.EndTime > startTime && lastOldPercistence.StartTime < startTime)
                    {
                        lastOldPercistence.EndTime = startTime;
                        persistentStates.Add(lastOldPercistence);
                    }
                }
            }

            CRIPersistence lastPersistence = (persistentStates.Count > 0) ?
                persistentStates.Last() : new CRIPersistence(PersistentContractilityState.Unknown, absoluteStartTime, absoluteStartTime);

            DateTime lastStateEndTime = lastPersistence.EndTime;

            float durabilityMinutes = (lastPersistence.PersistentState != PersistentContractilityState.Positive) ?
                CRIPluginSettings.Instance.ContractilityPositiveDurability : CRIPluginSettings.Instance.ContractilityNegativeDurability;
            int durabilitySeconds = (int)(durabilityMinutes * 60);
            long fromTime = 0;
            if (persistentStates.Count > 0)
            {
                fromTime = lastStateEndTime.AddSeconds(-durabilitySeconds).ToEpochTime();
            }



            List<Contractility> contractilitiesToCheck = new List<Contractility>(contractilities);
            contractilitiesToCheck.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

            int firstIndexToCheck = contractilitiesToCheck.FindIndex(c => c.EndTime > fromTime);
            if (firstIndexToCheck < 0)
                firstIndexToCheck = 0;

            int fromIndex = firstIndexToCheck;
            Contractility positiveReason = null;
            Contractility oldPositiveReason = m_positiveReasonContractility;

            while (fromIndex < contractilitiesToCheck.Count)
            {
                PersistentContractilityState lastState = lastPersistence.PersistentState;
                bool checkPositive = (lastState != PersistentContractilityState.Positive);
                Contractility currentPositiveReason = null;
                CRIPersistence newPersistence = CalculatePersistenceState(ref fromTime, ref fromIndex, contractilitiesToCheck, lastState, out currentPositiveReason);
                Contractility useContractility = (currentPositiveReason != null && newPersistence.PersistentState == PersistentContractilityState.Positive) ?
                                                                                                                                           currentPositiveReason : null;
                UpdatePersistentStatesHistory(persistentStates, lastPersistence, newPersistence, ref positiveReason, useContractility);
                lastPersistence = persistentStates.Last();
            }

            if (recalculateNeeded)
            {
                ApplyAcknowlegeHistory(persistentStates, persistenceHistoryToDelete, ref positiveReason);
            }

            CRIPersistence lastPositivePersistence = persistentStates.LastOrDefault(p => p.PersistentState == PersistentContractilityState.Positive && p.AcknowledgeTime == DateTime.MinValue);
            if (lastPositivePersistence != null)
            {
                m_positiveReasonContractility = (positiveReason != null) ?
                    positiveReason : oldPositiveReason;
            }
            else
            {
                m_positiveReasonContractility = null;
            }

        }

        private void ApplyAcknowlegeHistory(List<CRIPersistence> persistentStates, List<CRIPersistence> persistenceHistoryToDelete, ref Contractility positiveReason)
        {
            if (persistenceHistoryToDelete != null)
            {
                bool recalculateCurrentCRI = persistenceHistoryToDelete.Count > 0;
                while (persistenceHistoryToDelete.Count > 0)
                {
                    int firstOldIndex = persistenceHistoryToDelete.FindIndex(p => p.AcknowledgeTime > DateTime.MinValue && p.PersistentState == PersistentContractilityState.Positive);
                    if (firstOldIndex >= 0)
                    {
                        CRIPersistence firstOldAcknowledgedPositive = persistenceHistoryToDelete[firstOldIndex];

                        DateTime acknowledgeTime = firstOldAcknowledgedPositive.AcknowledgeTime;
                        string acknowlegeUser = firstOldAcknowledgedPositive.AcknowledgeUser;

                        int firstNewIndex = persistentStates.FindIndex(p => p.AcknowledgeTime == DateTime.MinValue && p.PersistentState == PersistentContractilityState.Positive && p.StartTime < acknowledgeTime);
                        int index = firstNewIndex;
                        while (index >= 0 && persistentStates[index].AcknowledgeTime == DateTime.MinValue)
                        {
                            persistentStates[index].AcknowledgeTime = acknowledgeTime;
                            persistentStates[index].AcknowledgeUser = acknowlegeUser;
                            index--;
                        }

                        persistenceHistoryToDelete.RemoveRange(0, firstOldIndex + 1);

                    }
                    else
                    {
                        persistenceHistoryToDelete.Clear();
                    }
                }

                if (recalculateCurrentCRI)
                {
                    RecalculateCurrentCRIObject(persistentStates, ref positiveReason);
                }
            }
        }

        private bool FindLastPositiveContractilityForPeriod(ref Contractility positiveReason, CRIPersistence lastPositivePersistance)
        {
            List<Contractility> contractilities = new List<Contractility>(ContractilitiesCalculator.Contractilities);
            int foundIndex = contractilities.FindLastIndex(c => c.EndTime.ToDateTime() <= lastPositivePersistance.EndTime
                                                                    && (c.CRIClassification == ContractilityClassification.Alert
                                                                    || c.CRIClassification == ContractilityClassification.Alert));
            if (foundIndex >= 0)
            {
                positiveReason = new Contractility(contractilities[foundIndex]);
                return true;
            }
            return false;
        }

        private void RecalculateCurrentCRIObject(List<CRIPersistence> persistentStates, ref Contractility positiveReason)
        {
            CRIObject criObject = new CRIObject();
            CRIPersistence lastPositivePersistance = persistentStates.LastOrDefault(p => p.PersistentState == PersistentContractilityState.Positive && p.AcknowledgeTime == DateTime.MinValue);
            CRIPersistence lastCRIPersistance = persistentStates.LastOrDefault();
            CRIPersistence lastDetectedPersistance = persistentStates.LastOrDefault(p => p.PersistentState != PersistentContractilityState.Unknown);
            PersistentContractilityState lastDetectedState = lastDetectedPersistance == null ?
                PersistentContractilityState.Unknown : lastDetectedPersistance.PersistentState;

            if (lastPositivePersistance != null)
            {
                criObject.ID = lastCRIPersistance.ID;
                criObject.ReviewTime = DateTime.MinValue;

                criObject.StartTime = lastPositivePersistance.StartTime.ToLocalTime();
                criObject.EndTime = lastPositivePersistance.EndTime.ToLocalTime();
                criObject.CRIStatusTime = lastPositivePersistance.StartTime.ToLocalTime();
                if (m_currentCRIStatus != null && m_currentCRIStatus.CRIStatus == CRIState.PositiveCurrent || m_currentCRIStatus.CRIStatus == CRIState.PositivePastNotYetReviewed)
                {
                    if (criObject.StartTime == m_currentCRIStatus.StartTime)
                    {
                        return;
                    }
                }

                if (FindLastPositiveContractilityForPeriod(ref positiveReason, lastPositivePersistance))
                {
                    criObject.CRIStatusTime = positiveReason.EndTime.ToDateTime().ToLocalTime();
                }



                m_positiveReasonContractility = new Contractility(positiveReason);

                switch (lastCRIPersistance.PersistentState)
                {
                    case PersistentContractilityState.Unknown:
                    case PersistentContractilityState.Positive:
                        criObject.CRIStatus = CRIState.PositiveCurrent;
                        break;
                    case PersistentContractilityState.Negative:
                        criObject.CRIStatus = CRIState.PositivePastNotYetReviewed;
                        break;
                }
                criObject.CRIStatusEvents = new CRIEvents(m_positiveReasonContractility.EventsCounted);
            }
            else if (lastCRIPersistance != null)
            {
                m_positiveReasonContractility = null;
                criObject.ID = lastCRIPersistance.ID;
                criObject.ReviewTime = lastCRIPersistance.AcknowledgeTime.ToLocalTime();
                criObject.StartTime = lastCRIPersistance.StartTime.ToLocalTime();
                criObject.EndTime = lastCRIPersistance.EndTime.ToLocalTime();
                criObject.CRIStatusTime = lastCRIPersistance.StartTime.ToLocalTime();
                switch (lastCRIPersistance.PersistentState)
                {
                    case PersistentContractilityState.Unknown:
                        criObject.CRIStatus = CRIState.UnknownNotEnoughTime;
                        break;
                    case PersistentContractilityState.Negative:
                        criObject.CRIStatus = CRIState.Negative;
                        break;
                    case PersistentContractilityState.Positive:
                        criObject.CRIStatus = CRIState.PositiveReviewed;
                        m_positiveReasonContractility = null;
                        break;
                }
                criObject.CRIStatusEvents = new CRIEvents(lastCRIPersistance.PersistentStateEvents);
            }
            else
            {
                criObject = null;
                m_positiveReasonContractility = null;
            }
            m_currentCRIStatus = criObject;

        }

        private void UpdatePersistentStatesHistory(List<CRIPersistence> persistentStates, CRIPersistence lastPersistence, CRIPersistence newPersistence, ref Contractility positiveReason, Contractility useContractility)
        {
            if (persistentStates.Count <= 0)
            {
                if (newPersistence.PersistentState != PersistentContractilityState.Unknown)
                {
                    persistentStates.Add(new CRIPersistence()
                    {
                        PersistentState = PersistentContractilityState.Unknown,
                        StartTime = lastPersistence.StartTime,
                        EndTime = newPersistence.EndTime.AddSeconds(-1)
                    });
                }
                persistentStates.Add(newPersistence);

            }
            else if (newPersistence.PersistentState != lastPersistence.PersistentState)
            {
                if ((lastPersistence.PersistentState == PersistentContractilityState.Unknown)
                    && newPersistence.StartTime <= lastPersistence.StartTime)
                {
                    persistentStates[persistentStates.Count - 1] = newPersistence;
                }
                else
                {
                    persistentStates[persistentStates.Count - 1].EndTime = newPersistence.StartTime.AddSeconds(-1);
                    persistentStates.Add(newPersistence);
                    if (newPersistence.PersistentState == PersistentContractilityState.Positive)
                    {
                        positiveReason = new Contractility(useContractility);
                    }
                }
            }
            else
            {
                persistentStates[persistentStates.Count - 1].EndTime = newPersistence.EndTime;
                persistentStates[persistentStates.Count - 1].PersistentStateEvents = newPersistence.PersistentStateEvents;
            }
        }

        private CRIPersistence CalculatePersistenceState(ref long fromTime, ref int fromIndex, List<Contractility> contractilities, PersistentContractilityState lastState, out Contractility currentPositiveReason)
        {
            bool positive = (lastState != PersistentContractilityState.Positive);
            float settingsDurabilityMinutes = positive ? CRIPluginSettings.Instance.ContractilityPositiveDurability : CRIPluginSettings.Instance.ContractilityNegativeDurability;
            int settingsPercentage = positive ? CRIPluginSettings.Instance.ContractilityPositivePercentage : CRIPluginSettings.Instance.ContractilityNegativePercentage;
            int settingsDurabilitySeconds = (int)(settingsDurabilityMinutes * 60);

            if (settingsDurabilitySeconds < 0)
                settingsDurabilitySeconds = 60;

            if (settingsPercentage > 100)
                settingsPercentage = 100;

            if (settingsPercentage < 0)
                settingsPercentage = 1;

            int index = fromIndex;
            long startTime = (contractilities[fromIndex].StartTime < fromTime) ? fromTime : contractilities[fromIndex].StartTime;
            long checkedEndTime = contractilities[fromIndex].EndTime;
            if (checkedEndTime - startTime > settingsDurabilitySeconds)
            {
                List<Contractility> single = new List<Contractility>();
                single.Add(contractilities[fromIndex]);
                CRIPersistence persistence = CalculatePersistentState(single, positive, 100, checkedEndTime - startTime, checkedEndTime, out currentPositiveReason);
                persistence.StartTime = startTime.ToDateTime().AddSeconds(settingsDurabilitySeconds);
                fromIndex++;
                fromTime = checkedEndTime + 1;
                return persistence;
            }

            long checkedStartTime = startTime;
            long duration = 0;
            var toCheck = AllContractilitiesInTimeInterval(contractilities, settingsDurabilitySeconds,
                                    ref index, ref fromTime, ref startTime, ref checkedEndTime, ref duration);

            if (contractilities.Count > 0 && contractilities[contractilities.Count - 1].Length == 0)
            {
                contractilities.RemoveAt(contractilities.Count - 1);
                Logger.WriteLogEntry(TraceEventType.Warning, "CRIPluginEpisode", "received contractility with length 0");
            }

            if (duration < settingsDurabilitySeconds)
            {
                long checkFromTime = checkedEndTime - settingsDurabilitySeconds;
                int firstIndexToCheck = contractilities.FindIndex(c => c.EndTime > checkFromTime);

                if (firstIndexToCheck >= 0 && fromIndex >= firstIndexToCheck &&
                    fromTime > checkFromTime && checkFromTime >= contractilities[firstIndexToCheck].StartTime)
                {
                    fromIndex = firstIndexToCheck;
                    fromTime = checkFromTime;
                    return CalculatePersistenceState(ref fromTime, ref  fromIndex, contractilities, lastState, out currentPositiveReason);
                }
                else
                {
                    fromTime = checkedEndTime;
                    fromIndex = index;

                    currentPositiveReason = null;
                    return new CRIPersistence()
                    {
                        PersistentState = PersistentContractilityState.Unknown,
                        StartTime = startTime.ToDateTime(),
                        EndTime = (startTime + duration).ToDateTime()
                    };
                }
            }


            fromIndex = index;
            fromTime = checkedEndTime;

            return CalculatePersistentState(toCheck, positive, settingsPercentage, duration, checkedEndTime, out currentPositiveReason);
        }

        private List<Contractility> AllContractilitiesInTimeInterval(List<Contractility> contractilities, long interval, ref int index, ref long fromTime, ref long startTime, ref long checkedEndTime, ref long duration)
        {
            List<Contractility> toCheck = new List<Contractility>();

            do
            {
                startTime = (contractilities[index].StartTime < fromTime) ? fromTime : contractilities[index].StartTime;
                Contractility contractility = contractilities[index];
                long curDuration = contractility.EndTime - startTime;
                if (curDuration <= 0)
                {
                    contractilities.RemoveAt(index);
                    continue;
                }
                if (duration + curDuration <= interval)
                {
                    duration += curDuration;
                    var useContractility = new Contractility()
                    {
                        CRIClassification = contractility.CRIClassification,
                        StartTime = startTime,
                        EndTime = contractility.EndTime,
                        EventsCounted = contractility.EventsCounted
                    };

                    if (duration < interval && index < contractilities.Count - 1)
                    {
                        useContractility.EndTime++;
                        duration++;
                    }

                    checkedEndTime = useContractility.EndTime;
                    toCheck.Add(useContractility);
                }
                else
                {
                    long remainder = interval - duration;
                    var curContractility = new Contractility()
                    {
                        CRIClassification = contractility.CRIClassification,
                        StartTime = startTime,
                        EndTime = startTime + remainder,
                        EventsCounted = contractility.EventsCounted
                    };

                    toCheck.Add(curContractility);
                    duration += remainder;
                    checkedEndTime = curContractility.EndTime;

                    if (checkedEndTime < contractility.EndTime - 1)
                    {
                        curContractility.StartTime = contractility.StartTime;
                        contractilities[index] = curContractility;
                        var contractilityReminder = new Contractility()
                        {
                            CRIClassification = contractility.CRIClassification,
                            StartTime = checkedEndTime + 1,
                            EndTime = contractility.EndTime,
                            EventsCounted = contractility.EventsCounted
                        };
                        contractilities.Insert(index + 1, contractilityReminder);
                    }
                }

            } while (++index < contractilities.Count && duration < interval);

            return toCheck;
        }

        private CRIPersistence CalculatePersistentState(List<Contractility> toCheck, bool positive, int settingsPercentage, long duration, long endTime, out Contractility currentPositiveReason)
        {
            PersistentContractilityState state = positive ? PersistentContractilityState.Positive : PersistentContractilityState.Negative;
            PersistentContractilityState oppositeState = positive ? PersistentContractilityState.Negative : PersistentContractilityState.Positive;
            Contractility firstPositiveReason = null;
            CRIEvents lastEvents = null;

            double unknownStateDuration = 0;
            double stateDuration = 0;
            double oppositeStateDuration = 0;

            foreach (var item in toCheck)
            {
                double itemDuration = item.EndTime - item.StartTime;
                switch (item.CRIClassification)
                {
                    case ContractilityClassification.Alert:
                    case ContractilityClassification.Danger:
                        {
                            stateDuration += (positive) ? itemDuration : 0;
                            oppositeStateDuration += (positive) ? 0 : itemDuration;
                            if (firstPositiveReason == null && positive)
                                firstPositiveReason = new Contractility()
                                {
                                    StartTime = item.StartTime,
                                    EndTime = item.EndTime,
                                    CRIClassification = item.CRIClassification,
                                    EventsCounted = new CRIEvents(item.EventsCounted)
                                };
                        }
                        break;
                    case ContractilityClassification.Normal:
                        {
                            stateDuration += (positive) ? 0 : itemDuration;
                            oppositeStateDuration += (positive) ? itemDuration : 0;
                        }
                        break;
                    case ContractilityClassification.Unknown:
                        unknownStateDuration += itemDuration;
                        break;
                }

                //if (item.CRIClassification != ContractilityClassification.Unknown)
                {
                    lastEvents = item.EventsCounted;
                }

            }

            double relativePortion = stateDuration / duration;
            PersistentContractilityState calculatedState = PersistentContractilityState.Unknown;
            if (relativePortion * 100 >= settingsPercentage)
            {
                calculatedState = state;
            }
            else if (unknownStateDuration < oppositeStateDuration)
            {
                calculatedState = oppositeState;
            }

            CRIEvents lastEventsCounted = (lastEvents == null) ? new CRIEvents() : new CRIEvents(lastEvents);
            if (calculatedState == PersistentContractilityState.Unknown)
            {
                lastEventsCounted.Accels.Value = -1;
                lastEventsCounted.Variability.Value = -1;
            }

            DateTime percistenceEndTime = endTime.ToDateTime();
            CRIPersistence calculatedPersistence = new CRIPersistence(calculatedState, percistenceEndTime, percistenceEndTime);
            calculatedPersistence.PersistentStateEvents = lastEventsCounted;
            //calculatedPersistence.PersistentStateEvents = (lastEventsCounted != null) ? new CRIEvents(lastEventsCounted) : new CRIEvents();
            currentPositiveReason = (firstPositiveReason != null) ? firstPositiveReason : null;
            return calculatedPersistence;
        }

        private CRIObject UpdateCurrentCRIStatus()
        {
            CRIObject criObject = new CRIObject();
            criObject.VisitKey = VisitKey;

            if (TracingStatus != TracingStatus.Live)
            {
                criObject.CRIStatus = CRIState.Off;
                return criObject;
            }

            if (String.IsNullOrEmpty(this.GA) || this.Fetuses == -1 || this.Fetuses == 0)
            {
                criObject.CRIStatus = CRIState.UnknownGAOrSingletonMissing;
                return criObject;
            }

            if (!IsGAValidForCRI() || this.Fetuses != 1)
            {
                criObject.CRIStatus = CRIState.UnknownGAOrSingletonNotMet;
                return criObject;
            }

            var lastCRIPersistance = PersistenceHistory.LastOrDefault();

            if (lastCRIPersistance != null)
            {
                CRIPersistence lastDetectedPersistance = PersistenceHistory.LastOrDefault(p => p.PersistentState != PersistentContractilityState.Unknown);
                CRIPersistence lastPositivePersistance = PersistenceHistory.LastOrDefault(p => p.PersistentState == PersistentContractilityState.Positive);
                PersistentContractilityState lastDetectedState = lastDetectedPersistance == null ?
                    PersistentContractilityState.Unknown : lastDetectedPersistance.PersistentState;

                criObject.ID = lastCRIPersistance.ID;
                criObject.ReviewTime = lastCRIPersistance.AcknowledgeTime.ToLocalTime();
                criObject.StartTime = lastCRIPersistance.StartTime.ToLocalTime();
                criObject.EndTime = lastCRIPersistance.EndTime.ToLocalTime();
                criObject.CRIStatusTime = lastCRIPersistance.EndTime.ToLocalTime();

                bool isReviewed = (lastPositivePersistance != null) ? lastPositivePersistance.AcknowledgeTime != DateTime.MinValue : false;

                if (isReviewed)
                {
                    criObject.ReviewTime = lastPositivePersistance.AcknowledgeTime.ToLocalTime();
                }
                else if (m_positiveReasonContractility == null && lastPositivePersistance != null)
                {
                    //m_positiveReasonContractility = new CRIEvents(lastPositivePersistance.PersistentStateEvents);
                    m_positiveReasonContractility = new Contractility()
                    {
                        CRIClassification = ContractilityClassification.Alert,
                        StartTime = lastPositivePersistance.EndTime.AddSeconds(-1).ToUniversalTime().ToEpochTime(),
                        EndTime = lastPositivePersistance.EndTime.ToUniversalTime().ToEpochTime(),
                        EventsCounted = new CRIEvents(lastPositivePersistance.PersistentStateEvents)
                    };
                }

                if (lastPositivePersistance != null && !isReviewed)
                {
                    criObject.StartTime = lastPositivePersistance.StartTime.ToLocalTime();
                    criObject.EndTime = lastPositivePersistance.EndTime.ToLocalTime();
                    criObject.CRIStatusTime = lastPositivePersistance.StartTime.ToLocalTime();
                }


                switch (lastCRIPersistance.PersistentState)
                {
                    case PersistentContractilityState.Unknown:
                        {
                            switch (lastDetectedState)
                            {
                                case PersistentContractilityState.Unknown:
                                    criObject.CRIStatus = CRIState.UnknownNotEnoughTime;
                                    break;
                                case PersistentContractilityState.Negative:
                                    criObject.CRIStatus = (!isReviewed && lastPositivePersistance != null) ?
                                        CRIState.PositivePastNotYetReviewed : CRIState.UnknownNotEnoughTime;
                                    break;
                                case PersistentContractilityState.Positive:
                                    criObject.CRIStatus = (isReviewed) ? CRIState.UnknownNotEnoughTime : CRIState.PositiveCurrent;
                                    break;
                            }
                        }
                        break;
                    case PersistentContractilityState.Negative:
                        criObject.CRIStatus = (isReviewed || lastPositivePersistance == null) ?
                                CRIState.Negative : CRIState.PositivePastNotYetReviewed;
                        break;
                    case PersistentContractilityState.Positive:
                        criObject.CRIStatus = isReviewed ? CRIState.PositiveReviewed : CRIState.PositiveCurrent;
                        break;
                }

                CRIEvents oldEvents = (m_currentCRIStatus != null) ? m_currentCRIStatus.CRIStatusEvents : null;
                DateTime oldStartTime = (m_currentCRIStatus != null) ? m_currentCRIStatus.StartTime : DateTime.MinValue;
                DateTime oldStatusTime = (m_currentCRIStatus != null) ? m_currentCRIStatus.CRIStatusTime : DateTime.MinValue;
                CRIState oldStatus = (m_currentCRIStatus != null) ? m_currentCRIStatus.CRIStatus : CRIState.UnknownNotEnoughTime;
                if (criObject.CRIStatus != CRIState.PositivePastNotYetReviewed && criObject.CRIStatus != CRIState.PositiveCurrent)
                {
                    criObject.CRIStatusEvents = new CRIEvents(lastCRIPersistance.PersistentStateEvents);
                }
                else if (lastPositivePersistance != null && m_positiveReasonContractility != null &&
                    (criObject.StartTime != oldStartTime || criObject.CRIStatus != oldStatus))
                {
                    criObject.CRIStatusEvents = new CRIEvents(m_positiveReasonContractility.EventsCounted);
                    criObject.CRIStatusTime = m_positiveReasonContractility.EndTime.ToDateTime().ToLocalTime();
                }
                else if ((oldStatus == CRIState.PositiveCurrent || oldStatus == CRIState.PositivePastNotYetReviewed)
                    && lastPositivePersistance != null && oldEvents != null && oldEvents.Contractions.Value >= 0)
                {
                    //use previous events which supposed to be from first unacknowledged positive contractility
                    criObject.CRIStatusEvents = new CRIEvents(oldEvents);
                    criObject.CRIStatusTime = oldStatusTime;
                }
                else
                {
                    criObject.CRIStatusEvents = (lastPositivePersistance != null) ?
                        new CRIEvents(lastPositivePersistance.PersistentStateEvents) : new CRIEvents(lastCRIPersistance.PersistentStateEvents);
                }
            }
            else
            {
                criObject.CRIStatus = CRIState.UnknownNotEnoughTime;
            }


            return criObject;
        }

        public List<CRIObject> GetPositivetNotYetReviewed()
        {
            lock (m_lock)
            {
                List<CRIObject> result = new List<CRIObject>();

                try
                {

                    var notReviewed = PersistenceHistory.Where(p => p.PersistentState == PersistentContractilityState.Positive && p.AcknowledgeTime == DateTime.MinValue);
                    foreach (var item in notReviewed)
                    {
                        CRIObject criObject = new CRIObject();
                        criObject.VisitKey = VisitKey;
                        criObject.ID = item.ID;
                        criObject.StartTime = item.StartTime.ToLocalTime();
                        criObject.EndTime = item.EndTime.ToLocalTime();
                        criObject.CRIStatus = CRIState.PositivePastNotYetReviewed;

                        result.Add(criObject);
                    }
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginEpisode", "Error occurred in GetPositivePastNotYetReviewed.", ex);
                }

                return result;
            }
        }

        private bool IsGAValidForCRI()
        {
            if (String.IsNullOrEmpty(GA))
            {
                return false;
            }

            int weeks = 0;
            int plus = this.GA.IndexOf("+");
            string weeksPart = (plus > 0) ? this.GA.Substring(0, plus) : this.GA;
            weeksPart.Trim();
            weeks = Convert.ToInt32(weeksPart);
            if (weeks >= 36 && plus < this.GA.Length)
            {
                string daysPart = this.GA.Substring(plus + 1);
                daysPart.Trim();
                if (Convert.ToInt32(daysPart) > 0)
                {
                    weeks++;
                }
            }

            return (weeks >= 36 && weeks <= 43);
        }

        public List<ContractilityDisplay> GetLastHourContractilities()
        {
            lock (m_lock)
            {
                List<ContractilityDisplay> lastHourContractilities = new List<ContractilityDisplay>();
                try
                {
                    if (!IsGAValidForCRI() || Fetuses != 1)
                    {
                        return lastHourContractilities;
                    }
                    List<Contractility> contractilitiesToCheck = new List<Contractility>(ContractilitiesCalculator.Contractilities);
                    if (contractilitiesToCheck.Count > 0)
                    {
                        DateTime time = DateTime.UtcNow;
                        DateTime startTime = time.AddHours(-1).ToLocalTime();
                        long now = time.ToEpochTime();
                        long start = now - 3600;
                        var contractilities = contractilitiesToCheck.Where(c => c.EndTime > start);
                        if (contractilities != null)
                        {
                            foreach (var contractility in contractilities)
                            {
                                ContractilityState state = ContractilityState.Unknown;
                                switch (contractility.CRIClassification)
                                {
                                    case ContractilityClassification.Unknown:
                                        break;
                                    case ContractilityClassification.Normal:
                                        state = ContractilityState.Normal;
                                        break;
                                    case ContractilityClassification.Alert:
                                        state = ContractilityState.Alert;
                                        break;
                                    case ContractilityClassification.Danger:
                                        state = ContractilityState.Danger;
                                        break;
                                }

                                DateTime contractilityStart = contractility.StartTime.ToDateTime().ToLocalTime();
                                DateTime contractilityEnd = contractility.EndTime.ToDateTime().ToLocalTime();

                                ContractilityDisplay contractilityDisplay = new ContractilityDisplay(contractilityStart < startTime ? startTime : contractilityStart, contractilityEnd, state);
                                lastHourContractilities.Add(contractilityDisplay);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginEpisode", "Error occurred in GetLastHourContractilities.", ex);
                }

                return lastHourContractilities;
            }
        }

        public bool SetPersistenciesToAck(string username, DateTime utcAckTime, bool isRecovery = false)
        {
            lock (m_lock)
            {
                bool bRes = false;

                try
                {
                    DateTime currentTime = DateTime.UtcNow;

                    //update AcknowledgeTime
                    for (int i = 0; i < PersistenceHistory.Count; i++)
                    {
                        if (PersistenceHistory[i].AcknowledgeTime == DateTime.MinValue)
                        {
                            if (!isRecovery)
                            {
                                PersistenceHistory[i].AcknowledgeTime = currentTime;
                                PersistenceHistory[i].AcknowledgeUser = username;
                            }
                            else
                            {
                                if (PersistenceHistory[i].StartTime <= utcAckTime)
                                {
                                    PersistenceHistory[i].AcknowledgeTime = currentTime;
                                    PersistenceHistory[i].AcknowledgeUser = username;
                                }
                            }
                        }
                    }

                    //save changes to database
                    SaveEpisode();

                    bRes = true;

                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginEpisode", "Error occurred in SetEpisodePersistencesToReviewed.", ex);
                }

                return bRes;
            }
        }

        public void AcknowledgeOldPositivePersistences()
        {
            var downTimeDuration = (DateTime.UtcNow - DateUpdatedUTC).TotalMinutes;
            if (downTimeDuration >= CRIPluginSettings.Instance.MinimalDowntimeDurationForSystemAck)
            {
                DateTime ackTime = DateTime.UtcNow.AddMinutes(-CRIPluginSettings.Instance.MinimalDowntimeDurationForSystemAck);
                SetPersistenciesToAck(SYSTEM_USER_NAME, ackTime, true);
            }

            IsAfterDownTime = false;
        }

        private void SaveEpisode()
        {
            int lastContractilityId = ContractilitiesCalculator.ContractilityId;

            List<CRIPluginEpisode> episodesToSave = new List<CRIPluginEpisode>();
            episodesToSave.Add(this);
            bool bRes = CRIPluginDBManager.Instance.SaveEpisodes(episodesToSave);

            if (bRes)
            {
                DateUpdatedUTC = DateTime.UtcNow;
                LastSavedContractilityId = lastContractilityId;
            }
        }

        public static bool SaveEpisodes(List<CRIPluginEpisode> episodesToSave)
        {
            bool bRes = true;
            try
            {
                if (episodesToSave != null && episodesToSave.Count > 0)
                {
                    DateTime utcNow = DateTime.UtcNow;

                    Dictionary<int, int> episodesLastContractilityIds = new Dictionary<int, int>();
                    foreach (CRIPluginEpisode episode in episodesToSave)
                        episodesLastContractilityIds.Add(episode.EpisodeId, episode.ContractilitiesCalculator.ContractilityId);

                    bRes = CRIPluginDBManager.Instance.SaveEpisodes(episodesToSave);
                    if (bRes)
                    {
                        foreach (CRIPluginEpisode episode in episodesToSave)
                        {
                            episode.DateUpdatedUTC = utcNow;
                            episode.LastSavedContractilityId = episodesLastContractilityIds[episode.EpisodeId];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, "CRIPluginEpisode", "Error occurred in SaveEpisodes.", ex);
                bRes = false;
            }
            return bRes;
        }
    }
}
