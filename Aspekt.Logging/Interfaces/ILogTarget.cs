using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging.Interfaces
{
    public interface ILogTarget
    {
        void Log(Levels level, String message);
    }
}
