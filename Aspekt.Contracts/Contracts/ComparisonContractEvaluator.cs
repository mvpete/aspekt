using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Contracts.Contracts
{
    class ComparisonContractEvaluator : IContractEvaluator
    {
        public IComparable CompareTo { get; set; }
        public Contract.Comparison Operator { get; set; }

        public ComparisonContractEvaluator(Contract.Comparison op, object compareTo)
        {
            if (!(compareTo is IComparable))
                throw new ArgumentException("Evaluation failed CompareTo is not of type IComparable");

            CompareTo = (IComparable)compareTo;
            Operator = op;

        }


        public bool Evaluate(object o)
        {
            // we need to test they're both IComparable


            if (!(o is IComparable))
                throw new ArgumentException("Object is not of type IComparable");

            IComparable value = (IComparable)o;

            switch (Operator)
            {
                case Contract.Comparison.LessThan:
                    return value.CompareTo(CompareTo) < 0;
                case Contract.Comparison.LessThanEqualTo:
                    return value.CompareTo(CompareTo) <= 0;
                case Contract.Comparison.GreaterThan:
                    return value.CompareTo(CompareTo) > 0;
                case Contract.Comparison.GreaterThanEqualTo:
                    return value.CompareTo(CompareTo) >= 0;
                case Contract.Comparison.EqualTo:
                    return value.CompareTo(CompareTo) == 0;
                case Contract.Comparison.NotEqualTo:
                    return value.CompareTo(CompareTo) != 0;
                default:
                    throw new ArgumentException("Invalid comparison operator");
            }

        }

        public override string ToString()
        {
            switch (Operator)
            {
                case Contract.Comparison.LessThan:
                    return $"value < {CompareTo}";
                case Contract.Comparison.LessThanEqualTo:
                    return $"value <= {CompareTo}";
                case Contract.Comparison.GreaterThan:
                    return $"value > {CompareTo}";
                case Contract.Comparison.GreaterThanEqualTo:
                    return $"value >= {CompareTo}";
                case Contract.Comparison.EqualTo:
                    return $"value == {CompareTo}";
                case Contract.Comparison.NotEqualTo:
                    return $"value != {CompareTo}";
                default:
                    throw new ArgumentException("Invalid comparison operator");
            }
        }
    }
}
