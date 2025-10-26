namespace Aspekt.Contracts
{
    /// <summary>
    /// PropertInvariant, by nature of how a property works, we can only ever evaluate the backing
    /// field on the Exit. Typically this property invariant would only ever be used on auto-properties
    /// which means they will be set via the set_Property function. Which will be tested on exit of
    /// the function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PropertyInvariantAttribute : Aspect
    {

        public string NameOf { get; set; }

        private readonly IContractEvaluator evaluator_;

        #region Constructors
        public PropertyInvariantAttribute(string nameOf, Contract.Constraint constraint)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(constraint);
        }
        // One of the following types: bool, byte, char,  double, float, int, long, short, string
        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, bool value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }
        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, byte value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }
        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, char value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }
        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, double value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }
        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, float value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }
        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, int value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }

        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, long value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }

        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, short value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }

        public PropertyInvariantAttribute(string nameOf, Contract.Comparison op, string value)
        {
            NameOf = nameOf;
            evaluator_ = ContractEvaluatorFactory.Create(op, value);

        }
        #endregion

        private void Evaluate(MethodArguments args, string condition)
        {
            if (args.Instance == null)
            {
                throw new InvalidOperationException("Invariants cannot exist on static methods.");
            }

            // find the field
            var inst = args.Instance;
            var type = inst.GetType();
            var property = inst.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).SingleOrDefault(f => f.Name == NameOf);
            if (property == null)
            {
                throw new MissingFieldException(
                    $"{NameOf} property does not exist on object {args.Instance.GetType()}.");
            }

            // This is a dirty hack to avoid a stackOverflow with properties. Just go direct to the backing field, so that I don't call the "Get/Set" method of the property.
            // If you put a backing field in yourself, please put the invariant on the field itself.
            var field = inst.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).SingleOrDefault(f => f.Name == $"<{property.Name}>k__BackingField");
            if (field == null)
            {
                throw new MissingFieldException(
                    $"{args.Instance.GetType()} does not contain standard backing field for property {NameOf}.");
            }

            var v = field.GetValue(inst);

            if (!evaluator_.Evaluate(field.GetValue(inst)))
            {
                throw new ContractViolatedException(
                    $"{args.FormattedName} {condition} property '{NameOf}' failed invariant {evaluator_}.");
            }
        }

        public override void OnExit(MethodArguments args)
        {
            Evaluate(args, "post-condition");
        }
        public Contract.Constraint Constraint { get; }
    }
}
