using System;
using Aspekt.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspekt.Test
{

    [FieldInvariant("myInt", Contract.Comparison.NotEqualTo, 5)]
    [PropertyInvariant(nameof(MyProperty), Contract.Constraint.NotNull)]
    class InvariantClass
    {
        int myInt = 0;
        public string MyProperty { get; set; } = "NotNull";

        public int GetMyInt()
        {
            return myInt;
        }

        public void SetMyInt(int value)
        {
            myInt = value;
        }
    }

    [TestClass]
    public class InvariantTest
    {
        [TestMethod]
        public void TestSetFieldSuccess()
        {
            var c = new InvariantClass();
            c.SetMyInt(4);
            Assert.AreEqual(4, c.GetMyInt());
            c.SetMyInt(6);
            Assert.AreEqual(6, c.GetMyInt());
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestSetFieldViolateInvariant()
        {
            var c = new InvariantClass();
            c.SetMyInt(5);
        }

        [TestMethod]
        public void TestSetPropertySuccess()
        {
            var c = new InvariantClass();
            c.MyProperty = "Value";
            Assert.AreEqual("Value", c.MyProperty);
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestSetPropertyViolateInvariant()
        {
            var c = new InvariantClass();
            c.MyProperty = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestSetPropertyViolateInvariantCtor()
        {
            var c = new InvariantClass { MyProperty = null };
        }
    }
}
