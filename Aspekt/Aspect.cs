namespace Aspekt
{
    public abstract class Aspect : Attribute
    {
        public virtual void OnEntry(MethodArguments args) { }
        public virtual void OnExit(MethodArguments args) { }
        public virtual void OnException(MethodArguments args, Exception e) { }

        // Modern async support
        public virtual ValueTask OnEntryAsync(MethodArguments args, CancellationToken cancellationToken = default)
        {
            return new ValueTask();
        }

        public virtual ValueTask OnExitAsync(MethodArguments args, CancellationToken cancellationToken = default)
        {
            return new ValueTask();
        }

        public virtual ValueTask OnExceptionAsync(MethodArguments args, Exception e, CancellationToken cancellationToken = default)
        {
            return new ValueTask();
        }

        // Enhanced async continuations
        public static Func<Task<T>, T> AsyncOnExit<T>(Aspect aspect, MethodArguments args)
        {
            return (Task<T> task) =>
            {
                aspect.OnExit(args);
                return task.Result;
            };
        }

        public static Func<Task<T>, ValueTask<T>> AsyncOnExitValueTask<T>(Aspect aspect, MethodArguments args, CancellationToken cancellationToken = default)
        {
            return async (Task<T> task) =>
            {
                var result = await task.ConfigureAwait(false);
                await aspect.OnExitAsync(args, cancellationToken).ConfigureAwait(false);
                return result;
            };
        }
    }

    /// <summary>
    /// Interface for aspects that need to handle method exit with return values.
    /// </summary>
    /// <typeparam name="T">The return type of the method.</typeparam>
    public interface IAspectExitHandler<T>
    {
        /// <summary>
        /// Called when a method with return type T exits.
        /// </summary>
        /// <param name="args">Method arguments.</param>
        /// <param name="returnValue">The return value of the method.</param>
        /// <returns>The potentially modified return value.</returns>
        public T OnExit(MethodArguments args, T returnValue);
    }

    /// <summary>
    /// Attribute to suppress aspect weaving warnings for specific methods.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="IgnoreAspectWarningAttribute"/> class.
    /// </remarks>
    /// <param name="warningNumbers">The warning numbers to ignore.</param>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
    public sealed class IgnoreAspectWarningAttribute(params int[] warningNumbers) : Attribute
    {
        /// <summary>
        /// Gets the warning numbers to ignore.
        /// </summary>
        public int[] WarningNumbers { get; } = warningNumbers ?? Array.Empty<int>();
    }
}
