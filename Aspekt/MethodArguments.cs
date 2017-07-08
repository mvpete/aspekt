using System;

namespace Aspekt
{
    public class MethodArguments
    {
        public String MethodName { get; internal set; }
        public Arguments Arguments { get; internal set; }
        public String FullName { get; internal set; }
        public object Instance { get; internal set; }
        public String FormattedName { get; internal set; }

        public MethodArguments(String methodName, String fullName, String methodNameFormat, Arguments args, object instance)
        {
            MethodName = methodName;
            FullName = fullName;
            if (args == null)
                Arguments = Arguments.Empty;
            else
                Arguments = args;

            Instance = instance;

            FormatName(methodNameFormat);
        }

        private void FormatName(String nameFormat)
        {
            FormattedName = string.Format(nameFormat, Arguments.ToArray());
        }
    }
}
