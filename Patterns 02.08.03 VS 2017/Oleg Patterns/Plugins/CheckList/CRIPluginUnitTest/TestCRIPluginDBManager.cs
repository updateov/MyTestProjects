using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CRIPlugin;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using CRIAlgorithm;
using System.Threading;
using CRIPluginDataModel;

namespace CRIPluginUnitTest
{
    [TestClass]
    [DeploymentItem(@"InputData\", "InputData")]
    //[DeploymentItem(@"DBs\", "DBs")]
    [DeploymentItem(@"x64\", "x64")]
    [DeploymentItem(@"x86\", "x86")]
    public class TestCRIPluginDBManager
    {
        //private static AutoResetEvent m_waitForEvent = new AutoResetEvent(false);

        [TestInitialize]
        public void TestInitialize()
        {
            string workingFolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            //prepare
            try
            {
                File.Delete(Path.Combine(workingFolder, @"DBs\EpisodeDB.db"));
                File.Delete(Path.Combine(workingFolder, @"DBs\EpisodeArchiveDB.db"));
            }
            catch
            {

            }

            //Test
            DBManager dbManager = DBManager.Instance;
            dbManager.CreateDB();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            DBManager.Instance.Dispose();
        }

        [TestMethod]
        public void CRIPluginDBManagerSaveAndLoadEpisodesTest()
        {
            var DbManager = CRIPluginDBManager.Instance;
            int count = 10;


            List<CRIPluginEpisode> saveEpisodes = new List<CRIPluginEpisode>();
            for (int i = 0; i < count; i++)
            {
                CRIPluginEpisode episode = new CRIPluginEpisode(i);
                saveEpisodes.Add(episode);
            }

            DbManager.SaveEpisodes(saveEpisodes);
            //bool bSucc = m_waitForEvent.WaitOne(10000);
            //Assert.IsTrue(bSucc);

            List<CRIPluginEpisode> loadEpisodes = DbManager.LoadEpisodes();
            Assert.IsTrue(loadEpisodes.Count == count);

        }

        [TestMethod]
        public void CRIPluginDBManagerDischargeEpisodesTest()
        {
            var dbManager = CRIPluginDBManager.Instance;
            int count = 10;

            List<CRIPluginEpisode> saveEpisodes = new List<CRIPluginEpisode>();
            for (int i = 0; i < count; i++)
            {
                CRIPluginEpisode episode = new CRIPluginEpisode(i);
                saveEpisodes.Add(episode);
            }

            dbManager.SaveEpisodes(saveEpisodes);
            //bool bSucc1 = m_waitForEvent.WaitOne(10000);
            //Assert.IsTrue(bSucc1);

            List<CRIPluginEpisode> loadAfterSaveEpisodes = dbManager.LoadEpisodes();
            Assert.IsTrue(loadAfterSaveEpisodes.Count == count);

            List<CRIPluginEpisode> dischargeEpisodes = new List<CRIPluginEpisode>(saveEpisodes);
            dischargeEpisodes.RemoveAt(0);

            dbManager.DischargeEpisodes(dischargeEpisodes);
            //bool bSucc2 = m_waitForEvent.WaitOne(10000);
            //Assert.IsTrue(bSucc2);

            List<CRIPluginEpisode> loadAfterDischargeEpisodes = dbManager.LoadEpisodes();
            Assert.IsTrue(loadAfterDischargeEpisodes.Count == 1);
        }

        //[TestMethod]
        //public void ConvertEpisodesToEpisodesModelsTest()
        //{
        //    var laborer = CRIPluginLaborer.Instance;
        //    int id = 153;
        //    var xml = XElement.Load(@"InputData\input1.xml");
        //    CRIPluginEpisode pluginEpisode = new CRIPluginEpisode(id);
        //    pluginEpisode.EpisodeStatus = EpisodeStatus.Admitted;
        //    laborer.Episodes.TryAdd(id, pluginEpisode);
        //    laborer.UpdateEpisodesData(xml);

        //    int cnt = ValidateCount(xml);
        //    Assert.IsTrue(cnt == 12);

        //    var testEpisodesModels = laborer.Episodes.Values.ToEpisodesModels(true);
        //    var testEpisodes = testEpisodesModels.ToEpisodes();

        //    var orgContractilities = laborer.Episodes[id].ContractilitiesCalculator.Contractilities;
        //    string orgStrContractilities = null;
        //    XmlSerializer serializer = new XmlSerializer(typeof(List<Contractility>));
        //    using (StringWriter textWriter = new StringWriter())
        //    {
        //        serializer.Serialize(textWriter, orgContractilities);
        //        orgStrContractilities = textWriter.ToString();
        //    }

        //    var conContractilities = testEpisodes[id].ContractilitiesCalculator.Contractilities;
        //    string conStrContractilities = null;
        //    XmlSerializer serializer2 = new XmlSerializer(typeof(List<Contractility>));
        //    using (StringWriter textWriter = new StringWriter())
        //    {
        //        serializer2.Serialize(textWriter, orgContractilities);
        //        conStrContractilities = textWriter.ToString();
        //    }

        //    Assert.AreEqual(orgStrContractilities, conStrContractilities);
        //}

        //[TestMethod]
        //public void ConvertEpisodesToEpisodesModelsLargeDataGapsTest()
        //{
        //    var laborer = CRIPluginLaborer.Instance;
        //    int id = 666;
        //    var xml = XElement.Load(@"InputData\input - with data gaps large.xml");
        //    CRIPluginEpisode pluginEpisode = new CRIPluginEpisode(id);
        //    pluginEpisode.EpisodeStatus = EpisodeStatus.Admitted;
        //    laborer.Episodes.TryAdd(id, pluginEpisode);
        //    laborer.UpdateEpisodesData(xml);     

        //    int cnt = ValidateCount(xml);
        //    Assert.IsTrue(cnt == 14);

        //    var testEpisodesModels = laborer.Episodes.Values.ToEpisodesModels(true);
        //    var testEpisodes = testEpisodesModels.ToEpisodes();

        //    var orgContractilities = laborer.Episodes[id].ContractilitiesCalculator.Contractilities;
        //    string orgStrContractilities = null;
        //    XmlSerializer serializer = new XmlSerializer(typeof(List<Contractility>));
        //    using (StringWriter textWriter = new StringWriter())
        //    {
        //        serializer.Serialize(textWriter, orgContractilities);
        //        orgStrContractilities = textWriter.ToString();
        //    }

        //    var conContractilities = testEpisodes[id].ContractilitiesCalculator.Contractilities;
        //    string conStrContractilities = null;
        //    XmlSerializer serializer2 = new XmlSerializer(typeof(List<Contractility>));
        //    using (StringWriter textWriter = new StringWriter())
        //    {
        //        serializer2.Serialize(textWriter, orgContractilities);
        //        conStrContractilities = textWriter.ToString();
        //    }

        //    Assert.AreEqual(orgStrContractilities, conStrContractilities);
        //}

        //[TestMethod]
        //public void ConvertEpisodesToEpisodesModelsSmallDataGapsTest()
        //{
        //    var laborer = CRIPluginLaborer.Instance;
        //    int id = 154;
        //    var xml = XElement.Load(@"InputData\input - with data gaps small.xml");
        //    CRIPluginEpisode pluginEpisode = new CRIPluginEpisode(id);
        //    pluginEpisode.EpisodeStatus = EpisodeStatus.Admitted;
        //    laborer.Episodes.TryAdd(id, pluginEpisode);
        //    laborer.UpdateEpisodesData(xml);

        //    int cnt = ValidateCount(xml);
        //    Assert.IsTrue(cnt == 12);

        //    var testEpisodesModels = laborer.Episodes.Values.ToEpisodesModels(true);
        //    var testEpisodes = testEpisodesModels.ToEpisodes();

        //    var orgContractilities = laborer.Episodes[id].ContractilitiesCalculator.Contractilities;
        //    string orgStrContractilities = null;
        //    XmlSerializer serializer = new XmlSerializer(typeof(List<Contractility>));
        //    using (StringWriter textWriter = new StringWriter())
        //    {
        //        serializer.Serialize(textWriter, orgContractilities);
        //        orgStrContractilities = textWriter.ToString();
        //    }

        //    var conContractilities = testEpisodes[id].ContractilitiesCalculator.Contractilities;
        //    string conStrContractilities = null;
        //    XmlSerializer serializer2 = new XmlSerializer(typeof(List<Contractility>));
        //    using (StringWriter textWriter = new StringWriter())
        //    {
        //        serializer2.Serialize(textWriter, orgContractilities);
        //        conStrContractilities = textWriter.ToString();
        //    }

        //    Assert.AreEqual(orgStrContractilities, conStrContractilities);
        //}

        private static int ValidateCount(XElement xml)
        {
            var patient = xml.Element("patient");
            var contractilitiesNode = patient.Element("contractilities");
            var contractilities = contractilitiesNode.Elements("contractility");
            int cnt = 0;
            foreach (var item in contractilities)
            {
                cnt++;
            }

            return cnt;
        }
    }
}
