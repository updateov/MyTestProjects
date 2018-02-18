using System;
using CRIAlgorithm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using CRIPlugin;
using System.Collections.Generic;
using System.Collections;
using PatternsEntities;

namespace CRIAlgorithmUnitTest
{
    [TestClass]
    [DeploymentItem(@"InputData\", "InputData")]
    public class CalculateContractilityUnitTest
    {
        #region Algorithm Calculation Tests

         [Ignore]
        //[TestMethod]
        public void CalculateContractilityTest()
        {
            var dataFeed = CRIPluginLaborer.Instance;
            var xml = XElement.Load(@"InputData\input1.xml");
            CRIPluginEpisode pluginEpisode = new CRIPluginEpisode(153);
            pluginEpisode.EpisodeStatus = EpisodeStatus.Admitted;
            pluginEpisode.TracingStatus = TracingStatus.Live;
            dataFeed.Episodes.TryAdd(153, pluginEpisode);
            dataFeed.UpdateEpisodesData(xml);
            //xml.Save(@"InputData\output1.xml");       
            int cnt = ValidateCount(xml);
            Assert.IsTrue(cnt == 11);
        }

         [Ignore]
        //[TestMethod]
        public void CalculateContractilityWithLargeDataGapsTest()
        {
            var dataFeed = CRIPluginLaborer.Instance;
            var xml = XElement.Load(@"InputData\input - with data gaps large.xml");
            CRIPluginEpisode pluginEpisode = new CRIPluginEpisode(666);
            pluginEpisode.EpisodeStatus = EpisodeStatus.Admitted;
            pluginEpisode.TracingStatus = TracingStatus.Live;            
            dataFeed.Episodes.TryAdd(666, pluginEpisode);
            dataFeed.UpdateEpisodesData(xml);
            //xml.Save(@"InputData\output - with data gaps large.xml");
            int cnt = ValidateCount(xml);
            Assert.IsTrue(cnt == 13);
        }

        [Ignore]
        //[TestMethod]
        public void CalculateContractilityWithSmallDataGapsTest()
        {
            var dataFeed = CRIPluginLaborer.Instance;
            var xml = XElement.Load(@"InputData\input - with data gaps small.xml");
            CRIPluginEpisode pluginEpisode = new CRIPluginEpisode(154);
            pluginEpisode.EpisodeStatus = EpisodeStatus.Admitted;
            pluginEpisode.TracingStatus = TracingStatus.Live;
            dataFeed.Episodes.TryAdd(154, pluginEpisode);
            dataFeed.UpdateEpisodesData(xml);
            //xml.Save(@"InputData\output - with data gaps small.xml");
            int cnt = ValidateCount(xml);
            Assert.IsTrue(cnt == 11);
        }

        #endregion

        #region CRI State Tests

        [TestMethod]
        public void CalculateReasonAccelsVariabilityNegativeCRI_Accels_0_Var_6_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.LastMeanBaselineVariability = 6.1f;
            calc.EventsRate[0].AccelerationRate = 0;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.Accels.IsReason || result.Variability.IsReason);
        }

        [TestMethod]
        public void CalculateReasonAccelsVariabilityNegativeCRI_Accels_1_Var_6_9_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.LastMeanBaselineVariability = 6.9f;
            calc.EventsRate[0].AccelerationRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.Accels.IsReason || result.Variability.IsReason);
        }

        [TestMethod]
        public void CalculateReasonAccelsVariabilityNegativeCRI_Accels_1_Var_5_9_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.LastMeanBaselineVariability = 5.9f;
            calc.EventsRate[0].AccelerationRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.Accels.IsReason || result.Variability.IsReason);
        }

        [TestMethod]
        public void CalculateReasonAccelsVariabilityPositiveCRI_Accels_0_Var_5_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.LastMeanBaselineVariability = 5.1f;
            calc.EventsRate[0].AccelerationRate = 0;
            var result = calc.LastCalculatedEvents;
            Assert.IsTrue(result.Accels.IsReason || result.Variability.IsReason);
        }

        [TestMethod]
        public void CalculateReasonLateDecelsNegativeCRI_Lates_0_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].LateDecelRate = 0;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.LateDecels.IsReason);
        }

        [TestMethod]
        public void CalculateReasonLateDecelsNegativeCRI_Lates_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].LateDecelRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.LateDecels.IsReason);
        }

        [TestMethod]
        public void CalculateReasonLateDecelPositiveCRI_Lates_2_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].LateDecelRate = 2;
            var result = calc.LastCalculatedEvents;
            Assert.IsTrue(result.LateDecels.IsReason);
        }

        [TestMethod]
        public void CalculateReasonLateDecelPositiveCRI_Lates_1_Proloned_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].LateDecelRate = 1;
            calc.EventsRate[0].ProlongedDecelRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsTrue(result.LateDecels.IsReason && result.ProlongedDecels.IsReason);
        }

        [TestMethod]
        public void CalculateReasonLateDecelPositiveCRI_Lates_1_LargeLong_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].LateDecelRate = 1;
            calc.EventsRate[0].LongAndLargeDecelRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsTrue(result.LateDecels.IsReason && result.LargeDeceles.IsReason);
        }

        [TestMethod]
        public void CalculateReasonProlongedDecelNegativeCRI_Prolonged_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].ProlongedDecelRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.ProlongedDecels.IsReason);
        }

        [TestMethod]
        public void CalculateReasonProlongedDecelNegativeCRI_Prolonged_1_LargeLong_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].ProlongedDecelRate = 1;
            calc.EventsRate[0].LongAndLargeDecelRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.ProlongedDecels.IsReason);
        }

        [TestMethod]
        public void CalculateReasonProlongedDecelNegativeCRI_LargeLong_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].ProlongedDecelRate = 1;
            calc.EventsRate[0].LongAndLargeDecelRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.LargeDeceles.IsReason);
        }

        [TestMethod]
        public void CalculateReasonProlongedDecelNegativeCRI_LargeLong_1_Prolonged_1_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].ProlongedDecelRate = 1;
            calc.EventsRate[0].LongAndLargeDecelRate = 1;
            var result = calc.LastCalculatedEvents;
            Assert.IsFalse(result.LargeDeceles.IsReason);
        }

        [TestMethod]
        public void CalculateReasonLateDecelPositiveCRI_LargeLong_3_Test()
        {
            TestCRICalculator calc = new TestCRICalculator();
            calc.EventsRate[0].LongAndLargeDecelRate = 3;
            var result = calc.LastCalculatedEvents;
            Assert.IsTrue(result.LargeDeceles.IsReason);
        }

        #endregion

        //[TestMethod]
        //public void AppendAndCleanUPTracing()
        //{
        //    TestCRICalculator calc = new TestCRICalculator();
        //    List<byte> toAdd = new List<byte>();
        //    for (int i = 0; i < 60; i++)
        //    {
        //        toAdd.AddRange(new byte[] { 10, 10, 10, 12, 13, 15, 35, 40, 50, 55, 70, 85, 84, 83, 84, 83, 84, 83, 82, 81, 80, 77, 75, 73, 70, 66, 61, 57, 53, 50, 43, 35, 30, 26, 24, 23, 20, 15, 13, 10, 10 });
        //    }

        //    for (int i = 0; i < 600; i++)
        //    {
        //        toAdd.Add(127);
        //    }

        //    calc.AppendUPTracings(false, 1428996351, toAdd);
        //    calc.LastCalculatedContractility = calc.NextUPTime - 355;
        //    calc.CleanUPTracings();

        //    var size = calc.UPTracing.Count;
        //    Assert.IsTrue(size == calc.ContractilityQualificationWindow + 355);
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
