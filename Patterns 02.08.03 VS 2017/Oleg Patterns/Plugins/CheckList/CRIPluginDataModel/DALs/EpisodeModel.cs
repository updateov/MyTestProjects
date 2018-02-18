using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIPluginDataModel
{
    public class EpisodeModel
    {
        public int EpisodeId { get; set; }
        public string EpisodeKey { get; set; }
        public string GA { get; set; }
        public int Fetuses { get; set; }        
        public int StatusId { get; set;}
        public int Tracing { get; set; }
        public int Artifact { get; set; }
        public int Contractility { get; set; }
        public int Action { get; set; }
        public int Incremental { get; set; }
        public string Serveruid { get; set; }
        public int LastMergeId { get; set; }
        public DateTime LastMergeTime { get; set; }

        public Guid EpisodeGuid { get; set; }
        public DateTime DateUpdated { get; set; }
        public DateTime DateInserted { get; set; }

        public List<ContractilityModel> Contractilities { get; set; }
        public List<PersistenceStateModel> PersistencieStates { get; set; }

        public EpisodeModel()
        {
            Contractilities = new List<ContractilityModel>();
            PersistencieStates = new List<PersistenceStateModel>();
        }

        public EpisodeModel(EpisodeModel model)
        {
            EpisodeId = model.EpisodeId;
            EpisodeKey = model.EpisodeKey;
            GA = model.GA;
            Fetuses = model.Fetuses;
            StatusId = model.StatusId;
            Tracing = model.Tracing;
            Artifact = model.Artifact;
            Contractility = model.Contractility;
            Action = model.Action;
            Incremental = model.Incremental;
            Serveruid = model.Serveruid;
            LastMergeId = model.LastMergeId;
            LastMergeTime = model.LastMergeTime;
            EpisodeGuid = model.EpisodeGuid;

            DateUpdated = model.DateUpdated;
            DateInserted = model.DateInserted;

            Contractilities = model.Contractilities;
            PersistencieStates = model.PersistencieStates;
        }
    }
}
