using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Test
{
    
    class TestAspect : Aspect
    {
        public String MethodName { get; internal set; }
        public TestAspect(String methodName)
        {
            MethodName = methodName;
        }

        public override void OnEntry(MethodArguments args)
        {
            ++Entries;
            Assert.AreEqual(MethodName, args.MethodName, "OnEntry - MethodNames don't match");
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            ++Exceptions;
            Assert.AreEqual(MethodName, args.MethodName, "OnException - MethodNames don't match");
        }

        public override void OnExit(MethodArguments args)
        {
            ++Exits;
            Assert.AreEqual(MethodName, args.MethodName, "OnExit - MethodNames don't match");
        }

        public static void Reset()
        {
            Entries = 0;
            Exits = 0;
            Exceptions = 0;
        }

        public static int Entries { get; set; }
        public static int Exits { get; set; }
        public static int Exceptions { get; set; }
    }
}
