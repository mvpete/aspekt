namespace Aspekt.Contracts.Aspects
{
    /// <summary>
    /// Specifies an invariant condition that must be true both before and after method execution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class InvariantAttribute : ContractAspect
    {
        /// <summary>
        /// Creates an invariant for a field or property.
        /// </summary>
        public InvariantAttribute(Contract.Target target, string targetName, Contract.Constraint constraint) 
            : base(target, targetName)
        {
            AddContract(ContractEvaluatorFactory.Create(constraint), targetName);
        }

        /// <summary>
        /// Creates an invariant with a comparison for a field or property.
        /// </summary>
        public InvariantAttribute(Contract.Target target, string targetName, Contract.Comparison comparison, object value) 
            : base(target, targetName)
        {
            AddContract(ContractEvaluatorFactory.Create(comparison, value), targetName);
        }

        public override void OnEntry(MethodArguments args)
        {
            EvaluateContracts(args, (methodName, condition) => new InvariantException(methodName, condition));
        }

        public override void OnExit(MethodArguments args)
        {
            EvaluateContracts(args, (methodName, condition) => new InvariantException(methodName, condition));
        }
    }
}
