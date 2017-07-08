using System;

namespace Aspekt
{

    // Any method that is decorated with this attribute is to be wrapped in a try/catch
    // where the catch block will catch, exceptionFrom type and wrap it with the exceptionTo
    // this means that the ctor of the exceptionTo, must accept an exceptionFrom as a param.
    [AttributeUsage(AttributeTargets.Method)]
    public class TranslateExceptionAttribute : Aspect
    {
        String message_;

        public TranslateExceptionAttribute(Type exceptionFrom, Type exceptionTo, String val)
        {
            ExceptionFrom = exceptionFrom;
            ExceptionTo = exceptionTo;
            message_ = val;
        }

        public TranslateExceptionAttribute(Type exceptionFrom, Type exceptionTo) 
            : this(exceptionFrom, exceptionTo, null)
        {
        }

        // what to do here
        public override void OnException(MethodArguments arg, Exception e)
        {
            //Console.WriteLine($"OnException - {e.Message}");
            if (e.GetType() == ExceptionFrom)
            {
                // then we rethrow
                var ctor = ExceptionTo.GetConstructor(new Type[] { ExceptionFrom });
                if (ctor == null)
                {
                    throw e;
                }
                var e2 = Convert.ChangeType(ctor.Invoke(new object[] { Convert.ChangeType(e, ExceptionFrom) }), ExceptionTo);
                throw (Exception)e2;
            }
            else
                throw e;
        }

        public Type ExceptionTo { get; }
        public Type ExceptionFrom { get; }


    }

}
