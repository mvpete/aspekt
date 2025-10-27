using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspekt.Contracts.Aspects;

namespace Aspekt.Contracts.Test
{
    [TestClass]
    public class SimpleParameterTest
    {
        [TestMethod]
        public void TestSimpleNotNull()
        {
            // Test that simple parameter-level NotNull works
            var test = new SimpleTestClass();
            
            // Valid call should work
            test.SimpleMethod("valid");
            
            // Null should throw
            Assert.ThrowsException<ArgumentNullException>(() => test.SimpleMethod(null));
        }
        
        [TestMethod]
        public void TestSimpleRequire()
        {
            // Test that simple parameter-level Require (defaults to NotNull) works
            var test = new SimpleTestClass();
            
            // Valid call should work
            test.SimpleRequireStringMethod("valid");
            
            // Null value should throw
            Assert.ThrowsException<PreconditionException>(() => test.SimpleRequireStringMethod(null));
        }
        
        [TestMethod]
        public void TestRequireWithComparison()
        {
            // Test that parameter-level Require with comparison works
            var test = new SimpleTestClass();
            
            // Valid call should work
            test.SimpleRequireComparisonMethod(5);
            
            // Invalid value should throw
            Assert.ThrowsException<PreconditionException>(() => test.SimpleRequireComparisonMethod(0));
        }

        [TestMethod]
        public void TestMethodLevelRequireComparison()
        {
            // Test that method-level Require with comparison works (for comparison with parameter-level)
            var test = new SimpleTestClass();
            
            // Valid call should work
            test.MethodLevelRequireComparison(5);
            
            // Invalid value should throw
            Assert.ThrowsException<PreconditionException>(() => test.MethodLevelRequireComparison(0));
        }
    }

    public class SimpleTestClass
    {
        public void SimpleMethod([NotNullAttribute] string input)
        {
            Console.WriteLine($"Input: {input}");
        }
        
        public void SimpleRequireStringMethod([Require(Contract.Constraint.NotNull)] string value)
        {
            Console.WriteLine($"Value: {value}");
        }
        
        public void SimpleRequireComparisonMethod([Require(Contract.Comparison.GreaterThan, 1)] int value)
        {
            Console.WriteLine($"Value: {value}");
        }

        // Method-level version for comparison
        [Require("value", Contract.Comparison.GreaterThan, 1)]
        public void MethodLevelRequireComparison(int value)
        {
            Console.WriteLine($"Method level value: {value}");
        }
    }
}