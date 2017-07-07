using System;

namespace Aspekt.Target
{
    class TargetAttribute : Aspect
    {
        public override void OnEntry(MethodArguments args)
        {
        }

        //public override void OnException(MethodArguments args, Exception e)
        //{
        //}

        public override void OnExit(MethodArguments args)
        {
        }
    }

    class Application
    {
        [Target]
        public void Test()
        {
            Console.WriteLine("the quick brown fox.");
        }
    }
}
