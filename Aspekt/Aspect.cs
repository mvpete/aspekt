using System;

namespace Aspekt
{
    public abstract class Aspect : System.Attribute
    {
        public virtual void OnEntry(MethodArguments args) { }
        public virtual void OnExit(MethodArguments args) { }
        public virtual void OnException(MethodArguments args, Exception e) { }
        public virtual TResult OnExit<TResult>(MethodArguments args, TResult result) { return result;  }
    }
}
