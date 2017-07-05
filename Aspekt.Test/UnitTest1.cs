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

        [TestAspect("CallMeMaybe")]
        void CallMeMaybe(String value)
        {
            
        }

        [TestMethod]
        public void TestEntryExit()
        {
            TestAspect.Reset();
            CallMe();
            Assert.AreEqual(TestAspect.Entries, 1);
            Assert.AreEqual(TestAspect.Exits, 1);
        }

        [TestMethod]
        public void TestMethodArguments()
        {
            bool called = false;
            TestAspect.Reset();
            TestAspect.OnEntryAction = (MethodArguments a) =>
            {
                Assert.AreEqual(a.Arguments.Count, 1);
                Assert.AreEqual(a.Arguments[0], "SomeValue");
                called = true;
            };

            CallMeMaybe("SomeValue");

            Assert.IsTrue(called);
        }
    }
}
