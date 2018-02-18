using Export.Algorithm;
using Export.Entities;
using Export.Entities.ExportControlConfig;
using Export.PluginDataModel;
using MMSInterfaces;
using PatternsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Export.Plugin
{
    public class PluginFactory
    {
        #region Singleton functionality

        private static PluginFactory s_PluginFactory = null;
        private static Object s_lockObject = new Object();

        private PluginFactory()
        {
        }

        public static PluginFactory Instance
        {
            get
            {
                if (s_PluginFactory == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_PluginFactory == null)
                        {
                            s_PluginFactory = new PluginFactory();
                        }
                    }
                }

                return s_PluginFactory;
            }
        }

        #endregion

        public Interval CreateInterval(CalculatedInterval calculatedInterval)
        {
            Interval interval = new Interval();

            if (calculatedInterval != null)
            {
                interval.IntervalId = calculatedInterval.IntervalID;
                interval.IntervalDuration = calculatedInterval.IntervalDuration;
                interval.StartTime = calculatedInterval.StartTime;
                interval.EndTime = calculatedInterval.EndTime;

                List<ConceptInfo> conceptsInfos = ExportManager.Instance.ConceptsInfos;
                foreach (var conceptInfo in conceptsInfos)
                {
                    Entities.BaseConcept concept = CreateConceptFromCalculatedInterval(conceptInfo, calculatedInterval);
                    interval.Concepts.Add(concept);
                }
            }

            return interval;
        }

        public IEnumerable<EpisodeModel> CreateEpisodesModels(IEnumerable<PluginEpisode> pluginEpisodes)
        {
            List<EpisodeModel> episodesModels = new List<EpisodeModel>();
            foreach (var pluginEpisode in pluginEpisodes)
            {
                EpisodeModel episodeModel = CreateEpisodeModel(pluginEpisode, new List<int>());
                episodesModels.Add(episodeModel);
            }

            return episodesModels;
        }

        public EpisodeModel CreateEpisodeModel(PluginEpisode pluginEpisode, List<int> dirtyIntervalIds)
        {
            EpisodeModel episodeModel = new EpisodeModel();

            episodeModel.EpisodeId = pluginEpisode.EpisodeId;
            episodeModel.VisitKey = pluginEpisode.VisitKey;
            episodeModel.IntervalDuration = pluginEpisode.CurrentIntervalDuration;
            episodeModel.EpisodeStatusId = (int)pluginEpisode.EpisodeStatus;
            episodeModel.DateUpdated = pluginEpisode.DateUpdatedUTC;
            episodeModel.DateInserted = pluginEpisode.DateInsertedUTC;
            episodeModel.LastMergeId = pluginEpisode.LastMergeId;
            episodeModel.LastMergeTime = pluginEpisode.LastMergeTime;
            episodeModel.EpisodeGuid = pluginEpisode.EpisodeGuid;

            var calculatedIntervalsList = pluginEpisode.IntervalsCalculator.CalculatedIntervalsList;
            foreach (var calculatedInterval in calculatedIntervalsList)
            {
                
                var propertiesInfos = calculatedInterval.GetType().GetProperties();

                var conceptsMapping = PluginDBManager.Instance.ConceptsIndex;
                foreach (var conceptMapping in conceptsMapping)
                {
                    PropertyInfo propertyInfo = propertiesInfos.FirstOrDefault(p => p.Name.Equals(conceptMapping.ColumnName));

                    object value = null;
                    if (propertyInfo != null)
                    {
                        value = propertyInfo.GetValue(calculatedInterval, null);

                        ArtifactCountersModel artifactCountersModel = new ArtifactCountersModel();
                        artifactCountersModel.IntervalId = calculatedInterval.IntervalID;
                        artifactCountersModel.SampleFromDate = calculatedInterval.StartTime;
                        artifactCountersModel.SampleToDate = calculatedInterval.EndTime;
                        artifactCountersModel.IntervalDuration = calculatedInterval.IntervalDuration;
                        artifactCountersModel.ConceptNumber = conceptMapping.ConceptNumber;
                        artifactCountersModel.ObjectOfCare = conceptMapping.ObjectOfCare;

                        artifactCountersModel.ConceptValue = value == null ? String.Empty : value.ToString();

                        artifactCountersModel.DateInserted = DateTime.UtcNow;
                        artifactCountersModel.IsNotApplicable = value == null ? true : value.Equals(-1);
                        artifactCountersModel.IsDirty = dirtyIntervalIds.Contains(calculatedInterval.IntervalID);

                        episodeModel.ArtifactCountersList.Add(artifactCountersModel);
                    }
    
                }
            }

            foreach (var exportedInterval in pluginEpisode.ExportedIntervals)
            {
                foreach (var concept in exportedInterval.Concepts)
                {
                    ArtifactCountersExportedModel artifactCountersExportedModel = new ArtifactCountersExportedModel();
                    artifactCountersExportedModel.ExportId = exportedInterval.ExportId;
                    artifactCountersExportedModel.IntervalId = exportedInterval.IntervalId;                    
                    artifactCountersExportedModel.SampleFromDate = exportedInterval.StartTime;
                    artifactCountersExportedModel.SampleToDate = exportedInterval.EndTime;
                    artifactCountersExportedModel.IntervalDuration = exportedInterval.IntervalDuration;
                    artifactCountersExportedModel.ConceptNumber = concept.Id;
                    artifactCountersExportedModel.ObjectOfCare = concept.OOC;

                    artifactCountersExportedModel.ConceptValue = concept.Value != null ? concept.Value.ToString() : String.Empty;
                    artifactCountersExportedModel.CalulatedValue = concept.OriginalValue != null ? concept.OriginalValue.ToString() : String.Empty;
                                                            
                    artifactCountersExportedModel.LoginName = exportedInterval.LoginName;
                    artifactCountersExportedModel.ExportedDate = DateTime.UtcNow;
                    artifactCountersExportedModel.DateInserted = DateTime.UtcNow;
                    artifactCountersExportedModel.IsStikeOut = concept.Value == null && concept.OriginalValue != null;
                    artifactCountersExportedModel.IsDirty = (exportedInterval.ExportId > pluginEpisode.LastSavedExportId);

                    episodeModel.ArtifactCountersExportedList.Add(artifactCountersExportedModel);
                }
            }

            return episodeModel;
        }

        public IEnumerable<PluginEpisode> CreatePluginEpisodes(IEnumerable<EpisodeModel> episodesModels)
        {
            List<PluginEpisode> episodes = new List<PluginEpisode>();

            foreach (var episodeModel in episodesModels)
            {
                PluginEpisode episode = CreatePluginEpisode(episodeModel);
                episodes.Add(episode);
            }

            return episodes;
        }

        public PluginEpisode CreatePluginEpisode(EpisodeModel episodeModel)
        {
            PluginEpisode pluginEpisode = new PluginEpisode(episodeModel.EpisodeId);
            pluginEpisode.VisitKey = episodeModel.VisitKey;
            pluginEpisode.EpisodeStatus = (EpisodeStatus)episodeModel.EpisodeStatusId;
            pluginEpisode.DateUpdatedUTC = episodeModel.DateUpdated;
            pluginEpisode.DateInsertedUTC = episodeModel.DateInserted;
            pluginEpisode.EpisodeGuid = episodeModel.EpisodeGuid;
            pluginEpisode.IntervalsCalculator.SetIntervalDuration(episodeModel.IntervalDuration);
            pluginEpisode.LastMergeId = episodeModel.LastMergeId;
            pluginEpisode.LastMergeTime = episodeModel.LastMergeTime;

            var artifactCountersOfIntervals = episodeModel.ArtifactCountersList.GroupBy(a => a.IntervalId);
            foreach (var artifactCountersOfInterval in artifactCountersOfIntervals)
            {
                CalculatedInterval calculatedInterval = new CalculatedInterval(artifactCountersOfInterval.First().IntervalDuration);
                calculatedInterval.IntervalID = artifactCountersOfInterval.First().IntervalId;
                calculatedInterval.StartTime = artifactCountersOfInterval.First().SampleFromDate;
                calculatedInterval.EndTime = artifactCountersOfInterval.First().SampleToDate;

                var conceptsMapping = PluginDBManager.Instance.ConceptsIndex;
                var propertiesInfos = calculatedInterval.GetType().GetProperties();
                foreach (var propertyInfo in propertiesInfos)
                {
                    var conceptMapping = conceptsMapping.FirstOrDefault(c => c.ColumnName.Equals(propertyInfo.Name));
                    if (conceptMapping != null)
                    {
                        var artifactCounterModel = artifactCountersOfInterval.FirstOrDefault(c => c.ConceptNumber == conceptMapping.ConceptNumber);
                        if (artifactCounterModel != null)
                        {
                            object valueAsObject = Convert.ChangeType(artifactCounterModel.ConceptValue, propertyInfo.PropertyType);
                            propertyInfo.SetValue(calculatedInterval, valueAsObject, null);
                        }
                    }
                }

                var ArtifactCountersOfExportedInterval = episodeModel.ArtifactCountersExportedList.Where(e => e.IntervalId == calculatedInterval.IntervalID);
                if (ArtifactCountersOfExportedInterval != null && ArtifactCountersOfExportedInterval.Count() > 0)
                {
                    calculatedInterval.ExportID = ArtifactCountersOfExportedInterval.Max(e => e.ExportId);
                }

                pluginEpisode.IntervalsCalculator.CalculatedIntervalsList.Add(calculatedInterval);
            }

            var exportedArtifactCountersOfIntervals = episodeModel.ArtifactCountersExportedList.GroupBy(a => a.ExportId);
            foreach (var exportedArtifactCountersOfInterval in exportedArtifactCountersOfIntervals)
            {
                Interval exportedInterval = new Interval();
                exportedInterval.IntervalId = exportedArtifactCountersOfInterval.First().IntervalId;
                exportedInterval.ExportId = exportedArtifactCountersOfInterval.First().ExportId;
                exportedInterval.StartTime = exportedArtifactCountersOfInterval.First().SampleFromDate;
                exportedInterval.EndTime = exportedArtifactCountersOfInterval.First().SampleToDate;
                exportedInterval.IntervalDuration = exportedArtifactCountersOfInterval.First().IntervalDuration;
                exportedInterval.LoginName = exportedArtifactCountersOfInterval.First().LoginName;

                List<ConceptInfo> conceptsInfos = ExportManager.Instance.ConceptsInfos;
                foreach (var conceptInfo in conceptsInfos)
                {
                    string value = null;
                    string originalValue = null;
                    var exportedArtifactCounter = exportedArtifactCountersOfInterval.FirstOrDefault(a => a.ConceptNumber == conceptInfo.ConceptNo);
                    if (exportedArtifactCounter != null && !exportedArtifactCounter.IsStikeOut)
                    {
                        value = exportedArtifactCounter.ConceptValue;
                        originalValue = exportedArtifactCounter.CalulatedValue;
                    }

                    Entities.BaseConcept concept = CreateConceptFromModel(conceptInfo, value, originalValue);
                    exportedInterval.Concepts.Add(concept);
                }

                pluginEpisode.ExportedIntervals.Add(exportedInterval);

            }

            if (pluginEpisode.IntervalsCalculator.CalculatedIntervalsList.Count > 0)
            {
                pluginEpisode.LastSavedIntervalId = pluginEpisode.IntervalsCalculator.CalculatedIntervalsList.Max(c => c.IntervalID);
                pluginEpisode.LastSavedExportId = pluginEpisode.IntervalsCalculator.CalculatedIntervalsList.Max(c => c.ExportID);
                pluginEpisode.IntervalsCalculator.IntervalId = pluginEpisode.IntervalsCalculator.CalculatedIntervalsList.Max(c => c.IntervalID) + 1;
            }

            return pluginEpisode;
        }

        private Entities.BaseConcept CreateConceptFromModel(ConceptInfo conceptInfo, object value, object originalValue)
        {
            Entities.BaseConcept concept = null;

            if (PluginSettings.Instance.CALMServicesEnabled)
            {
                ExportEntity entity = ExportManager.Instance.ExportDataConfig.GetEntityById(conceptInfo.ConceptNo);
                if (entity == null)
                {
                    return null;
                }

                var conceptMapping = PluginDBManager.Instance.ConceptsIndex.FirstOrDefault(a => a.ConceptNumber == conceptInfo.ConceptNo);
                if (conceptMapping == null)
                {
                    return null;
                }

                switch (entity.ControlType)
                {
                    case ExportControlTypes.Int:
                    case ExportControlTypes.Rounding:
                    case ExportControlTypes.CheckBox:
                        concept = CreateIntConcept(conceptInfo, value, originalValue);
                        break;
                    case ExportControlTypes.Double:
                        concept = CreateDoubleConcept(conceptInfo, value, originalValue);
                        break;
                    case ExportControlTypes.String:
                    case ExportControlTypes.Range:
                    case ExportControlTypes.RangeDoubleConcept:
                        concept = CreateStringConcept(conceptInfo, value, originalValue);
                        break;
                    case ExportControlTypes.Combo:
                        concept = CreateComboConcept(conceptInfo, value, originalValue);
                        break;
                    case ExportControlTypes.ComboMultiValue:
                        concept = CreateComboMultiValueConcept(conceptInfo, value, originalValue);
                        break;
                    case ExportControlTypes.CalculatedCombo:
                        concept = CreateCalculatedComboConcept(conceptInfo, value, originalValue);
                        break;
                    case ExportControlTypes.CalculatedCheckboxGroup:
                        concept = CreateCalculatedCheckboxGroup(conceptInfo, value, originalValue, null);
                        break;

                    default:
                        break;
                }
                if (concept != null)
                {
                    concept.Id = conceptMapping.ConceptNumber;
                    concept.OOC = conceptMapping.ObjectOfCare;
                    concept.Name = conceptMapping.ColumnName;
                    concept.OrderNumber = conceptInfo.OrderNumberInGroup;
                }
            }
            else
            {
                var conceptMapping = PluginDBManager.Instance.ConceptsIndex.FirstOrDefault(a => a.ConceptNumber == conceptInfo.ConceptNo);
                if (conceptMapping == null)
                {
                    return null;
                }

                switch (conceptMapping.ConceptType)
                {
                    case "Int":
                        concept = CreateIntConcept(conceptInfo, value, originalValue);
                        break;
                    case "Double":
                        concept = CreateDoubleConcept(conceptInfo, value, originalValue);
                        break;
                    case "String":
                        concept = CreateStringConcept(conceptInfo, value, originalValue);
                        break;
                    case "Combo":
                        concept = CreateComboConcept(conceptInfo, value, originalValue);
                        break;
                    case "CalculatedCombo":
                        concept = CreateCalculatedComboConcept(conceptInfo, value, originalValue);
                        break;
                    case "CalculatedCheckboxGroup":
                        concept = CreateCalculatedCheckboxGroup(conceptInfo, value, originalValue, null);
                        break;

                    default:
                        break;
                }
                if (concept != null)
                {
                    concept.Id = conceptMapping.ConceptNumber;
                    concept.OOC = conceptMapping.ObjectOfCare;
                    concept.Name = conceptMapping.ColumnName;
                    concept.OrderNumber = conceptInfo.OrderNumberInGroup;
                }
            }


            return concept;
        }

        private Entities.BaseConcept CreateConceptFromCalculatedInterval(ConceptInfo conceptInfo, CalculatedInterval calculatedInterval)
        {
            Entities.BaseConcept concept = null;
            if (PluginSettings.Instance.CALMServicesEnabled)
            {
                ExportEntity entity = ExportManager.Instance.ExportDataConfig.GetEntityById(conceptInfo.ConceptNo);
                if (entity == null)
                {
                    return null;
                }

                var conceptMapping = PluginDBManager.Instance.ConceptsIndex.FirstOrDefault(a => a.ConceptNumber == conceptInfo.ConceptNo);
                if (conceptMapping == null)
                {
                    return null;
                }

                var propertiesInfos = calculatedInterval.GetType().GetProperties();
                PropertyInfo propertyInfo = propertiesInfos.FirstOrDefault(p => p.Name.Equals(conceptMapping.ColumnName));

                object value = null;
                if (propertyInfo != null)
                {
                    value = propertyInfo.GetValue(calculatedInterval, null);
                    if (value != null && value.Equals(-1))
                    {
                        value = null;
                    }
                }

                switch (entity.ControlType)
                {
                    case ExportControlTypes.Int:
                    case ExportControlTypes.Rounding:
                    case ExportControlTypes.CheckBox:
                        concept = CreateIntConcept(conceptInfo, value, value);
                        break;
                    case ExportControlTypes.Double:
                        concept = CreateDoubleConcept(conceptInfo, value, value);
                        break;
                    case ExportControlTypes.String:
                    case ExportControlTypes.Range:
                    case ExportControlTypes.RangeDoubleConcept:
                        concept = CreateStringConcept(conceptInfo, value, value);
                        break;
                    case ExportControlTypes.Combo:
                        concept = CreateComboConcept(conceptInfo, value, value);
                        break;
                    case ExportControlTypes.ComboMultiValue:
                        concept = CreateComboMultiValueConcept(conceptInfo, value, value);
                        break;
                    case ExportControlTypes.CalculatedCombo:
                        concept = CreateCalculatedComboConcept(conceptInfo, value, value);
                        break;
                    case ExportControlTypes.CalculatedCheckboxGroup:
                        concept = CreateCalculatedCheckboxGroup(conceptInfo, value, value, calculatedInterval);
                        break;

                    default:
                        break;
                }
            }
            else
            {
                var conceptMapping = PluginDBManager.Instance.ConceptsIndex.FirstOrDefault(a => a.ConceptNumber == conceptInfo.ConceptNo);
                if (conceptMapping == null)
                {
                    return null;
                }

                var propertiesInfos = calculatedInterval.GetType().GetProperties();
                PropertyInfo propertyInfo = propertiesInfos.FirstOrDefault(p => p.Name.Equals(conceptMapping.ColumnName));

                object value = null;
                if (propertyInfo != null)
                {
                    value = propertyInfo.GetValue(calculatedInterval, null);
                    if (value != null && value.Equals(-1))
                    {
                        value = null;
                    }
                }

                switch (conceptMapping.ConceptType)
                {
                    case "Int":
                        concept = CreateIntConcept(conceptInfo, value, value);
                        break;
                    case "Double":
                        concept = CreateDoubleConcept(conceptInfo, value, value);
                        break;
                    case "String":
                        concept = CreateStringConcept(conceptInfo, value, value);
                        break;
                    case "Combo":
                        concept = CreateComboConcept(conceptInfo, value, value);
                        break;
                    case "CalculatedCombo":
                        concept = CreateCalculatedComboConcept(conceptInfo, value, value);
                        break;
                    case "CalculatedCheckboxGroup":
                        concept = CreateCalculatedCheckboxGroup(conceptInfo, value, value, calculatedInterval);
                        break;

                    default:
                        break;
                }

            }

            if (concept != null)
            {
                concept.Id = conceptInfo.ConceptNo;
                concept.OOC = conceptInfo.ObjectOFCare;
                concept.Name = conceptInfo.Caption;
                concept.OrderNumber = conceptInfo.OrderNumberInGroup;
            }

            return concept;
        }

        private Entities.IntConcept CreateIntConcept(ConceptInfo conceptInfo, object value, object originalValue)
        {
            Entities.IntConcept concept = new Entities.IntConcept();

            concept.Min = conceptInfo.MinValue.HasValue ? conceptInfo.MinValue.Value : 0;
            concept.Max = conceptInfo.MaxValue.HasValue ? conceptInfo.MaxValue.Value : Int32.MaxValue;            

            concept.Value = value;
            concept.OriginalValue = originalValue;

            return concept;
        }

        private Entities.DoubleConcept CreateDoubleConcept(ConceptInfo conceptInfo, object value, object originalValue)
        {
            Entities.DoubleConcept concept = new Entities.DoubleConcept();

            concept.Min = conceptInfo.MinValue.HasValue ? conceptInfo.MinValue.Value : 0;
            concept.Max = conceptInfo.MaxValue.HasValue ? conceptInfo.MaxValue.Value : Double.MaxValue;            
            
            if (value != null && value.ToString() != String.Empty)
            {
                double doubleValue = Convert.ToDouble(value);
                doubleValue = Math.Round(doubleValue, conceptInfo.DecimalPlaces);
                concept.Value = doubleValue;
            }
            else
            {
                concept.Value = String.Empty;
            }

            if (originalValue != null && originalValue.ToString() != String.Empty)
            {
                double doubleOriginalValue = Convert.ToDouble(originalValue);
                doubleOriginalValue = Math.Round(doubleOriginalValue, conceptInfo.DecimalPlaces);
                concept.OriginalValue = doubleOriginalValue;
            }
            else
            {
                concept.OriginalValue = String.Empty;
            }

            return concept;
        }

        private Entities.StringConcept CreateStringConcept(ConceptInfo conceptInfo, object value, object originalValue)
        {
            Entities.StringConcept concept = new Entities.StringConcept();

            concept.Value = value;
            concept.OriginalValue = originalValue;

            return concept;
        }

        private Entities.ComboConcept CreateComboConcept(ConceptInfo conceptInfo, object value, object originalValue)
        {
            Entities.ComboConcept concept = new Entities.ComboConcept();

            concept.Value = value;
            concept.OriginalValue = originalValue;

            concept.Items = conceptInfo.ResponseList;

            return concept;
        }

        private Entities.ComboMultiValueConcept CreateComboMultiValueConcept(ConceptInfo conceptInfo, object value, object originalValue)
        {
            Entities.ComboMultiValueConcept concept = new Entities.ComboMultiValueConcept();

            concept.Value = value;
            concept.OriginalValue = originalValue;

            foreach (var subConceptInfo in conceptInfo.SubConcepts)
            {
                Entities.ComboMultiValueConceptItem item = new Entities.ComboMultiValueConceptItem();
                item.Id = subConceptInfo.ConceptNo;
                item.Value = subConceptInfo.Caption;
                item.Min = subConceptInfo.MinValue.HasValue ? subConceptInfo.MinValue.Value : 0;
                item.Max = subConceptInfo.MaxValue.HasValue ? subConceptInfo.MaxValue.Value : Double.MaxValue;
                item.OrderNumber = subConceptInfo.OrderNumberInGroup;

                concept.Items.Add(item);
            }

            return concept;

        }


        private Entities.CalculatedComboConcept CreateCalculatedComboConcept(ConceptInfo conceptInfo, object value, object originalValue)
        {
            Entities.CalculatedComboConcept concept = new Entities.CalculatedComboConcept();

            concept.Value = value;
            concept.OriginalValue = originalValue;

            foreach (var subConceptInfo in conceptInfo.SubConcepts)
            {
                Entities.CalculatedComboConceptItem item = new Entities.CalculatedComboConceptItem();
                item.Id = subConceptInfo.ConceptNo;
                item.Value = subConceptInfo.Caption;
                item.Min = subConceptInfo.MinValue.HasValue ? subConceptInfo.MinValue.Value : 0;
                item.Max = subConceptInfo.MaxValue.HasValue ? subConceptInfo.MaxValue.Value : Double.MaxValue;                
                item.OrderNumber = subConceptInfo.OrderNumberInGroup;

                concept.Items.Add(item);
            }

            return concept;
        }

        private Entities.CalculatedCheckboxGroupConcept CreateCalculatedCheckboxGroup(ConceptInfo conceptInfo, object value, object originalValue, CalculatedInterval calculatedInterval)
        {
            Entities.CalculatedCheckboxGroupConcept concept = new Entities.CalculatedCheckboxGroupConcept();
            concept.Value = value;
            concept.OriginalValue = originalValue;

            if (calculatedInterval != null)
            {
                foreach (var subConceptInfo in conceptInfo.SubConcepts)
                {
                    Entities.CalculatedCheckboxGroupConceptItem item = new Entities.CalculatedCheckboxGroupConceptItem();
                    item.Id = subConceptInfo.ConceptNo;
                    item.Value = subConceptInfo.Caption;
                    item.OrderNumber = subConceptInfo.OrderNumberInGroup;
                    item.IsGrouping = subConceptInfo.MaxValue == -1;

                    var subConceptMapping = PluginDBManager.Instance.ConceptsIndex.FirstOrDefault(a => a.ConceptNumber == subConceptInfo.ConceptNo);
                    if (subConceptMapping != null)
                    {
                        var propertiesInfos = calculatedInterval.GetType().GetProperties();
                        PropertyInfo propertyInfo = propertiesInfos.FirstOrDefault(p => p.Name.Equals(subConceptMapping.ColumnName));

                        object calculatedValue = null;
                        if (propertyInfo != null)
                        {
                            calculatedValue = propertyInfo.GetValue(calculatedInterval, null);
                            item.CalculatedValue = calculatedValue.ToString();
                        }
                    }

                    if (subConceptInfo.ConceptType == ConceptType.SubConcept)
                    {
                        concept.Items.Add(item);
                    }
                }
            }

            return concept;
        }

    }
}
