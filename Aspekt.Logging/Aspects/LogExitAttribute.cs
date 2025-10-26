namespace Aspekt.Logging
{
    public sealed class LogExitAttribute : Aspect
    {
        public override void OnExit(MethodArguments args)
        {
            LoggingService.OnExit(args);
        }
    }
}
