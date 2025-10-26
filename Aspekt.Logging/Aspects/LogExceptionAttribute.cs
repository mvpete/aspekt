namespace Aspekt.Logging
{
    public sealed class LogExceptionAttribute : Aspect
    {
        public override void OnException(MethodArguments args, Exception e)
        {
            LoggingService.OnException(args, e);
        }
    }
}
