using Aspekt;
using Aspekt.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aspekt
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LogAttribute : Aspect
    {
        public override void OnEntry(MethodArguments args)
        {
            LoggingService.OnEntry(Thread.CurrentThread, args);
        }
        public override void OnExit(MethodArguments args)
        {
            LoggingService.OnExit(Thread.CurrentThread, args);
        }
        public override void OnException(MethodArguments args, Exception e)
        {
            LoggingService.OnException(Thread.CurrentThread, args, e);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class LogEntryAttribute : Aspect
    {
        public override void OnEntry(MethodArguments args)
        {
            LoggingService.OnEntry(Thread.CurrentThread, args);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class LogExitAttribute : Aspect
    {
        public override void OnExit(MethodArguments args)
        {
            LoggingService.OnExit(Thread.CurrentThread, args);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class LogExceptionAttribute : Aspect
    {
        public override void OnException(MethodArguments args, Exception e)
        {
            LoggingService.OnException(Thread.CurrentThread, args, e);
        }
    }

}
