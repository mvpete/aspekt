using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aspekt.Bootstrap
{
    /// <summary>
    /// A class that defines traits about a method.
    /// </summary>
    static class MethodTraits
    {
        public static bool HasMethod(TypeDefinition td, String methodName, params Type[] types)
        {
            return td.Methods.Any((mth) => { return mth.Name == methodName && CompareParameters(mth.Parameters, types); });
        }


        private static bool CompareParameters(Collection<ParameterDefinition> pars, params Type[] types)
        {
            if (pars.Count != types.Length)
                return false;
            for (int i = 0; i < pars.Count; ++i)
            {
                if (pars[i].ParameterType.FullName != types[i].FullName)
                    return false;
            }
            return true;
        }

    }
}
