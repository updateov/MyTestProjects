using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.PluginDataModel
{
    public class EpisodeModel
    {
        public int EpisodeId { get; set; }
        public string VisitKey { get; set; }
        public int IntervalDuration { get; set; }
        public int EpisodeStatusId { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateInserted { get; set; }
        public int TotalNumOfCountersIntervals { get; set; }
        public int LastMergeId { get; set; }
        public DateTime LastMergeTime { get; set; }
        
        public Guid EpisodeGuid { get; set; } 
        public List<ArtifactCountersModel> ArtifactCountersList { get; set; }
        public List<ArtifactCountersExportedModel> ArtifactCountersExportedList { get; set; }

        public EpisodeModel()
        {
            ArtifactCountersList = new List<ArtifactCountersModel>();
            ArtifactCountersExportedList = new List<ArtifactCountersExportedModel>();
        }

        public EpisodeModel(EpisodeModel model)
        {
            EpisodeId = model.EpisodeId;
            VisitKey = model.VisitKey;
            IntervalDuration = model.IntervalDuration;
            EpisodeStatusId = model.EpisodeStatusId;
            TotalNumOfCountersIntervals = model.TotalNumOfCountersIntervals;
            LastMergeId = model.LastMergeId;
            LastMergeTime = model.LastMergeTime;
            EpisodeGuid = model.EpisodeGuid;
            DateUpdated = model.DateUpdated;
            DateInserted = model.DateInserted;
            ArtifactCountersList = model.ArtifactCountersList;
            ArtifactCountersExportedList = model.ArtifactCountersExportedList;
        }
    }
}
