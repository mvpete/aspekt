using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Test
{
    
    class DummyClass
    {
        [TestAspect("Call")]
        public int Call()
        {
            return 5;
        }
    }
}
