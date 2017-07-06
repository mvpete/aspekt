using System;

namespace Aspekt
{
    public abstract class Aspect : System.Attribute
    {
        public abstract void OnEntry(MethodArguments args);
        public abstract void OnExit(MethodArguments args);
        public abstract void OnException(MethodArguments args, Exception e);
    }
}
