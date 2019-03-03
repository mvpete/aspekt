namespace Aspekt.Foundation.Test
{
    internal class DummyClass
    {
        [MockAspect("Call")]
        public int Call()
        {
            return 5;
        }
    }
}
