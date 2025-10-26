namespace Aspekt.Logging
{
    public sealed class LogAttribute : Aspect
    {
        public override void OnEntry(MethodArguments args)
        {
            LoggingService.OnEntry(args);
        }
        public override void OnExit(MethodArguments args)
        {
            LoggingService.OnExit(args);
        }
        public override void OnException(MethodArguments args, Exception e)
        {
            LoggingService.OnException(args, e);
        }

    }
}
