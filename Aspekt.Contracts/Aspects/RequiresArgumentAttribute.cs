namespace Aspekt.Contracts
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RequiresArgumentAttribute : Aspect
    {
        public string ArgumentName { get; set; }
        public int? ArgumentIndex { get; set; }
        public Type ArgumentType { get; set; }

        private readonly IContractEvaluator evaluator_;

        // One of the following types: bool, byte, char,  double, float, int, long, short, string
        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, bool value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, byte value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, char value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, double value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, float value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, int value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, long value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, short value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Comparison op, string value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }


        public RequiresArgumentAttribute(string argumentName, Type argumentType, Contract.Constraint c)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(c);

        }
        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, bool value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, byte value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, char value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, double value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, float value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, int value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, long value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, short value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, string value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            evaluator_ = ContractEvaluatorFactory.Create(op, value);
        }

        public override void OnEntry(MethodArguments args)
        {
            object? arg = null;
            if (ArgumentIndex != null)
            {
                arg = args.Arguments.GetArgumentByIndex(ArgumentIndex.Value);
            }
            else
            {
                arg = args.Arguments.GetArgumentValueByName(ArgumentName);
            }

            if (!evaluator_.Evaluate(arg))
            {
                throw new ContractViolatedException($"{args.FormattedName} failed pre-condition {evaluator_}.");
            }
        }
        public Contract.Comparison Op { get; }
    }
}
