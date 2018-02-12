using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspekt.Contracts;
using Aspekt.Contracts.Contracts;

namespace Aspekt.Test
{
    class TestObject
    {
        [RequiresArgument("value", typeof(int), Contract.Comparison.LessThan, 5)]
        public void ValueMustBeLessThan5(int value)
        {
        }
    }


    [TestClass]
    public class ContractTest
    {
        [TestMethod]
        public void TestRequiresArgumentLessThan()
        {
            TestObject to = new TestObject();
            to.ValueMustBeLessThan5(4);
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestRequiresArgumentLessThanViolatedEqualTo()
        {
            TestObject to = new TestObject();
            to.ValueMustBeLessThan5(5);
        }
        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestRequiresArgumentLessThanViolated()
        {
            TestObject to = new TestObject();
            to.ValueMustBeLessThan5(6);
        }

        [TestMethod]
        public void TestRequiresArgumentLessThanEqualTo()
        {
            Assert.Fail();
        }
        [TestMethod]
        public void TestRequiresArgumentLessThanEqualToViolated()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void TestRequiresArgumentGreaterThan()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void TestRequiresArgumentGreaterThanViolated()
        {
            Assert.Fail();
        }

        [TestMethod]
        public void TestRequiresArgumentGreaterThanEqualTo()
        {
            Assert.Fail();
        }
        [TestMethod]
        public void TestRequiresArgumentLessGreaterEqualToViolated()
        {
            Assert.Fail();
        }

    }
}
