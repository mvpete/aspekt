using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Contracts.Contracts
{
    class NullContractEvaluator : IContractEvaluator
    {
        public Contract.Constraint Constraint { get; set; }
        public NullContractEvaluator(Contract.Constraint c)
        {
            Constraint = c;
        }

        public bool Evaluate(object o)
        {
            switch (Constraint)
            {
                case Contract.Constraint.NotNull:
                    return o != null;
                case Contract.Constraint.Null:
                    return o == null;
                default:
                    throw new InvalidOperationException("Invalid constraint type");
            }
        }

        public override String ToString()
        {
            switch (Constraint)
            {
                case Contract.Constraint.NotNull:
                    return "value != null";
                case Contract.Constraint.Null:
                    return "value == null";
                default:
                    throw new InvalidOperationException("Invalid constraint type");
            }
        }
    }
}
