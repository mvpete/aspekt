using Aspekt.Contracts.Contracts;
using System;
using System.Linq;

namespace Aspekt.Contracts
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class InvariantAttribute : Aspect
    {

        String NameOf { get; set; }
        Type ConstraintType { get; set; }
        Contract.Target Target { get; set; }

        IContractEvaluator Evaluator;
        #region Constructors
        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Constraint constraint)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(constraint);
        }
        // One of the following types: bool, byte, char,  double, float, int, long, short, string
        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, bool value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, byte value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, char value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, double value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, float value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, int value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }

        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, long value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }

        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, short value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }

        public InvariantAttribute(String nameOf, Type constraintType, Contract.Target target, Contract.Comparison op, string value)
        {
            NameOf = nameOf;
            ConstraintType = constraintType;
            Target = target;
            Evaluator = ContractEvaluatorFactory.Create(op, value);

        }
        #endregion
        private void Evaluate(MethodArguments args, String condition)
        {
            if (args.Instance == null)
                throw new InvalidOperationException("Invariants cannot exist on static methods.");

            // check invariant here.
            // use reflection to get the type. Thennnnn
            switch (Target)
            {
                case Contract.Target.Field:
                    {
                        // find the field
                        var inst = args.Instance;
                        var field = inst.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).SingleOrDefault(f => f.Name == NameOf);
                        if (field == null)
                            throw new MissingFieldException($"{NameOf} field does not exist on object {args.Instance.GetType()}.");

                        if (!Evaluator.Evaluate(field.GetValue(inst)))
                            throw new ContractViolatedException($"{args.FormattedName} {condition} field '{NameOf}' failed invariant {Evaluator.ToString()}.");
                    }
                    break;
                case Contract.Target.Property:
                    {
                        // find the field
                        var inst = args.Instance;
                        var field = inst.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic).SingleOrDefault(f => f.Name == NameOf);
                        if (field == null)
                            throw new MissingFieldException($"{NameOf} field does not exist on object {args.Instance.GetType()}.");

                        if (!Evaluator.Evaluate(field.GetValue(inst)))
                            throw new ContractViolatedException($"{args.FormattedName} {condition} property '{NameOf}' failed invariant {Evaluator.ToString()}.");
                    }
                    break;
                default:
                    throw new ArgumentException("Invalid Contract Target");
            }
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
