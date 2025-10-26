namespace Aspekt.Contracts
{
    public static class ContractEvaluatorFactory
    {
        public static IContractEvaluator Create(Contract.Comparison op, object value)
        {
            return new ComparisonContractEvaluator(op, value);
        }
        public static IContractEvaluator Create(Contract.Constraint constraint)
        {
            return new NullContractEvaluator(constraint);
        }
    }
}
