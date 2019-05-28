using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt
{
    public interface IAspectExitHandler<TResult>
    {
        TResult OnExit(MethodArguments args, TResult result);
    }
}
