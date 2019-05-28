using System;
using System.Threading.Tasks;

namespace Aspekt
{
    public abstract class Aspect : System.Attribute
    {
        public virtual void OnEntry(MethodArguments args) { }
        public virtual void OnExit(MethodArguments args) { }
        public virtual void OnException(MethodArguments args, Exception e) { }

        public static Func<Task<T>,T> AsyncOnExit<T>(Aspect a, MethodArguments args)
        {
            return (Task<T> t) =>
            {
                a.OnExit(args);
                return t.Result;
            };
        }
    }
}
