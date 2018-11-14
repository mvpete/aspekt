using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging
{
    public class LogExitAttribute : Aspect
    {
        public override void OnExit(MethodArguments args)
        {
            LoggingService.OnExit(args);
        }
    }
}
