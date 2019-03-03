using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspekt.Test
{
    internal class StringValueHandlerAttribute : Aspect, IAspectExitHandler<string>
    {
        public static string Result { get; set; } = null;

        public StringValueHandlerAttribute()
        {
            Result = null;
        }

        public string OnExit(MethodArguments args, string result)
        {
            Result = result;
            return "Hello World";
        }
    }

    internal class IntValueHandlerAttribute : Aspect, IAspectExitHandler<int>
    {
        public static int Result { get; set; } = 0;

        public IntValueHandlerAttribute()
        {
            Result = 0;
        }

        public int OnExit(MethodArguments args, int result)
        {
            Result = result;
            return 42;
        }
    }
    
    internal class StringIntHandlerAttribute : Aspect, IAspectExitHandler<string>, IAspectExitHandler<int>
    {
        public static string StringResult { get; set; } = null;
        public static int IntResult { get; set; } = 0;

        public StringIntHandlerAttribute()
        {
            StringResult = null;
            IntResult = 0;
        }

        public string OnExit(MethodArguments args, string result)
        {
            StringResult = result;
            return "Hello World";
        }

        public int OnExit(MethodArguments args, int result)
        {
            IntResult = result;
            return 42;
        }
    }

    internal class VoidOverrideHandlerAttribute : Aspect, IAspectExitHandler<int>
    {
        public static bool OnExitCalled { get; set; } = false;
        public static bool OnExitSpecilaizationCalled { get; set; } = false;

        public VoidOverrideHandlerAttribute()
        {
            OnExitCalled = false;
            OnExitSpecilaizationCalled = false;
        }

        public override void OnExit(MethodArguments args)
        {
            OnExitCalled = true;
        }

        public int OnExit(MethodArguments args, int result)
        {
            OnExitSpecilaizationCalled = true;
            return result;
        }
    }


    internal class ExitHandlerClassUnderText
    {
        [StringValueHandler]
        public string TestReturnString()
        {
            return string.Empty;
        }

        [StringValueHandler]
        public string TestReturnString(string value)
        {
            return value;
        }

        [IntValueHandler]
        public int TestReturnInt()
        {
            return -1;
        }

        [IntValueHandler]
        public int TestReturnInt(int value)
        {
            return value;
        }

        [IgnoreAspectWarningAttribute]
        [StringValueHandler]
        public void TestDoesNotWorkString()
        {
        }

        [IgnoreAspectWarningAttribute]
        [IntValueHandler]
        public void TestDoesNotWorkInt()
        {
        }

        [IgnoreAspectWarningAttribute]
        [VoidOverrideHandler]
        public int OnExitOverrides()
        {
            return 42;
        }

        [IgnoreAspectWarningAttribute]
        [StringValueHandler]
        public int ReturnsAnInt()
        {
            return -1;
        }

        [IgnoreAspectWarningAttribute]
        [StringIntHandler]
        [IntValueHandler]
        public void VoidFunction()
        {
        }

        [StringIntHandler]
        public string TestDoubleOverrideString()
        {
            return "Goodbye World.";
        }

        [StringIntHandler]
        public int TestDoubleOverrideInt()
        {
            return -1;
        }

        [StringValueHandler]
        public static string TestStaticReturnString()
        {
            return string.Empty;
        }

        [IntValueHandler]
        public static int TestStaticReturnInt()
        {
            return -1;
        }

    }


    [TestClass]
    public class AspectExitHandlerTest
    {
        [TestMethod]
        public void TestOnExitObjectResultNoParameters()
        {
            var cut = new ExitHandlerClassUnderText();

            var res = cut.TestReturnString();

            Assert.AreEqual("Hello World", res);
            Assert.AreEqual(string.Empty, StringValueHandlerAttribute.Result);

        }

        [TestMethod]
        public void TestOnExitObjectResultWithParameters()
        {
            var cut = new ExitHandlerClassUnderText();

            var res = cut.TestReturnString("Test Result");

            Assert.AreEqual("Hello World", res);
            Assert.AreEqual("Test Result", StringValueHandlerAttribute.Result);
        }

        [TestMethod]
        public void TestOnExitValueResultNoParameters()
        {
            var cut = new ExitHandlerClassUnderText();

            var res = cut.TestReturnInt();

            Assert.AreEqual(42, res);
            Assert.AreEqual(-1, IntValueHandlerAttribute.Result);
        }
               

        [TestMethod]
        public void TestOnExitValueResultWithParameters()
        {
            var cut = new ExitHandlerClassUnderText();

            var res = cut.TestReturnInt(314);

            Assert.AreEqual(42, res);
            Assert.AreEqual(314, IntValueHandlerAttribute.Result);
        }

        [TestMethod]
        public void TestVoidOnExitOverrideInterface()
        {
            var cut = new ExitHandlerClassUnderText();

            cut.OnExitOverrides();

            Assert.IsFalse(VoidOverrideHandlerAttribute.OnExitSpecilaizationCalled);
            Assert.IsTrue(VoidOverrideHandlerAttribute.OnExitCalled);
        }

        [TestMethod]
        public void TestOnExitInvalidTypeIsNotPlaced()
        {
            var cut = new ExitHandlerClassUnderText();

            cut.ReturnsAnInt();

            Assert.AreEqual(null, StringValueHandlerAttribute.Result);
        }

        [TestMethod]
        public void TestOnExitVoidIsNotPlaced()
        {
            var cut = new ExitHandlerClassUnderText();

            cut.VoidFunction();

            Assert.AreEqual(null, StringValueHandlerAttribute.Result);
            Assert.AreEqual(0, IntValueHandlerAttribute.Result);
        }

        [TestMethod]
        public void TestMultipleInterfaceCorrectType()
        {
            var cut = new ExitHandlerClassUnderText();

            var intRes = cut.TestDoubleOverrideInt();
            Assert.AreEqual(-1, StringIntHandlerAttribute.IntResult);
            Assert.AreEqual(42, intRes);

            var strRes = cut.TestDoubleOverrideString();

            Assert.AreEqual("Goodbye World.", StringIntHandlerAttribute.StringResult);
            Assert.AreEqual("Hello World", strRes);
        }

        [TestMethod]
        public void TestStaticOnExitObjectResultNoParameters()
        {
            var res = ExitHandlerClassUnderText.TestStaticReturnString();

            Assert.AreEqual("Hello World", res);
            Assert.AreEqual(string.Empty, StringValueHandlerAttribute.Result);
        }

        [TestMethod]
        public void TestStaticOnExitValueResultNoParameters()
        {
            var res = ExitHandlerClassUnderText.TestStaticReturnInt();

            Assert.AreEqual(42, res);
            Assert.AreEqual(-1, IntValueHandlerAttribute.Result);
        }

    }
}
