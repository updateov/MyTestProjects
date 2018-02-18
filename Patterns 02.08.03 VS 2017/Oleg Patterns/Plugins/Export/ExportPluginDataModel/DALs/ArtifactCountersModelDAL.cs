using PatternsEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Export.PluginDataModel.DALs
{
    public static class ArtifactCountersModelDAL
    {


        #region SaveItems
        /// <summary>
        /// Adds the or update the Contractilities data, this is used both for EpisdoeDB and EpisodeArchiveDB.
        /// </summary>
        /// <param name="sqliteConnection">The sqlite connection.</param>
        /// <param name="episode">The episode.</param>
        /// <returns></returns>
        public static void SaveItems(SQLiteConnection sqliteConnection, EpisodeModel episode, bool onlyDirtyItems = true)
        {
            #region SaveSQL query STR
            string SaveSQL = @"INSERT OR REPLACE  into ArtifactCounters 
                                (
                                    EpisodeId,
	                                IntervalId,
                                    ObjectOfCare,
                                    ConceptNumber,
                                    SampleFromDate,
                                    SampleToDate,
                                    IntervalDuration,
                                    ConceptValue,
                                    IsNotApplicable,
                                    DateInserted
                                )";
            string ValueSQL = @"  SELECT 
                                    '{0}'  AS EpisodeId,
                                    '{1}'  AS IntervalId,
                                    '{2}'  AS ObjectOfCare,
                                    '{3}'  AS ConceptNumber,
	                                '{4}'  AS SampleFromDate,
	                                '{5}'  AS SampleToDate,
	                                '{6}'  AS IntervalDuration,
                                     {7}  AS ConceptValue, --since it's nullable, we need to add the appostraphy only if it's not null
                                    '{8}'  AS IsNotApplicable,
                                    '{9}'  AS DateInserted
                                  ";
            #endregion SaveSQL query STR
            string resultsToExecute = null;
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
                {
                    var results = from item in episode.ArtifactCountersList
                                  where (item.IsDirty == true && onlyDirtyItems == true) || onlyDirtyItems == false
                                  select (string.Format(ValueSQL,
                                                           episode.EpisodeId,
                                                           item.IntervalId,
                                                           item.ObjectOfCare,
                                                           item.ConceptNumber,
                                                           item.SampleFromDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.SampleToDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.IntervalDuration,
                                                           (item.ConceptValue == null) ? "NULL" : "'" + item.ConceptValue + "'",
                                                           Convert.ToInt32(item.IsNotApplicable),
                                                           ((item.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : item.DateInserted).ToString("yyyy-MM-dd HH:mm:ss")
                                                        )
                                           );

                    while (results.Count() > 400)
                    {
                        resultsToExecute = SaveSQL + String.Join(" UNION ALL ", results.Take(400));
                        cmd.CommandText = resultsToExecute;
                        cmd.ExecuteNonQuery();
                        results = results.Skip(400);
                    }

                    if (results.Count() > 0)
                    {
                        resultsToExecute = SaveSQL + String.Join(" UNION ALL ", results);
                        cmd.CommandText = resultsToExecute;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Error Saving to SQLite{0}; Query:{1}", ex, resultsToExecute));
            }
        }
        #endregion SaveItems

        #region LoadItems
        public static void LoadItems(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                cmd.CommandText = string.Format("SELECT * FROM ArtifactCounters where EpisodeId = {0} order by SampleFromDate", episode.EpisodeId);

                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        ArtifactCountersModel artifactCountersModel  = new ArtifactCountersModel()
                        {
                            
                            IntervalId = rdr.GetInt32(rdr.GetOrdinal("IntervalId")),
                            ObjectOfCare = rdr.GetInt32(rdr.GetOrdinal("ObjectOfCare")),
                            ConceptNumber = rdr.GetInt32(rdr.GetOrdinal("ConceptNumber")),
                            SampleFromDate = rdr.GetDateTime(rdr.GetOrdinal("SampleFromDate")),
                            SampleToDate = rdr.GetDateTime(rdr.GetOrdinal("SampleToDate")),
                            IntervalDuration = rdr.GetInt32(rdr.GetOrdinal("IntervalDuration")),
                            ConceptValue = rdr.IsDBNull(rdr.GetOrdinal("ConceptValue")) ? null : rdr["ConceptValue"].ToString(),
                            IsNotApplicable = rdr.GetBoolean(rdr.GetOrdinal("IsNotApplicable")),
                            DateInserted = rdr.GetDateTime(rdr.GetOrdinal("DateInserted")),
                            IsDirty = false
                        };

                        episode.ArtifactCountersList.Add(artifactCountersModel);
                    }
                }
            }
        }
        #endregion LoadItems

        #region ArchiveItems
        public static bool ArchiveItems(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            //Task 4299: No need to save history for ArtifactCounters
            //return SaveItems(sqliteConnection, episode, false);
            return true;
        }
        #endregion ArchiveItems

        #region DeleteItems
        public static void DeleteItems(SQLiteConnection sqliteConnection, int episodeId)
        {

            using (SQLiteCommand sqliteCommand = new SQLiteCommand(sqliteConnection))
            {
                sqliteCommand.CommandText = string.Format("delete from ArtifactCounters where EpisodeId = {0}", episodeId);
                sqliteCommand.ExecuteNonQuery();
            }

        }
        #endregion DeleteItems

    }
}
