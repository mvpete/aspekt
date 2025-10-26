using Aspekt.Logging.Interfaces;

namespace Aspekt.Logging.Formatters
{
    public class DefaultLogFormatter : ILogMessageFormatter
    {
        public string Format(Events evt, MethodArguments a, Exception e)
        {
            switch (evt)
            {
                case Events.OnEnter:
                    return string.Format($"Entering {a.FormattedName}", a.Arguments.ToArray());
                case Events.OnExit:
                    return string.Format($"Exiting {a.FullName}");
                case Events.OnException:
                    return string.Format($"Exception occured {a.FullName}; {e.Message}");
                default:
                    throw new InvalidOperationException("unknown log event");
            }
        }
    }
}
