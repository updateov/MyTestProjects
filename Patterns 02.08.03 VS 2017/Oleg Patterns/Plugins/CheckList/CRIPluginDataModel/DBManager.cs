using CRIPluginDataModel.DALs;
using CRIEntities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CommonLogger;

namespace CRIPluginDataModel
{
    /// <summary>
    /// Singleton class for loading/Saving/Archiving episodeDb Data
    /// </summary>
    public class DBManager : IDBManager, IDisposable
    {

        #region Parameters & Members

        protected SQLiteConnection m_sqliteConnection;
        protected SQLiteConnection m_archiveSQLiteConnection;

        protected const string m_episodeDBFile = @"EpisodeDB.db";
        protected const string m_episodeArchiveDBFile = @"EpisodeArchiveDB.db";
        protected string m_rootFolder = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DBs");

        protected const string EpisodeDB_Script = "CRIPluginDataModel.DBs.EpisodeDB_Script.sql";
        protected const string EpisodeArchiveDB_Script = "CRIPluginDataModel.DBs.EpisodeArchiveDB_Script.sql";

        protected const string MODULE_NAME_FOR_LOGGER = "CRIPluginDBManager";

        protected SQLiteConnection ArchiveSQLiteConnection
        {
            get
            {
                if (m_archiveSQLiteConnection == null)
                {
                    m_archiveSQLiteConnection = new SQLiteConnection(string.Format("Data Source={0}; Version=3;new=False;datetimeformat=ISO8601;foreign keys=True", Path.Combine(m_rootFolder, m_episodeArchiveDBFile)));
                    m_archiveSQLiteConnection.Open();
                }
                return m_archiveSQLiteConnection;
            }
        }

        protected SQLiteConnection SQLiteConnection
        {
            get
            {
                if (m_sqliteConnection == null)
                {
                    m_sqliteConnection = new SQLiteConnection(string.Format("Data Source={0}; Version=3;new=False;datetimeformat=ISO8601;foreign keys=True", Path.Combine(m_rootFolder, m_episodeDBFile)));
                    m_sqliteConnection.Open();
                }
                return m_sqliteConnection;
            }
        }

        #endregion Parameters

        #region Singleton functionality

        private static volatile DBManager s_instance;
        private static object s_lockObject = new Object();
        private static object s_lockArchive = new Object();
        private static object s_lockSelect = new Object();
        private static object s_lockSave = new Object();

        public static DBManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_lockObject)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new DBManager();
                        }
                    }
                }
                return s_instance;
            }
        }

        #endregion

        #region constructor

        /// <summary>   
        /// Prevents a default instance of the <see cref=MODULE_NAME_FOR_LOGGER/> class from being created.
        /// </summary>
        private DBManager()
        {


        }

        #endregion constructor

        /// <summary>
        /// Dischargeds the episodes.
        /// Save the episode data in the ArchiveEpisodeDB and delete the episode data from EpisodeDB
        /// </summary>
        /// <param name="episodes">The episodes.</param>
        /// <returns></returns>
        public bool DischargeEpisodes(List<EpisodeModel> episodes)
        {
            lock (s_lockArchive)
            {
                bool bRes = false;
                int originalEpisodeId;
                try
                {
                    foreach (var episode in episodes)
                    {
                        EpisodeModel clonedEpisode = new EpisodeModel(episode);
                        //save the episod in the Archive Database
                        using (SQLiteTransaction transaction = ArchiveSQLiteConnection.BeginTransaction())
                        {
                            /*
                             * 1.when we archive we need to replace the episode ID with new ID, since patterns might use the same episodeId twice
                                    and we need it to be unique
                                2. save the episodeId in order to delete it from , after 
                             */
                            originalEpisodeId = clonedEpisode.EpisodeId;

                            int episodeId = EpisodeModelDAL.GetNextEpisodeIdFromEpisdoeArchiveDB(ArchiveSQLiteConnection);
                            clonedEpisode.EpisodeId = episodeId;

                            try
                            {
                                EpisodeModelDAL.AddOrUpdate(ArchiveSQLiteConnection, clonedEpisode);
                                ContractilityModelDAL.SaveToArchiveDB(ArchiveSQLiteConnection, clonedEpisode);
                                PersistenceStateModelDAL.SaveToArchiveDB(ArchiveSQLiteConnection, clonedEpisode);
                                transaction.Commit();
                                Logger.WriteLogEntry(TraceEventType.Verbose, MODULE_NAME_FOR_LOGGER, String.Format("Succeeded to save episode {0} to DB", clonedEpisode.EpisodeId));
                            }
                            catch
                            {
                                transaction.Rollback();
                                Logger.WriteLogEntry(TraceEventType.Error, MODULE_NAME_FOR_LOGGER, String.Format("Failed to save episode {0} to DB", clonedEpisode.EpisodeId));
                                throw;
                            }
                        }

                        // Delete the episode From EpisodeDB
                        EpisodeModelDAL.DeleteDischargedEpisodeData(SQLiteConnection, originalEpisodeId);


                    }

                    bRes = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, MODULE_NAME_FOR_LOGGER, "Error Archiving Episodes to sqllite.", ex);
                    bRes = false;
                }

                return bRes;
            }
        }

        /// <summary>
        /// Saves all episodes to EpisodesDB.
        /// </summary>
        /// <param name="episodes">The episodes.</param>
        /// <returns></returns>
        public bool SaveAll(List<EpisodeModel> episodes)
        {
            lock (s_lockArchive)
            {
                bool bRes = false;

                try
                {
                    //save the episod in the Archive Database
                    using (SQLiteTransaction transaction = SQLiteConnection.BeginTransaction())
                    {
                        List<int> episodeIds = new List<int>();
                        try
                        {
                            foreach (var episode in episodes)
                            {
                                EpisodeModelDAL.AddOrUpdate(SQLiteConnection, episode);
                                ContractilityModelDAL.AddOrUpdate(SQLiteConnection, episode);
                                PersistenceStateModelDAL.AddOrUpdate(SQLiteConnection, episode);
                                episodeIds.Add(episode.EpisodeId);
                            }

                            transaction.Commit();
                            Logger.WriteLogEntry(TraceEventType.Verbose, MODULE_NAME_FOR_LOGGER, String.Format("Succeeded to save episodes {0} to DB", string.Join(";", episodeIds)));
                        }
                        catch
                        {
                            transaction.Rollback();
                            Logger.WriteLogEntry(TraceEventType.Error, MODULE_NAME_FOR_LOGGER, String.Format("Failed to save episodes {0} to DB", string.Join(";", episodeIds)));
                            throw;
                        }
                    }

                    bRes = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, MODULE_NAME_FOR_LOGGER, "Error Saving information to sqllite.", ex);
                    bRes = false;
                }

                return bRes;
            }
        }

        /// <summary>
        /// Loads all episdoes from EpisodesDB.
        /// </summary>
        /// <returns></returns>
        public bool LoadAll(out List<EpisodeModel> episodes)
        {
            bool result = false;
            lock (s_lockSelect)
            {
                episodes = new List<EpisodeModel>();

                try
                {
                    //load all episodes from DB
                    EpisodeModelDAL.LoadEpisodes(SQLiteConnection, episodes);

                    foreach (var episode in episodes)
                    {

                        PersistenceStateModelDAL.LoadEpisodePersistencies(SQLiteConnection, episode);
                        ContractilityModelDAL.LoadEpisodeContractilities(SQLiteConnection, episode);
                    }
                    result = true;

                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, MODULE_NAME_FOR_LOGGER, "Error Loading information From sqllite.", ex);
                    result = false;
                }

                return result;
            }
        }

        public bool LastPersistenceId(out int persistenceId)
        {
            bool result = false;
            persistenceId = -1;

            try
            {
                persistenceId = PersistenceStateModelDAL.GetLastPersistenceId(SQLiteConnection);
                result = true;
            }
            catch(Exception ex)
            {
                Logger.WriteLogEntry(TraceEventType.Critical, MODULE_NAME_FOR_LOGGER, "Error getting last persistenceId from sqlite.", ex);
                result = false;
            }

            return result;
        }
        #region CreateDB
        public bool CreateDB()
        {
            lock (s_lockObject)
            {
                //TODO: Handle Exception;
                bool result = true;
                string episodeDBFileName = Path.Combine(m_rootFolder, m_episodeDBFile);
                string episodeArchiveDBFileName = Path.Combine(m_rootFolder, m_episodeArchiveDBFile);
                string sql;

                try
                {
                    if (!Directory.Exists(m_rootFolder))
                        Directory.CreateDirectory(m_rootFolder);

                    if (!File.Exists(episodeDBFileName))
                    {
                        SQLiteConnection.CreateFile(episodeDBFileName);
                        using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(EpisodeDB_Script)))
                            sql = reader.ReadToEnd();

                        using (SQLiteCommand cmd = new SQLiteCommand(sql, SQLiteConnection))
                            cmd.ExecuteNonQuery();
                        Logger.WriteLogEntry(TraceEventType.Information, 7005, MODULE_NAME_FOR_LOGGER, string.Format("Created sqliteDB {0}", episodeDBFileName));
                    }

                    if (!File.Exists(episodeArchiveDBFileName))
                    {
                        SQLiteConnection.CreateFile(episodeArchiveDBFileName);
                        using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(EpisodeArchiveDB_Script)))
                            sql = reader.ReadToEnd();

                        using (SQLiteCommand cmd = new SQLiteCommand(sql, ArchiveSQLiteConnection))
                            cmd.ExecuteNonQuery();
                        Logger.WriteLogEntry(TraceEventType.Information, 7005, MODULE_NAME_FOR_LOGGER, string.Format("Created sqliteDB {0}", episodeArchiveDBFileName));
                    }


                    var version = Assembly.GetExecutingAssembly().GetName().Version;
                    var ReleaseDate = System.IO.File.GetCreationTime(Assembly.GetExecutingAssembly().Location);
                }
                catch(Exception ex)
                {
                    result = false;
                    Logger.WriteLogEntry(TraceEventType.Critical, 7000, MODULE_NAME_FOR_LOGGER, "Error creating New sqliteDB.", ex);
                }
                return result;
            }
        }
        #endregion CreateDB

        #region Dispose

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            //close the connection to the episode DB
            if (m_sqliteConnection != null)
            {
                m_sqliteConnection.Close();
                m_sqliteConnection = null;
            }

            if (m_archiveSQLiteConnection != null)
            {
                m_archiveSQLiteConnection.Close();
                m_archiveSQLiteConnection = null;
            }
        }

        #endregion

        #region merge

        public bool DeleteEpisodeData(int episodeID)
        {
            bool res = false;
            lock (s_lockArchive)
            {
                try
                {
                    EpisodeModelDAL.DeleteDischargedEpisodeData(SQLiteConnection, episodeID);
                    res = true;
                }
                catch (Exception ex)
                {
                    Logger.WriteLogEntry(TraceEventType.Critical, MODULE_NAME_FOR_LOGGER, "Error deleting episode from sqllite.", ex);
                }
            }
            return res;
        }
        #endregion
    }
}
