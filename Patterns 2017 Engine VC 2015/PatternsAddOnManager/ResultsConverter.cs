using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeriGen.Patterns.Engine.Data;
using System.Diagnostics;
using CommonLogger;

namespace PatternsAddOnManager
{
    /// <summary>
    /// This class is an adapter between Patterns engine output
    /// and WCF API.
    /// </summary>
    public class ResultsConverter
    {
        public ResultsConverter()
        {
        }

        public List<Artifact> Convert(PatternsSessionData Session)
        {
            lock (Session.LockObject)
            {
                Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Results Converter, Convert", "Result size is:" + Session.Results.Count.ToString());
                Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Results Converter, Convert", "Session Last Detected Artifact - " + Session.LastDetectedArtifact.ToString());
                var toRet = new List<Artifact>();
                var results = from c in Session.Results
                              //where c.StartTime != Session.LastDetectedArtifact // - Oleg: No Chrono fix 
                              select c;

                Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Results Converter, Convert", "Answering criteria results - " + results.Count().ToString());
                if (results.Count() <= 0)
                {
                    Logger.WriteLogEntry(TraceEventType.Information, "Patterns Add On Manager, Results Converter, Convert", "No artifacts to convert");
                    return toRet;
                }

                foreach (var item in results)
                {
                    var itemToAdd = new Artifact() { StartTime = item.StartTime.ToLocalTime(), EndTime = item.EndTime.ToLocalTime() };
                    Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Results Converter, Convert", "LastDetectedArtifact at " + Session.LastDetectedArtifact.ToString());
                    switch (item.Category)
                    {
                        case ArtifactCategories.Acceleration:
                            itemToAdd.ArtifactData = FillAccelerationData(item as PeriGen.Patterns.Engine.Data.Acceleration);
                            itemToAdd.Category = Resource.IDS_ACCELERATION;
                            Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Results Converter, Convert", "Acceleration, started at " + itemToAdd.StartTime.ToString());
                            break;
                        case ArtifactCategories.Baseline:
                            itemToAdd.ArtifactData = FillBaselineData(item as PeriGen.Patterns.Engine.Data.Baseline);
                            itemToAdd.Category = Resource.IDS_BASELINE;
                            Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Results Converter, Convert", "Baseline, started at " + itemToAdd.StartTime.ToString());
                            break;
                        case ArtifactCategories.Contraction:
                            itemToAdd.ArtifactData = FillContractionData(item as PeriGen.Patterns.Engine.Data.Contraction);
                            itemToAdd.Category = Resource.IDS_CONTRACTION;
                            Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Results Converter, Convert", "Contraction, started at " + itemToAdd.StartTime.ToString());
                            break;
                        case ArtifactCategories.Deceleration:
                            itemToAdd.ArtifactData = FillDecelerationData(item as PeriGen.Patterns.Engine.Data.Deceleration);
                            itemToAdd.Category = Resource.IDS_DECELERATION;
                            Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Results Converter, Convert", "Deceleration, started at " + itemToAdd.StartTime.ToString());
                            break;
                        default:
                            Logger.WriteLogEntry(TraceEventType.Verbose, "Patterns Add On Manager, Results Converter, Convert", "No Artifact Found Here!!!");
                            continue;
                    }

                    toRet.Add(itemToAdd);
                }

                return toRet;
            }
        }

        private ArtifactType FillAccelerationData(PeriGen.Patterns.Engine.Data.Acceleration item)
        {
            var toRet = new Acceleration() { Category = Resource.IDS_ACCELERATION };
            FillAcelerationFields(toRet, item);
            return toRet;
        }

        private void FillAcelerationFields(Acceleration toRet, PatternEvent item)
        {
                toRet.Id = item.Id;
                toRet.Confidence = item.Confidence;
                toRet.Height = item.Height;
                toRet.IsNonInterpretable = item.IsNonInterpretable;
                toRet.PeakTime = item.PeakTime.ToLocalTime();
                toRet.PeakValue = item.PeakValue;
                toRet.Repair = item.Repair;
        }

        private ArtifactType FillBaselineData(PeriGen.Patterns.Engine.Data.Baseline item)
        {
            var toRet = new Baseline()
            {
                Category = Resource.IDS_BASELINE,
                Id = item.Id,
                Y1 = item.Y1,
                Y2 = item.Y2,
                BaselineVariability = item.BaselineVariability
            };

            return toRet;
        }

        private ArtifactType FillContractionData(PeriGen.Patterns.Engine.Data.Contraction item)
        {
            var toRet = new Contraction()
            {
                Category = Resource.IDS_CONTRACTION,
                Id = item.Id,
                PeakTime = item.PeakTime.ToLocalTime()
            };

            return toRet;
        }

        private ArtifactType FillDecelerationData(PeriGen.Patterns.Engine.Data.Deceleration item)
        {
            var toRet = new Deceleration() { Category = Resource.IDS_DECELERATION };
            FillAcelerationFields(toRet, item);
            toRet.ContractionStart = item.ContractionStart;
            toRet.DecelerationCategory = DecelCategoryToString(item.DecelerationCategory);
            toRet.HasSixtiesNonReassuringFeature = item.HasSixtiesNonReassuringFeature;
            toRet.IsEarlyDeceleration = item.IsEarlyDeceleration;
            toRet.IsLateDeceleration = item.IsLateDeceleration;
            toRet.IsNonAssociatedDeceleration = item.IsNonAssociatedDeceleration;
            toRet.IsVariableDeceleration = item.IsVariableDeceleration;
            toRet.NonReassuringFeatures = NonReassuringToString(item.NonReassuringFeatures);

            return toRet;
        }

        private String DecelCategoryToString(DecelerationCategories decelerationCategory)
        {
            var toRet = Resource.IDS_DECEL_CAT_NONE;
            switch (decelerationCategory)
            {
                case DecelerationCategories.Early:
                    toRet = Resource.IDS_DECEL_CAT_EARLY;
                    break;
                case DecelerationCategories.Late:
                    toRet = Resource.IDS_DECEL_CAT_LATE;
                    break;
                case DecelerationCategories.NonAssociated:
                    toRet = Resource.IDS_DECEL_CAT_NON_ASSOC;
                    break;
                case DecelerationCategories.None:
                    toRet = Resource.IDS_DECEL_CAT_NONE;
                    break;
                case DecelerationCategories.Variable:
                    toRet = Resource.IDS_DECEL_CAT_VARIABLE;
                    break;
                default:
                    toRet = Resource.IDS_DECEL_CAT_NONE;
                    break;
            }

            return toRet;
        }

        private string NonReassuringToString(AtypicalCharacteristics atypicalCharacteristics)
        {
            var toRet = Resource.IDS_ATYP_CHAR_NONE;
            switch (atypicalCharacteristics)
            {
                case AtypicalCharacteristics.Biphasic:
                    toRet = Resource.IDS_ATYP_CHAR_BIPHASIC;
                    break;
                case AtypicalCharacteristics.LossRise:
                    toRet = Resource.IDS_ATYP_CHAR_LOSS_RISE;
                    break;
                case AtypicalCharacteristics.LossVariability:
                    toRet = Resource.IDS_ATYP_CHAR_LOSS_VAR;
                    break;
                case AtypicalCharacteristics.LowerBaseline:
                    toRet = Resource.IDS_ATYP_CHAR_LOWER_BL;
                    break;
                case AtypicalCharacteristics.None:
                    toRet = Resource.IDS_ATYP_CHAR_NONE;
                    break;
                case AtypicalCharacteristics.Prolonged:
                    toRet = Resource.IDS_ATYP_CHAR_PROLONGED;
                    break;
                case AtypicalCharacteristics.ProlongedSecondRise:
                    toRet = Resource.IDS_ATYP_CHAR_PROLONGED_SEC_RISE;
                    break;
                case AtypicalCharacteristics.Sixties:
                    toRet = Resource.IDS_ATYP_CHAR_SIXTIES;
                    break;
                case AtypicalCharacteristics.SlowReturn:
                    toRet = Resource.IDS_ATYP_CHAR_SLOW_RET;
                    break;
                default:
                    if ((atypicalCharacteristics & AtypicalCharacteristics.Prolonged) > 0)
                        toRet = Resource.IDS_ATYP_CHAR_PROLONGED;
                    else
                        toRet = Resource.IDS_ATYP_CHAR_NONE;
                    break;
            }

            return toRet;
        }
    }
}
