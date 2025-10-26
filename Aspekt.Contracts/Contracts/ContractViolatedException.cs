namespace Aspekt.Contracts
{
    public class ContractViolatedException : Exception
    {
        public ContractViolatedException(string message)
            : base(message)
        {
        }
        public ContractViolatedException()
        {
        }
    }
}
