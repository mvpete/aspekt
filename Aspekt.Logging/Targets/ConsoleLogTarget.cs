using Aspekt.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging.Targets
{
    public class ConsoleLogTarget : ILogTarget
    {
        public Levels Level { get; set; }

        public ConsoleColor TextColor
        {
            get { return Console.ForegroundColor; }
            set { Console.ForegroundColor = value; }
        }

        public void Write(string message)
        {
            Console.Write(message);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}
