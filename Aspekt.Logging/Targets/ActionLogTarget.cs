using Aspekt.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging.Targets
{
    public class ActionLogTarget : ILogTarget
    {
        public Action<Levels, string> OnLog { get; set; } = (l,s)=>{};

        public void Log(Levels level, string message)
        {
            OnLog(level, message);
        }
    }
}
