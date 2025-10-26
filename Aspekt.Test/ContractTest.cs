using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspekt.Contracts;


namespace Aspekt.Test
{
    internal class TestObject
    {
        [RequiresArgument("value", typeof(int), Contract.Comparison.LessThan, 5)]
        public void ValueMustBeLessThan5(int value)
        {
        }

        [RequiresArgument("value", typeof(int), Contract.Comparison.LessThanEqualTo, 5)]
        public void ValueMustBeLessThanEqualTo5(int value)
        {
        }

        [RequiresArgument("value", typeof(int), Contract.Comparison.GreaterThan, 5)]
        public void ValueMustBeGreaterThan5(int value)
        {
        }

        [RequiresArgument("value", typeof(int), Contract.Comparison.GreaterThanEqualTo, 5)]
        public void ValueMustBeGreaterThanEqualTo5(int value)
        {
        }

        [NotNull("value")]
        public void ValueNotNull(string value)
        {
        }

        [RequiresArgument("value", typeof(string), Contract.Constraint.NotNull)]
        public void ValueNotNullConstraint(string value)
        {
        }

    }


    [TestClass]
    public class ContractTest
    {
        [TestMethod]
        public void TestRequiresArgumentLessThan()
        {
            var to = new TestObject();
            to.ValueMustBeLessThan5(4);
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestRequiresArgumentLessThanViolatedEqualTo()
        {
            var to = new TestObject();
            to.ValueMustBeLessThan5(5);
        }
        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestRequiresArgumentLessThanViolated()
        {
            var to = new TestObject();
            to.ValueMustBeLessThan5(6);
        }

        [TestMethod]
        public void TestRequiresArgumentLessThanEqualTo()
        {
            var to = new TestObject();
            to.ValueMustBeLessThanEqualTo5(int.MinValue);
            to.ValueMustBeLessThanEqualTo5(4);
            to.ValueMustBeLessThanEqualTo5(5);
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestRequiresArgumentLessThanEqualToViolated()
        {
            var to = new TestObject();
            to.ValueMustBeLessThanEqualTo5(6);
        }

        [TestMethod]
        public void TestRequiresArgumentGreaterThan()
        {
            var to = new TestObject();
            to.ValueMustBeGreaterThan5(int.MaxValue);
            to.ValueMustBeGreaterThan5(10);
            to.ValueMustBeGreaterThan5(6);
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestRequiresArgumentGreaterThanViolated()
        {
            var to = new TestObject();
            to.ValueMustBeGreaterThan5(5);
        }

        [TestMethod]
        public void TestRequiresArgumentGreaterThanEqualTo()
        {
            var to = new TestObject();
            to.ValueMustBeGreaterThanEqualTo5(int.MaxValue);
            to.ValueMustBeGreaterThanEqualTo5(10);
            to.ValueMustBeGreaterThanEqualTo5(5);
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestRequiresArgumentLessGreaterEqualToViolated()
        {
            var to = new TestObject();
            to.ValueMustBeGreaterThanEqualTo5(4);
        }

        [TestMethod]
        [ExpectedException(typeof(ContractViolatedException))]
        public void TestRequiresArgumentNotNull()
        {
            var to = new TestObject();
            to.ValueNotNullConstraint(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestNotNull()
        {
            var to = new TestObject();
            to.ValueNotNull(null);
        }

    }
}
