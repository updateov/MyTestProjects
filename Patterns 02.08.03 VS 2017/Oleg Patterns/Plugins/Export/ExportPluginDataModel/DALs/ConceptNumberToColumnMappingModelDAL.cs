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
    public static class ConceptNumberToColumnMappingModelDAL
    {
        #region SaveSQL Constant
        private const string SaveSQL = @"INSERT OR REPLACE into ConceptNumberToColumnMapping 
                                                (
                                                    ConceptNumber,
                                                    ObjectOfCare,	
                                                    ColumnName,
                                                    ConceptType,
                                                    Comments,		
	                                                DateUpdated,
                                                    DateInserted
                                                )
                                                SELECT 
                                                    '{0}' AS ConceptNumber,
	                                                '{1}' AS ObjectOfCare,	
                                                    '{2}' AS ColumnName,
                                                    (select ConceptTypeId from ConceptTypes where name = '{3}') as ConceptType,
	                                                '{4}' AS Comments,		
	                                                DATETIME('now', 'UTC') as DateUpdated,
                                                    '{5}' AS DateInserted
                                                ";
        #endregion SaveSQL Constant

        #region AddItems

        private const string AddSQL = @"INSERT OR IGNORE into ConceptNumberToColumnMapping 
                                                (
                                                    ConceptNumber,
                                                    ObjectOfCare,	
                                                    ColumnName,
                                                    ConceptTypeId,
                                                    Comments,		
	                                                DateUpdated,
                                                    DateInserted
                                                )
                                                SELECT 
                                                    '{0}' AS ConceptNumber,
	                                                '{1}' AS ObjectOfCare,	
                                                    '{2}' AS ColumnName,
                                                    (select ConceptTypeId from ConceptTypes where name = '{3}') as ConceptTypeId,
	                                                '{4}' AS Comments,		
	                                                DATETIME('now', 'UTC') as DateUpdated,
                                                    '{5}' AS DateInserted
                                                ";

        public static void AddItems(SQLiteConnection sqliteConnection, List<ConceptNumberToColumnMappingModel> concepts)
        {
            string InsertOrIgnoreQuery = String.Empty;

            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                foreach (ConceptNumberToColumnMappingModel concept in concepts)
                {
                    InsertOrIgnoreQuery = string.Format(AddSQL, concept.ConceptNumber,
                                                    concept.ObjectOfCare,
                                                    concept.ColumnName,
                                                    concept.ConceptType,
                                                    concept.Comments,
                                                    ((concept.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : concept.DateInserted).ToString("yyyy-MM-dd HH:mm:ss")
                                                    );

                    cmd.CommandText = InsertOrIgnoreQuery;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion AddItems

        #region SaveItems
        public static void SaveItems(SQLiteConnection sqliteConnection, ConceptNumberToColumnMappingModel concepts)
        {

            string InsertOrUpdateQuery = null;
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                InsertOrUpdateQuery = string.Format(SaveSQL, concepts.ConceptNumber,
                                                                concepts.ObjectOfCare,
                                                                concepts.ColumnName,
                                                                concepts.ConceptType,
                                                                concepts.Comments,
                                                                ((concepts.DateInserted == DateTime.MinValue) ? DateTime.UtcNow : concepts.DateInserted).ToString("yyyy-MM-dd HH:mm:ss")
                                                                );


                cmd.CommandText = InsertOrUpdateQuery;
                cmd.ExecuteNonQuery();
            }
        }
        #endregion SaveItems

        #region LoadItems
        public static void LoadItems(SQLiteConnection sqliteConnection, List<ConceptNumberToColumnMappingModel> concepts)
        {
            //TODO: add filter to load only the active episodes
            using (SQLiteCommand cmd = new SQLiteCommand(sqliteConnection))
            {
                cmd.CommandText = @"SELECT  
                                    a.ConceptNumber,
                                    a.ObjectOfCare,
                                    a.ColumnName,
                                    b.Name AS ConceptType,
                                    a.Comments,
                                    a.DateUpdated,
                                a.DateInserted
                                FROM ConceptNumberToColumnMapping a
                                inner join ConceptTypes b
                                on a.ConceptTypeId = b.ConceptTypeId";

                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        ConceptNumberToColumnMappingModel concept = new ConceptNumberToColumnMappingModel()
                        {
                            ConceptNumber = rdr.GetInt32(rdr.GetOrdinal("ConceptNumber")),
                            ObjectOfCare = rdr.GetInt32(rdr.GetOrdinal("ObjectOfCare")),
                            ColumnName = rdr["ColumnName"].ToString(),
                            ConceptType = rdr["ConceptType"].ToString(),
                            Comments = rdr["Comments"].ToString(),
                            DateUpdated = rdr.GetDateTime(rdr.GetOrdinal("DateUpdated")),
                            DateInserted = rdr.GetDateTime(rdr.GetOrdinal("DateInserted"))
                        };
                        concepts.Add(concept);
                    }
                }
            }
        }
        #endregion LoadItems


    }
}
