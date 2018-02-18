using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MedianFilter;

namespace MedianFilterTest
{
    [TestClass]
    public class MedianFilterUnitTest
    {
        [TestMethod]
        public void TestMedianFilter()
        {
            MedianFilterCalculator calc = new MedianFilterCalculator();
            //byte[] bytes1 = new byte[100];
            //Random rnd1 = new Random();

            //rnd1.NextBytes(bytes1);
            //String toOut = String.Empty;
            //foreach (var item in bytes1)
            //{
            //    toOut += item + ", ";
            //}

            //System.Diagnostics.Trace.Write(toOut);
            calc.TracingsData.AddRange(new byte[] { 203, 116, 222, 111, 18, 212, 241, 3, 105, 207, 53, 72, 201, 93, 103, 221, 218, 216, 251, 37, 23, 110, 215, 16, 146, 195, 11, 240, 39, 72, 33, 11, 58, 117, 255, 177, 45, 22, 60, 47, 43, 226, 248, 126, 72, 241, 40, 246, 225, 233, 214, 249, 160, 121, 147, 92, 157, 205, 221, 78, 200, 1, 219, 33, 174, 41, 13, 83, 94, 181, 175, 196, 155, 204, 249, 53, 118, 88, 199, 161, 155, 20, 14, 62, 114, 39, 106, 193, 226, 162, 20, 95, 56, 237, 102, 42, 7, 215, 207, 30 });

            calc.CalculateMedian();

            String str = String.Empty;
            foreach (var item in calc.Median20MinResult)
            {
                str += item + ", ";
            }

            System.Diagnostics.Trace.WriteLine(str);
            str = String.Empty;
            foreach (var item in calc.Median8MinResult)
            {
                str += item + ", ";
            }

            System.Diagnostics.Trace.WriteLine(str);
        }
    }
}
