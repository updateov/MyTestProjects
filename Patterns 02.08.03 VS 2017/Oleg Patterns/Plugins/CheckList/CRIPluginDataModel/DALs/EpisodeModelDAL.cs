using CRIEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CRIPluginDataModel.DALs
{
    public static class EpisodeModelDAL
    {
        #region SaveSQL Constant
        private const string SaveSQL = @"INSERT OR REPLACE  into Episodes 
                                                (
	                                                EpisodeId	 ,
	                                                EpisodeKey	 ,
	                                                GA			 ,
	                                                Fetuses		 ,
	                                                StatusId	 ,
	                                                Tracing		 ,
	                                                Artifact	 ,
	                                                Contractility,
	                                                Action		 ,
	                                                Incremental	 ,
	                                                Serveruid	 ,
                                                    LastMergeId,
                                                    LastMergeTime,
                                                    EpisodeGuid,
	                                                DateUpdated	,
                                                    DateInserted
                                                )
                                                SELECT 
                                                    '{0}' as EpisodeId,
	                                                '{1}' as EpisodeKey,
	                                                '{2}' as GA,
	                                                '{3}' as Fetuses,
	                                                '{4}' as StatusId,
	                                                '{5}' as Tracing,
	                                                '{6}' as Artifact,
	                                                '{7}' as Contractility,
	                                                '{8}' as Action,
	                                                '{9}' as Incremental,
	                                                '{10}' as Serveruid,
                                                    '{11}' as LastMergeId,
                                                    '{12}' as LastMergeTime,
                                                    '{13}' as EpisodeGuid,
	                                                '{14}' DateUpdated,
                                                    '{15}' as DateInserted
                                                ";
        #endregion SaveSQL Constant

        #region AddOrUpdate
        public static void AddOrUpdate(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            string InsertOrUpdateQuery = null;
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                //get the unique hascode of the eppisode key
                if (episode.EpisodeGuid == null || episode.EpisodeGuid == Guid.Empty)
                    episode.EpisodeGuid = Guid.NewGuid();

                InsertOrUpdateQuery = string.Format(SaveSQL, episode.EpisodeId,
                                                                episode.EpisodeKey,
                                                                episode.GA,
                                                                episode.Fetuses,
                                                                episode.StatusId,
                                                                episode.Tracing,
                                                                episode.Artifact,
                                                                episode.Contractility,
                                                                episode.Action,
                                                                episode.Incremental,
                                                                episode.Serveruid,
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
        #endregion AddOrUpdate

        #region LoadEpisodes
        public static void LoadEpisodes(SQLiteConnection sqliteConnection, List<EpisodeModel> episodes)
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
                            EpisodeKey = rdr["EpisodeKey"].ToString(),
                            GA = rdr["GA"].ToString(),
                            Fetuses = rdr.GetInt32(rdr.GetOrdinal("Fetuses")),
                            StatusId = rdr.GetInt32(rdr.GetOrdinal("StatusId")),
                            Tracing = rdr.GetInt32(rdr.GetOrdinal("Tracing")),
                            Artifact = rdr.GetInt32(rdr.GetOrdinal("Artifact")),
                            Contractility = rdr.GetInt32(rdr.GetOrdinal("Contractility")),
                            Action = rdr.GetInt32(rdr.GetOrdinal("Action")),
                            Incremental = rdr.GetInt32(rdr.GetOrdinal("Incremental")),
                            Serveruid = rdr["Serveruid"].ToString(),
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
        #endregion LoadEpisodes

        #region DeleteDischargedEpisodeData
        public static void DeleteDischargedEpisodeData(SQLiteConnection sqliteConnection, int episodeId)
        {

            using (SQLiteTransaction sqliteTransaction = sqliteConnection.BeginTransaction())
            using (SQLiteCommand sqliteCommand = new SQLiteCommand(sqliteConnection))
            {
                sqliteCommand.CommandText = string.Format("delete from Persistencies where EpisodeId = {0}", episodeId);
                sqliteCommand.ExecuteNonQuery();

                sqliteCommand.CommandText = string.Format("delete from Contractilities where EpisodeId = {0}", episodeId);
                sqliteCommand.ExecuteNonQuery();

                sqliteCommand.CommandText = string.Format("delete from Episodes where EpisodeId = {0}", episodeId);
                sqliteCommand.ExecuteNonQuery();

                sqliteTransaction.Commit();
            }
        }
        #endregion DeleteDischargedEpisodeData

        #region GetNextEpisodeIdFromEpisdoeArchiveDB
        public static int GetNextEpisodeIdFromEpisdoeArchiveDB(SQLiteConnection sqliteConnection)
        {
            string sql = "select IFNULL(MAX(EpisodeId),0) FROM Episodes;";
            int result = 0;
            using (SQLiteCommand sqliteCommand = new SQLiteCommand(sql, sqliteConnection))
            {
                result = int.Parse(sqliteCommand.ExecuteScalar().ToString());
            }

            return result + 1;
        }
        #endregion GetNextEpisodeIdFromEpisdoeArchiveDB
    }
}
