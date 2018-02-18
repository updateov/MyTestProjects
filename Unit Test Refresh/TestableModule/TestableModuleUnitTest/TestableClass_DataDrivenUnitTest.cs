using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using TestableModule;

namespace TestableModuleUnitTest
{
    [TestClass]
    [DeploymentItem(@"InputData\", "InputData")]
    public class TestableClass_DataDrivenUnitTest
    {
        [TestMethod]
        public void Reverse_DataDrivenTestMethod1()
        {
            var xml = XElement.Load(@"InputData\input.xml");
            var tc = new TestableClass();
            foreach (var item in xml.Elements("string"))
            {
                var res = tc.Reverse(item.Attribute("inStr").Value);
                Assert.AreEqual(res, item.Attribute("outStr").Value);
            }
        }
    }
}
