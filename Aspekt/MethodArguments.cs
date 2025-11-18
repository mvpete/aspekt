namespace Aspekt
{
    public class MethodArguments
    {
        public string MethodName { get; internal set; } = string.Empty;
        public Arguments Arguments { get; internal set; } = Arguments.Empty;
        public string FullName { get; internal set; } = string.Empty;
        public object? Instance { get; internal set; }
        public string FormattedName { get; internal set; } = string.Empty;

        // New properties for enhanced metadata
        public Type? ReturnType { get; internal set; }
        public string SourceFilePath { get; internal set; } = string.Empty;
        public int LineNumber { get; internal set; }

        /// <summary>
        /// Controls the execution flow after OnEntry is called.
        /// Set this property in OnEntry to conditionally control method execution.
        /// </summary>
        public ExecutionAction Action { get; set; } = ExecutionAction.Continue;

        public MethodArguments(string methodName, string fullName, string methodNameFormat, Arguments? args, object? instance)
        {
            MethodName = methodName ?? throw new ArgumentNullException(nameof(methodName));
            FullName = fullName ?? throw new ArgumentNullException(nameof(fullName));
            Arguments = args ?? Arguments.Empty;
            Instance = instance;

            FormatName(methodNameFormat);
        }

        private void FormatName(string? nameFormat)
        {
            if (string.IsNullOrEmpty(nameFormat))
            {
                FormattedName = MethodName;
                return;
            }

            try
            {
                FormattedName = string.Format(nameFormat, Arguments.Values.ToArray());
            }
            catch (FormatException)
            {
                FormattedName = MethodName; // Fallback to method name if formatting fails
            }
        }
    }
}
