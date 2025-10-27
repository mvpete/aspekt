using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspekt.Contracts.Aspects;

namespace Aspekt.Contracts.Test
{
    [TestClass]
    public class ParameterContractTests
    {
        [TestMethod]
        public void TestParameterNotNullAttribute()
        {
            // Test that parameter-level NotNull attribute works
            var result = ProcessWithNotNull("valid string");
            Assert.AreEqual("valid string", result);

            // Test that null parameter throws exception
            Assert.ThrowsException<ArgumentNullException>(() => ProcessWithNotNull(null));
        }

        [TestMethod]
        public void TestParameterRequireAttribute()
        {
            // Test that parameter-level Require attribute works
            var result = ProcessWithRequire("valid string");
            Assert.AreEqual("valid string", result);

            // Test that null parameter throws exception
            Assert.ThrowsException<PreconditionException>(() => ProcessWithRequire(null));
        }

        // Test methods with parameter-level attributes
        public string ProcessWithNotNull([NotNullAttribute] string input)
        {
            return input;
        }

        public string ProcessWithRequire([Require(Contract.Constraint.NotNull)] string input)
        {
            return input;
        }
    }
}