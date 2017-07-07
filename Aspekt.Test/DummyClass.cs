namespace Aspekt.Test
{
    
    class DummyClass
    {
        [TestAspect("Call")]
        public int Call()
        {
            return 5;
        }
    }
}
