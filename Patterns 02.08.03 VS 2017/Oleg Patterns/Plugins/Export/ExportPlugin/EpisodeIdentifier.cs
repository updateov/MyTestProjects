using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Export.Plugin
{
    public class EpisodeIdentifier
    {
        public int EpisodeId { get; set; }
        public String VisitKey { get; set; }
        public int LastIntervalId { get; set; }
        public DateTime LastIntervalTime { get; set; }

        public EpisodeIdentifier(int episodeId, String visitKey, int lastIntervalId, DateTime lastIntervalTime)
        {
            EpisodeId = episodeId;
            VisitKey = visitKey;
            LastIntervalId = lastIntervalId;
            LastIntervalTime = lastIntervalTime;
        }
    }
}
