using CRIAlgorithm;
using CRIPluginDataModel;
using CRIEntities;
using PatternsPluginsCommon;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PatternsEntities;
using CommonLogger;

namespace CRIPlugin
{
    public class CRIPluginDBManager
    {
        private Object m_lock = new Object();

        #region Singleton functionality

        private static CRIPluginDBManager s_criPluginDBManager = null;
        private static Object s_lockObject = new Object();

        private CRIPluginDBManager()
        {

        }

        public static CRIPluginDBManager Instance
        {
            get
            {
                if (s_criPluginDBManager == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_criPluginDBManager == null)
                        {
                            s_criPluginDBManager = new CRIPluginDBManager();
                        }
                    }
                }

                return s_criPluginDBManager;
            }
        }

        #endregion

        #region Work with DAL

        public List<CRIPluginEpisode> LoadEpisodes()
        {
            lock (m_lock)
            {
                List<CRIPluginEpisode> episodes = new List<CRIPluginEpisode>();
                DBManager.Instance.CreateDB();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                List<EpisodeModel> episodesModels;
                bool bSucc = DBManager.Instance.LoadAll(out episodesModels);

                stopwatch.Stop();

                episodes = ToPluginEpisodes(episodesModels);

                bool bPersistenceSucc = DBManager.Instance.LastPersistenceId(out CRIPersistence.NextId);

                Logger.WriteLogEntry(TraceEventType.Information, "CRIPluginDBManager", String.Format("Loaded {0} episodes from DB, time = {1}", episodes.Count, stopwatch.ElapsedMilliseconds));

                return episodes;
            }
        }

        public bool SaveEpisodes(IEnumerable<CRIPluginEpisode> episodes)
        {
            lock (m_lock)
            {
                var episodesModels = ToEpisodesModels(episodes, true);

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                bool bSucc = DBManager.Instance.SaveAll(episodesModels);

                stopwatch.Stop();

                TraceEventType eventType = TraceEventType.Verbose;
                if (episodes.Count() > 1)
                {
                    if (stopwatch.ElapsedMilliseconds > 5000)
                        eventType = TraceEventType.Warning;
                }
                else
                {
                    if (stopwatch.ElapsedMilliseconds > 1000)
                        eventType = TraceEventType.Warning;
                }

                Logger.WriteLogEntry(eventType, "CRIPluginDBManager", String.Format("{0} to save {1} episodes to DB, time={2}", bSucc ? "Succeeded" : "Failed", episodes.Count(), stopwatch.ElapsedMilliseconds));
                foreach (var episode in episodesModels)
                {
                    int contractilitiesCount = episode.Contractilities.Count(c => c.IsDirty);
                    int persistencieCount = episode.PersistencieStates.Count(c => c.IsDirty);
                    Logger.WriteLogEntry(eventType, "CRIPluginDBManager", String.Format("Saved EpisodeID={0}, Contractilities={1}, Persistences={2}", episode.EpisodeId, contractilitiesCount, persistencieCount));
                }

                return bSucc;
            }
        }

        public bool DischargeEpisodes(IEnumerable<CRIPluginEpisode> episodesToDischarge)
        {
            lock (m_lock)
            {
                var episodesModels = ToEpisodesModels(episodesToDischarge, CRIPluginSettings.Instance.SaveStatisticalDataset);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                bool bSucc = DBManager.Instance.DischargeEpisodes(episodesModels);

                stopwatch.Stop();

                TraceEventType eventType = TraceEventType.Verbose;
                if (stopwatch.ElapsedMilliseconds > 1000)
                    eventType = TraceEventType.Warning;

                Logger.WriteLogEntry(eventType, "CRIPluginDBManager", String.Format("{0} to discharge {1} episodes from DB, time={2}", bSucc ? "Succeed" : "Failed", episodesToDischarge.Count(), stopwatch.ElapsedMilliseconds));
                foreach (var episode in episodesModels)
                {
                    Logger.WriteLogEntry(eventType, "CRIPluginDBManager", String.Format("Saved EpisodeID={0}, Contractilities={1}, Persistences={2}", episode.EpisodeId, episode.Contractilities.Count, episode.PersistencieStates.Count));
                }

                return bSucc;
            }
        }

        private List<EpisodeModel> ToEpisodesModels(IEnumerable<CRIPluginEpisode> pluginEpisodes, bool SaveStatisticalDataset)
        {
            List<EpisodeModel> episodesModels = new List<EpisodeModel>();
            foreach (var pluginEpisode in pluginEpisodes)
            {
                EpisodeModel episodeModel = new EpisodeModel();
                episodeModel.EpisodeId = pluginEpisode.EpisodeId;
                episodeModel.StatusId = (int)pluginEpisode.EpisodeStatus;
                episodeModel.DateUpdated = pluginEpisode.DateUpdatedUTC;
                episodeModel.DateInserted = pluginEpisode.DateInsertedUTC;
                episodeModel.EpisodeGuid = pluginEpisode.EpisodeGuid;
                episodeModel.LastMergeId = pluginEpisode.LastMergeId;
                episodeModel.LastMergeTime = pluginEpisode.LastMergeTime;
                if (SaveStatisticalDataset)
                {
                    episodeModel.EpisodeKey = pluginEpisode.VisitKey;
                    episodeModel.GA = pluginEpisode.GA;
                    episodeModel.Fetuses = pluginEpisode.Fetuses;
                    
                    if (pluginEpisode.EpisodeStatus != EpisodeStatus.ToHistory || pluginEpisode.EpisodeStatus == EpisodeStatus.ToHistory && !String.IsNullOrEmpty(pluginEpisode.GA) && Convert.ToInt32(pluginEpisode.GA.Remove(pluginEpisode.GA.IndexOf('+'))) >= 36 && pluginEpisode.Fetuses == 1)//story 6763
                    {
                        foreach (var contractility in pluginEpisode.ContractilitiesCalculator.Contractilities)
                        {
                            ContractilityModel contractilityModel = new ContractilityModel();
                            contractilityModel.ContractilityId = contractility.Id;
                            contractilityModel.StartTime = contractility.StartTime.ToDateTime();
                            contractilityModel.EndTime = contractility.EndTime.ToDateTime();
                            contractilityModel.Classification = (int)contractility.CRIClassification;
                            contractilityModel.Variability = contractility.EventsCounted.Variability.Value;
                            contractilityModel.IsVariabilityForStatus = contractility.EventsCounted.Variability.IsReason;
                            contractilityModel.Accels = contractility.EventsCounted.Accels.Value;
                            contractilityModel.IsAccelsForStatus = contractility.EventsCounted.Accels.IsReason;
                            contractilityModel.Contractions = contractility.EventsCounted.Contractions.Value;
                            contractilityModel.IsContractionsForStatus = contractility.EventsCounted.Contractions.IsReason;
                            contractilityModel.LongContractions = contractility.EventsCounted.LongContractions.Value;
                            contractilityModel.IsLongContractionsForStatus = contractility.EventsCounted.LongContractions.IsReason;
                            contractilityModel.LargeDeceles = contractility.EventsCounted.LargeDeceles.Value;
                            contractilityModel.IsLargeDecelesForStatus = contractility.EventsCounted.LargeDeceles.IsReason;
                            contractilityModel.LateDecels = contractility.EventsCounted.LateDecels.Value;
                            contractilityModel.IsLateDecelsForStatus = contractility.EventsCounted.LateDecels.IsReason;
                            contractilityModel.ProlongedDecels = contractility.EventsCounted.ProlongedDecels.Value;
                            contractilityModel.IsProlongedDecelsForStatus = contractility.EventsCounted.ProlongedDecels.IsReason;
                            contractilityModel.IsDirty = contractility.Id > pluginEpisode.LastSavedContractilityId;

                            episodeModel.Contractilities.Add(contractilityModel);
                        }
                    }
                
                }
                if (pluginEpisode.EpisodeStatus != EpisodeStatus.ToHistory || pluginEpisode.EpisodeStatus == EpisodeStatus.ToHistory && !String.IsNullOrEmpty(pluginEpisode.GA) && Convert.ToInt32(pluginEpisode.GA.Remove(pluginEpisode.GA.IndexOf('+'))) >= 36 && pluginEpisode.Fetuses == 1)//story 6763
                {
                    foreach (var criPersistance in pluginEpisode.PersistenceHistory)
                    {
                        PersistenceStateModel persistanceStateModel = new PersistenceStateModel();
                        persistanceStateModel.PersistenceId = criPersistance.ID;
                        persistanceStateModel.StartTime = criPersistance.StartTime;
                        persistanceStateModel.EndTime = criPersistance.EndTime;
                        persistanceStateModel.State = (int)criPersistance.PersistentState;
                        persistanceStateModel.AcknowledgeTime = criPersistance.AcknowledgeTime;
                        persistanceStateModel.AcknowledgeUserType = criPersistance.AcknowledgeUser;
                        persistanceStateModel.Variability = criPersistance.PersistentStateEvents.Variability.Value;
                        persistanceStateModel.IsVariabilityForStatus = criPersistance.PersistentStateEvents.Variability.IsReason;
                        persistanceStateModel.Accels = criPersistance.PersistentStateEvents.Accels.Value;
                        persistanceStateModel.IsAccelsForStatus = criPersistance.PersistentStateEvents.Accels.IsReason;
                        persistanceStateModel.Contractions = criPersistance.PersistentStateEvents.Contractions.Value;
                        persistanceStateModel.IsContractionsForStatus = criPersistance.PersistentStateEvents.Contractions.IsReason;
                        persistanceStateModel.LongContractions = criPersistance.PersistentStateEvents.LongContractions.Value;
                        persistanceStateModel.IsLongContractionsForStatus = criPersistance.PersistentStateEvents.LongContractions.IsReason;
                        persistanceStateModel.LargeDeceles = criPersistance.PersistentStateEvents.LargeDeceles.Value;
                        persistanceStateModel.IsLargeDecelesForStatus = criPersistance.PersistentStateEvents.LargeDeceles.IsReason;
                        persistanceStateModel.LateDecels = criPersistance.PersistentStateEvents.LateDecels.Value;
                        persistanceStateModel.IsLateDecelsForStatus = criPersistance.PersistentStateEvents.LateDecels.IsReason;
                        persistanceStateModel.ProlongedDecels = criPersistance.PersistentStateEvents.ProlongedDecels.Value;
                        persistanceStateModel.IsProlongedDecelsForStatus = criPersistance.PersistentStateEvents.ProlongedDecels.IsReason;

                        persistanceStateModel.IsDirty = criPersistance.IsDirty;

                        episodeModel.PersistencieStates.Add(persistanceStateModel);
                    }
                }
                episodesModels.Add(episodeModel);
            }

            return episodesModels;
        }

        private List<CRIPluginEpisode> ToPluginEpisodes(IEnumerable<EpisodeModel> episodesModels)
        {
            List<CRIPluginEpisode> episodes = new List<CRIPluginEpisode>();

            foreach (var episodeModel in episodesModels)
            {
                CRIPluginEpisode pluginEpisode = new CRIPluginEpisode(episodeModel.EpisodeId);
                pluginEpisode.VisitKey = episodeModel.EpisodeKey;
                pluginEpisode.GA = episodeModel.GA;
                pluginEpisode.Fetuses = episodeModel.Fetuses;
                pluginEpisode.EpisodeStatus = (EpisodeStatus)episodeModel.StatusId;
                pluginEpisode.DateUpdatedUTC = episodeModel.DateUpdated;
                pluginEpisode.DateInsertedUTC = episodeModel.DateInserted;
                pluginEpisode.EpisodeGuid = episodeModel.EpisodeGuid;
                pluginEpisode.LastMergeId = episodeModel.LastMergeId;
                pluginEpisode.LastMergeTime = episodeModel.LastMergeTime;

                foreach (var contractilityModel in episodeModel.Contractilities)
                {
                    Contractility contractility = new Contractility();
                    contractility.Id = contractilityModel.ContractilityId;
                    contractility.StartTime = contractilityModel.StartTime.ToEpochTime();
                    contractility.EndTime = contractilityModel.EndTime.ToEpochTime();
                    contractility.CRIClassification = (ContractilityClassification)contractilityModel.Classification;
                    contractility.EventsCounted.Variability.Value = contractilityModel.Variability;
                    contractility.EventsCounted.Variability.IsReason = contractilityModel.IsVariabilityForStatus;
                    contractility.EventsCounted.Accels.Value = contractilityModel.Accels;
                    contractility.EventsCounted.Accels.IsReason = contractilityModel.IsAccelsForStatus;
                    contractility.EventsCounted.Contractions.Value = contractilityModel.Contractions;
                    contractility.EventsCounted.Contractions.IsReason = contractilityModel.IsContractionsForStatus;
                    contractility.EventsCounted.LongContractions.Value = contractilityModel.LongContractions;
                    contractility.EventsCounted.LongContractions.IsReason = contractilityModel.IsLongContractionsForStatus;
                    contractility.EventsCounted.LargeDeceles.Value = contractilityModel.LargeDeceles;
                    contractility.EventsCounted.LargeDeceles.IsReason = contractilityModel.IsLargeDecelesForStatus;
                    contractility.EventsCounted.LateDecels.Value = contractilityModel.LateDecels;
                    contractility.EventsCounted.LateDecels.IsReason = contractilityModel.IsLateDecelsForStatus;
                    contractility.EventsCounted.ProlongedDecels.Value = contractilityModel.ProlongedDecels;
                    contractility.EventsCounted.ProlongedDecels.IsReason = contractilityModel.IsProlongedDecelsForStatus;
                    contractility.EventsCounted.Variability.Value = contractilityModel.Variability;
                    contractility.EventsCounted.Variability.IsReason = contractilityModel.IsVariabilityForStatus;

                    pluginEpisode.ContractilitiesCalculator.Contractilities.Add(contractility);
                }

                var contractilities = pluginEpisode.ContractilitiesCalculator.Contractilities;
                while (contractilities.Count > 1 && contractilities.Last().CRIClassification == ContractilityClassification.Unknown)
                {
                    contractilities.RemoveAt(contractilities.Count - 1);                    
                }

                if (contractilities.Count <= 0)
                    contractilities.Add(new Contractility(0, 0, ContractilityClassification.Unknown));                                    

                pluginEpisode.ContractilitiesCalculator.LastCalculatedContractility = pluginEpisode.ContractilitiesCalculator.Contractilities.Last().EndTime;
                pluginEpisode.ContractilitiesCalculator.ContractilityId = Math.Max(0, pluginEpisode.ContractilitiesCalculator.Contractilities.Last().Id);
                pluginEpisode.LastSavedContractilityId = pluginEpisode.ContractilitiesCalculator.ContractilityId;
                pluginEpisode.ContractilitiesCalculator.Contractilities.RemoveAll(c => c.Id == -1);

                foreach (var persistanceState in episodeModel.PersistencieStates)
                {
                    CRIPersistence criPersistance = new CRIPersistence();
                    criPersistance.ID = persistanceState.PersistenceId;
                    criPersistance.StartTime = persistanceState.StartTime;
                    criPersistance.EndTime = persistanceState.EndTime;
                    criPersistance.PersistentState = (PersistentContractilityState)persistanceState.State;
                    criPersistance.AcknowledgeTime = persistanceState.AcknowledgeTime;
                    criPersistance.AcknowledgeUser = persistanceState.AcknowledgeUserType;
                    criPersistance.PersistentStateEvents.Variability.Value = persistanceState.Variability;
                    criPersistance.PersistentStateEvents.Variability.IsReason = persistanceState.IsVariabilityForStatus;
                    criPersistance.PersistentStateEvents.Accels.Value = persistanceState.Accels;
                    criPersistance.PersistentStateEvents.Accels.IsReason = persistanceState.IsAccelsForStatus;
                    criPersistance.PersistentStateEvents.Contractions.Value = persistanceState.Contractions;
                    criPersistance.PersistentStateEvents.Contractions.IsReason = persistanceState.IsContractionsForStatus;
                    criPersistance.PersistentStateEvents.LongContractions.Value = persistanceState.LongContractions;
                    criPersistance.PersistentStateEvents.LongContractions.IsReason = persistanceState.IsLongContractionsForStatus;
                    criPersistance.PersistentStateEvents.LargeDeceles.Value = persistanceState.LargeDeceles;
                    criPersistance.PersistentStateEvents.LargeDeceles.IsReason = persistanceState.IsLargeDecelesForStatus;
                    criPersistance.PersistentStateEvents.LateDecels.Value = persistanceState.LateDecels;
                    criPersistance.PersistentStateEvents.LateDecels.IsReason = persistanceState.IsLateDecelsForStatus;
                    criPersistance.PersistentStateEvents.ProlongedDecels.Value = persistanceState.ProlongedDecels;
                    criPersistance.PersistentStateEvents.ProlongedDecels.IsReason = persistanceState.IsProlongedDecelsForStatus;

                    pluginEpisode.PersistenceHistory.Add(criPersistance);
                }

                episodes.Add(pluginEpisode);
            }

            return episodes;
        }

        public bool DeleteEpisodeData(int episodeID)
        {
            bool res = false;
            lock (m_lock)
            {
                res = DBManager.Instance.DeleteEpisodeData(episodeID);
            }
            return res;
        }

        #endregion
    }
}
