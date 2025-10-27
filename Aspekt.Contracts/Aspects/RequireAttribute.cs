namespace Aspekt.Contracts.Aspects
{
    /// <summary>
    /// Specifies a precondition that must be true when a method is called.
    /// Can be applied to methods or directly to parameters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Parameter, AllowMultiple = true)]
    public sealed class RequireAttribute : Aspect
    {
        public string ParameterName { get; set; } = string.Empty;
        public Contract.Constraint? Constraint { get; set; }
        public Contract.Comparison? Comparison { get; set; }
        public object? ComparisonValue { get; set; }

        /// <summary>
        /// Constructor for method-level usage specifying parameter name and constraint
        /// </summary>
        public RequireAttribute(string parameterName, Contract.Constraint constraint)
        {
            ParameterName = parameterName;
            Constraint = constraint;
        }



        /// <summary>
        /// Constructor for method-level usage specifying parameter name and comparison with int value
        /// </summary>
        public RequireAttribute(string parameterName, Contract.Comparison comparison, int value)
        {
            ParameterName = parameterName;
            Comparison = comparison;
            ComparisonValue = value;
        }

        /// <summary>
        /// Constructor for parameter-level usage with constraint (parameter name will be injected by weaver)
        /// </summary>
        public RequireAttribute(Contract.Constraint constraint)
        {
            Constraint = constraint;
        }

        /// <summary>
        /// Constructor for parameter-level usage with comparison and int value (parameter name will be injected by weaver)
        /// </summary>
        public RequireAttribute(Contract.Comparison comparison, int value)
        {
            Comparison = comparison;
            ComparisonValue = value;
        }

        public override void OnEntry(MethodArguments args)
        {
            var paramValue = args.Arguments.GetArgumentValueByName(ParameterName);

            if (Constraint.HasValue)
            {
                switch (Constraint.Value)
                {
                    case Contract.Constraint.NotNull:
                        if (paramValue == null)
                            throw new PreconditionException(args.MethodName, $"{ParameterName} must not be null");
                        break;
                    case Contract.Constraint.Null:
                        if (paramValue != null)
                            throw new PreconditionException(args.MethodName, $"{ParameterName} must be null");
                        break;
                }
            }
            else if (Comparison.HasValue && ComparisonValue != null)
            {
                var result = CompareValues(paramValue, ComparisonValue, Comparison.Value);
                if (!result)
                {
                    var compDesc = GetComparisonDescription(Comparison.Value);
                    throw new PreconditionException(args.MethodName, $"{ParameterName} must be {compDesc} {ComparisonValue}");
                }
            }
        }

        private bool CompareValues(object? actual, object expected, Contract.Comparison comparison)
        {
            if (actual == null) return false;

            if (actual is IComparable comparable && expected is IComparable)
            {
                var result = comparable.CompareTo(expected);
                return comparison switch
                {
                    Contract.Comparison.LessThan => result < 0,
                    Contract.Comparison.LessThanEqualTo => result <= 0,
                    Contract.Comparison.GreaterThan => result > 0,
                    Contract.Comparison.GreaterThanEqualTo => result >= 0,
                    Contract.Comparison.EqualTo => result == 0,
                    Contract.Comparison.NotEqualTo => result != 0,
                    _ => false
                };
            }

            return false;
        }

        private string GetComparisonDescription(Contract.Comparison comparison) => comparison switch
        {
            Contract.Comparison.LessThan => "less than",
            Contract.Comparison.LessThanEqualTo => "less than or equal to",
            Contract.Comparison.GreaterThan => "greater than",
            Contract.Comparison.GreaterThanEqualTo => "greater than or equal to",
            Contract.Comparison.EqualTo => "equal to",
            Contract.Comparison.NotEqualTo => "not equal to",
            _ => "compared to"
        };
    }
}
