using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TestableAppWithModel;

namespace TestableAppWithModelUnitTest
{
    [TestClass]
    public class TestableAppWithModelUnitTest1
    {
        [TestMethod]
        public void TestMoq1()
        {
            var moqPerson = new Mock<IModelClass>();
            moqPerson.Setup(c => c.ValidateModel()).Returns(true);
            var mainModel = new ModelMainClass(moqPerson.Object);
            Assert.IsTrue(mainModel.F());
        }
    }
}
