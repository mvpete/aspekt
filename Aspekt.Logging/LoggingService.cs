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
        public static ILogFormatter Formatter { get; set; }
        public static ILogTarget Target { get; set; }

        static  LoggingService()
        {
            Formatter = new DefaultLogFormatter();
            Target = new ConsoleLogTarget();
            Start();
        }

        public static void Start()
        {
            lock (staticLock_)
            {
                IsRunning = true;
                if (Runner == null)
                {
                    Runner = new Thread(() =>
                    {
                        ProcessQueue();
                    });
                    Runner.Start();
                }
            }
        }

        public static void Stop()
        {
            lock (staticLock_)
            {
                IsRunning = false;
                Runner = null;
            }
        }

        private static String FormatArgs(String direction, MethodArguments args)
        {
            return $"{direction} {args.FormattedName}";
        }

        private static String FormatArgs(MethodArguments args, Exception e)
        {
            return $"Exception - {args.FullName} - {e.Message}";
        }

        public static void OnEntry(Thread tid, MethodArguments args)
        {
            if (Target==null)
                return;

            var msg = new LogMessage();
            msg.Time = DateTimeOffset.Now;
            msg.Thread = tid;
            msg.Component = args.Instance;
            msg.Message = FormatArgs("Entering", args);
            msg.Level = Levels.Trace;
             
            Messages.Enqueue(msg);
        }

        public static void OnExit(Thread tid, MethodArguments args)
        {
            if (Target == null)
                return;

            var msg = new LogMessage();
            msg.Time = DateTimeOffset.Now;
            msg.Thread = tid;
            msg.Component = args.Instance;
            msg.Message = FormatArgs("Exiting", args);
            msg.Level = Levels.Trace;

            Messages.Enqueue(msg);
        }

        public static void OnException(Thread tid, MethodArguments args, Exception e)
        {
            if (Target == null)
                return;

            var msg = new LogMessage();
            msg.Time = DateTimeOffset.Now;
            msg.Thread = tid;
            msg.Component = args.Instance;
            msg.Message = FormatArgs(args, e);
            msg.Level = Levels.Warning;

            Messages.Enqueue(msg);
        }

        public static void Log(LogMessage item)
        {
            Messages.Enqueue(item);
        }

        public static void Log(Levels lvl, DateTimeOffset time, object obj, Thread thread, String message)
        {
            var lm = new LogMessage();
            lm.Component = obj;
            lm.Level = lvl;
            lm.Message = message;
            lm.Thread = thread;
            lm.Time = time;
            Log(lm);
        }

        public static void Log(Levels lvl, DateTimeOffset time, object obj, String message)
        {
            Log(lvl, time, obj, Thread.CurrentThread, message);
        }

        public static void Log(Levels lvl, String message)
        {
            Log(lvl, DateTimeOffset.Now, null, Thread.CurrentThread, message);
        }

        static Thread Runner;

        static ConcurrentQueue<LogMessage> Messages = new ConcurrentQueue<LogMessage>();

        public static bool IsRunning { get; set; }

        private static object staticLock_ = new object();

        private static void ProcessQueue()
        {
            while (IsRunning)
            {
                LogMessage msg = null;
                if (!Messages.TryDequeue(out msg))
                {
                    continue;
                }

                ProcessMessage(msg);
            }
        }

        private static void ProcessMessage(LogMessage msg)
        {
            if (Target.Level <= msg.Level)
            {
                Formatter.Format(msg, Target);
            }
        }
    }
}
