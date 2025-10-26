using Mono.Cecil;
using Mono.Collections.Generic;

namespace Aspekt.Bootstrap
{
    /// <summary>
    /// A class that defines traits about a method.
    /// </summary>
    internal static class MethodTraits
    {
        public static bool HasMethod(TypeDefinition td, string methodName, params Type[] types)
        {
            return td.Methods.Any((mth) => { return mth.Name == methodName && CompareParameters(mth.Parameters, types); });
        }

        public static bool HasGenericMethod(TypeDefinition td, string methodName, int paramCount)
        {
            return td.Methods.Any((mth) => { return mth.Name == methodName && mth.Parameters.Count == paramCount; });
        }

        private static bool CompareParameters(Collection<ParameterDefinition> pars, params Type[] types)
        {
            if (pars.Count != types.Length)
                return false;
            for (var i = 0; i < pars.Count; ++i)
            {
                if (pars[i].ParameterType.FullName != types[i].FullName)
                    return false;
            }
            return true;
        }

    }
}
