using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestableApp;

namespace TestableAppUnitTest
{
    [TestClass]
    public class TestableAppUnitTest1
    {
        public long Param1 { get; set; }
        public long Param2 { get; set; }

        public TestableClass TestedObj { get; set; }

        [TestInitialize]
        public void InitialMethod()
        {
            TestedObj = new TestableClass();
            Param1 = 12;
            Param2 = 29;
        }

        [TestMethod]
        public void TestCalculate_1_2()
        {
            var toTest = TestedObj.Calculate(1, 2);
            Assert.IsTrue(toTest == 3);
        }

        [Ignore]
        [TestMethod]
        public void TestCalculate_3_5()
        {
            var toTest = TestedObj.Calculate(3, 5);
            Assert.IsTrue(toTest == 8);
        }

        [TestMethod]
        public void TestCalculate_Param1_Param2()
        {
            var toTest = TestedObj.Calculate(Param1, Param2);
            Assert.IsTrue(toTest == 41);
        }
    }
}
