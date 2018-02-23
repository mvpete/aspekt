using Aspekt.Contracts.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Contracts
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequiresArgumentAttribute : Aspect
    {
        String ArgumentName { get; set; }
        int? ArgumentIndex { get; set; }
        Type ArgumentType { get; set; }
        IContractEvaluator Evaluator { get; set; }
        // One of the following types: bool, byte, char,  double, float, int, long, short, string
        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, bool value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, byte value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, char value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }
        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, double value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }
        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, float value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, int value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, long value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, short value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Comparison op, string value)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }


        public RequiresArgumentAttribute(String argumentName, Type argumentType, Contract.Constraint c)
        {
            ArgumentName = argumentName;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(c);

        }
        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, bool value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, byte value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, char value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }
        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, double value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }
        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, float value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, int value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, long value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, short value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }

        public RequiresArgumentAttribute(int argumentIndex, Type argumentType, Contract.Comparison op, string value)
        {
            ArgumentIndex = argumentIndex;
            ArgumentType = argumentType;

            Evaluator = ContractEvaluatorFactory.Create(op, value);
        }
        public override void OnEntry(MethodArguments args)
        {
            object arg = null;
            if (ArgumentIndex != null && ArgumentIndex.Value < args.Arguments.Count)
                arg = args.Arguments.ElementAt(ArgumentIndex.Value).Value;
            else
                arg = args.Arguments.GetArgumentValueByName(ArgumentName);

            if (arg == null)
                throw new ArgumentException($"Argument '{(ArgumentName == null ? ArgumentIndex.Value.ToString() : ArgumentName)}' does not exist");

            if (!Evaluator.Evaluate(arg))
                throw new ContractViolatedException($"{args.FormattedName} failed pre-condition {Evaluator.ToString()}.");
        }


    }
}
