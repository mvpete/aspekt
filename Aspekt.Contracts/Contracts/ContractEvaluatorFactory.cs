using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
