
using System;
using System.Runtime.Serialization;

namespace Aspekt.Contracts
{
    /// <summary>
    /// Exception thrown when a contract violation occurs.
    /// </summary>
    [Serializable]
    public class ContractException : Exception
    {
        public string ContractType { get; }
        public string MethodName { get; }

        public ContractException(string contractType, string methodName, string message) 
            : base($"{contractType} contract violation in {methodName}: {message}")
        {
            ContractType = contractType;
            MethodName = methodName;
        }

        public ContractException(string contractType, string methodName, string message, Exception innerException) 
            : base($"{contractType} contract violation in {methodName}: {message}", innerException)
        {
            ContractType = contractType;
            MethodName = methodName;
        }

#if !NETSTANDARD2_1
        protected ContractException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ContractType = info.GetString(nameof(ContractType)) ?? "";
            MethodName = info.GetString(nameof(MethodName)) ?? "";
        }

        [Obsolete]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ContractType), ContractType);
            info.AddValue(nameof(MethodName), MethodName);
        }
        public ContractException()
        {
        }
#endif
    }

    /// <summary>
    /// Exception thrown when a precondition contract is violated.
    /// </summary>
    [Serializable]
    public class PreconditionException : ContractException
    {
        public PreconditionException(string methodName, string condition) 
            : base("Precondition", methodName, condition) { }

        public PreconditionException(string methodName, string condition, Exception innerException) 
            : base("Precondition", methodName, condition, innerException) { }

#if !NETSTANDARD2_1
        protected PreconditionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public PreconditionException()
        {
        }
#endif
    }

    /// <summary>
    /// Exception thrown when a postcondition contract is violated.
    /// </summary>
    [Serializable]
    public class PostconditionException : ContractException
    {
        public PostconditionException(string methodName, string condition) 
            : base("Postcondition", methodName, condition) { }

        public PostconditionException(string methodName, string condition, Exception innerException) 
            : base("Postcondition", methodName, condition, innerException) { }

#if !NETSTANDARD2_1
        protected PostconditionException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public PostconditionException()
        {
        }
#endif
    }

    /// <summary>
    /// Exception thrown when an invariant contract is violated.
    /// </summary>
    [Serializable]
    public class InvariantException : ContractException
    {
        public InvariantException(string methodName, string condition) 
            : base("Invariant", methodName, condition) { }

        public InvariantException(string methodName, string condition, Exception innerException) 
            : base("Invariant", methodName, condition, innerException) { }

#if !NETSTANDARD2_1
        protected InvariantException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        public InvariantException()
        {
        }
#endif
    }
}
