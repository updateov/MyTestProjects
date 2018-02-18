using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.PluginDataModel
{
    public class ArtifactCountersModel
    {
        public int IntervalId { get; set; }
        public int ObjectOfCare { get; set; }
        public int ConceptNumber { get; set; }
        public DateTime SampleFromDate { get; set; }
        public DateTime SampleToDate { get; set; }
        public int IntervalDuration { get; set; }
        public string ConceptValue { get; set; }
        public bool IsNotApplicable { get; set; }
        public bool IsDirty { get; set; }
        public DateTime DateInserted { get; set; }

        public ArtifactCountersModel()
        {

        }

        public ArtifactCountersModel(ArtifactCountersModel model)
        {
            IntervalId = model.IntervalId;
            ObjectOfCare = model.ObjectOfCare;
            ConceptNumber = model.ConceptNumber;
            SampleFromDate = model.SampleFromDate;
            SampleToDate = model.SampleToDate;
            IntervalDuration = model.IntervalDuration;
            ConceptValue = model.ConceptValue;
            IsNotApplicable = model.IsNotApplicable;
            IsDirty = model.IsDirty;
            DateInserted = model.DateInserted;
            
        }
    }
}
