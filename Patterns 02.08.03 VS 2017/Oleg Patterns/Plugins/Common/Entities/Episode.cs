using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace PatternsEntities
{


    public abstract class Episode
    {
        public int EpisodeId { get; private set; }
        public string VisitKey { get; set; }

        public int Tracing { get; set; }
        public int Artifact { get; set; }
        public int Action { get; set; }
        public bool IsIncremental { get; set; }
        public string Serveruid { get; set; }
        public int LastMergeId { get; set; }
        public DateTime LastMergeTime { get; set; }

        public TracingStatus TracingStatus { get; set; }
        public EpisodeStatus EpisodeStatus { get; set; }        
        public bool IsAfterDownTime { get; set; }

        public DateTime DateUpdatedUTC { get; set; }
        public DateTime DateInsertedUTC { get; set; }
        public Guid EpisodeGuid { get; set; }

        public Episode(int episodeID)
        {
            EpisodeId = episodeID;
            VisitKey = String.Empty;

            Tracing = -1;
            Artifact = -1;
            Action = -1;
            IsIncremental = false;
            Serveruid = String.Empty;

            LastMergeId = -1;
            LastMergeTime = DateTime.MinValue;
            

            TracingStatus = TracingStatus.Error;
            EpisodeStatus = EpisodeStatus.Unknown;
            IsAfterDownTime = true;

            DateUpdatedUTC = DateTime.MinValue;
            DateInsertedUTC = DateTime.MinValue;
            EpisodeGuid = Guid.NewGuid();
        }

        public void Reset()
        {
            Tracing = -1;
            Artifact = -1;
            Action = -1;
            IsIncremental = false;            

            TracingStatus = TracingStatus.Error;
            EpisodeStatus = EpisodeStatus.Unknown;
            IsAfterDownTime = true;

            DateUpdatedUTC = DateTime.MinValue;
            DateInsertedUTC = DateTime.MinValue;
        }
    }
}
