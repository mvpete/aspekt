using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aspekt.Logging
{
    public enum Levels { Trace = 0, Debug = 1, Information = 2, Warning = 3, Error = 4, Fatal = 5}
    public enum Events { OnEnter, OnExit, OnException }

}
