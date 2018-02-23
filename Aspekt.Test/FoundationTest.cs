using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Aspekt.Test
{
    [TestClass]
    public class FoundationTest
    {
        [MockAspect("CallMe")]
        void CallMe()
        {
            Thread.Sleep(500);
        }

        [MockAspect("CallMeMaybe")]
        void CallMeMaybe(String value)
        {

        }

        [MockAspect("CallMeException")]
        void CallMeException()
        {
            throw new Exception("Blah");
        }

        [TestMethod]
        public void TestEntryExit()
        {
            MockAspect.Reset();
            CallMe();
            Assert.AreEqual(MockAspect.Entries, 1);
            Assert.AreEqual(MockAspect.Exits, 1);
        }

        [TestMethod]
        public void TestMethodArguments()
        {
            bool called = false;
            MockAspect.Reset();
            MockAspect.OnEntryAction = (MethodArguments a) =>
            {
                Assert.AreEqual(a.Arguments.Count, 1);
                Assert.AreEqual("SomeValue", a.Arguments[0].Value);
                Assert.AreEqual("value", a.Arguments[0].Name);
                Assert.AreEqual(typeof(string), a.Arguments[0].Type);
                called = true;
            };

            CallMeMaybe("SomeValue");

            Assert.IsTrue(called);
        }

        [TestMethod]
        public void TestMethodException()
        {
            bool called = false;
            MockAspect.Reset();
            MockAspect.OnExceptionAction = (args, e) =>
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
            MockAspect.Reset();
            DummyClass dc = new DummyClass();
            bool called = false;
            MockAspect.OnEntryAction = (args) =>
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
