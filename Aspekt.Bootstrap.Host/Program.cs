using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Bootstrap.Host
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap.Apply(args[0]);
        }
    }
}
