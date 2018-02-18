using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CRIPluginDataModel;
using System.Data.SQLite;


namespace CRIPluginDataModelUnitTest
{
    [TestClass]
    //[DeploymentItem(@"DBs\", "DBs")]
    [DeploymentItem(@"x64\", "x64")]
    [DeploymentItem(@"x86\", "x86")]
    public class CRIPluginDBManagerTest
    {
        private const string m_episodeDBFile = @"EpisodeDB.db";
        private const string m_episodeArchiveDBFile = @"EpisodeArchiveDB.db";
        private const string EpisodeDB_Script = "CRIPluginDataModelUnitTest.EpisodeDB_SampleData.sql";
        
        private const string ConnectionStringTemplate = "Data Source={0}; Version=3;new=False;datetimeformat=ISO8601;foreign keys=True";

        private static string m_rootFolder = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DBs");
        private static string episodeDBFilePath = Path.Combine(m_rootFolder, m_episodeDBFile);
        private static string episodeArchiveDBFilePath = Path.Combine(m_rootFolder, m_episodeArchiveDBFile);

        [TestCleanup]
        public void TestCleanup()
        {
            DBManager.Instance.Dispose();
        }

        #region CheckListPluginDBManagerCreateNewEpisodeDBShouldSucceedTest

        [TestMethod]
        public void CheckListPluginDBManagerCreateNewEpisodeDBShouldSucceedTest()
        {
            string sql;
            string workingFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //prepere
            if(File.Exists(episodeDBFilePath))
                File.Delete(episodeDBFilePath);
           
            if(File.Exists(episodeArchiveDBFilePath))
             File.Delete(episodeArchiveDBFilePath);

            //Test
            DBManager dbManager = DBManager.Instance;
            dbManager.CreateDB();

            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(EpisodeDB_Script)))
                sql = reader.ReadToEnd();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {

                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, sqliteConnection))
                    cmd.ExecuteNonQuery();
            }
            //Validate
            //TODO, add new episode, and archive the episode
        }
        #endregion CheckListPluginDBManagerCreateNewEpisodeDBShouldSucceedTest

        #region CheckListPluginDBManagerLoadAllEpisodeShouldSucceedTest

        [TestMethod]
        public void CheckListPluginDBManagerPluginDBManagerLoadAllEpisodeShouldSucceedTest()
        {
            string sql;
            CheckListPluginDBManagerCreateNewEpisodeDBShouldSucceedTest();
            
            DBManager dbManager = DBManager.Instance;

            List<EpisodeModel> episodes;
             Assert.IsTrue(dbManager.LoadAll(out episodes), "LoadAll");

          
            //todo: add mew episoded ... save and verify that the count has changed

            //int i = dbManager.LastPersistenceId();
            
            episodes[0].Contractilities[0].IsDirty = true;
            episodes[0].Contractilities[1].IsDirty = true;
            episodes[0].PersistencieStates[0].IsDirty = true;
            episodes[0].PersistencieStates[1].IsDirty = true;

            dbManager.SaveAll(episodes);

            //verify that the data can be read - all the data types are correct
            List<EpisodeModel> episodes1;
            Assert.IsTrue(dbManager.LoadAll(out episodes1));

            //Verify
            int EpisodesCount;
            int ContractilitiesCount;
            int PersistencesCount;
            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {
                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Episodes", sqliteConnection))
                    EpisodesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Contractilities", sqliteConnection))
                    ContractilitiesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Persistencies", sqliteConnection))
                    PersistencesCount = int.Parse(cmd.ExecuteScalar().ToString());

                Assert.AreEqual(EpisodesCount, 4, "Not all episoded were deleted");
                Assert.AreEqual(ContractilitiesCount, 12, "Not all Contractilities were deleted");
                Assert.AreEqual(PersistencesCount, 13, "Not all Persistencies were deleted");
            }

        }
        #endregion CheckListPluginDBManagerLoadAllEpisodeShouldSucceedTest

        #region CheckListPluginDBManagerArchiveAllEpisodeShouldSucceedTest

        [TestMethod]
        public void CheckListPluginDBManagerArchiveAllEpisodeShouldSucceedTest()
        {
            string sql;
            //Prepere
            CheckListPluginDBManagerCreateNewEpisodeDBShouldSucceedTest();

            DBManager dbManager = DBManager.Instance;

            List<EpisodeModel> episodes ;
            Assert.IsTrue(dbManager.LoadAll(out episodes));

            //ACT
            dbManager.DischargeEpisodes(episodes);
            
            //Verify
            int EpisodesCount;
            int ContractilitiesCount;
            int PersistenciesCount;

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {
                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Episodes", sqliteConnection))
                   EpisodesCount  = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Contractilities", sqliteConnection))
                    ContractilitiesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Persistencies", sqliteConnection))
                    PersistenciesCount = int.Parse(cmd.ExecuteScalar().ToString());
                
                Assert.AreEqual(EpisodesCount, 0,"Not all episoded were deleted");
                Assert.AreEqual(ContractilitiesCount, 0, "Not all Contractilities were deleted");
                Assert.AreEqual(PersistenciesCount, 0, "Not all Persistencies were deleted"); 
            }

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeArchiveDBFilePath)))
            {
                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Episodes", sqliteConnection))
                    EpisodesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Contractilities", sqliteConnection))
                    ContractilitiesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Persistencies", sqliteConnection))
                    PersistenciesCount = int.Parse(cmd.ExecuteScalar().ToString());

                Assert.AreEqual(EpisodesCount, 4, "Not all episoded were Archived");
                Assert.AreEqual(ContractilitiesCount, 12, "Not all Contractilities were Archived");
                Assert.AreEqual(PersistenciesCount, 13, "Not all Persistencies were Archived");
            }

        }

        #endregion CheckListPluginDBManagerArchiveAllEpisodeShouldSucceedTest

        #region CheckListPluginDBManagertArchiveAllEpisodeShouldSucceedTest
        /*
         obsolete Test 
         */
        [TestMethod]
        public void CheckListPluginDBManagerArchiveAllEpisodeManually()
        {
            //string sql;
            //Prepere
            CheckListPluginDBManagerCreateNewEpisodeDBShouldSucceedTest();

            DBManager dbManager = DBManager.Instance;

            List<EpisodeModel> episodes ;
            Assert.IsTrue(dbManager.LoadAll(out episodes));

            //ACT
            dbManager.DischargeEpisodes(episodes);


        }

        #endregion CheckListPluginDBManagerArchiveAllEpisodeShouldSucceedTest


    }
}
