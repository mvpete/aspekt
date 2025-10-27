using System.Diagnostics;

namespace Aspekt.Logging
{
    public class LoggingAspect : Aspect
    {
        private readonly object? logger_;
        private readonly Levels logLevel_;
        private readonly bool logParameters_;
        private readonly bool logExecutionTime_;

        public LoggingAspect(Levels logLevel = Levels.Information, 
                           bool logParameters = true, 
                           bool logExecutionTime = true)
        {
            logLevel_ = logLevel;
            logParameters_ = logParameters;
            logExecutionTime_ = logExecutionTime;
        }

        public LoggingAspect(object logger, 
                           Levels logLevel = Levels.Information,
                           bool logParameters = true, 
                           bool logExecutionTime = true) : this(logLevel, logParameters, logExecutionTime)
        {
            logger_ = logger;
        }

        private Stopwatch? stopwatch_;

        public override void OnEntry(MethodArguments args)
        {
            if (logExecutionTime_)
            {
                stopwatch_ = Stopwatch.StartNew();
            }

            var message = logParameters_ && args.Arguments.Count > 0
                ? $"Entering {args.FullName} with parameters: [{string.Join(", ", args.Arguments.Values)}]"
                : $"Entering {args.FullName}";

            LogMessage(logLevel_, message);
        }

        public override void OnExit(MethodArguments args)
        {
            var executionTime = stopwatch_?.ElapsedMilliseconds ?? 0;
            stopwatch_?.Stop();

            var message = logExecutionTime_
                ? $"Exiting {args.FullName} (executed in {executionTime}ms)"
                : $"Exiting {args.FullName}";

            LogMessage(logLevel_, message);
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            var executionTime = stopwatch_?.ElapsedMilliseconds ?? 0;
            stopwatch_?.Stop();

            var message = logExecutionTime_
                ? $"Exception in {args.FullName} after {executionTime}ms: {e.Message}"
                : $"Exception in {args.FullName}: {e.Message}";

            LogMessage(Levels.Error, message, e);
        }

        private void LogMessage(Levels level, string message, Exception? exception = null)
        {
            // Fallback to console logging
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = exception != null
                ? $"[{timestamp}] [{level}] {message} - Exception: {exception}"
                : $"[{timestamp}] [{level}] {message}";
                
            Console.WriteLine(logEntry);
        }
        public Levels LogLevel { get; }
    }

    // Convenience attributes
    public sealed class TraceAttribute : LoggingAspect
    {
        public TraceAttribute() : base(Levels.Trace, logParameters: true, logExecutionTime: true) { }
    }
}
