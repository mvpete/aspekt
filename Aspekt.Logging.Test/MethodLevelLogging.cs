namespace Aspekt.Logging.Test
{
    internal class MethodLevelLogging
    {
        [LogExit]
        public void Method1()
        {
        }

        [LogEntry]
        public void Method2(int argument)
        {
        }

        public void Method3(string argument)
        {
        }

        [LogException]
        public void ThrowException()
        {
            
        }
    }
}
