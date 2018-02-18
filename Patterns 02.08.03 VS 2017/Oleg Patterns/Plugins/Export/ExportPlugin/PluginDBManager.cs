using CommonLogger;
using Export.Entities;
using Export.Entities.ExportControlConfig;
using Export.PluginDataModel;
using MMSInterfaces;
using PatternsEntities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Export.Plugin
{
    public class PluginDBManager
    {
        private Object m_lock = new Object();
        public List<ConceptNumberToColumnMappingModel> ConceptsIndex;

        #region Singleton functionality

        private static PluginDBManager s_PluginDBManager = null;
        private static Object s_lockObject = new Object();

        private PluginDBManager()
        {
            Init();
        }

        private void Init()
        {
            DBManager.Instance.CreateDB();
            List<ConceptNumberToColumnMappingModel> conceptsToAdd = new List<ConceptNumberToColumnMappingModel>();
            if (PluginSettings.Instance.CALMServicesEnabled)
            {
                

                foreach (int conceptId in ExportManager.Instance.ExportDataConfig.GetConceptsFromConfig())
                {
                    ExportEntity conceptConfig = ExportManager.Instance.ExportDataConfig.GetEntityById(conceptId);

                    ConceptInfo conceptInfo = ExportManager.Instance.ConceptsInfos.FirstOrDefault(c => c.ConceptNo == conceptId);

                    if (conceptConfig != null && conceptInfo != null)
                    {
                        ConceptNumberToColumnMappingModel conceptMapping = new ConceptNumberToColumnMappingModel();

                        conceptMapping.ConceptNumber = conceptId;
                        conceptMapping.ObjectOfCare = conceptInfo.ObjectOFCare;
                        conceptMapping.ColumnName = String.Empty;
                        conceptMapping.Comments = String.Empty;
                        conceptMapping.ConceptType = ConvertConceptTypeToString(conceptConfig.ControlType);
                        conceptMapping.DateInserted = DateTime.MinValue;
                        conceptMapping.DateUpdated = DateTime.MinValue;

                        conceptsToAdd.Add(conceptMapping);
                    }
                }
                DBManager.Instance.AddConceptsToColumnMapping(conceptsToAdd);
            }
            ConceptsIndex = new List<ConceptNumberToColumnMappingModel>();
            bool bSucc = DBManager.Instance.GetConceptNumberToColumnMapping(ref ConceptsIndex);
        }

        private string ConvertConceptTypeToString(ExportControlTypes controlType)
        {
            string typeName = String.Empty;

            switch (controlType)
            {
                case ExportControlTypes.Int:
                case ExportControlTypes.Rounding:
                case ExportControlTypes.CheckBox:
                    typeName = "Int";
                    break;
                case ExportControlTypes.Double:
                    typeName = "Double";
                    break;
                case ExportControlTypes.String:
                case ExportControlTypes.Range:
                case ExportControlTypes.RangeDoubleConcept:
                    typeName = "String";
                    break;
                case ExportControlTypes.Combo:
                    typeName = "Combo";
                    break;
                case ExportControlTypes.ComboMultiValue:
                    typeName = "ComboMultiValue";
                    break;
                case ExportControlTypes.CalculatedCombo:
                    typeName = "CalculatedCombo";
                    break;
                case ExportControlTypes.CalculatedCheckboxGroup:
                    typeName = "CalculatedCheckboxGroup";
                    break;

                default:
                    break;
            }

            return typeName;
        }

        public static PluginDBManager Instance
        {
            get
            {
                if (s_PluginDBManager == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_PluginDBManager == null)
                        {
                            s_PluginDBManager = new PluginDBManager();
                        }
                    }
                }

                return s_PluginDBManager;
            }
        }

        #endregion

        #region Events & Delegates

        #endregion

        #region Work with DAL

        public List<PluginEpisode> LoadEpisodes()
        {
            lock (m_lock)
            {
                List<PluginEpisode> pluginEpisodes = new List<PluginEpisode>();

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                List<EpisodeModel> episodesModels = new List<EpisodeModel>();
                bool bSuccess = DBManager.Instance.LoadItems(out episodesModels);

                stopwatch.Stop();

                pluginEpisodes = PluginFactory.Instance.CreatePluginEpisodes(episodesModels).ToList();

                Logger.WriteLogEntry(TraceEventType.Information, "ExportPluginDBManager", String.Format("Loaded {0} episodes from DB, time = {1}", pluginEpisodes.Count, stopwatch.ElapsedMilliseconds));

                return pluginEpisodes;
            }
        }

        public bool SaveEpisodes(List<EpisodeModel> episodesModels)
        {
            lock (m_lock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                bool bSucc = DBManager.Instance.SaveItems(episodesModels);

                stopwatch.Stop();

                TraceEventType eventType = TraceEventType.Verbose;
                if (episodesModels.Count > 1)
                {
                    if (stopwatch.ElapsedMilliseconds > 5000)
                        eventType = TraceEventType.Warning;
                }
                else
                {
                    if (stopwatch.ElapsedMilliseconds > 1000)
                        eventType = TraceEventType.Warning;
                }

                Logger.WriteLogEntry(eventType, "ExportPluginDBManager", String.Format("{0} to save {1} episodes to DB, time={2}", bSucc ? "Succeeded" : "Failed", episodesModels.Count, stopwatch.ElapsedMilliseconds));
                return bSucc;
            }
        }

        public bool DischargeEpisodes(EpisodeModel episodesModel)
        {
            lock (m_lock)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                bool bSucc = DBManager.Instance.DischargedEpisodes(episodesModel);

                stopwatch.Stop();

                TraceEventType eventType = TraceEventType.Verbose;
                if (stopwatch.ElapsedMilliseconds > 1000)
                    eventType = TraceEventType.Warning;

                //Logger.WriteLogEntry(eventType, "ExportPluginDBManager", String.Format("{0} to discharge {1} episodes from DB, time={2}", bSucc ? "Succeed" : "Failed", episodesModels.Count(), stopwatch.ElapsedMilliseconds));
                Logger.WriteLogEntry(eventType, "ExportPluginDBManager", String.Format("{0} to discharge {1} episode from DB, time={2}", bSucc ? "Succeed" : "Failed", episodesModel.EpisodeId, stopwatch.ElapsedMilliseconds));

                return bSucc;
            }
        }

        #endregion
    }
}
