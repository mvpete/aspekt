namespace Aspekt
{
    // Any method that is decorated with this attribute is to be wrapped in a try/catch
    // where the catch block will catch, exceptionFrom type and wrap it with the exceptionTo
    // this means that the ctor of the exceptionTo, must accept an exceptionFrom as a param.
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TranslateExceptionAttribute : Aspect
    {
        public TranslateExceptionAttribute(Type exceptionFrom, Type exceptionTo, string val)
        {
            ExceptionFrom = exceptionFrom;
            ExceptionTo = exceptionTo;
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
                var ctor = ExceptionTo.GetConstructor([ExceptionFrom]);
                if (ctor == null)
                {
                    throw e;
                }
                var e2 = Convert.ChangeType(ctor.Invoke([Convert.ChangeType(e, ExceptionFrom)]), ExceptionTo);
                throw (Exception)e2;
            }
            else
            {
                throw e;
            }
        }

        public Type ExceptionTo { get; }
        public Type ExceptionFrom { get; }
    }

}
