
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;


[module: Aspekt.Test.ModuleLevelAspect()]

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

        [MockAspect("TestIntFn")]
        int TestIntFn()
        {
            return 1 + 1;
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
            Assert.AreEqual(1, MockAspect.Entries, "OnEntry");
            Assert.AreEqual(1, MockAspect.Exits, "OnExit");
        }

        [TestMethod]
        public void TestEntryExitRetValFn()
        {
            MockAspect.Reset();
            TestIntFn();
            Assert.AreEqual(1, MockAspect.Entries, "OnEntry");
            Assert.AreEqual(1, MockAspect.Exits, "OnExit");
        }

        [TestMethod]
        public void TestMethodArguments()
        {
            bool called = false;
            MockAspect.Reset();
            MockAspect.OnEntryAction = (MethodArguments a) =>
            {
                Assert.AreEqual(1, a.Arguments.Count);
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

        /// <summary>
        /// This method, will have module decoration. So I will set up the callbacks in the 
        /// test method. Then call this.
        /// </summary>
        /// <returns></returns>
        public int UseThisMethodToTestModule()
        {
            return 3 + 3;
        }

        /// <summary>
        /// This method intentionally has no decoration. So that I can test the module / assembly level aspects.
        /// </summary>
        public void ThrowAnException()
        {
            throw new Exception("thrown");
        }

        /// <summary>
        /// These methods pose an interesting problem. Because unless I move the module level
        /// aspect to a different module, these methods will get instrumented with the module
        /// level aspect.
        /// </summary>
        [TestMethod]
        public void TestModuleLevelAspect()
        {
            int entry = 0;
            int exit = 0;
            int exception = 0;

            ModuleLevelAspect.OnMethodEntry = (e) => { entry++; };
            ModuleLevelAspect.OnMethodExit = (e) => { exit++; };
            ModuleLevelAspect.OnMethodException = (e, ex) => { exception++; };

            UseThisMethodToTestModule();

            Assert.AreEqual(1, entry, "Entry");
            Assert.AreEqual(1, exit, "Exit");
            Assert.AreEqual(0, exception, "Exception");
        }

        [TestMethod]
        public void TestModuleLevelAspectException()
        {
            int entry = 0;
            int exit = 0;
            int exception = 0;

            ModuleLevelAspect.OnMethodEntry = (e) => { entry++; };
            ModuleLevelAspect.OnMethodExit = (e) => { exit++; };
            ModuleLevelAspect.OnMethodException = (e, ex) => { exception++; };
            try
            {
                ThrowAnException();
            }
            catch
            {
                // gulp.
            }

            Assert.AreEqual(1, entry, "Entry");
            Assert.AreEqual(0, exit, "Exit");
            Assert.AreEqual(1, exception, "Exception");
        }

        /// <summary>
        /// Remember that because the .ctor is a method as well, we need to account for it getting called.
        /// </summary>
        [TestMethod]
        public void TestClassLevelAspect()
        {
            int entry = 0;
            int exit = 0;
            int exception = 0;

            ClassLevelAspect.OnMethodEntry = (e) => { entry++; };
            ClassLevelAspect.OnMethodExit = (e) => { exit++; };
            ClassLevelAspect.OnMethodException = (e, ex) => { exception++; };

            ClassUnderTest interest = new ClassUnderTest();

            interest.TestMethod1();
            Assert.AreEqual(2, entry, "Entry");
            Assert.AreEqual(2, exit, "Exit");
            Assert.AreEqual(0, exception, "Exception");

        }


        [TestMethod]
        public void TestClassLevelAspectException()
        {
            int entry = 0;
            int exit = 0;
            int exception = 0;

            ClassLevelAspect.OnMethodEntry = (e) => { entry++; };
            ClassLevelAspect.OnMethodExit = (e) => { exit++; };
            ClassLevelAspect.OnMethodException = (e, ex) => { exception++; };

            ClassUnderTest interest = new ClassUnderTest();

            try
            {
                interest.GenerateException();
            }
            catch
            {
                // gulp.
            }

            Assert.AreEqual(2, entry, "Entry");
            Assert.AreEqual(1, exit, "Exit");
            Assert.AreEqual(1, exception, "Exception");
        }
    }
}
