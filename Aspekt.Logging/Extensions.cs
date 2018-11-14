using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging
{
    static class Extensions
    {
        public static string ToLogString(this Levels level)
        {
            switch (level)
            {
                case Levels.Trace:
                    return "T";
                case Levels.Debug:
                    return "D";
                case Levels.Information:
                    return "I";
                case Levels.Warning:
                    return "W";
                case Levels.Error:
                    return "E";
                case Levels.Fatal:
                    return "F";
                default:
                    throw new InvalidOperationException("unknown log level");
            }
        }
    }
}
