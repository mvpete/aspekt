﻿using System;

namespace Aspekt.Target
{
    class TargetAttribute : Aspect
    {
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

    class Application
    {

        [Logged]
        public static void Bar()
        {

            Console.WriteLine("Bar None");

        }


        public enum Choice { Yes, No}
        [Target]
        [Logged]
        public static void Test(object o, Type t, int i, Choice c)
        {
            Console.WriteLine("the quick brown fox.");
        }
    }
}
