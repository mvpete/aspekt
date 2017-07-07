using System;

namespace Aspekt.Target
{
    class Program
    {
        static void Main(string[] args)
        {
            Application.Test("Object", typeof(Program), 15, Application.Choice.No);
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
