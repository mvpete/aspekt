using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aspekt.Logging
{
    public enum Levels { Trace = 0, Debug = 1, Information = 2, Warning = 3, Error = 4 }

    public class LogMessage
    {
        public DateTimeOffset Time { get; set; }
        public object Component { get; set; }
        public Thread Thread { get; set; }
        public Levels Level { get; set; }
        public String Message { get; set; }
    }
}
