namespace Aspekt.Contracts
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class NotNull : Aspect
    {
        public string ArgumentName { get; set; }
        public int? ArgumentIndex { get; set; }

        public NotNull(int argumentIndex)
        {
            ArgumentIndex = argumentIndex;
        }
        public NotNull(string argumentName)
        {
            ArgumentName = argumentName;
        }

        public override void OnEntry(MethodArguments args)
        {
            object? arg = null;
            if (ArgumentIndex != null)
            {
                arg = args.Arguments.GetArgumentByIndex(ArgumentIndex.Value);
            }
            else
            {
                arg = args.Arguments.GetArgumentValueByName(ArgumentName);
            }

            if (arg == null)
            {
                throw new ArgumentNullException(ArgumentName ?? ArgumentIndex.ToString());
            }
        }
    }
}
