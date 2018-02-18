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
    public static class ArtifactCountersExportedModelDAL
    {

        #region SaveItems

        public static void SaveItems(SQLiteConnection sqliteConnection, EpisodeModel episode, bool onlyDirtyItems = true)
        {

            #region SaveSQL query STR
            string SaveSQL = @"INSERT OR REPLACE  into ArtifactCountersExported 
                                (
	                                EpisodeId, 
	                                ExportId,
                                    ObjectOfCare,
                                    ConceptNumber,
                                    IntervalId,
                                    SampleFromDate,
                                    SampleToDate,
                                    ExportedDate,
                                    IntervalDuration,
                                    ConceptValue,
                                    CalulatedValue,
                                    IsStikeOut,
                                    LoginName,
                                    DateInserted
                                )";
            string ValueSQL = @"  SELECT 
                                    '{0}'  AS EpisodeId,
                                    '{1}'  AS ExportId,
                                    '{2}'  AS ObjectOfCare,
                                    '{3}'  AS ConceptNumber,
                                    '{4}'  AS IntervalId,
	                                '{5}'  AS SampleFromDate,
	                                '{6}'  AS SampleToDate,
	                                '{7}'  AS ExportedDate,
	                                '{8}'  AS IntervalDuration,
                                    {9}  AS ConceptValue, --since it's nullable, we need to add the appostraphy only if it's not null
                                    {10}  AS CalulatedValue, --since it's nullable, we need to add the appostraphy only if it's not null
                                    '{11}' AS IsStikeOut,
                                    '{12}' AS LoginName,
                                    '{13}' AS DateInserted
                                  ";
            #endregion SaveSQL query STR
            string resultsToExecute = null;
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
                {
                    var results = from item in episode.ArtifactCountersExportedList
                                  where (item.IsDirty == true && onlyDirtyItems == true) || onlyDirtyItems == false
                                  select (string.Format(ValueSQL,
                                                           episode.EpisodeId,
                                                           item.ExportId,
                                                           item.ObjectOfCare,
                                                           item.ConceptNumber,
                                                           item.IntervalId,
                                                           item.SampleFromDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.SampleToDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.ExportedDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                                           item.IntervalDuration,
                                                           (item.ConceptValue == null) ? "NULL" : "'" + item.ConceptValue + "'",
                                                           (item.CalulatedValue == null) ? "NULL" : "'" + item.CalulatedValue + "'",
                                                           Convert.ToInt32(item.IsStikeOut),
                                                           item.LoginName,
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
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error Saving to SQLite{0}; Query:{1}", ex, resultsToExecute));
            }
        }
        #endregion SaveItems

        #region LoadEpisodePersistencies

        public static void LoadItems(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                cmd.CommandText = string.Format("SELECT * FROM ArtifactCountersExported where EpisodeId = {0} order by SampleFromDate", episode.EpisodeId);

                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        ArtifactCountersExportedModel artifactCountersExportedModel = new ArtifactCountersExportedModel()
                        {
                            ExportId = rdr.GetInt32(rdr.GetOrdinal("ExportId")),
                            ObjectOfCare = rdr.GetInt32(rdr.GetOrdinal("ObjectOfCare")),
                            ConceptNumber = rdr.GetInt32(rdr.GetOrdinal("ConceptNumber")),
                            IntervalId = rdr.GetInt32(rdr.GetOrdinal("IntervalId")),
                            SampleFromDate = rdr.GetDateTime(rdr.GetOrdinal("SampleFromDate")),
                            SampleToDate = rdr.GetDateTime(rdr.GetOrdinal("SampleToDate")),
                            ExportedDate = rdr.GetDateTime(rdr.GetOrdinal("ExportedDate")),
                            IntervalDuration = rdr.GetInt32(rdr.GetOrdinal("IntervalDuration")),
                            ConceptValue = rdr.IsDBNull(rdr.GetOrdinal("ConceptValue")) ? null : rdr["ConceptValue"].ToString(),
                            CalulatedValue = rdr.IsDBNull(rdr.GetOrdinal("CalulatedValue")) ? null : rdr["CalulatedValue"].ToString(),
                            IsStikeOut = rdr.GetBoolean(rdr.GetOrdinal("IsStikeOut")),
                            LoginName = rdr.GetOrdinal("LoginName").ToString(),
                            DateInserted = rdr.GetDateTime(rdr.GetOrdinal("DateInserted")),
                            IsDirty = false
                        };

                        episode.ArtifactCountersExportedList.Add(artifactCountersExportedModel);
                    }
                }
            }

        }
        #endregion LoadEpisodePersistencies

        #region ArchiveItems
        public static void ArchiveItems(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            
            //when archiving insert the data to fact table as well

            ArtifactCountersExportedModelFactDAL.SaveItems(sqliteConnection, episode);
            SaveItems(sqliteConnection, episode, false);;

        }
        #endregion ArchiveItems

        #region DeleteItems
        public static void DeleteItems(SQLiteConnection sqliteConnection, int episodeId)
        {

            using (SQLiteCommand sqliteCommand = new SQLiteCommand(sqliteConnection))
            {
                sqliteCommand.CommandText = string.Format("delete from ArtifactCountersExported where EpisodeId = {0}", episodeId);
                sqliteCommand.ExecuteNonQuery();
            }

        }
        #endregion DeleteItems
    }
}
