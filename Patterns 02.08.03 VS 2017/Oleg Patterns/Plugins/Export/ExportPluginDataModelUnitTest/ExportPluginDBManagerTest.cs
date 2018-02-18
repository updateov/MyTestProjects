using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Export.PluginDataModel;
using Export.PluginDataModel.DALs;
using System.Data.SQLite;
using System.Data;

namespace Export.PluginDataModelUnitTest
{
    [TestClass]
    //[DeploymentItem(@"DBs\", "DBs")]
    [DeploymentItem(@"x64\", "x64")]
    [DeploymentItem(@"x86\", "x86")]
    public class ExportPluginDBManagerTest
    {
        #region Parameters
        protected const string m_DBFile = @"ExportDB.db";
        protected const string m_ArchiveDBFile = @"ExportArchiveDB.db";

        private const string EpisodeDB_Script = "Export.PluginDataModelUnitTest.ExportDB_SampleData.sql";
        
        private const string ConnectionStringTemplate = "Data Source={0}; Version=3;new=False;datetimeformat=ISO8601;foreign keys=True";

        private static string m_rootFolder = Path.Combine(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "DBs");
        private static string episodeDBFilePath = Path.Combine(m_rootFolder, m_DBFile);
        private static string episodeArchiveDBFilePath = Path.Combine(m_rootFolder, m_ArchiveDBFile);
        
        #endregion

        [TestCleanup]
        public void TestCleanup()
        {
            DBManager.Instance.Dispose();
        }

        #region ExportPluginCreateNewEpisodeDBShouldSucceedTest

        [TestMethod]
        public void ExportPluginCreateNewExportDBShouldSucceedTest()
        {
            //prepere
            if(File.Exists(episodeDBFilePath))
                File.Delete(episodeDBFilePath);
           
            if(File.Exists(episodeArchiveDBFilePath))
             File.Delete(episodeArchiveDBFilePath);

            //Test
            DBManager dbManager = DBManager.Instance;
            dbManager.CreateDB();

            //Validate
            //TODO: add new episode, and archive the episode
        }
        #endregion ExportPluginCreateNewEpisodeDBShouldSucceedTest

        #region ExportPluginLoadAllEpisodeShouldSucceedTest

         [Ignore]
        //[TestMethod]
        public void ExportPluginLoadAllEpisodeShouldSucceedTest()
        {
            string sql;
            ExportPluginCreateNewExportDBShouldSucceedTest();

            DBManager dbManager = DBManager.Instance;

            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(EpisodeDB_Script)))
                sql = reader.ReadToEnd();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {

                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, sqliteConnection))
                    cmd.ExecuteNonQuery();
            }

            List<EpisodeModel> episodes = new List<EpisodeModel>();

            Assert.IsTrue(dbManager.LoadItems(out episodes));



            //TODO: add mew episoded ... save and verify that the count has changed

            //int i = dbManager.LastPersistenceId();

            episodes[0].ArtifactCountersList[0].IsDirty = true;
            episodes[0].ArtifactCountersList[1].IsDirty = true;
            episodes[0].ArtifactCountersExportedList[0].IsDirty = true;
            episodes[0].ArtifactCountersExportedList[1].IsDirty = true;

            dbManager.SaveItems(episodes);

            //verify that the data can be read - all the data types are correct
            List<EpisodeModel> episodes1 = new List<EpisodeModel>();
            Assert.IsTrue(dbManager.LoadItems(out episodes1));

            //Verify
            int EpisodesCount;
            int ArtifactCountersCount;
            int ArtifactCountersExportedCount;
            int totalNumOfConcepts;
            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {
                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Episodes", sqliteConnection))
                    EpisodesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCounters", sqliteConnection))
                    ArtifactCountersCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCountersExported", sqliteConnection))
                    ArtifactCountersExportedCount = int.Parse(cmd.ExecuteScalar().ToString());


                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ConceptNumberToColumnMapping", sqliteConnection))
                    totalNumOfConcepts = int.Parse(cmd.ExecuteScalar().ToString());

                Assert.AreEqual(EpisodesCount, 6, "Not all episoded were Saved");
                Assert.AreEqual(ArtifactCountersCount, totalNumOfConcepts*4, "Not all Contractilities were saved");
                Assert.AreEqual(ArtifactCountersExportedCount, totalNumOfConcepts * 4, "Not all Persistencies were Saved");
            }

        }
        #endregion ExportPluginLoadAllEpisodeShouldSucceedTest

        #region ExportPluginSaveIntervalDurationShouldSucceedTest

         [Ignore]
        //[TestMethod]
        public void ExportPluginSaveIntervalDurationShouldSucceedTest()
        {
            string sql;
            ExportPluginCreateNewExportDBShouldSucceedTest();

            DBManager dbManager = DBManager.Instance;

            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(EpisodeDB_Script)))
                sql = reader.ReadToEnd();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {

                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, sqliteConnection))
                    cmd.ExecuteNonQuery();
            }

            List<EpisodeModel> episodes = new List<EpisodeModel>();

            Assert.IsTrue(dbManager.LoadItems(out episodes));

            //ACT
            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {

                sqliteConnection.Open();
                EpisodeModelDAL.SaveIntervalDuration(sqliteConnection, episodes[0].EpisodeId, 55);
                //Assert.IsTrue(res, "Failed to update Interval Duration!!");

            }

            //Verify

            //TODO: checked that the value was changed and new row was added to the episode audit table

            /*
            int EpisodesCount;
            int ArtifactCountersCount;
            int ArtifactCountersExportedCount;
            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {
                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Episodes", sqliteConnection))
                    EpisodesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCounters", sqliteConnection))
                    ArtifactCountersCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCountersExported", sqliteConnection))
                    ArtifactCountersExportedCount = int.Parse(cmd.ExecuteScalar().ToString());

                Assert.AreEqual(EpisodesCount, 6, "Not all episoded were deleted");
                Assert.AreEqual(ArtifactCountersCount, 10, "Not all Contractilities were deleted");
                Assert.AreEqual(ArtifactCountersExportedCount, 8, "Not all Persistencies were deleted");
            }
            */
        }
        #endregion ExportPluginSaveIntervalDurationShouldSucceedTest

        #region ExportPluginArchiveAllEpisodeShouldSucceedTest

         [Ignore]
        //[TestMethod]
        public void ExportPluginArchiveAllEpisodeShouldSucceedTest()
        {
            string sql;
            //Prepere
            ExportPluginCreateNewExportDBShouldSucceedTest();

            DBManager dbManager = DBManager.Instance;

            using (var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(EpisodeDB_Script)))
                sql = reader.ReadToEnd();

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {

                sqliteConnection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(sql, sqliteConnection))
                    cmd.ExecuteNonQuery();
            }

            List<EpisodeModel> episodes = new List<EpisodeModel>();
            Assert.IsTrue(dbManager.LoadItems(out episodes));

            //ACT
            foreach (var episode in episodes)
            {
                Assert.IsTrue(dbManager.DischargedEpisodes(episode));
            }
            //Verify
            int totalNumOfConcepts;
            int EpisodesCount;
            int ArtifactCountersCount;
            int ArtifactCountersNullValuesCount;
            int ArtifactCountersExportedCount;
            int ArtifactCountersExportedNullValuesCount;
            int TotalNumOfCountersIntervals;

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeDBFilePath)))
            {
                sqliteConnection.Open();
             
                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Episodes", sqliteConnection))
                    EpisodesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCounters", sqliteConnection))
                    ArtifactCountersCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCountersExported", sqliteConnection))
                    ArtifactCountersExportedCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ConceptNumberToColumnMapping", sqliteConnection))
                    totalNumOfConcepts = int.Parse(cmd.ExecuteScalar().ToString());

                Assert.AreEqual(EpisodesCount, 0,"Not all episoded were deleted");
                Assert.AreEqual(ArtifactCountersCount, 0, "Not all ArtifactCounters were deleted");
                Assert.AreEqual(ArtifactCountersExportedCount, 0, "Not all ArtifactCountersExported were deleted"); 
            }

            using (SQLiteConnection sqliteConnection = new SQLiteConnection(string.Format(ConnectionStringTemplate, episodeArchiveDBFilePath)))
            {
                sqliteConnection.Open();

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from Episodes", sqliteConnection))
                    EpisodesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select sum(TotalNumOfCountersIntervals) from Episodes", sqliteConnection))
                    TotalNumOfCountersIntervals = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCounters", sqliteConnection))
                    ArtifactCountersCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCounters Where ConceptValue is null", sqliteConnection))
                    ArtifactCountersNullValuesCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCountersExported", sqliteConnection))
                    ArtifactCountersExportedCount = int.Parse(cmd.ExecuteScalar().ToString());

                using (SQLiteCommand cmd = new SQLiteCommand("select Count(*) from ArtifactCountersExported Where ConceptValue is null", sqliteConnection))
                    ArtifactCountersExportedNullValuesCount = int.Parse(cmd.ExecuteScalar().ToString());

                Assert.AreEqual(EpisodesCount, 6, "Not all episoded were Archived");
                Assert.AreEqual(TotalNumOfCountersIntervals, 4, "TotalNumOfCountersIntervals is not correct");
                Assert.AreEqual(ArtifactCountersCount, 0, "Not all Contractilities were Archived");
                Assert.AreEqual(ArtifactCountersNullValuesCount, 0, "Not all Contractilities were Archived");
                //the table ArtifactCountersExported will contain 4 times of the totalNumOfConcepts
                Assert.AreEqual(ArtifactCountersExportedCount, totalNumOfConcepts*4, "Not all Persistencies were Archived");
                //the table ArtifactCountersExported will contain 1 times of the totalNumOfConcepts, where the exported values = null
                Assert.AreEqual(ArtifactCountersExportedNullValuesCount, totalNumOfConcepts, "Not all Persistencies were Archived");
            }

        }

        #endregion ExportPluginArchiveAllEpisodeShouldSucceedTest

        #region ExportPluginArchiveAllEpisodeShouldSucceedTest
        /*
         obsolete Test 
         */
        [TestMethod]
        public void ExportPluginArchiveAllEpisodeManually()
        {
            //string sql;
            //Prepere
            ExportPluginCreateNewExportDBShouldSucceedTest();

            DBManager dbManager = DBManager.Instance;

            List<EpisodeModel> episodes = new List<EpisodeModel>();
            Assert.IsTrue(dbManager.LoadItems(out episodes));

            //ACT
            foreach(var episode in episodes)
                dbManager.DischargedEpisodes(episode);


        }

        #endregion ExportPluginArchiveAllEpisodeShouldSucceedTest

        #region ExportPluginGetConceptNumberToColumnMappingShouldSucceedTest

        [TestMethod]
        public void ExportPluginGetConceptNumberToColumnMappingShouldSucceedTest()
        {
            ExportPluginCreateNewExportDBShouldSucceedTest();

            DBManager dbManager = DBManager.Instance;

            List<ConceptNumberToColumnMappingModel> concepts = new List<ConceptNumberToColumnMappingModel>();

            Assert.IsTrue(dbManager.GetConceptNumberToColumnMapping(ref concepts));
            Assert.AreNotEqual(concepts.Count, 0, "No concepts were found..");


        }
        #endregion ExportPluginGetConceptNumberToColumnMappingShouldSucceedTest




    }
}
