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
    public static class PersistenceStateModelDAL
    {

        #region AddOrUpdate

        public static void AddOrUpdate(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {

            #region SaveSQL Const
            string SaveSQL = @"INSERT OR REPLACE  into Persistencies 
                                                (
	                                                PersistenceId,
                                                    EpisodeId,
                                                    StartTime,
                                                    EndTime,
                                                    State,
                                                    AcknowledgeDate,
                                                    AcknowledgeUserType,
                                                    Variability,
                                                    IsVariabilityForStatus,
                                                    Accels,
                                                    IsAccelsForStatus,
                                                    Contractions,
                                                    IsContractionsForStatus,
                                                    LongContractions,
                                                    IsLongContractionsForStatus,
                                                    LargeDeceles,
                                                    IsLargeDecelesForStatus,
                                                    LateDecels,
                                                    IsLateDecelsForStatus,
                                                    ProlongedDecels,
                                                    IsProlongedDecelsForStatus,
	                                                DateUpdated,	
                                                    DateInserted	
                                                )";
            string ValueSQL = @"  SELECT 
                                                '{0}' as  PersistenceId,
	                                            '{1}' as  EpisodeId,
	                                            '{2}' as  StartTime,
	                                            '{3}' as  EndTime,
	                                            '{4}' as  State,
	                                            '{5}' as  AcknowledgeDate,
	                                            '{6}' as  AcknowledgeUserType,
	                                            '{7}' as  Variability,
	                                            '{8}' as  IsVariabilityForStatus,
	                                            '{9}' as  Accels,
	                                            '{10}' as IsAccelsForStatus,
	                                            '{11}' as Contractions,
	                                            '{12}' as IsContractionsForStatus,
	                                            '{13}' as LongContractions,
                                                '{14}' as IsLongContractionsForStatus,
                                                '{15}' as LargeDeceles,
                                                '{16}' as IsLargeDecelesForStatus,
                                                '{17}' as LateDecels,
                                                '{18}' as IsLateDecelsForStatus,
                                                '{19}' as ProlongedDecels,
                                                '{20}' as IsProlongedDecelsForStatus,
	                                            '{21}' DateUpdated,
                                                '{22}' as DateInserted
                                                ";
            #endregion SaveSQL Const

            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                var results = from Persistenciestate in episode.PersistencieStates
                              where Persistenciestate.IsDirty == true
                              select (string.Format(ValueSQL,
                                                       Persistenciestate.PersistenceId,
                                                       episode.EpisodeId,
                                                       Persistenciestate.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Persistenciestate.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Persistenciestate.State,
                                                       Persistenciestate.AcknowledgeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Persistenciestate.AcknowledgeUserType,
                                                       Persistenciestate.Variability,
                                                       Convert.ToInt32(Persistenciestate.IsVariabilityForStatus),
                                                       Persistenciestate.Accels,
                                                       Convert.ToInt32(Persistenciestate.IsAccelsForStatus),
                                                       Persistenciestate.Contractions,
                                                       Convert.ToInt32(Persistenciestate.IsContractionsForStatus),
                                                       Persistenciestate.LongContractions,
                                                       Convert.ToInt32(Persistenciestate.IsLongContractionsForStatus),
                                                       Persistenciestate.LargeDeceles,
                                                      Convert.ToInt32(Persistenciestate.IsLargeDecelesForStatus),
                                                       Persistenciestate.LateDecels,
                                                       Convert.ToInt32(Persistenciestate.IsLateDecelsForStatus),
                                                       Persistenciestate.ProlongedDecels,
                                                      Convert.ToInt32(Persistenciestate.IsProlongedDecelsForStatus),
                                                      DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                                                      ((Persistenciestate.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : Persistenciestate.DateInserted).ToString("yyyy-MM-dd HH:mm:ss")
                                                    )
                                       );

                while (results.Count() > 400)
                {
                    var resultsToExecute = results.Take(400);
                    cmd.CommandText = SaveSQL + String.Join(" UNION ALL ", resultsToExecute);
                    cmd.ExecuteNonQuery();
                    results = results.Skip(400);
                }

                if (results.Count() > 0)
                {
                    cmd.CommandText = SaveSQL + String.Join(" UNION ALL ", results);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        #endregion AddOrUpdate

        #region SaveToArchiveDB

        public static void SaveToArchiveDB(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            #region SaveSQL Const
            string SaveSQL = @"INSERT OR REPLACE  into Persistencies 
                            (
	                            PersistenceId,
                                EpisodeId,
                                StartDateComputed,
                                StartTime,
                                EndTime,
                                State,
                                AcknowledgeDate,
                                AcknowledgeUserType,
                                Variability,
                                IsVariabilityForStatus,
                                Accels,
                                IsAccelsForStatus,
                                Contractions,
                                IsContractionsForStatus,
                                LongContractions,
                                IsLongContractionsForStatus,
                                LargeDeceles,
                                IsLargeDecelesForStatus,
                                LateDecels,
                                IsLateDecelsForStatus,
                                ProlongedDecels,
                                IsProlongedDecelsForStatus,
	                            DateUpdated,
                                DateInserted
                            )";
            string ValueSQL = @"  SELECT 
                                    '{0}' as  PersistenceId,
	                                '{1}' as  EpisodeId,
                                    DATE('{2}') AS StartDateComputed,
	                                '{2}' as  StartTime,
	                                '{3}' as  EndTime,
	                                '{4}' as  State,
	                                '{5}' as  AcknowledgeDate,
	                                '{6}' as  AcknowledgeUserType,
	                                '{7}' as  Variability,
	                                '{8}' as  IsVariabilityForStatus,
	                                '{9}' as  Accels,
	                                '{10}' as IsAccelsForStatus,
	                                '{11}' as Contractions,
	                                '{12}' as IsContractionsForStatus,
	                                '{13}' as LongContractions,
                                    '{14}' as IsLongContractionsForStatus,
                                    '{15}' as LargeDeceles,
                                    '{16}' as IsLargeDecelesForStatus,
                                    '{17}' as LateDecels,
                                    '{18}' as IsLateDecelsForStatus,
                                    '{19}' as ProlongedDecels,
                                    '{20}' as IsProlongedDecelsForStatus,
	                                '{21}' as DateUpdated,
                                    '{22}' as DateInserted
                                    ";
            #endregion SaveSQL Const

            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                var results = from Persistenciestate in episode.PersistencieStates
                              select (string.Format(ValueSQL,
                                                       Persistenciestate.PersistenceId,
                                                       episode.EpisodeId,
                                                       Persistenciestate.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Persistenciestate.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Persistenciestate.State,
                                                       Persistenciestate.AcknowledgeTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Persistenciestate.AcknowledgeUserType,
                                                       Persistenciestate.Variability,
                                                       Convert.ToInt32(Persistenciestate.IsVariabilityForStatus),
                                                       Persistenciestate.Accels,
                                                       Convert.ToInt32(Persistenciestate.IsAccelsForStatus), //10
                                                       Persistenciestate.Contractions,
                                                       Convert.ToInt32(Persistenciestate.IsContractionsForStatus),
                                                       Persistenciestate.LongContractions,
                                                       Convert.ToInt32(Persistenciestate.IsLongContractionsForStatus),
                                                       Persistenciestate.LargeDeceles,
                                                      Convert.ToInt32(Persistenciestate.IsLargeDecelesForStatus),
                                                       Persistenciestate.LateDecels,
                                                       Convert.ToInt32(Persistenciestate.IsLateDecelsForStatus),
                                                       Persistenciestate.ProlongedDecels,
                                                      Convert.ToInt32(Persistenciestate.IsProlongedDecelsForStatus), //20 
                                                      DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                                                      ((Persistenciestate.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : Persistenciestate.DateInserted).ToString("yyyy-MM-dd HH:mm:ss")
                                                    )
                                       );

                while (results.Count() > 400)
                {
                    var resultsToExecute = results.Take(400);
                    cmd.CommandText = SaveSQL + String.Join(" UNION ALL ", resultsToExecute);
                    cmd.ExecuteNonQuery();
                    results = results.Skip(400);
                }

                if (results.Count() > 0)
                {
                    cmd.CommandText = SaveSQL + String.Join(" UNION ALL ", results);
                    cmd.ExecuteNonQuery();
                }
            }

        }
        #endregion SaveToArchiveDB

        #region LoadEpisodePersistencies

        public static void LoadEpisodePersistencies(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                cmd.CommandText = string.Format("SELECT * FROM Persistencies where EpisodeId = {0} order by PersistenceId", episode.EpisodeId);

                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        PersistenceStateModel PersistenceStateModel = new PersistenceStateModel()
                        {
                            PersistenceId = rdr.GetInt32(rdr.GetOrdinal("PersistenceId")),
                            StartTime = rdr.GetDateTime(rdr.GetOrdinal("StartTime")),
                            EndTime = rdr.GetDateTime(rdr.GetOrdinal("EndTime")),
                            State = rdr.GetInt32(rdr.GetOrdinal("State")),
                            AcknowledgeTime = rdr.GetDateTime(rdr.GetOrdinal("AcknowledgeDate")),
                            AcknowledgeUserType = rdr["AcknowledgeUserType"].ToString(),
                            Variability = rdr.GetDouble(rdr.GetOrdinal("Variability")),
                            IsVariabilityForStatus = rdr.GetBoolean(rdr.GetOrdinal("IsVariabilityForStatus")),
                            Accels = rdr.GetInt32(rdr.GetOrdinal("Accels")), //way long and not int?
                            IsAccelsForStatus = rdr.GetBoolean(rdr.GetOrdinal("IsAccelsForStatus")),
                            Contractions = rdr.GetInt32(rdr.GetOrdinal("Contractions")),
                            IsContractionsForStatus = rdr.GetBoolean(rdr.GetOrdinal("IsContractionsForStatus")),
                            LongContractions = rdr.GetInt32(rdr.GetOrdinal("LongContractions")),
                            IsLongContractionsForStatus = rdr.GetBoolean(rdr.GetOrdinal("IsLongContractionsForStatus")),
                            LargeDeceles = rdr.GetInt32(rdr.GetOrdinal("LargeDeceles")),
                            IsLargeDecelesForStatus = rdr.GetBoolean(rdr.GetOrdinal("IsLargeDecelesForStatus")),
                            LateDecels = rdr.GetInt32(rdr.GetOrdinal("LateDecels")),
                            IsLateDecelsForStatus = rdr.GetBoolean(rdr.GetOrdinal("IsLateDecelsForStatus")),
                            ProlongedDecels = rdr.GetInt32(rdr.GetOrdinal("ProlongedDecels")),
                            IsProlongedDecelsForStatus = rdr.GetBoolean(rdr.GetOrdinal("IsProlongedDecelsForStatus")),
                            DateUpdated = rdr.GetDateTime(rdr.GetOrdinal("DateUpdated")),
                            DateInserted = rdr.GetDateTime(rdr.GetOrdinal("DateInserted")),
                            IsDirty = false
                        };

                        episode.PersistencieStates.Add(PersistenceStateModel);
                    }
                }
            }

        }
        #endregion LoadEpisodePersistencies

        #region GetLastPersistenceId
        public static int GetLastPersistenceId(SQLiteConnection sqliteConnection)
        {
            int result = -1;

            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                cmd.CommandText = "select max(PersistenceId) from Persistencies";
                bool bSucc = int.TryParse(cmd.ExecuteScalar().ToString(), out result);
                if (!bSucc)
                {
                    result = -1;
                }
            }

            return result;
        }
        #endregion GetLastPersistenceId

    }
}
