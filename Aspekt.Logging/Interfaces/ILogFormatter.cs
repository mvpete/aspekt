namespace Aspekt.Logging.Interfaces
{
    public interface ILogMessageFormatter
    {
        string Format(Events evt, MethodArguments a, Exception e);
    }
}
