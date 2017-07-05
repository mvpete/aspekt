using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt
{
    public class MethodArguments
    {
        public String MethodName { get; set; }
        public Arguments Arguments { get; set; }
        public String FullName { get; set; }

        public MethodArguments(String methodName, String fullName, Arguments args)
        {
            MethodName = methodName;
            FullName = fullName;
            if (args == null)
                Arguments = Arguments.Empty;
            else
                Arguments = args;
        }
    }
}
