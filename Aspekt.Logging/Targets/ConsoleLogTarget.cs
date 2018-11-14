using Aspekt.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aspekt.Logging.Targets
{
    public class ConsoleLogTarget : ILogTarget
    {
        public void Log(Levels level, string message)
        {
            var now = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.fff");
            var tid = Thread.CurrentThread.ManagedThreadId;
            var lvl = level.ToLogString();
            Console.WriteLine($"{now} {tid} {lvl}: {message}");            
        }

    }
}
