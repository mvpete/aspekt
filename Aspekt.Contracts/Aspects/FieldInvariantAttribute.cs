using Aspekt.Contracts;
using System;
using System.Linq;
using System.Reflection;

namespace Aspekt.Contracts
{
    /// <summary>
    /// On a field we can validate the condition both on entry and on exit of the function.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class FieldInvariantAttribute : Aspect
    {

        String NameOf { get; set; }

        IContractEvaluator Evaluator;

        #region Constructors
        public FieldInvariantAttribute(String nameOf, Contract.Constraint constraint)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(constraint);
        }
        // One of the following types: bool, byte, char,  double, float, int, long, short, string
        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, bool value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, byte value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, char value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, double value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, float value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, int value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }

        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, long value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }

        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, short value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }

        public FieldInvariantAttribute(String nameOf, Contract.Comparison op, string value)
        {
            NameOf = nameOf;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        #endregion
        private void Evaluate(MethodArguments args, String condition)
        {
            if (args.Instance == null)
                throw new InvalidOperationException("Invariants cannot exist on static methods.");

            // check invariant here.
            // use reflection to get the type. Thennnnn
            // find the field
            var inst = args.Instance;
            var field = inst.GetType().GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).SingleOrDefault(f => f.Name == NameOf);
            if (field == null)
                throw new MissingFieldException($"{NameOf} field does not exist on object {args.Instance.GetType()}.");

            if (!Evaluator.Evaluate(field.GetValue(inst)))
                throw new ContractViolatedException($"{args.FormattedName} {condition} field '{NameOf}' failed invariant {Evaluator.ToString()}.");
        }

        public override void OnEntry(MethodArguments args)
        {
            Evaluate(args, "pre-condition");
        }
        public override void OnExit(MethodArguments args)
        {
            Evaluate(args, "post-condition");
        }

    }
}
