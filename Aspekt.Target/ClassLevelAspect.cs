namespace Aspekt.Test
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal sealed class ClassLevelAspect : Aspect
    {
        public static Action<MethodArguments> OnMethodEntry { get; set; } = (e) => throw new NotImplementedException();
        public static Action<MethodArguments> OnMethodExit { get; set; } = (e) => throw new NotImplementedException();
        public static Action<MethodArguments, Exception> OnMethodException { get; set; } = (e, ex) => throw new NotImplementedException();

        public override void OnEntry(MethodArguments args)
        {
            OnMethodEntry(args);
        }

        public override void OnExit(MethodArguments args)
        {
            OnMethodExit(args);
        }

        public override void OnException(MethodArguments args, Exception e)
        {
            OnMethodException(args, e);
        }

    }
}
