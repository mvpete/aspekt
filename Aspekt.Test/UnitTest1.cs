using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Aspekt.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestAspect("CallMe")]
        void CallMe()
        {
            Thread.Sleep(500);
        }

        [TestMethod]
        public void TestEntryExit()
        {
            TestAspect.Reset();
            CallMe();
            Assert.AreEqual(TestAspect.Entries, 1);
            Assert.AreEqual(TestAspect.Exits, 1);
        }
    }
}
