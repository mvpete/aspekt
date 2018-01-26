namespace Aspekt.Test
{
    
    class DummyClass
    {
        [MockAspect("Call")]
        public int Call()
        {
            return 5;
        }
    }
}
