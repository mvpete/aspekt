using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Aspekt.Test
{
    #region Test Attributes

    /// <summary>
    /// Attribute that always continues with normal execution
    /// </summary>
    internal sealed class ContinueExecutionAttribute : Aspect
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitCalled { get; set; } = false;
        public static int MethodExecutionCount { get; set; } = 0;

        public ContinueExecutionAttribute()
        {
            OnEntryCalled = false;
            OnExitCalled = false;
        }

        public override void OnEntry(MethodArguments args)
        {
            OnEntryCalled = true;
            args.Action = ExecutionAction.Continue;
        }

        public override void OnExit(MethodArguments args)
        {
            OnExitCalled = true;
        }
    }

    /// <summary>
    /// Attribute that skips method execution but calls OnExit
    /// </summary>
    internal sealed class SkipMethodAttribute : Aspect
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitCalled { get; set; } = false;
        public static int MethodExecutionCount { get; set; } = 0;

        public SkipMethodAttribute()
        {
            OnEntryCalled = false;
            OnExitCalled = false;
        }

        public override void OnEntry(MethodArguments args)
        {
            OnEntryCalled = true;
            args.Action = ExecutionAction.SkipMethod;
        }

        public override void OnExit(MethodArguments args)
        {
            OnExitCalled = true;
        }
    }

    /// <summary>
    /// Attribute that skips both method execution and OnExit
    /// </summary>
    internal sealed class SkipMethodAndExitAttribute : Aspect
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitCalled { get; set; } = false;
        public static int MethodExecutionCount { get; set; } = 0;

        public SkipMethodAndExitAttribute()
        {
            OnEntryCalled = false;
            OnExitCalled = false;
        }

        public override void OnEntry(MethodArguments args)
        {
            OnEntryCalled = true;
            args.Action = ExecutionAction.SkipMethodAndExit;
        }

        public override void OnExit(MethodArguments args)
        {
            OnExitCalled = true;
        }
    }

    /// <summary>
    /// Attribute that conditionally skips based on parameter value
    /// </summary>
    internal sealed class ConditionalExecutionAttribute : Aspect
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitCalled { get; set; } = false;

        public ConditionalExecutionAttribute()
        {
            OnEntryCalled = false;
            OnExitCalled = false;
        }

        public override void OnEntry(MethodArguments args)
        {
            OnEntryCalled = true;
            if (args.Arguments.Count > 0)
            {
                var value = (int)args.Arguments.GetArgumentValueByName("value");
                if (value <= 0)
                {
                    args.Action = ExecutionAction.SkipMethod;
                }
            }
        }

        public override void OnExit(MethodArguments args)
        {
            OnExitCalled = true;
        }
    }

    #endregion

    #region Test Classes

    internal class ExecutionActionTestClass
    {
        public static int ContinueMethodCallCount = 0;
        public static int SkipMethodCallCount = 0;
        public static int SkipAllMethodCallCount = 0;
        public static int ConditionalMethodCallCount = 0;

        [ContinueExecution]
        public void ContinueMethod()
        {
            ContinueMethodCallCount++;
        }

        [ContinueExecution]
        public int ContinueMethodWithReturn()
        {
            ContinueMethodCallCount++;
            return 42;
        }

        [ContinueExecution]
        public string ContinueMethodWithStringReturn()
        {
            ContinueMethodCallCount++;
            return "executed";
        }

        [SkipMethod]
        public void SkipMethodVoid()
        {
            SkipMethodCallCount++;
        }

        [SkipMethod]
        public int SkipMethodWithReturn()
        {
            SkipMethodCallCount++;
            return 42;
        }

        [SkipMethod]
        public string SkipMethodWithStringReturn()
        {
            SkipMethodCallCount++;
            return "executed";
        }

        [SkipMethodAndExit]
        public void SkipAllVoid()
        {
            SkipAllMethodCallCount++;
        }

        [SkipMethodAndExit]
        public int SkipAllWithReturn()
        {
            SkipAllMethodCallCount++;
            return 42;
        }

        [SkipMethodAndExit]
        public string SkipAllWithStringReturn()
        {
            SkipAllMethodCallCount++;
            return "executed";
        }

        [ConditionalExecution]
        public void ConditionalMethod(int value)
        {
            ConditionalMethodCallCount++;
        }

        [ConditionalExecution]
        public int ConditionalMethodWithReturn(int value)
        {
            ConditionalMethodCallCount++;
            return value * 2;
        }

        [ConditionalExecution]
        public string ConditionalMethodWithStringReturn(int value)
        {
            ConditionalMethodCallCount++;
            return $"Value: {value}";
        }

        // Async methods for testing error handling
        [SkipMethod]
        public async System.Threading.Tasks.Task AsyncMethodWithSkip()
        {
            await System.Threading.Tasks.Task.Delay(1);
        }

        [SkipMethodAndExit]
        public async System.Threading.Tasks.Task<int> AsyncMethodWithSkipAndExitReturningValue()
        {
            await System.Threading.Tasks.Task.Delay(1);
            return 42;
        }
    }

    #endregion

    [TestClass]
    public class ExecutionActionTest
    {
        [TestInitialize]
        public void ResetStaticState()
        {
            // Reset Continue attribute state
            ContinueExecutionAttribute.OnEntryCalled = false;
            ContinueExecutionAttribute.OnExitCalled = false;
            ContinueExecutionAttribute.MethodExecutionCount = 0;

            // Reset SkipMethod attribute state
            SkipMethodAttribute.OnEntryCalled = false;
            SkipMethodAttribute.OnExitCalled = false;
            SkipMethodAttribute.MethodExecutionCount = 0;

            // Reset SkipMethodAndExit attribute state
            SkipMethodAndExitAttribute.OnEntryCalled = false;
            SkipMethodAndExitAttribute.OnExitCalled = false;
            SkipMethodAndExitAttribute.MethodExecutionCount = 0;

            // Reset ConditionalExecution attribute state
            ConditionalExecutionAttribute.OnEntryCalled = false;
            ConditionalExecutionAttribute.OnExitCalled = false;

            // Reset method call counts
            ExecutionActionTestClass.ContinueMethodCallCount = 0;
            ExecutionActionTestClass.SkipMethodCallCount = 0;
            ExecutionActionTestClass.SkipAllMethodCallCount = 0;
            ExecutionActionTestClass.ConditionalMethodCallCount = 0;
        }

        #region Continue Tests

        [TestMethod]
        public void Continue_VoidMethod_ShouldExecuteNormally()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            testClass.ContinueMethod();

            // Assert
            Assert.IsTrue(ContinueExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ContinueExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(1, ExecutionActionTestClass.ContinueMethodCallCount, "Method should execute");
        }

        [TestMethod]
        public void Continue_MethodWithReturn_ShouldExecuteNormally()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.ContinueMethodWithReturn();

            // Assert
            Assert.AreEqual(42, result, "Should return actual value");
            Assert.IsTrue(ContinueExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ContinueExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(1, ExecutionActionTestClass.ContinueMethodCallCount, "Method should execute");
        }

        [TestMethod]
        public void Continue_MethodWithStringReturn_ShouldExecuteNormally()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.ContinueMethodWithStringReturn();

            // Assert
            Assert.AreEqual("executed", result, "Should return actual value");
            Assert.IsTrue(ContinueExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ContinueExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(1, ExecutionActionTestClass.ContinueMethodCallCount, "Method should execute");
        }

        #endregion

        #region SkipMethod Tests

        [TestMethod]
        public void SkipMethod_VoidMethod_ShouldSkipMethodButCallOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            testClass.SkipMethodVoid();

            // Assert
            Assert.IsTrue(SkipMethodAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipMethodAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipMethodCallCount, "Method body should NOT execute");
        }

        [TestMethod]
        public void SkipMethod_MethodWithReturn_ShouldReturnDefaultAndCallOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.SkipMethodWithReturn();

            // Assert
            Assert.AreEqual(0, result, "Should return default(int) which is 0");
            Assert.IsTrue(SkipMethodAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipMethodAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipMethodCallCount, "Method body should NOT execute");
        }

        [TestMethod]
        public void SkipMethod_MethodWithStringReturn_ShouldReturnNullAndCallOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.SkipMethodWithStringReturn();

            // Assert
            Assert.IsNull(result, "Should return default(string) which is null");
            Assert.IsTrue(SkipMethodAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipMethodAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipMethodCallCount, "Method body should NOT execute");
        }

        #endregion

        #region SkipMethodAndExit Tests

        [TestMethod]
        public void SkipMethodAndExit_VoidMethod_ShouldSkipMethodAndOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            testClass.SkipAllVoid();

            // Assert
            Assert.IsTrue(SkipMethodAndExitAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsFalse(SkipMethodAndExitAttribute.OnExitCalled, "OnExit should NOT be called");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipAllMethodCallCount, "Method body should NOT execute");
        }

        [TestMethod]
        public void SkipMethodAndExit_MethodWithReturn_ShouldReturnDefaultAndSkipOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.SkipAllWithReturn();

            // Assert
            Assert.AreEqual(0, result, "Should return default(int) which is 0");
            Assert.IsTrue(SkipMethodAndExitAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsFalse(SkipMethodAndExitAttribute.OnExitCalled, "OnExit should NOT be called");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipAllMethodCallCount, "Method body should NOT execute");
        }

        [TestMethod]
        public void SkipMethodAndExit_MethodWithStringReturn_ShouldReturnNullAndSkipOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.SkipAllWithStringReturn();

            // Assert
            Assert.IsNull(result, "Should return default(string) which is null");
            Assert.IsTrue(SkipMethodAndExitAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsFalse(SkipMethodAndExitAttribute.OnExitCalled, "OnExit should NOT be called");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipAllMethodCallCount, "Method body should NOT execute");
        }

        #endregion

        #region Conditional Execution Tests

        [TestMethod]
        public void ConditionalExecution_WithPositiveValue_ShouldExecute()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            testClass.ConditionalMethod(5);

            // Assert
            Assert.IsTrue(ConditionalExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ConditionalExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(1, ExecutionActionTestClass.ConditionalMethodCallCount, "Method should execute");
        }

        [TestMethod]
        public void ConditionalExecution_WithZeroValue_ShouldSkip()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            testClass.ConditionalMethod(0);

            // Assert
            Assert.IsTrue(ConditionalExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ConditionalExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(0, ExecutionActionTestClass.ConditionalMethodCallCount, "Method should NOT execute");
        }

        [TestMethod]
        public void ConditionalExecution_WithNegativeValue_ShouldSkip()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            testClass.ConditionalMethod(-5);

            // Assert
            Assert.IsTrue(ConditionalExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ConditionalExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(0, ExecutionActionTestClass.ConditionalMethodCallCount, "Method should NOT execute");
        }

        [TestMethod]
        public void ConditionalExecution_WithReturnAndPositiveValue_ShouldExecuteAndReturnValue()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.ConditionalMethodWithReturn(5);

            // Assert
            Assert.AreEqual(10, result, "Should return 5 * 2 = 10");
            Assert.IsTrue(ConditionalExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ConditionalExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(1, ExecutionActionTestClass.ConditionalMethodCallCount, "Method should execute");
        }

        [TestMethod]
        public void ConditionalExecution_WithReturnAndZeroValue_ShouldReturnDefault()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.ConditionalMethodWithReturn(0);

            // Assert
            Assert.AreEqual(0, result, "Should return default(int) which is 0");
            Assert.IsTrue(ConditionalExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ConditionalExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(0, ExecutionActionTestClass.ConditionalMethodCallCount, "Method should NOT execute");
        }

        [TestMethod]
        public void ConditionalExecution_WithStringReturnAndPositiveValue_ShouldExecuteAndReturnValue()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.ConditionalMethodWithStringReturn(5);

            // Assert
            Assert.AreEqual("Value: 5", result, "Should return formatted string");
            Assert.IsTrue(ConditionalExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ConditionalExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(1, ExecutionActionTestClass.ConditionalMethodCallCount, "Method should execute");
        }

        [TestMethod]
        public void ConditionalExecution_WithStringReturnAndZeroValue_ShouldReturnNull()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.ConditionalMethodWithStringReturn(0);

            // Assert
            Assert.IsNull(result, "Should return default(string) which is null");
            Assert.IsTrue(ConditionalExecutionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ConditionalExecutionAttribute.OnExitCalled, "OnExit should be called");
            Assert.AreEqual(0, ExecutionActionTestClass.ConditionalMethodCallCount, "Method should NOT execute");
        }

        #endregion

        #region Async Method Error Tests

        [TestMethod]
        public async System.Threading.Tasks.Task AsyncMethod_WithSkipMethod_ShouldThrowNotSupportedException()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NotSupportedException>(
                async () => await testClass.AsyncMethodWithSkip(),
                "Should throw NotSupportedException for async methods with SkipMethod");
        }

        [TestMethod]
        public async System.Threading.Tasks.Task AsyncMethod_WithSkipMethodAndExit_ShouldThrowNotSupportedException()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act & Assert
            await Assert.ThrowsExceptionAsync<NotSupportedException>(
                async () => await testClass.AsyncMethodWithSkipAndExitReturningValue(),
                "Should throw NotSupportedException for async methods with SkipMethodAndExit");
        }

        #endregion
    }
}
