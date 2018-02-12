using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Contracts.Contracts
{
    public class ContractViolatedException : Exception
    {
        public ContractViolatedException(string message)
            : base(message)
        {
        }
    }
}
