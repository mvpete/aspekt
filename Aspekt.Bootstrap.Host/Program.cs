namespace Aspekt.Bootstrap.Host
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Bootstrap.Apply(args[0], new ReferencedAssembly[] {});
        }
    }
}
