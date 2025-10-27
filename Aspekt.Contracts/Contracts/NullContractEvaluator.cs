namespace Aspekt.Contracts
{
    internal class NullContractEvaluator(Contract.Constraint c) : IContractEvaluator
    {
        public Contract.Constraint Constraint { get; set; } = c;

        public bool Evaluate(object? o)
        {
            return Constraint switch
            {
                Contract.Constraint.NotNull => o != null,
                Contract.Constraint.Null => o == null,
                _ => throw new InvalidOperationException("Invalid constraint type"),
            };
        }

        public override string ToString()
        {
            return Constraint switch
            {
                Contract.Constraint.NotNull => "value != null",
                Contract.Constraint.Null => "value == null",
                _ => throw new InvalidOperationException("Invalid constraint type"),
            };
        }
    }
}
