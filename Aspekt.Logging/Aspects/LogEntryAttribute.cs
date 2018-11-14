using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging
{
    public class LogEntryAttribute : Aspect
    {
        public override void OnEntry(MethodArguments args)
        {
            LoggingService.OnEntry(args);
        }
    }
}
