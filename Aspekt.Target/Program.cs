
using System;

namespace Aspekt.Target
{
    internal class ReturnValueAttribute : Aspect
    {
        public override TResult OnExit<TResult>(MethodArguments args, TResult result)
        {
              return default(TResult);
        }
    }

    public class Program
    {
        //private void Foo()
        //{
        //    var la = new LogAttribute();
        //}

        
        //private int BarInt()
        //{
        //    var rv = new ReturnValueAttribute();

        //    var ret = 3;

        //    ret = rv.OnExit(null, ret);

        //    return ret;

        //}

        //private string BarString(string s)
        //{
        //    var rv = new ReturnValueAttribute();

        //    var ret = "Hello World";

        //    ret = rv.OnExit(null, ret);

        //    return ret;
        //}

        
        [ReturnValue]
        private int BarIntTest()
        {
            return 3;
        }

        [ReturnValue]
        private int BarIntTest(string param)
        {
            return 4;
        }

        [ReturnValue]
        private string BarStringTest()
        {
            return "Hello";
        }

        [ReturnValue]
        private string BarStringTest(string param)
        {
            return "World";
        }




        public static void Main(string[] args)
        {
            while (true)
            {

                //Application.Test("Object", typeof(Program), 15, Application.Choice.No);
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
