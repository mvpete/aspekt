
using System;

namespace Aspekt.Target
{
    internal class IntReturnValueHandler : Aspect, IAspectExitHandler<int>
    {
        public int OnExit(MethodArguments args, int result)
        {
              return 3;
        }
    }

    internal class StringReturnValueHandler : Aspect, IAspectExitHandler<string>
    {

        public override void OnExit(MethodArguments args)
        {
            Console.WriteLine("String Exit Handler");
        }

        public string OnExit(MethodArguments args, string result)
        {
            return "Hello World";
        }
    }

    internal class Foo
    {
        [IntReturnValueHandler]
        public int BarIntTest()
        {
            return 6;
        }

        [IntReturnValueHandler]
        [StringReturnValueHandler]
        public int BarIntTest(int param)
        {
            return param;
        }

        [StringReturnValueHandler]
        public string BarStringTest()
        {
            return "Goodbye World";
        }

        [StringReturnValueHandler]
        public string BarStringTest(string param)
        {
            return param;
        }

        [IgnoreWarning]
        [IntReturnValueHandler]
        public void TestVoid()
        {
        }
    }

    public class Program
    {
        //private void Foo()
        //{
        //    var la = new LogAttribute();
        //}


        private int BarInt()
        {
            var rv = new IntReturnValueHandler();

            var ret = 3;

            ret = rv.OnExit(null, ret);

            return ret;

        }

        private string BarString(string s)
        {
            var rv = new StringReturnValueHandler();

            var ret = "Hello World";

            ret = rv.OnExit(null, ret);

            return ret;
        }


        




        public static void Main(string[] args)
        {
            while (true)
            {

                var foo = new Foo();

                Console.Write("BarIntTest(): ");
                Console.WriteLine(foo.BarIntTest());

                Console.Write("BarIntTest(5): ");
                Console.WriteLine(foo.BarIntTest(6));

                Console.Write("BarStringTest(): ");
                Console.WriteLine(foo.BarStringTest());

                Console.Write("BarStringTest(): ");
                Console.WriteLine(foo.BarStringTest("Goodbye World"));

                var k = Console.ReadKey();
                if (k.Key == ConsoleKey.Q)
                {
                    break;
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
