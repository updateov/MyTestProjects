using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestableModule;

namespace TestableModuleUnitTest
{
    [TestClass]
    public class TestableClassUnitTest
    {
        [TestMethod]
        public void Reverse_TestMethod1()
        {
            var tc = new TestableClass();
            var res = tc.Reverse("123456");
            Assert.AreEqual(res, "654321");
        }

        [TestMethod]
        public void Reverse_FailTEstMethod1()
        {
            var tc = new TestableClass();
            var res = tc.Reverse("123456");
            Assert.AreNotEqual(res, "134652");
        }
    }
}
