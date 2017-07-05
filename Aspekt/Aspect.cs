using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt
{
    public abstract class Aspect : System.Attribute
    {
        public abstract void OnEntry(MethodArguments args);
        public abstract void OnExit(MethodArguments args);
        public abstract void OnException(MethodArguments args, Exception e);
    }
}
