using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Logging.Test
{
    [Log]
    class ClassLevelLogging
    {
        public void NoArguments()
        {
        }

        public void WithArgument(string argument)
        {
        }

        public void WithArguments(string argument1, int argument2)
        {
        }

        public void GenerateException()
        {
            throw new Exception("blargh!");
        }
    }
}
