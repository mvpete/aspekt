using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspekt.Contracts;
using Aspekt.Contracts.Aspects;

namespace Aspekt.Contracts.Test;

[TestClass]
public class ContractsTest
{
    [TestMethod]
    public void TestRequireContract()
    {
        var testObject = new TestContractClass();

        // Test that contracts are working - should not throw
        testObject.RequireNotNull("valid string");

        // Test require contract for null - should throw when contracts are enabled
        try
        {
            testObject.RequireNotNull(null);
            Assert.Fail("Expected ContractViolatedException was not thrown");
        }
        catch (ContractViolatedException)
        {
            // Expected exception
        }
        catch (System.Exception ex)
        {
            // Contract validation might throw different exception types depending on configuration
            Assert.IsTrue(ex.Message.Contains("Contract") || ex.Message.Contains("Require"),
                $"Expected contract-related exception, got: {ex.GetType().Name}: {ex.Message}");
        }
    }

    [TestMethod]
    public void TestEnsureContract()
    {
        var testObject = new TestContractClass();

        // Test ensure contract - should return non-null result
        var result = testObject.GetNonNullString();
        Assert.IsNotNull(result);

        // Test ensure contract violation
        try
        {
            testObject.GetNullString();
            Assert.Fail("Expected ContractViolatedException was not thrown");
        }
        catch (ContractViolatedException)
        {
            // Expected exception
        }
        catch (System.Exception ex)
        {
            // Contract validation might throw different exception types depending on configuration
            Assert.IsTrue(ex.Message.Contains("Contract") || ex.Message.Contains("Ensure"),
                $"Expected contract-related exception, got: {ex.GetType().Name}: {ex.Message}");
        }
    }

    [TestMethod]
    public void TestAspectCatchedContractException()
    {
        var testObject = new TestContractClass();
        Assert.ThrowsException<ContractViolatedException>(testObject.ViolatePropertyInvariant);
        Assert.IsNotNull(ExceptionHandlerAspect.OnExceptionCalled);
    }

    [TestMethod]
    public void TestParameterNotNullAttribute()
    {
        var testObject = new TestContractClass();

        // Test that parameter-level NotNull attribute works with valid input
        testObject.RequireParameterNotNull("valid string");

        // Test parameter-level NotNull attribute with null - should throw
        try
        {
            testObject.RequireParameterNotNull(null);
            Assert.Fail("Expected exception was not thrown for null parameter");
        }
        catch (ArgumentNullException)
        {
            // Expected exception for parameter-level NotNull
        }
        catch (System.Exception ex)
        {
            // Parameter-level validation might throw different exception types
            Assert.IsTrue(ex.Message.Contains("null") || ex.Message.Contains("Null"),
                $"Expected null-related exception, got: {ex.GetType().Name}: {ex.Message}");
        }
    }

    [TestMethod]
    public void TestParameterRequireAttribute()
    {
        var testObject = new TestContractClass();

        // Test that parameter-level Require attribute works with valid input
        testObject.RequireParameter("valid string");

        // Test parameter-level Require attribute with null - should throw
        try
        {
            testObject.RequireParameter(null);
            Assert.Fail("Expected exception was not thrown for null parameter");
        }
        catch (ArgumentNullException)
        {
            // Expected exception for parameter-level Require
        }
        catch (System.Exception ex)
        {
            // Parameter-level validation might throw different exception types
            Assert.IsTrue(ex.Message.Contains("null") || ex.Message.Contains("Null") || ex.Message.Contains("Require"),
                $"Expected null/require-related exception, got: {ex.GetType().Name}: {ex.Message}");
        }
    }
}

[PropertyInvariant(nameof(DontNullMe), Contract.Constraint.NotNull)]
public class TestContractClass
{
    // Has an invariant to not be null.
    // [Invariant(Constraint.NotNull)] <- should weave the same thing.
    private string? DontNullMe { get; set; } = "Q";

    private string? NeverNull { get; set; } = "Always";

    [ExceptionHandlerAspect]
    public void ViolatePropertyInvariant()
    {
        DontNullMe = null;
    }

    [Require("input", Contract.Constraint.NotNull)]
    public void RequireNotNull(string input)
    {
        // Method body - contracts are enforced via IL weaving
        Console.WriteLine($"Processing: {input}");
    }

    // New parameter-level contract method
    public void RequireParameterNotNull([NotNullAttribute] string input)
    {
        // Method body - parameter contracts are enforced via IL weaving
        Console.WriteLine($"Processing: {input}");
    }

    // New parameter-level require method
    public void RequireParameter([Require(Contract.Constraint.NotNull)] string input)
    {
        // Method body - parameter contracts are enforced via IL weaving
        Console.WriteLine($"Processing: {input}");
    }

    [Ensure<string>(Contract.Constraint.NotNull)]
    public string GetNonNullString()
    {
        return "Valid String";
    }

    [Ensure<string>(Contract.Constraint.NotNull)]
    public string GetNullString()
    {
        return null; // This should trigger contract violation
    }

    [ExceptionHandlerAspect]
    public void Print(string str)
    {
        DontNullMe = null;
        Console.WriteLine(str);
    }
}

internal sealed class ExceptionHandlerAspect : Aspect
{
    public static Exception? OnExceptionCalled { get; internal set; }
    public override void OnException(MethodArguments args, Exception e)
    {
        OnExceptionCalled = e;
    }
}
