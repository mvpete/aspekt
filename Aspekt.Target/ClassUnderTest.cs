using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Test
{
    /// <summary>
    /// This class is specifically used to test the class level aspect
    /// </summary>
    /// 
    [ClassLevelAspect]
    class ClassUnderTest
    {

        public void Test(__arglist)
        {
            var it = new ArgIterator(__arglist);
        }

        public int TestMethod1()
        {
            // I really don't need to do anything
            return 1 + 1;

        }

        public int TestMethod2(bool t)
        {
            if (t)
                return 3;
            return 3 + 3;
        }

        public void GenerateException()
        {
            throw new Exception("Generated");
        }
    }
}
