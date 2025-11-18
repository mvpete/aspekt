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

    /// <summary>
    /// Attribute that skips method execution AND has exception handling (tests IL correctness)
    /// </summary>
    internal sealed class SkipWithExceptionHandlerAttribute : Aspect
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitCalled { get; set; } = false;
        public static bool OnExceptionCalled { get; set; } = false;
        public static Exception? LastException { get; set; } = null;

        public SkipWithExceptionHandlerAttribute()
        {
            OnEntryCalled = false;
            OnExitCalled = false;
            OnExceptionCalled = false;
            LastException = null;
        }

        public override void OnEntry(MethodArguments args)
        {
            OnEntryCalled = true;
            if (args.Arguments.Count > 0)
            {
                var shouldSkip = (bool)args.Arguments.GetArgumentValueByName("shouldSkip");
                if (shouldSkip)
                {
                    args.Action = ExecutionAction.SkipMethod;
                }
            }
        }

        public override void OnExit(MethodArguments args)
        {
            OnExitCalled = true;
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            OnExceptionCalled = true;
            LastException = e;
        }
    }

    /// <summary>
    /// Attribute that implements IAspectExitHandler<T> (NOT void OnExit) and uses SkipMethod
    /// This tests that SkipMethod calls the generic OnExit method
    /// </summary>
    internal sealed class SkipWithExitHandlerAttribute<T> : Aspect, IAspectExitHandler<T>
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitHandlerCalled { get; set; } = false;
        public static T? LastReturnValue { get; set; } = default;

        public SkipWithExitHandlerAttribute()
        {
            OnEntryCalled = false;
            OnExitHandlerCalled = false;
            LastReturnValue = default;
        }

        public override void OnEntry(MethodArguments args)
        {
            OnEntryCalled = true;
            args.Action = ExecutionAction.SkipMethod;
        }

        // Implementing IAspectExitHandler<T> - this should be called when skipping
        public T OnExit(MethodArguments args, T returnValue)
        {
            OnExitHandlerCalled = true;
            LastReturnValue = returnValue;
            return returnValue;
        }

        // NOTE: We're NOT overriding void OnExit(MethodArguments)
        // This tests that IAspectExitHandler<T>.OnExit is called even without void OnExit
    }

    /// <summary>
    /// Attribute that skips method and has OnException - tests that OnException is NOT called when skipping
    /// </summary>
    internal sealed class SkipMethodWithExceptionAttribute : Aspect
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitCalled { get; set; } = false;
        public static bool OnExceptionCalled { get; set; } = false;
        public static Exception? LastException { get; set; } = null;

        public SkipMethodWithExceptionAttribute()
        {
            OnEntryCalled = false;
            OnExitCalled = false;
            OnExceptionCalled = false;
            LastException = null;
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

        public override void OnException(MethodArguments args, Exception e)
        {
            OnExceptionCalled = true;
            LastException = e;
        }
    }

    /// <summary>
    /// Attribute that skips method+exit and has OnException
    /// </summary>
    internal sealed class SkipMethodAndExitWithExceptionAttribute : Aspect
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitCalled { get; set; } = false;
        public static bool OnExceptionCalled { get; set; } = false;
        public static Exception? LastException { get; set; } = null;

        public SkipMethodAndExitWithExceptionAttribute()
        {
            OnEntryCalled = false;
            OnExitCalled = false;
            OnExceptionCalled = false;
            LastException = null;
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

        public override void OnException(MethodArguments args, Exception e)
        {
            OnExceptionCalled = true;
            LastException = e;
        }
    }


    /// <summary>
    /// Attribute with IAspectExitHandler<T> that has OnException
    /// </summary>
    internal sealed class SkipWithExitHandlerAndExceptionAttribute<T> : Aspect, IAspectExitHandler<T>
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitHandlerCalled { get; set; } = false;
        public static bool OnExceptionCalled { get; set; } = false;
        public static T? LastReturnValue { get; set; } = default;
        public static Exception? LastException { get; set; } = null;

        public SkipWithExitHandlerAndExceptionAttribute()
        {
            OnEntryCalled = false;
            OnExitHandlerCalled = false;
            OnExceptionCalled = false;
            LastReturnValue = default;
            LastException = null;
        }

        public override void OnEntry(MethodArguments args)
        {
            OnEntryCalled = true;
            args.Action = ExecutionAction.SkipMethod;
        }

        public T OnExit(MethodArguments args, T returnValue)
        {
            OnExitHandlerCalled = true;
            LastReturnValue = returnValue;
            return returnValue;
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            OnExceptionCalled = true;
            LastException = e;
        }
    }


    /// <summary>
    /// Attribute with conditional skip and exception handling
    /// </summary>
    internal sealed class ConditionalSkipWithExceptionAttribute : Aspect
    {
        public static bool OnEntryCalled { get; set; } = false;
        public static bool OnExitCalled { get; set; } = false;
        public static bool OnExceptionCalled { get; set; } = false;
        public static Exception? LastException { get; set; } = null;
        public static bool SkipRequested { get; set; } = false;

        public ConditionalSkipWithExceptionAttribute()
        {
            OnEntryCalled = false;
            OnExitCalled = false;
            OnExceptionCalled = false;
            LastException = null;
            SkipRequested = false;
        }

        public override void OnEntry(MethodArguments args)
        {
            OnEntryCalled = true;
            if (args.Arguments.Count > 0)
            {
                var shouldSkip = (bool)args.Arguments.GetArgumentValueByName("shouldSkip");
                if (shouldSkip)
                {
                    SkipRequested = true;
                    args.Action = ExecutionAction.SkipMethod;
                }
            }
        }

        public override void OnExit(MethodArguments args)
        {
            OnExitCalled = true;
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            OnExceptionCalled = true;
            LastException = e;
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

        // Methods with exception handlers AND skip functionality (tests IL correctness with Leave instructions)
        [SkipWithExceptionHandler]
        public void MethodWithExceptionHandlerSkipped(bool shouldSkip)
        {
            SkipMethodCallCount++;
            if (!shouldSkip)
            {
                throw new System.InvalidOperationException("Test exception");
            }
        }

        [SkipWithExceptionHandler]
        public int MethodWithExceptionHandlerSkippedReturnsValue(bool shouldSkip)
        {
            SkipMethodCallCount++;
            if (!shouldSkip)
            {
                throw new System.InvalidOperationException("Test exception");
            }
            return 99;
        }

        [SkipWithExceptionHandler]
        public string MethodWithExceptionHandlerSkippedReturnsString(bool shouldSkip)
        {
            SkipMethodCallCount++;
            if (!shouldSkip)
            {
                throw new System.InvalidOperationException("Test exception");
            }
            return "success";
        }

        // Methods with IAspectExitHandler<T> and skip functionality
        [SkipWithExitHandler<int>]
        public int MethodWithExitHandlerInt()
        {
            SkipMethodCallCount++;
            return 42;
        }

        [SkipWithExitHandler<string>]
        public string MethodWithExitHandlerString()
        {
            SkipMethodCallCount++;
            return "executed";
        }

        // Methods with skip and OnException
        [SkipMethodWithException]
        public void MethodWithSkipAndException()
        {
            SkipMethodCallCount++;
        }

        [SkipMethodWithException]
        public int MethodWithSkipAndExceptionReturnsInt()
        {
            SkipMethodCallCount++;
            return 42;
        }

        [SkipMethodWithException]
        public string MethodWithSkipAndExceptionReturnsString()
        {
            SkipMethodCallCount++;
            return "executed";
        }

        [SkipMethodAndExitWithException]
        public void MethodWithSkipAllAndException()
        {
            SkipAllMethodCallCount++;
        }


        [SkipWithExitHandlerAndException<int>]
        public int MethodWithSkipExitHandlerAndException()
        {
            SkipMethodCallCount++;
            return 42;
        }

        [SkipWithExitHandlerAndException<string>]
        public string MethodWithSkipExitHandlerAndExceptionReturnsString()
        {
            SkipMethodCallCount++;
            return "executed";
        }

        [ConditionalSkipWithException]
        public void ConditionalMethodThatThrows(bool shouldSkip)
        {
            ConditionalMethodCallCount++;
            throw new System.InvalidOperationException("Method threw exception");
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

            // Reset SkipWithExceptionHandler attribute state
            SkipWithExceptionHandlerAttribute.OnEntryCalled = false;
            SkipWithExceptionHandlerAttribute.OnExitCalled = false;
            SkipWithExceptionHandlerAttribute.OnExceptionCalled = false;
            SkipWithExceptionHandlerAttribute.LastException = null;

            // Reset SkipWithExitHandler<int> attribute state
            SkipWithExitHandlerAttribute<int>.OnEntryCalled = false;
            SkipWithExitHandlerAttribute<int>.OnExitHandlerCalled = false;
            SkipWithExitHandlerAttribute<int>.LastReturnValue = default;

            // Reset SkipWithExitHandler<string> attribute state
            SkipWithExitHandlerAttribute<string>.OnEntryCalled = false;
            SkipWithExitHandlerAttribute<string>.OnExitHandlerCalled = false;
            SkipWithExitHandlerAttribute<string>.LastReturnValue = default;

            // Reset SkipMethodWithException attribute state
            SkipMethodWithExceptionAttribute.OnEntryCalled = false;
            SkipMethodWithExceptionAttribute.OnExitCalled = false;
            SkipMethodWithExceptionAttribute.OnExceptionCalled = false;
            SkipMethodWithExceptionAttribute.LastException = null;

            // Reset SkipMethodAndExitWithException attribute state
            SkipMethodAndExitWithExceptionAttribute.OnEntryCalled = false;
            SkipMethodAndExitWithExceptionAttribute.OnExitCalled = false;
            SkipMethodAndExitWithExceptionAttribute.OnExceptionCalled = false;
            SkipMethodAndExitWithExceptionAttribute.LastException = null;

            // Reset SkipWithExitHandlerAndException<int> attribute state
            SkipWithExitHandlerAndExceptionAttribute<int>.OnEntryCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<int>.OnExitHandlerCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<int>.OnExceptionCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<int>.LastReturnValue = default;
            SkipWithExitHandlerAndExceptionAttribute<int>.LastException = null;

            // Reset SkipWithExitHandlerAndException<string> attribute state
            SkipWithExitHandlerAndExceptionAttribute<string>.OnEntryCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<string>.OnExitHandlerCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<string>.OnExceptionCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<string>.LastReturnValue = default;
            SkipWithExitHandlerAndExceptionAttribute<string>.LastException = null;

            // Reset ConditionalSkipWithException attribute state
            ConditionalSkipWithExceptionAttribute.OnEntryCalled = false;
            ConditionalSkipWithExceptionAttribute.OnExitCalled = false;
            ConditionalSkipWithExceptionAttribute.OnExceptionCalled = false;
            ConditionalSkipWithExceptionAttribute.LastException = null;
            ConditionalSkipWithExceptionAttribute.SkipRequested = false;

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

        #region Exception Handler with Skip Tests (IL Correctness)

        [TestMethod]
        public void ExceptionHandler_WithSkip_VoidMethod_ShouldSkipAndCallOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            testClass.MethodWithExceptionHandlerSkipped(shouldSkip: true);

            // Assert
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnExitCalled, "OnExit should be called (SkipMethod calls OnExit)");
            Assert.IsFalse(SkipWithExceptionHandlerAttribute.OnExceptionCalled, "OnException should NOT be called (method was skipped)");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipMethodCallCount, "Method body should NOT execute");
        }

        [TestMethod]
        public void ExceptionHandler_WithSkip_ReturnsInt_ShouldReturnDefaultAndCallOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.MethodWithExceptionHandlerSkippedReturnsValue(shouldSkip: true);

            // Assert
            Assert.AreEqual(0, result, "Should return default(int) which is 0");
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnExitCalled, "OnExit should be called");
            Assert.IsFalse(SkipWithExceptionHandlerAttribute.OnExceptionCalled, "OnException should NOT be called");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipMethodCallCount, "Method body should NOT execute");
        }

        [TestMethod]
        public void ExceptionHandler_WithSkip_ReturnsString_ShouldReturnNullAndCallOnExit()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.MethodWithExceptionHandlerSkippedReturnsString(shouldSkip: true);

            // Assert
            Assert.IsNull(result, "Should return default(string) which is null");
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnExitCalled, "OnExit should be called");
            Assert.IsFalse(SkipWithExceptionHandlerAttribute.OnExceptionCalled, "OnException should NOT be called");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipMethodCallCount, "Method body should NOT execute");
        }

        [TestMethod]
        public void ExceptionHandler_WithoutSkip_ShouldExecuteAndRethrowException()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act & Assert
            Assert.ThrowsException<System.InvalidOperationException>(
                () => testClass.MethodWithExceptionHandlerSkipped(shouldSkip: false),
                "OnException observes but rethrows the exception");

            // Assert that OnException was called (observing the exception)
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnExceptionCalled, "OnException should be called to observe the exception");
            Assert.IsNotNull(SkipWithExceptionHandlerAttribute.LastException, "Exception should be captured");
            Assert.IsInstanceOfType(SkipWithExceptionHandlerAttribute.LastException, typeof(System.InvalidOperationException), "Should capture the correct exception type");
            Assert.AreEqual(1, ExecutionActionTestClass.SkipMethodCallCount, "Method body SHOULD execute");
        }

        [TestMethod]
        public void ExceptionHandler_WithoutSkip_ReturnsInt_ShouldExecuteAndRethrowException()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act & Assert
            Assert.ThrowsException<System.InvalidOperationException>(
                () => testClass.MethodWithExceptionHandlerSkippedReturnsValue(shouldSkip: false),
                "OnException observes but rethrows the exception");

            // Assert that OnException was called
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExceptionHandlerAttribute.OnExceptionCalled, "OnException should be called");
            Assert.IsNotNull(SkipWithExceptionHandlerAttribute.LastException, "Exception should be captured");
            Assert.AreEqual(1, ExecutionActionTestClass.SkipMethodCallCount, "Method body SHOULD execute");
        }

        #endregion

        #region IAspectExitHandler with Skip Tests

        [TestMethod]
        public void SkipMethod_WithExitHandlerInt_ShouldCallOnExitWithDefaultValue()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.MethodWithExitHandlerInt();

            // Assert
            Assert.AreEqual(0, result, "Should return default(int) which is 0");
            Assert.IsTrue(SkipWithExitHandlerAttribute<int>.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExitHandlerAttribute<int>.OnExitHandlerCalled, "IAspectExitHandler<int>.OnExit should be called");
            Assert.AreEqual(0, SkipWithExitHandlerAttribute<int>.LastReturnValue, "OnExit should receive default(int) = 0");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipMethodCallCount, "Method body should NOT execute");
        }

        [TestMethod]
        public void SkipMethod_WithExitHandlerString_ShouldCallOnExitWithNull()
        {
            // Arrange
            var testClass = new ExecutionActionTestClass();

            // Act
            var result = testClass.MethodWithExitHandlerString();

            // Assert
            Assert.IsNull(result, "Should return default(string) which is null");
            Assert.IsTrue(SkipWithExitHandlerAttribute<string>.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExitHandlerAttribute<string>.OnExitHandlerCalled, "IAspectExitHandler<string>.OnExit should be called");
            Assert.IsNull(SkipWithExitHandlerAttribute<string>.LastReturnValue, "OnExit should receive default(string) = null");
            Assert.AreEqual(0, ExecutionActionTestClass.SkipMethodCallCount, "Method body should NOT execute");
        }

        #endregion

        #region OnException Tests

        [TestMethod]
        public void SkipMethod_WithOnException_ShouldNotCallOnException()
        {
            SkipMethodWithExceptionAttribute.OnEntryCalled = false;
            SkipMethodWithExceptionAttribute.OnExitCalled = false;
            SkipMethodWithExceptionAttribute.OnExceptionCalled = false;

            var testClass = new ExecutionActionTestClass();
            testClass.MethodWithSkipAndException();

            Assert.IsTrue(SkipMethodWithExceptionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipMethodWithExceptionAttribute.OnExitCalled, "OnExit should be called when skipping");
            Assert.IsFalse(SkipMethodWithExceptionAttribute.OnExceptionCalled, "OnException should NOT be called (no exception occurred)");
            Assert.IsNull(SkipMethodWithExceptionAttribute.LastException, "No exception should be stored");
        }

        [TestMethod]
        public void SkipMethod_WithOnException_ReturnsInt_ShouldNotCallOnException()
        {
            SkipMethodWithExceptionAttribute.OnEntryCalled = false;
            SkipMethodWithExceptionAttribute.OnExitCalled = false;
            SkipMethodWithExceptionAttribute.OnExceptionCalled = false;

            var testClass = new ExecutionActionTestClass();
            var result = testClass.MethodWithSkipAndExceptionReturnsInt();

            Assert.AreEqual(0, result, "Should return default(int) = 0");
            Assert.IsTrue(SkipMethodWithExceptionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipMethodWithExceptionAttribute.OnExitCalled, "OnExit should be called when skipping");
            Assert.IsFalse(SkipMethodWithExceptionAttribute.OnExceptionCalled, "OnException should NOT be called");
        }

        [TestMethod]
        public void SkipMethodAndExit_WithOnException_ShouldNotCallOnException()
        {
            SkipMethodAndExitWithExceptionAttribute.OnEntryCalled = false;
            SkipMethodAndExitWithExceptionAttribute.OnExitCalled = false;
            SkipMethodAndExitWithExceptionAttribute.OnExceptionCalled = false;

            var testClass = new ExecutionActionTestClass();
            testClass.MethodWithSkipAllAndException();

            Assert.IsTrue(SkipMethodAndExitWithExceptionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsFalse(SkipMethodAndExitWithExceptionAttribute.OnExitCalled, "OnExit should NOT be called");
            Assert.IsFalse(SkipMethodAndExitWithExceptionAttribute.OnExceptionCalled, "OnException should NOT be called");
        }

        [TestMethod]
        public void SkipWithExitHandlerAndException_ShouldCallExitHandlerNotException()
        {
            SkipWithExitHandlerAndExceptionAttribute<int>.OnEntryCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<int>.OnExitHandlerCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<int>.OnExceptionCalled = false;

            var testClass = new ExecutionActionTestClass();
            var result = testClass.MethodWithSkipExitHandlerAndException();

            Assert.AreEqual(0, result, "Should return default(int) = 0");
            Assert.IsTrue(SkipWithExitHandlerAndExceptionAttribute<int>.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExitHandlerAndExceptionAttribute<int>.OnExitHandlerCalled, "IAspectExitHandler<T>.OnExit should be called");
            Assert.AreEqual(0, SkipWithExitHandlerAndExceptionAttribute<int>.LastReturnValue, "OnExit should receive default(int)");
            Assert.IsFalse(SkipWithExitHandlerAndExceptionAttribute<int>.OnExceptionCalled, "OnException should NOT be called");
        }

        [TestMethod]
        public void ConditionalSkip_WithSkipTrue_ThrowsInMethod_ShouldNotCallOnException()
        {
            ConditionalSkipWithExceptionAttribute.OnEntryCalled = false;
            ConditionalSkipWithExceptionAttribute.OnExitCalled = false;
            ConditionalSkipWithExceptionAttribute.OnExceptionCalled = false;
            ConditionalSkipWithExceptionAttribute.SkipRequested = false;

            var testClass = new ExecutionActionTestClass();
            testClass.ConditionalMethodThatThrows(shouldSkip: true);

            Assert.IsTrue(ConditionalSkipWithExceptionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(ConditionalSkipWithExceptionAttribute.SkipRequested, "Skip should be requested");
            Assert.IsTrue(ConditionalSkipWithExceptionAttribute.OnExitCalled, "OnExit should be called when skipping");
            Assert.IsFalse(ConditionalSkipWithExceptionAttribute.OnExceptionCalled, "OnException should NOT be called (method was skipped)");
        }

        [TestMethod]
        public void ConditionalSkip_WithSkipFalse_ThrowsInMethod_ShouldCallOnException()
        {
            ConditionalSkipWithExceptionAttribute.OnEntryCalled = false;
            ConditionalSkipWithExceptionAttribute.OnExitCalled = false;
            ConditionalSkipWithExceptionAttribute.OnExceptionCalled = false;
            ConditionalSkipWithExceptionAttribute.SkipRequested = false;

            var testClass = new ExecutionActionTestClass();

            Assert.ThrowsException<InvalidOperationException>(() => testClass.ConditionalMethodThatThrows(shouldSkip: false));

            Assert.IsTrue(ConditionalSkipWithExceptionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsFalse(ConditionalSkipWithExceptionAttribute.SkipRequested, "Skip should NOT be requested");
            Assert.IsFalse(ConditionalSkipWithExceptionAttribute.OnExitCalled, "OnExit should NOT be called (exception occurred)");
            Assert.IsTrue(ConditionalSkipWithExceptionAttribute.OnExceptionCalled, "OnException should be called");
            Assert.IsNotNull(ConditionalSkipWithExceptionAttribute.LastException);
            Assert.AreEqual("Method threw exception", ConditionalSkipWithExceptionAttribute.LastException.Message);
        }

        [TestMethod]
        public void SkipMethod_WithOnException_ReturnsString_ShouldNotCallOnException()
        {
            SkipMethodWithExceptionAttribute.OnEntryCalled = false;
            SkipMethodWithExceptionAttribute.OnExitCalled = false;
            SkipMethodWithExceptionAttribute.OnExceptionCalled = false;

            var testClass = new ExecutionActionTestClass();
            var result = testClass.MethodWithSkipAndExceptionReturnsString();

            Assert.IsNull(result, "Should return default(string) = null");
            Assert.IsTrue(SkipMethodWithExceptionAttribute.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipMethodWithExceptionAttribute.OnExitCalled, "OnExit should be called when skipping");
            Assert.IsFalse(SkipMethodWithExceptionAttribute.OnExceptionCalled, "OnException should NOT be called");
        }

        [TestMethod]
        public void SkipWithExitHandlerAndException_ReturnsString_ShouldCallExitHandlerNotException()
        {
            SkipWithExitHandlerAndExceptionAttribute<string>.OnEntryCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<string>.OnExitHandlerCalled = false;
            SkipWithExitHandlerAndExceptionAttribute<string>.OnExceptionCalled = false;

            var testClass = new ExecutionActionTestClass();
            var result = testClass.MethodWithSkipExitHandlerAndExceptionReturnsString();

            Assert.IsNull(result, "Should return default(string) = null");
            Assert.IsTrue(SkipWithExitHandlerAndExceptionAttribute<string>.OnEntryCalled, "OnEntry should be called");
            Assert.IsTrue(SkipWithExitHandlerAndExceptionAttribute<string>.OnExitHandlerCalled, "IAspectExitHandler<T>.OnExit should be called");
            Assert.IsNull(SkipWithExitHandlerAndExceptionAttribute<string>.LastReturnValue, "OnExit should receive default(string) = null");
            Assert.IsFalse(SkipWithExitHandlerAndExceptionAttribute<string>.OnExceptionCalled, "OnException should NOT be called");
        }

        #endregion
    }
}
