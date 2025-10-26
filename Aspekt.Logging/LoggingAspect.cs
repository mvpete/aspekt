using System.Diagnostics;

namespace Aspekt.Logging
{
    public class LoggingAspect : Aspect
    {
        private readonly object? _logger;
        private readonly Levels _logLevel;
        private readonly bool _logParameters;
        private readonly bool _logExecutionTime;

        public LoggingAspect(Levels logLevel = Levels.Information, 
                           bool logParameters = true, 
                           bool logExecutionTime = true)
        {
            _logLevel = logLevel;
            _logParameters = logParameters;
            _logExecutionTime = logExecutionTime;
        }

        public LoggingAspect(object logger, 
                           Levels logLevel = Levels.Information,
                           bool logParameters = true, 
                           bool logExecutionTime = true) : this(logLevel, logParameters, logExecutionTime)
        {
            _logger = logger;
        }

        private Stopwatch? _stopwatch;

        public override void OnEntry(MethodArguments args)
        {
            if (_logExecutionTime)
            {
                _stopwatch = Stopwatch.StartNew();
            }

            var message = _logParameters && args.Arguments.Count > 0
                ? $"Entering {args.FullName} with parameters: [{string.Join(", ", args.Arguments.Values)}]"
                : $"Entering {args.FullName}";

            LogMessage(_logLevel, message);
        }

        public override void OnExit(MethodArguments args)
        {
            var executionTime = _stopwatch?.ElapsedMilliseconds ?? 0;
            _stopwatch?.Stop();

            var message = _logExecutionTime
                ? $"Exiting {args.FullName} (executed in {executionTime}ms)"
                : $"Exiting {args.FullName}";

            LogMessage(_logLevel, message);
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            var executionTime = _stopwatch?.ElapsedMilliseconds ?? 0;
            _stopwatch?.Stop();

            var message = _logExecutionTime
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
