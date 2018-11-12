using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Aspekt.Bootstrap
{
    /// <summary>
    /// A captured method target encapsulates the method, arguments, and exception
    /// handler used for the aspects. We don't want to create more than one try
    /// catch for each aspect.
    /// </summary>
    class MethodTarget
    {
        public MethodDefinition Method { get; internal set; }
        public VariableDefinition MethodArguments { get; set; }
        public VariableDefinition Exception { get; set; }
        public ExceptionHandler ExceptionHandler { get; set; }

        public Instruction StartInstruction { get; set; }

        public MethodTarget(MethodDefinition md)
        {
            Method = md;
        }
    }
}
