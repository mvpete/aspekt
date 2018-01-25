using System;

namespace Aspekt.Target
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Application.Test("Object", typeof(Program), 15, Application.Choice.No);
                var k = Console.ReadKey();
                if (k.Key == ConsoleKey.Q)
                    break;
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
