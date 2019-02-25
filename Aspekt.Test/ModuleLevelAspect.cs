using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Test
{
    [AttributeUsage(AttributeTargets.Module, AllowMultiple = false)]
    internal class ModuleLevelAspect : Aspect
    {
        public static Action<MethodArguments> OnMethodEntry { get; set; } = (e) => { };
        public static Action<MethodArguments> OnMethodExit { get; set; } = (e) => { };
        public static Action<MethodArguments, Exception> OnMethodException { get; set; } = (e, ex) => { };

        public override void OnEntry(MethodArguments args)
        {
            OnMethodEntry(args);
        }

        public override void OnExit(MethodArguments args)
        {
            OnMethodExit(args);
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            OnMethodException(args, e);
        }
    }
}
