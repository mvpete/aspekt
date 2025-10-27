using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aspekt.Test
{
    internal sealed class AsyncOnExitAspectAttribute : Aspect
    {
        public static bool OnExitAsyncCalled { get; set; } = false;
        public static MethodArguments? LastMethodArguments { get; set; }

        public AsyncOnExitAspectAttribute()
        {
            OnExitAsyncCalled = false;
            LastMethodArguments = null;
        }

        public override async ValueTask OnExitAsync(MethodArguments args, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken); // Simulate async work
            OnExitAsyncCalled = true;
            LastMethodArguments = args;
        }
    }

    internal sealed class AsyncOnExceptionAspectAttribute : Aspect
    {
        public static bool OnExceptionAsyncCalled { get; set; } = false;
        public static Exception? LastException { get; set; }
        public static MethodArguments? LastMethodArguments { get; set; }

        public AsyncOnExceptionAspectAttribute()
        {
            OnExceptionAsyncCalled = false;
            LastException = null;
            LastMethodArguments = null;
        }

        public override async ValueTask OnExceptionAsync(MethodArguments args, Exception e, CancellationToken cancellationToken = default)
        {
            await Task.Delay(10, cancellationToken); // Simulate async work
            OnExceptionAsyncCalled = true;
            LastException = e;
            LastMethodArguments = args;
        }
    }

    internal sealed class SyncOnExitWithAsyncMethodAspectAttribute : Aspect
    {
        public static bool OnExitCalled { get; set; } = false;
        public static MethodArguments? LastMethodArguments { get; set; }

        public SyncOnExitWithAsyncMethodAspectAttribute()
        {
            OnExitCalled = false;
            LastMethodArguments = null;
        }

        public override void OnExit(MethodArguments args)
        {
            OnExitCalled = true;
            LastMethodArguments = args;
        }
    }

    internal class AsyncTestClass
    {
        [AsyncOnExitAspect]
        public async Task<string> AsyncMethodWithReturn()
        {
            await Task.Delay(1);
            return "Hello World";
        }

        [AsyncOnExitAspect]
        public async Task AsyncMethodVoid()
        {
            await Task.Delay(1);
        }

        [AsyncOnExitAspect]
        public async ValueTask<int> AsyncValueTaskWithReturn()
        {
            await Task.Delay(1);
            return 42;
        }

        [AsyncOnExitAspect]
        public async ValueTask AsyncValueTaskVoid()
        {
            await Task.Delay(1);
        }

        [AsyncOnExceptionAspect]
        public async Task<string> AsyncMethodThatThrows()
        {
            await Task.Delay(1);
            throw new InvalidOperationException("Test exception");
        }

        [SyncOnExitWithAsyncMethodAspect]
        public async Task<string> AsyncMethodWithSyncOnExit()
        {
            await Task.Delay(1);
            return "Sync OnExit";
        }
    }

    [TestClass]
    public class AsyncAspectTest
    {
        [TestMethod]
        public async Task OnExitAsync_ShouldBeCalledForAsyncTaskWithReturn()
        {
            // Arrange
            var testClass = new AsyncTestClass();
            AsyncOnExitAspectAttribute.OnExitAsyncCalled = false;

            // Act
            var result = await testClass.AsyncMethodWithReturn();

            // Assert
            Assert.AreEqual("Hello World", result);
            Assert.IsTrue(AsyncOnExitAspectAttribute.OnExitAsyncCalled);
            Assert.IsNotNull(AsyncOnExitAspectAttribute.LastMethodArguments);
            Assert.AreEqual(nameof(AsyncTestClass.AsyncMethodWithReturn), AsyncOnExitAspectAttribute.LastMethodArguments.MethodName);
        }

        [TestMethod]
        public async Task OnExitAsync_ShouldBeCalledForAsyncTaskVoid()
        {
            // Arrange
            var testClass = new AsyncTestClass();
            AsyncOnExitAspectAttribute.OnExitAsyncCalled = false;

            // Act
            await testClass.AsyncMethodVoid();

            // Assert
            Assert.IsTrue(AsyncOnExitAspectAttribute.OnExitAsyncCalled);
            Assert.IsNotNull(AsyncOnExitAspectAttribute.LastMethodArguments);
            Assert.AreEqual(nameof(AsyncTestClass.AsyncMethodVoid), AsyncOnExitAspectAttribute.LastMethodArguments.MethodName);
        }

        [TestMethod]
        public async Task OnExitAsync_ShouldBeCalledForAsyncValueTaskWithReturn()
        {
            // Arrange
            var testClass = new AsyncTestClass();
            AsyncOnExitAspectAttribute.OnExitAsyncCalled = false;

            // Act
            var result = await testClass.AsyncValueTaskWithReturn();

            // Assert
            Assert.AreEqual(42, result);
            Assert.IsTrue(AsyncOnExitAspectAttribute.OnExitAsyncCalled);
            Assert.IsNotNull(AsyncOnExitAspectAttribute.LastMethodArguments);
            Assert.AreEqual(nameof(AsyncTestClass.AsyncValueTaskWithReturn), AsyncOnExitAspectAttribute.LastMethodArguments.MethodName);
        }

        [TestMethod]
        public async Task OnExitAsync_ShouldBeCalledForAsyncValueTaskVoid()
        {
            // Arrange
            var testClass = new AsyncTestClass();
            AsyncOnExitAspectAttribute.OnExitAsyncCalled = false;

            // Act
            await testClass.AsyncValueTaskVoid();

            // Assert
            Assert.IsTrue(AsyncOnExitAspectAttribute.OnExitAsyncCalled);
            Assert.IsNotNull(AsyncOnExitAspectAttribute.LastMethodArguments);
            Assert.AreEqual(nameof(AsyncTestClass.AsyncValueTaskVoid), AsyncOnExitAspectAttribute.LastMethodArguments.MethodName);
        }

        [TestMethod]
        public async Task OnExceptionAsync_ShouldBeCalledWhenAsyncMethodThrows()
        {
            // Arrange
            var testClass = new AsyncTestClass();
            AsyncOnExceptionAspectAttribute.OnExceptionAsyncCalled = false;

            // Act & Assert
            try
            {
                await testClass.AsyncMethodThatThrows();
                Assert.Fail("Expected exception was not thrown");
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("Test exception", ex.Message);
                Assert.IsTrue(AsyncOnExceptionAspectAttribute.OnExceptionAsyncCalled);
                Assert.IsNotNull(AsyncOnExceptionAspectAttribute.LastException);
                Assert.IsInstanceOfType(AsyncOnExceptionAspectAttribute.LastException, typeof(InvalidOperationException));
                Assert.IsNotNull(AsyncOnExceptionAspectAttribute.LastMethodArguments);
                Assert.AreEqual(nameof(AsyncTestClass.AsyncMethodThatThrows), AsyncOnExceptionAspectAttribute.LastMethodArguments.MethodName);
            }
        }

        [TestMethod]
        public async Task SyncOnExit_ShouldBeCalledForAsyncMethod()
        {
            // Arrange
            var testClass = new AsyncTestClass();
            SyncOnExitWithAsyncMethodAspectAttribute.OnExitCalled = false;

            // Act
            var result = await testClass.AsyncMethodWithSyncOnExit();

            // Assert
            Assert.AreEqual("Sync OnExit", result);
            Assert.IsTrue(SyncOnExitWithAsyncMethodAspectAttribute.OnExitCalled);
            Assert.IsNotNull(SyncOnExitWithAsyncMethodAspectAttribute.LastMethodArguments);
            Assert.AreEqual(nameof(AsyncTestClass.AsyncMethodWithSyncOnExit), SyncOnExitWithAsyncMethodAspectAttribute.LastMethodArguments.MethodName);
        }
    }
}