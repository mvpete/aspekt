using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Contracts.Aspects
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class NotNull : Aspect
    {
        String ArgumentName { get; set; }
        int? ArgumentIndex { get; set; }

        public NotNull(int argumentIndex)
        {
            ArgumentIndex = argumentIndex;
        }
        public NotNull(string argumentName)
        {
            ArgumentName = argumentName;
        }

        public override void OnEntry(MethodArguments args)
        {
            object arg = null;
            if (ArgumentIndex != null && ArgumentIndex.Value < args.Arguments.Count)
                arg = args.Arguments.ElementAt(ArgumentIndex.Value).Value;
            else
                arg = args.Arguments.GetArgumentValueByName(ArgumentName);

            if (arg == null)
                throw new ArgumentException($"Argument '{(ArgumentName == null ? ArgumentIndex.Value.ToString() : ArgumentName)}' does not exist");

            if (arg == null)
                throw new ArgumentNullException(ArgumentName ?? ArgumentIndex.ToString());

        }
    }
}
