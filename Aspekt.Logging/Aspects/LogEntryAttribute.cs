namespace Aspekt.Logging
{
    public sealed class LogEntryAttribute : Aspect
    {
        public override void OnEntry(MethodArguments args)
        {
            LoggingService.OnEntry(args);
        }
    }
}
