using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspekt.Contracts;
using Aspekt.Contracts.Aspects;

namespace Aspekt.Contracts.Test
{
    [TestClass]
    public class ParameterValidationDemo
    {
        [TestMethod]
        public void TestCurrentWorkingParameterValidation()
        {
            var demo = new ParameterValidationExamples();

            // Test working method-level parameter contracts
            demo.ValidateWithRequire("valid string");
            
            // Test null parameter throws exception
            Assert.ThrowsException<PreconditionException>(() => demo.ValidateWithRequire(null));
        }

        [TestMethod]
        public void TestMultipleParameterValidation()
        {
            var demo = new ParameterValidationExamples();

            // Test method with simple parameter contracts (only NotNull for now)
            demo.ValidateMultipleParametersSimple("valid", "test");
            
            // Test null first parameter
            Assert.ThrowsException<PreconditionException>(() => demo.ValidateMultipleParametersSimple(null, "test"));
            
            // Test null second parameter  
            Assert.ThrowsException<PreconditionException>(() => demo.ValidateMultipleParametersSimple("valid", null));
        }

        [TestMethod]
        public void TestParameterLevelSyntax()
        {
            var demo = new ParameterValidationExamples();

            // Test parameter-level attributes work
            demo.ParameterLevelSyntax("valid", 5);
            
            // Test null parameter throws exception
            Assert.ThrowsException<ArgumentNullException>(() => demo.ParameterLevelSyntax(null, 5));
            
            // Test invalid value throws exception
            Assert.ThrowsException<PreconditionException>(() => demo.ParameterLevelSyntax("valid", 0));
        }
    }

    public class ParameterValidationExamples
    {
        /// <summary>
        /// Current working approach - method-level attributes that specify parameter names
        /// This works with the current Aspekt weaver implementation.
        /// </summary>
        [Require("input", Contract.Constraint.NotNull)]
        public void ValidateWithRequire(string input)
        {
            Console.WriteLine($"Processing: {input}");
        }

        /// <summary>
        /// Multiple parameter validation using method-level attributes (simplified)
        /// </summary>
        [Require("name", Contract.Constraint.NotNull)]
        [Require("other", Contract.Constraint.NotNull)]
        public void ValidateMultipleParametersSimple(string name, string other)
        {
            Console.WriteLine($"Processing: {name}, {other}");
        }

        /// <summary>
        /// New parameter-level syntax that should now work with weaver enhancements
        /// </summary>
        public void ParameterLevelSyntax([NotNullAttribute] string input, [Require(Contract.Comparison.GreaterThan, 1)] int value)
        {
            Console.WriteLine($"Parameter syntax: {input} = {value}");
        }
    }
}