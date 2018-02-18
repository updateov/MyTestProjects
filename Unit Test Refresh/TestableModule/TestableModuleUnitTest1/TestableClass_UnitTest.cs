using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestableModule;

namespace TestableModuleUnitTest1
{
    [TestClass]
    public class TestableClass_UnitTest
    {
        TestableClass m_tc = null;

        [TestInitialize]
        public void Startup()
        {
            m_tc = new TestableClass();
        }

        [TestCleanup]
        public void Finish()
        {
            m_tc = null;
        }

        [TestMethod]
        public void Reverse_TestMethod2()
        {
            //TestableClass tc = new TestableClass();
            var result = m_tc.Reverse("123456");
            Assert.AreEqual(result, "654321");
        }

        [TestMethod]
        public void Reverse_NegativrTestMethod2()
        {
            TestableClass tc = new TestableClass();
            var result = tc.Reverse("123456");
            Assert.AreNotEqual(result, "1235");
        }

        [TestMethod]
        public void Reverse_EmptyString()
        {
            TestableClass tc = new TestableClass();
            var result = tc.Reverse(String.Empty);
            Assert.AreEqual(result, String.Empty);
        }
    }
}
