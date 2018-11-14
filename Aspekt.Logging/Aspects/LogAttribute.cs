using Aspekt;
using Aspekt.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aspekt.Logging
{
    public class LogAttribute : Aspect
    {
        public override void OnEntry(MethodArguments args)
        {
            LoggingService.OnEntry(args);
        }
        public override void OnExit(MethodArguments args)
        {
            LoggingService.OnExit(args);
        }
        public override void OnException(MethodArguments args, Exception e)
        {
            LoggingService.OnException(args, e);
        }

    }
}
