using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Aspekt.Foundation.Test
{

    internal class MockAspect : Aspect
    {
        public string MethodName { get; internal set; }
        public MockAspect(string methodName)
        {
            MethodName = methodName;
        }

        public override void OnEntry(MethodArguments args)
        {
            ++Entries;
            OnEntryAction?.Invoke(args);
            Assert.AreEqual(MethodName, args.MethodName, "OnEntry - MethodNames don't match");
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            ++Exceptions;
            OnExceptionAction?.Invoke(args, e);
            Assert.AreEqual(MethodName, args.MethodName, "OnException - MethodNames don't match");
        }

        public override void OnExit(MethodArguments args)
        {
            ++Exits;
            OnExitAction?.Invoke(args);
            Assert.AreEqual(MethodName, args.MethodName, "OnExit - MethodNames don't match");
        }

        public static void Reset()
        {
            Entries = 0;
            Exits = 0;
            Exceptions = 0;

            OnEntryAction = null;
            OnExitAction = null;
            OnExceptionAction = null;

        }

        public static Action<MethodArguments> OnEntryAction;
        public static Action<MethodArguments> OnExitAction;
        public static Action<MethodArguments, Exception> OnExceptionAction;

        public static int Entries { get; set; }
        public static int Exits { get; set; }
        public static int Exceptions { get; set; }


    }
}
