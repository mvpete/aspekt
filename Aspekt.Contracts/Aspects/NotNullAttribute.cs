using Aspekt.Contracts;

namespace Aspekt.Contracts.Aspects
{
    /// <summary>
    /// Specifies that a parameter must not be null.
    /// Can be applied to parameters: void Method([NotNull] string param)
    /// Or as method-level attribute: [NotNull("param")] void Method(string param)
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NotNullAttribute : Aspect
    {
        public string ParameterName { get; set; } = string.Empty;

        /// <summary>
        /// Constructor for method-level usage with explicit parameter name
        /// </summary>
        /// <param name="parameterName">Name of the parameter to validate</param>
        public NotNullAttribute(string parameterName)
        {
            ParameterName = parameterName;
        }

        /// <summary>
        /// Constructor for parameter-level usage (parameter name will be injected by weaver)
        /// </summary>
        public NotNullAttribute()
        {
            // Parameter name will be set by the weaver when converting from parameter-level to method-level
        }

        public override void OnEntry(MethodArguments args)
        {
            var arg = args.Arguments.GetArgumentValueByName(ParameterName);
            if (arg == null)
            {
                throw new ArgumentNullException(ParameterName);
            }
        }
    }
}