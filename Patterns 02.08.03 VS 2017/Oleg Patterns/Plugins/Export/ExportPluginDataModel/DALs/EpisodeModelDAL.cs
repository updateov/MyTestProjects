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
    public static class EpisodeModelDAL
    {
        #region SaveSQL Constant
        private const string SaveSQL = @"INSERT OR REPLACE  into Episodes 
                                                (
	                                                EpisodeId	 ,
	                                                VisitKey	 ,
                                                    IntervalDuration,
                                                    EpisodeStatusId,
                                                    TotalNumOfCountersIntervals,
                                                    LastMergeId,
                                                    LastMergeTime,
                                                    EpisodeGuid,
	                                                DateUpdated	,
                                                    DateInserted
                                                )
                                                SELECT 
                                                    '{0}' AS EpisodeId,
	                                                '{1}' AS VisitKey,
	                                                '{2}' AS IntervalDuration,
                                                    '{3}' AS EpisodeStatusId,
                                                    '{4}' AS TotalNumOfCountersIntervals,
                                                    '{5}' AS LastMergeId,
                                                    '{6}' AS LastMergeTime,                                           
                                                    '{7}' as EpisodeGuid,
	                                                '{8}' as DateUpdated,
                                                    '{9}' AS DateInserted 
                                                ";
        #endregion SaveSQL Constant

        #region SaveItems
        public static void SaveItems(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            string InsertOrUpdateQuery = null;
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                //get the unique hashcode of the eppisode key
                if (episode.EpisodeGuid == null || episode.EpisodeGuid == Guid.Empty)
                    episode.EpisodeGuid = Guid.NewGuid();


                InsertOrUpdateQuery = string.Format(SaveSQL, episode.EpisodeId,
                                                                episode.VisitKey,
                                                                episode.IntervalDuration,
                                                                episode.EpisodeStatusId,
                                                                episode.ArtifactCountersList.Select(i => i.SampleFromDate).Distinct().Count(),
                                                                episode.LastMergeId,
                                                                episode.LastMergeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                episode.EpisodeGuid,
                                                                DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                                                                ((episode.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : episode.DateInserted).ToString("yyyy-MM-dd HH:mm:ss")
                                                                );


                cmd.CommandText = InsertOrUpdateQuery;
                cmd.ExecuteNonQuery();
            }

        }
        #endregion SaveItems

        #region SaveItems
        //TODO: check if still needed
        public static void SaveIntervalDuration(SQLiteConnection sqliteConnection, int episodeId, int IntervalDuration)
        {
            //update to the new value
            string sql = string.Format("UPDATE Episodes set IntervalDuration = '{0}' where EpisodeId = '{1}'",
                                        IntervalDuration,
                                        episodeId);
            using (SQLiteCommand cmd = new SQLiteCommand(sql, sqliteConnection))
                cmd.ExecuteNonQuery();

        }
        #endregion SaveItems

        #region LoadItems
        public static void LoadItems(SQLiteConnection sqliteConnection, List<EpisodeModel> episodes)
        {
            //TODO: add filter to load only the active episodes
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                cmd.CommandText = "SELECT * FROM Episodes order by EpisodeId";

                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        EpisodeModel episodeModel = new EpisodeModel()
                        {
                            EpisodeId = rdr.GetInt32(rdr.GetOrdinal("EpisodeId")),
                            VisitKey = rdr["VisitKey"].ToString(),
                            IntervalDuration = rdr.GetInt32(rdr.GetOrdinal("IntervalDuration")),
                            EpisodeStatusId = rdr.GetInt32(rdr.GetOrdinal("EpisodeStatusId")),
                            TotalNumOfCountersIntervals = rdr.GetInt32(rdr.GetOrdinal("TotalNumOfCountersIntervals")),
                            LastMergeId = rdr.GetInt32(rdr.GetOrdinal("LastMergeId")),
                            LastMergeTime = rdr.GetDateTime(rdr.GetOrdinal("LastMergeTime")),
                            EpisodeGuid = rdr.GetGuid(rdr.GetOrdinal("EpisodeGuid")),
                            DateUpdated = rdr.GetDateTime(rdr.GetOrdinal("DateUpdated")),
                            DateInserted = rdr.GetDateTime(rdr.GetOrdinal("DateInserted"))
                        };

                        episodes.Add(episodeModel);
                    }
                }
            }
        }
        #endregion LoadItems

        #region ArchiveItems
        public static void ArchiveItems(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            //TODO: move the audit data to history database
            SaveItems(sqliteConnection, episode);
        }
        #endregion ArchiveItems

        #region DeleteItems
        public static void DeleteItems(SQLiteConnection sqliteConnection, int episodeId)
        {

            using (SQLiteCommand sqliteCommand = new SQLiteCommand(sqliteConnection))
            {
                sqliteCommand.CommandText = string.Format("delete from Episodes where EpisodeId = {0}", episodeId);
                sqliteCommand.ExecuteNonQuery();
            }

        }
        #endregion DeleteItems

        #region DeleteDischargedEpisodeData
        public static void DeleteDischargedEpisodeData(SQLiteConnection sqliteConnection, int episodeId)
        {

            using (SQLiteTransaction sqliteTransaction = sqliteConnection.BeginTransaction())
            {
                ArtifactCountersModelDAL.DeleteItems(sqliteConnection, episodeId);
                ArtifactCountersExportedModelDAL.DeleteItems(sqliteConnection, episodeId);
                EpisodeModelDAL.DeleteItems(sqliteConnection, episodeId);


                sqliteTransaction.Commit();
            }
        }
        #endregion DeleteDischargedEpisodeData

        #region GetNextEpisodeIdFromEpisdoeArchiveDB
        public static int GetNextEpisodeIdFromEpisdoeArchiveDB(SQLiteConnection sqliteConnection, Guid episodeGuid)
        {
            object result;
            using (SQLiteCommand sqliteCommand = new SQLiteCommand(sqliteConnection))
            {
                sqliteCommand.CommandText = string.Format("select EpisodeId from Episodes where EpisodeGuid = '{0}';", episodeGuid);
                result = sqliteCommand.ExecuteScalar();
                if (result == DBNull.Value || result == null)
                {
                    sqliteCommand.CommandText = "select IFNULL(MAX(EpisodeId),0) + 1 FROM Episodes;";
                    result = sqliteCommand.ExecuteScalar();
                }
            }

            return int.Parse(result.ToString());
        }
        #endregion GetNextEpisodeIdFromEpisdoeArchiveDB

    }
}
