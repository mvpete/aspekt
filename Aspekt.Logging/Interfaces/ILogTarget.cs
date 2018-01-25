using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging.Interfaces
{
    public interface ILogTarget
    {
        Levels Level { get; }
        void Write(String message);
        void WriteLine(String message);
        void WriteLine();
        ConsoleColor TextColor { get; set; }
    }
}
