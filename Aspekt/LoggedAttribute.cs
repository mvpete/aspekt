﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LoggedAttribute : Aspect
    {
        public override void OnEntry(MethodArguments args)
        {
            Console.WriteLine($"OnEntry - {args.FullName}");
        }
        public override void OnExit(MethodArguments args)
        {
            Console.WriteLine($"OnExit- {args.FullName}");
        }
        public override void OnException(MethodArguments args, Exception e)
        {
            Console.WriteLine($"OnEception- {args.FullName}; '{e.ToString()}'");
        }

        
    }
}
