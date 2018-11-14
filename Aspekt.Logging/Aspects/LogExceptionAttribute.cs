using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging
{
    public class LogExceptionAttribute : Aspect
    {
        public override void OnException(MethodArguments args, Exception e)
        {
            LoggingService.OnException(args, e);
        }
    }
}
