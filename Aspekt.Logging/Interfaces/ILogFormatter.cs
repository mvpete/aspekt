using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging.Interfaces
{
    public interface ILogMessageFormatter
    {
        string Format(Events evt, MethodArguments a, Exception e);
    }
}
