using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace Aspekt.Bootstrap
{
    internal interface ILogger
    {
        void WriteLine(string line);
    }

    internal class ConsoleLogger : ILogger
    {
        public void WriteLine(string line)
        {
            Console.WriteLine(line);
        }
    }

    internal static class WeaverLog
    {
        public static ILogger Logger { get; set; } = new ConsoleLogger();

        public static void LogWarning(string filename, int line, int? col, int errNo, string message)
        {
            var colText = col.HasValue ? $", {col.Value}" : "";
            Logger.WriteLine($"{filename}({line}): warning AK{errNo.ToString("D4")}: {message}");
        }

        public static void LogError(string filename, int line, int? col, int errNo, string message)
        {
            var colText = col.HasValue ? $", {col.Value}" : "";
            Logger.WriteLine($"{filename}({line}): error AK{errNo.ToString("D4")}: {message}");
        }

        public static void LogMethodWarning(MethodDefinition md, int errNo, string message)
        {
            if (md.CustomAttributes.Any(cs => cs.AttributeType.FullName == md.Module.ImportReference(typeof(IgnoreWarningAttribute)).FullName))
            {
                return;
            }

            var filename = "unknown file";
            var line = 0;
            var col = 0;
            
            if (md.DebugInformation.HasSequencePoints)
            {
                var sp = md.DebugInformation.SequencePoints[0];
                filename = sp.Document.Url;
                line = sp.StartLine;
                col = sp.StartColumn;
            }
            LogWarning(filename, line, col, errNo, message);
        }

        public static void LogMethodError(MethodDefinition md, int errNo, string message)
        {
          

            var filename = "unknown file";
            var line = 0;
            var col = 0;

            if (md.DebugInformation.HasSequencePoints)
            {
                var sp = md.DebugInformation.SequencePoints[0];
                filename = sp.Document.Url;
                line = sp.StartLine;
                col = sp.StartColumn;
            }
            LogError(filename, line, col, errNo, message);
        }
    }
}
