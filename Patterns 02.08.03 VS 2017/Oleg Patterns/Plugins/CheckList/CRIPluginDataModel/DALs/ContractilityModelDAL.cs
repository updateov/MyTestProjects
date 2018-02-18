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
    public static class ContractilityModelDAL
    {
        
        #region AddOrUpdate

        /// <summary>
        /// Adds the or update the Contractilities data, this is used both for EpisdoeDB and EpisodeArchiveDB.
        /// </summary>
        /// <param name="sqliteConnection">The sqlite connection.</param>
        /// <param name="episode">The episode.</param>
        /// <returns></returns>
        public static void AddOrUpdate(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            #region SaveSQL query STR
            string SaveSQL = @"INSERT OR REPLACE  into Contractilities 
                                (
	                                ContractilityId,
                                    EpisodeId,
                                    StartTime,
                                    EndTime,
                                    Classification,
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
                                    '{0}' as  ContractilityId,
	                                '{1}' as  EpisodeId,
	                                '{2}' as  StartTime,
	                                '{3}' as  EndTime,
	                                '{4}' as  Classification,
	                                '{5}' as  Variability,
	                                '{6}' as  IsVariabilityForStatus,
	                                '{7}' as  Accels,
	                                '{8}' as  IsAccelsForStatus,
	                                '{9}' as  Contractions,
	                                '{10}' as IsContractionsForStatus,
	                                '{11}' as LongContractions,
	                                '{12}' as IsLongContractionsForStatus,
	                                '{13}' as LargeDeceles,
                                    '{14}' as IsLargeDecelesForStatus,
                                    '{15}' as LateDecels,
                                    '{16}' as IsLateDecelsForStatus,
                                    '{17}' as ProlongedDecels,
                                    '{18}' as IsProlongedDecelsForStatus,
	                                '{19}' AS DateUpdated,
                                    '{20}' AS DateInserted
                                  ";
            #endregion SaveSQL query STR

            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                var results = from Contractility in episode.Contractilities
                              where Contractility.IsDirty == true
                              select (string.Format(ValueSQL,
                                                       Contractility.ContractilityId,
                                                       episode.EpisodeId,
                                                       Contractility.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Contractility.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Contractility.Classification,
                                                       Contractility.Variability,
                                                       Convert.ToInt32(Contractility.IsVariabilityForStatus),
                                                       Contractility.Accels,
                                                       Convert.ToInt32(Contractility.IsAccelsForStatus),
                                                       Contractility.Contractions,
                                                       Convert.ToInt32(Contractility.IsContractionsForStatus),
                                                       Contractility.LongContractions,
                                                       Convert.ToInt32(Contractility.IsLongContractionsForStatus),
                                                       Contractility.LargeDeceles,
                                                       Convert.ToInt32(Contractility.IsLargeDecelesForStatus),
                                                       Contractility.LateDecels,
                                                       Convert.ToInt32(Contractility.IsLateDecelsForStatus),
                                                       Contractility.ProlongedDecels,
                                                       Convert.ToInt32(Contractility.IsProlongedDecelsForStatus),
                                                       DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       ((Contractility.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : Contractility.DateInserted).ToString("yyyy-MM-dd HH:mm:ss")

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

        /// <summary>
        /// Adds the or update the Contractilities data, this is used both for EpisdoeDB and EpisodeArchiveDB.
        /// </summary>
        /// <param name="sqliteConnection">The sqlite connection.</param>
        /// <param name="episode">The episode.</param>
        /// <returns></returns>
        public static void SaveToArchiveDB(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            #region SaveSQL query STR
            string SaveSQL = @"INSERT OR REPLACE  into Contractilities 
                                (
	                                ContractilityId,
                                    EpisodeId,
                                    StartDateComputed,
                                    StartTime,
                                    EndTime,
                                    Classification,
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
                                    '{0}' as  ContractilityId,
	                                '{1}' as  EpisodeId,
                                    DATE('{2}') AS StartDateComputed,
	                                '{2}' as  StartTime,
	                                '{3}' as  EndTime,
	                                '{4}' as  Classification,
	                                '{5}' as  Variability,
	                                '{6}' as  IsVariabilityForStatus,
	                                '{7}' as  Accels,
	                                '{8}' as  IsAccelsForStatus,
	                                '{9}' as  Contractions,
	                                '{10}' as IsContractionsForStatus,
	                                '{11}' as LongContractions,
	                                '{12}' as IsLongContractionsForStatus,
	                                '{13}' as LargeDeceles,
                                    '{14}' as IsLargeDecelesForStatus,
                                    '{15}' as LateDecels,
                                    '{16}' as IsLateDecelsForStatus,
                                    '{17}' as ProlongedDecels,
                                    '{18}' as IsProlongedDecelsForStatus,
	                                '{19}' AS DateUpdated,
                                    '{20}' AS DateInserted
                                  ";
            #endregion SaveSQL query STR

            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                var results = from Contractility in episode.Contractilities
                              select (string.Format(ValueSQL,
                                                       Contractility.ContractilityId,
                                                       episode.EpisodeId,
                                                       Contractility.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Contractility.EndTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                                       Contractility.Classification,
                                                       Contractility.Variability,
                                                       Convert.ToInt32(Contractility.IsVariabilityForStatus),
                                                       Contractility.Accels,
                                                       Convert.ToInt32(Contractility.IsAccelsForStatus),
                                                       Contractility.Contractions,
                                                       Convert.ToInt32(Contractility.IsContractionsForStatus),
                                                       Contractility.LongContractions,
                                                       Convert.ToInt32(Contractility.IsLongContractionsForStatus),
                                                       Contractility.LargeDeceles,
                                                       Convert.ToInt32(Contractility.IsLargeDecelesForStatus),
                                                       Contractility.LateDecels,
                                                       Convert.ToInt32(Contractility.IsLateDecelsForStatus),
                                                       Contractility.ProlongedDecels,
                                                       Convert.ToInt32(Contractility.IsProlongedDecelsForStatus),
                                                       DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), //DateUpdated
                                                       ((Contractility.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : Contractility.DateInserted).ToString("yyyy-MM-dd HH:mm:ss")
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

        #region LoadEpisodeContractilities
        public static void LoadEpisodeContractilities(SQLiteConnection sqliteConnection, EpisodeModel episode)
        {
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                cmd.CommandText = string.Format("SELECT * FROM Contractilities where EpisodeId = {0} order by ContractilityId", episode.EpisodeId);

                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        ContractilityModel contractilityModel = new ContractilityModel()
                        {
                            ContractilityId = rdr.GetInt32(rdr.GetOrdinal("ContractilityId")),
                            StartTime = rdr.GetDateTime(rdr.GetOrdinal("StartTime")),
                            EndTime = rdr.GetDateTime(rdr.GetOrdinal("EndTime")),
                            Classification = rdr.GetInt32(rdr.GetOrdinal("Classification")),
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

                        episode.Contractilities.Add(contractilityModel);
                    }
                }
            }
        }
        #endregion LoadEpisodeContractilities
    }
}
