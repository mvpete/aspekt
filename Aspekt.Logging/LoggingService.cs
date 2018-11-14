using Aspekt.Logging.Formatters;
using Aspekt.Logging.Interfaces;
using Aspekt.Logging.Targets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aspekt.Logging
{
    
     public static class LoggingService
    {
        public static ILogMessageFormatter Formatter { get; set; }
        public static ILogTarget DefaultTarget { get; set; }

        public static Levels OnEntryLevel { get; set; } = Levels.Debug;
        public static Levels OnExitLevel { get; set; } = Levels.Debug;
        public static Levels OnExceptionLevel { get; set; } = Levels.Warning;


        static LoggingService()
        {
            Formatter = new DefaultLogFormatter();
            DefaultTarget = new ConsoleLogTarget();
        }

        public static void OnEntry(MethodArguments args)
        {
            if (DefaultTarget==null)
                return;
            DefaultTarget.Log(OnEntryLevel, Formatter.Format(Events.OnEnter, args, null));
        }

        public static void OnExit(MethodArguments args)
        {
            if (DefaultTarget == null)
                return;

            DefaultTarget.Log(OnExitLevel, Formatter.Format(Events.OnExit, args, null));
        }

        public static void OnException(MethodArguments args, Exception e)
        {
            if (DefaultTarget == null)
                return;

            DefaultTarget.Log(OnExceptionLevel, Formatter.Format(Events.OnException, args, e));
        }
      
    }
}
