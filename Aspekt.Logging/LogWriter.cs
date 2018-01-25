using System;

namespace Aspekt.Logging
{
    public static class LogWriter
    {
        public static void Trace(String message)
        {
            LoggingService.Log(Levels.Trace, message);
        }

        public static void Debug(String message)
        {
            LoggingService.Log(Levels.Debug, message);
        }

        public static void Information(String message)
        {
            LoggingService.Log(Levels.Information, message);
        }

        public static void Warning(String message)
        {
            LoggingService.Log(Levels.Warning, message);
        }

        public static void Error(String message)
        {
            LoggingService.Log(Levels.Error, message);
        }

    }
}
