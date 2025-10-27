namespace Aspekt.Contracts.Aspects
{
    /// <summary>
    /// Specifies a precondition that must be true when a method is called.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor, AllowMultiple = true)]
    public sealed class RequireAttribute : ContractAspect
    {
        /// <summary>
        /// Creates a precondition for a method parameter.
        /// </summary>
        public RequireAttribute(string parameterName, Contract.Constraint constraint) 
            : base(Contract.Target.Property, parameterName) // Using Property as default for parameters
        {
            AddContract(ContractEvaluatorFactory.Create(constraint), parameterName);
        }

        /// <summary>
        /// Creates a precondition with a comparison for a method parameter.
        /// </summary>
        public RequireAttribute(string parameterName, Contract.Comparison comparison, object value) 
            : base(Contract.Target.Property, parameterName)
        {
            AddContract(ContractEvaluatorFactory.Create(comparison, value), parameterName);
        }

        /// <summary>
        /// Creates a precondition for a field or property.
        /// </summary>
        public RequireAttribute(Contract.Target target, string targetName, Contract.Constraint constraint) 
            : base(target, targetName)
        {
            AddContract(ContractEvaluatorFactory.Create(constraint), targetName);
        }

        /// <summary>
        /// Creates a precondition with a comparison for a field or property.
        /// </summary>
        public RequireAttribute(Contract.Target target, string targetName, Contract.Comparison comparison, object value) 
            : base(target, targetName)
        {
            AddContract(ContractEvaluatorFactory.Create(comparison, value), targetName);
        }

        public override void OnEntry(MethodArguments args)
        {
            EvaluateContracts(args, (methodName, condition) => new PreconditionException(methodName, condition));
        }
        public string ParameterName { get; }
    }
}
