namespace Aspekt.Contracts.Aspects
{
    /// <summary>
    /// Specifies a postcondition that must be true when a method returns.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class EnsureAttribute : ContractAspect
    {
        private object? _returnValue;

        /// <summary>
        /// Creates a postcondition for the return value.
        /// </summary>
        public EnsureAttribute(Contract.Constraint constraint) 
            : base(Contract.Target.Property, "ReturnValue")
        {
            AddContract(ContractEvaluatorFactory.Create(constraint), "ReturnValue");
        }

        /// <summary>
        /// Creates a postcondition with a comparison for the return value.
        /// </summary>
        public EnsureAttribute(Contract.Comparison comparison, object value) 
            : base(Contract.Target.Property, "ReturnValue")
        {
            AddContract(ContractEvaluatorFactory.Create(comparison, value), "ReturnValue");
        }

        /// <summary>
        /// Creates a postcondition for a field or property.
        /// </summary>
        public EnsureAttribute(Contract.Target target, string targetName, Contract.Constraint constraint) 
            : base(target, targetName)
        {
            AddContract(ContractEvaluatorFactory.Create(constraint), targetName);
        }

        /// <summary>
        /// Creates a postcondition with a comparison for a field or property.
        /// </summary>
        public EnsureAttribute(Contract.Target target, string targetName, Contract.Comparison comparison, object value) 
            : base(target, targetName)
        {
            AddContract(ContractEvaluatorFactory.Create(comparison, value), targetName);
        }

        protected override object? GetTargetValue(MethodArguments args)
        {
            if (TargetName == "ReturnValue")
            {
                // For return value contracts, we need to capture the return value
                // This would need IL weaving support to capture the actual return value
                return _returnValue;
            }

            return base.GetTargetValue(args);
        }

        public override void OnExit(MethodArguments args)
        {
            EvaluateContracts(args, (methodName, condition) => new PostconditionException(methodName, condition));
        }

        // This method would be called by the IL weaver to set the return value
        internal void SetReturnValue(object? returnValue)
        {
            _returnValue = returnValue;
        }
        public Contract.Constraint Constraint { get; }
    }
}
