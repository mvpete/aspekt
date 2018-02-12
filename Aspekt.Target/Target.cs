using Aspekt;
using Aspekt.Logging;
using System;

namespace Aspekt.Target
{
    class TargetAttribute : Aspect
    {
        public TargetAttribute(object s)
        {
        }
        public override void OnEntry(MethodArguments args)
        {
        }

        public override void OnException(MethodArguments args, Exception e)
        {
        }

        public override void OnExit(MethodArguments args)
        {
        }
    }

    class TestAttribute : Aspect
    {
        public TestAttribute(object obj)
        {

        }
    } 

    class Application
    {

        public void Function(object o)
        {
            TestAttribute ta = new TestAttribute(6);
        }

        [Log]
        public static void Bar()
        {



            Console.WriteLine("Bar None");

        }
        
        public enum Choice { Yes, No}
        [Target(6)]
        [Log]
        public static void Test(object o, Type t, int i, Choice c)
        {
            LogWriter.Debug("the quick brown fox.");
        }
    }
}
