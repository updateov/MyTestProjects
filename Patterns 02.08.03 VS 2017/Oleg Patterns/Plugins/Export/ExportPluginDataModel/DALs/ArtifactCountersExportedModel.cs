using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.PluginDataModel
{
    public class ArtifactCountersExportedModel
    {
        public int ExportId { get; set; }
        public int ObjectOfCare { get; set; }
        public int ConceptNumber { get; set; }
        public int IntervalId { get; set; }
        public DateTime SampleFromDate { get; set; }
        public DateTime SampleToDate { get; set; }
        public DateTime ExportedDate { get; set; }
        public int IntervalDuration { get; set; }
        public string ConceptValue { get; set; }
        public string CalulatedValue { get; set; }
        public bool IsStikeOut { get; set; }
        public string LoginName { get; set; }
        public string ContractionIntervalRange { get; set; }
        public string ContractionDurationRange { get; set; }
        public string ContractionIntensityRange { get; set; }
        public string OriginalContractionDurationRange { get; set; }
        public string OriginalContractionIntensityRange { get; set; }
        public bool IsDirty { get; set; }
        public DateTime DateInserted { get; set; }

        public ArtifactCountersExportedModel()
        {

        }

        public ArtifactCountersExportedModel(ArtifactCountersExportedModel model)
        {
            ExportId = model.ExportId;
            ObjectOfCare = model.ObjectOfCare;
            ConceptNumber = model.ConceptNumber;
            IntervalId = model.IntervalId;
            SampleFromDate = model.SampleFromDate;
            SampleToDate = model.SampleToDate;
            SampleToDate = model.SampleToDate;
            ExportedDate = model.ExportedDate;
            IntervalDuration = model.IntervalDuration;
            ConceptValue = model.ConceptValue;
            CalulatedValue = model.CalulatedValue;
            IsStikeOut = model.IsStikeOut;
            LoginName = model.LoginName;
            IsDirty = model.IsDirty;
            DateInserted = model.DateInserted;
        }
    }
}
