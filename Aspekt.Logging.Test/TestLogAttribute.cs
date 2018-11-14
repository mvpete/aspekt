using System;
using Aspekt.Logging.Interfaces;
using Aspekt.Logging.Targets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspekt.Logging.Test
{
    [TestClass]
    public class TestLogAttribute
    {
        ActionLogTarget Target { get; set; }
        [TestInitialize]
        public void InitializeLogging()
        {
            Target = new ActionLogTarget();
            LoggingService.DefaultTarget = Target;
            LoggingService.OnEntryLevel = Levels.Debug;
            LoggingService.OnExitLevel = Levels.Information;
            LoggingService.OnExceptionLevel = Levels.Fatal;
        }

        [TestMethod]
        public void TestClassLevelLogging()
        {
            bool called = false;           

            Target.OnLog = (l, s) =>
            {
                called = true;
                switch (l)
                {
                    case Levels.Trace:
                        Assert.Fail("Trace not assigned");
                        break;
                    case Levels.Debug:
                        Assert.IsTrue(s.StartsWith("Entering"));
                        break;
                    case Levels.Information:
                        Assert.IsTrue(s.StartsWith("Exiting"));
                        break;
                    case Levels.Warning:
                        Assert.Fail("Warning not assigned");
                        break;
                    case Levels.Error:
                        Assert.Fail("Error not assigned");
                        break;
                    case Levels.Fatal:
                        Assert.IsTrue(s.StartsWith("Exception"));
                        break;
                    default:
                        Assert.Fail();
                        break;
                }
            };


            var cl = new ClassLevelLogging();

            cl.NoArguments();
            cl.WithArgument("Test");
            cl.WithArguments("Test", 1);
            try
            {
                cl.GenerateException();
            }
            catch
            {
                // gulp.
            }
            Assert.IsTrue(called, "Not called");
        }

        [TestMethod]
        public void TestClassLevelLoggingParameters()
        {
            bool failed = true;
            Exception e = null;
            // Ensure constructor here, because it will be logged
            var cl = new ClassLevelLogging();

            Target.OnLog = (l, s) =>
            {
                try
                {
                    // The assert will force an exception, we catch it and set the failure;
                    switch (l)
                    {
                        case Levels.Trace:
                            Assert.Fail("Trace not assigned");
                            break;
                        case Levels.Debug:
                            Assert.AreEqual(s, "Entering System.Void Aspekt.Logging.Test.ClassLevelLogging::WithArguments(Test,1)");
                            break;
                        case Levels.Information:
                            Assert.AreEqual(s, "Exiting System.Void Aspekt.Logging.Test.ClassLevelLogging::WithArguments");
                            break;
                        case Levels.Warning:
                            Assert.Fail("Warning not assigned");
                            break;
                        case Levels.Error:
                            Assert.Fail("Error not assigned");
                            break;
                        case Levels.Fatal:
                            Assert.Fail("Exception");
                            break;
                        default:
                            Assert.Fail();
                            break;
                    }
                    failed = false;
                }
                catch (Exception ex)
                {
                    e = ex;
                    
                }
                
            };

            cl.WithArguments("Test", 1);

            Assert.IsFalse(failed, e==null?"Not called":e.Message);
        }

        [TestMethod]
        public void TestClassLevelLoggingCtor()
        {
        }
    }
}
