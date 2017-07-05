using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt
{

    // Any method that is decorated with this attribute is to be wrapped in a try/catch
    // where the catch block will catch, exceptionFrom type and wrap it with the exceptionTo
    // this means that the ctor of the exceptionTo, must accept an exceptionFrom as a param.
    [AttributeUsage(AttributeTargets.Method)]
    public class TranslateExceptionAttribute : Aspect
    {

        Type exceptionTo_;
        Type exceptionFrom_;
        String message_;

        public TranslateExceptionAttribute(Type exceptionFrom, Type exceptionTo, String val)
        {
            exceptionFrom_ = exceptionFrom;
            exceptionTo_ = exceptionTo;
            message_ = val;
        }

        public TranslateExceptionAttribute(Type exceptionFrom, Type exceptionTo)
        {
            exceptionTo_   = exceptionTo;
            exceptionFrom_ = exceptionFrom;
        }

        public TranslateExceptionAttribute(String s)
        {
        }

        public TranslateExceptionAttribute(int i)
        {
        }

        public override void OnEntry(MethodArguments arg)
        {
        }

        public override void OnExit(MethodArguments arg)
        {
        }

        // what to do here
        public override void OnException(Exception e)
        {
            //Console.WriteLine($"OnException - {e.Message}");
            if (e.GetType() == exceptionFrom_)
            {
                // then we rethrow
                var ctor = exceptionTo_.GetConstructor(new Type[] { exceptionFrom_ });
                if (ctor == null)
                {
                    throw e;
                }
                var e2 = Convert.ChangeType(ctor.Invoke(new object[] { Convert.ChangeType(e, exceptionFrom_) }), exceptionTo_);
                throw (Exception)e2;
            }
            else
                throw e;
        }

        public Type ExceptionTo {  get { return exceptionTo_;  } }
        public Type ExceptionFrom { get { return exceptionFrom_;  } }


    }

}
