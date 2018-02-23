using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aspekt.Logging.Formatters;
using Aspekt.Logging;
using System.Threading;

namespace Aspekt.Test
{
    /// <summary>
    /// Summary description for TestDefaultLogFormatter
    /// </summary>
    [TestClass]
    public class TestDefaultLogFormatter
    {
        [TestMethod]
        public void TestDefaultLogFormatterTrace()
        {
            String message = null;
            bool blankLine = false;

            var target = new MockLogTarget();
            target.OnWrite = (msg) => { message = msg; };
            target.OnWriteBlankLine = () => { blankLine = true; };

            // Test the log formatter
            var fmt = new DefaultLogFormatter();

            var lm = new LogMessage();
            lm.Time = DateTimeOffset.Now;
            lm.Level = Levels.Trace;
            lm.Message = "A log message";
            lm.Component = null;
            lm.Thread = Thread.CurrentThread;
            fmt.Format(lm, target);

            Assert.IsTrue(blankLine, "Newline Not Written");
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains("T:"));
        }

        [TestMethod]
        public void TestDefaultLogFormatterDebug()
        {
            String message = null;
            bool blankLine = false;

            var target = new MockLogTarget();
            target.OnWrite = (msg) => { message = msg; };
            target.OnWriteBlankLine = () => { blankLine = true; };

            // Test the log formatter
            var fmt = new DefaultLogFormatter();

            var lm = new LogMessage();
            lm.Time = DateTimeOffset.Now;
            lm.Level = Levels.Debug;
            lm.Message = "A log message";
            lm.Component = null;
            lm.Thread = Thread.CurrentThread;
            fmt.Format(lm, target);

            Assert.IsTrue(blankLine, "Newline Not Written");
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains("D:"), "Message does not contain correct level");
        }

        [TestMethod]
        public void TestDefaultLogFormatterInfo()
        {
            String message = null;
            bool blankLine = false;

            var target = new MockLogTarget();
            target.OnWrite = (msg) => { message = msg; };
            target.OnWriteBlankLine = () => { blankLine = true; };

            // Test the log formatter
            var fmt = new DefaultLogFormatter();

            var lm = new LogMessage();
            lm.Time = DateTimeOffset.Now;
            lm.Level = Levels.Information;
            lm.Message = "A log message";
            lm.Component = null;
            lm.Thread = Thread.CurrentThread;
            fmt.Format(lm, target);

            Assert.IsTrue(blankLine, "Newline Not Written");
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains("I:"), "Message does not contain correct level");
        }

        [TestMethod]
        public void TestDefaultLogFormatterWarn()
        {
            String message = null;
            bool blankLine = false;

            var target = new MockLogTarget();
            target.OnWrite = (msg) => { message = msg; };
            target.OnWriteBlankLine = () => { blankLine = true; };

            // Test the log formatter
            var fmt = new DefaultLogFormatter();

            var lm = new LogMessage();
            lm.Time = DateTimeOffset.Now;
            lm.Level = Levels.Warning;
            lm.Message = "A log message";
            lm.Component = null;
            lm.Thread = Thread.CurrentThread;
            fmt.Format(lm, target);

            Assert.IsTrue(blankLine, "Newline Not Written");
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains("W:"), "Message does not contain correct level");
        }

        [TestMethod]
        public void TestDefaultLogFormatterError()
        {
            String message = null;
            bool blankLine = false;

            var target = new MockLogTarget();
            target.OnWrite = (msg) => { message = msg; };
            target.OnWriteBlankLine = () => { blankLine = true; };

            // Test the log formatter
            var fmt = new DefaultLogFormatter();

            var lm = new LogMessage();
            lm.Time = DateTimeOffset.Now;
            lm.Level = Levels.Error;
            lm.Message = "A log message";
            lm.Component = null;
            lm.Thread = Thread.CurrentThread;
            fmt.Format(lm, target);

            Assert.IsTrue(blankLine, "Newline Not Written");
            Assert.IsNotNull(message);
            Assert.IsTrue(message.Contains("E:"), "Message does not contain correct level");
        }

        [TestMethod]
        public void TestDefaultLogFormatterCR()
        {
        }

        [TestMethod]
        public void TestDefaultLogFormatterCRLF()
        {
        }
    }
}
