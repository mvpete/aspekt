using Aspekt.Logging;
using System;

namespace Aspekt.Target
{
    public class Program
    {
        [Log]
        private void Foo()
        {
            var la = new LogAttribute();
        }

        public static void Main(string[] args)
        {
            while (true)
            {

                Application.Test("Object", typeof(Program), 15, Application.Choice.No);
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
