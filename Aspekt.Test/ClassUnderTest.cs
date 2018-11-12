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
        public int TestMethod1()
        {
            // I really don't need to do anything
            return 1 + 1;

        }

        public void GenerateException()
        {
            throw new Exception("Generated");
        }
    }
}
