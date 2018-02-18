using PatternsEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Export.PluginDataModel
{
    public interface IDBManager
    {
        bool LoadItems(out List<EpisodeModel> episodes);

        bool SaveItems(List<EpisodeModel> episodes);

        bool SaveIntervalDuration(int episodeId, int IntervalDuration);

        bool DischargedEpisodes(EpisodeModel episode);

        bool GetConceptNumberToColumnMapping(ref List<ConceptNumberToColumnMappingModel> concepts);

        bool CreateDB();


    }
}
