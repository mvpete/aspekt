namespace Aspekt.Logging.Test
{
    [Log]
    internal class ClassLevelLogging
    {
        public void NoArguments()
        {
        }

        public void WithArgument(string argument)
        {
        }

        public void WithArguments(string argument1, int argument2)
        {
        }

        public void GenerateException()
        {
            throw new Exception("blargh!");
        }
    }
}
