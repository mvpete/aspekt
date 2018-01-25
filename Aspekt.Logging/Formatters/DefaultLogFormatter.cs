using Aspekt.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging.Formatters
{
    public class DefaultLogFormatter : ILogFormatter
    {

        public enum LineEndings { CR, CRLF, DEFAULT }

        public LineEndings LineEnding { get; set; }

        public DefaultLogFormatter()
        {
            LineEnding = LineEndings.DEFAULT;
        }


        public void Format(LogMessage msg, ILogTarget target)
        {
            var message = $"{msg.Time.UtcDateTime.ToString("o")} {ToString(msg.Level)}: tid={msg.Thread.ManagedThreadId}; {msg.Message}";
            target.Write(message);
            switch (LineEnding)
            {
                case LineEndings.CR:
                    target.Write("\r");
                    break;
                case LineEndings.CRLF:
                    target.Write("\r\n");
                    break;
                default:
                    target.WriteLine();
                    break;
            }

        }

        private String ToString(Levels lvl)
        {
            switch (lvl)
            {
                case Levels.Trace:
                    return "T";
                case Levels.Debug:
                    return "D";
                case Levels.Information:
                    return "I";
                case Levels.Warning:
                    return "W";
                default:
                case Levels.Error:
                    return "E";
            }
        }

    }
}
