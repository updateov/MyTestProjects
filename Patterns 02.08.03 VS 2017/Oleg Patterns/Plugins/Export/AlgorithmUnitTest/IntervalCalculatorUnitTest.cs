using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Export.Algorithm;

namespace ExportAlgorithmUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private TestIntervalCalculator IntervalsCalculator { get; set; }

        [TestInitialize]
        public void TestInitialize()
        {
            IntervalsCalculator = new TestIntervalCalculator();
            IntervalsCalculator.FillTestContractions();
        }

        [TestMethod]
        public void CalculateCTRDuration_TestMethod()
        {
            Random rnd1 = new Random();
            Random rnd2 = new Random();
            int indStart = rnd1.Next(0, 24);
            int indEnd = rnd2.Next(indStart + 2, 49);
            long start = IntervalsCalculator.GetCTRStartTime(indStart);
            long end = IntervalsCalculator.GetCTREndTime(indEnd);
            List<String> durationRangeList = IntervalsCalculator.GetContractionDurationRange(start, end);
            String durationRange = String.Empty;
            if (durationRangeList != null && durationRangeList.Count == 2)
            {
                durationRange = durationRangeList[1];
            }

            long minDur = -1, maxDur = -1;
            for (int i = indStart; i <= indEnd; i++)
            {
                if (IntervalsCalculator.m_durations[i] < minDur || minDur <= -1)
                    minDur = IntervalsCalculator.m_durations[i];

                if (IntervalsCalculator.m_durations[i] > maxDur || maxDur <= -1)
                    maxDur = IntervalsCalculator.m_durations[i];
            }

            minDur = (long)(Math.Round(minDur / 5f, MidpointRounding.AwayFromZero) * 5f);
            maxDur = (long)(Math.Round(maxDur / 5f, MidpointRounding.AwayFromZero) * 5f);
            String toRet = String.Empty;
            if (minDur == maxDur)
                toRet = minDur.ToString();
            else
                toRet = minDur.ToString() + " - " + maxDur.ToString();

            Console.Out.WriteLine("Calculated DurationLength = {0}, TestDurationLength = {1}", durationRange, toRet);
            Assert.IsTrue(toRet.Equals(durationRange));
        }
    }
}
