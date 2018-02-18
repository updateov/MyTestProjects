using CRIEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CRIPluginDataModel
{
    public interface IDBManager
    {
        bool LoadAll(out List<EpisodeModel> episodes);

        bool SaveAll(List<EpisodeModel> episodes);

        /// <summary>
        /// Dischargeds the episodes.
        /// Save the episode data in the ArchiveEpisodeDB and delete the episode data from EpisodeDB
        /// </summary>
        /// <param name="episodes">The episodes.</param>
        /// <returns></returns>
        bool DischargeEpisodes(List<EpisodeModel> episodes);

        bool CreateDB();

        bool LastPersistenceId(out int persistenceId );

    }
}
