namespace Aspekt
{
    /// <summary>
    /// Defines the execution action that should be taken after OnEntry is called.
    /// This allows attributes to conditionally control method execution.
    /// </summary>
    public enum ExecutionAction
    {
        /// <summary>
        /// Continue with normal execution (default behavior).
        /// The method body will execute, and OnExit will be called.
        /// </summary>
        Continue = 0,

        /// <summary>
        /// Skip the method body execution, but still call OnExit.
        /// The method will return the default value for its return type.
        /// </summary>
        SkipMethod = 1,

        /// <summary>
        /// Skip both the method body execution and OnExit.
        /// The method will return the default value for its return type.
        /// </summary>
        SkipMethodAndExit = 2
    }
}
