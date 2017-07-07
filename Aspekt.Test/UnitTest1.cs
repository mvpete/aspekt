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

        [TestAspect("CallMeException")]
        void CallMeException()
        {
            throw new Exception("Blah");
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

        [TestMethod]
        public void TestMethodException()
        {
            bool called = false;
            TestAspect.Reset();
            TestAspect.OnExceptionAction = (args, e) =>
            {
                called = true;
                Assert.AreEqual(e.Message, "Blah");
            };
            try
            {
                CallMeException();
            }
            catch (Exception e) // because the exception will rethrow after
            {
                Assert.AreEqual(e.Message, "Blah");
                Assert.IsTrue(called);
                return; //
            }

            Assert.Fail();
        }

        [TestMethod]
        public void TestInstanceEqual()
        {
            TestAspect.Reset();
            DummyClass dc = new DummyClass();
            bool called = false;
            TestAspect.OnEntryAction = (args) =>
            {
                called = true;
                Assert.AreSame(args.Instance, dc, "object reference mismatch");
            };
            dc.Call();
            Assert.IsTrue(called);



            // Assert that dc == Property
        }
    }
}
