using System;
using System.Linq;

namespace Aspekt
{
    public class MethodArguments
    {
        public string MethodName { get; internal set; }
        public Arguments Arguments { get; internal set; }
        public string FullName { get; internal set; }
        public object Instance { get; internal set; }
        public string FormattedName { get; internal set; }

        public MethodArguments(string methodName, string fullName, string methodNameFormat, Arguments args, object instance)
        {
            MethodName = methodName;
            FullName = fullName;
            Arguments = args ?? Arguments.Empty;

            Instance = instance;

            FormatName(methodNameFormat);
        }

        private void FormatName(string nameFormat)
        {
            FormattedName = string.Format(nameFormat, Arguments.Values.ToArray());
        }
    }
}
