using Aspekt.Logging;
using Aspekt.Logging.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Test
{
    public class MockLogTarget : ILogTarget
    {
        public Func<ConsoleColor> OnGetTextColor = () => { throw new NotImplementedException(); };
        public Action<ConsoleColor> OnSetTextColor = (c) => { throw new NotImplementedException(); };
        public Func<Levels> OnGetLevel = () => { throw new NotImplementedException(); };

        public Action OnWriteBlankLine = () => { throw new NotImplementedException(); };
        public Action<String> OnWrite = (msg) => { throw new NotImplementedException(); };
        public Action<String> OnWriteLine = (msg) => { throw new NotImplementedException(); };

        public Levels Level
        {
            get
            {
                return OnGetLevel();
            }
        }

        public ConsoleColor TextColor
        {
            get
            {
                return OnGetTextColor();
            }

            set
            {
                OnSetTextColor(value);
            }
        }

        public void Write(string message)
        {
            OnWrite(message);
        }

        public void WriteLine()
        {
            OnWriteBlankLine();
        }

        public void WriteLine(string message)
        {
            OnWriteLine(message);
        }
    }
}
