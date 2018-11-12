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
    /// The attribute enumerator will detect and enumerate methods within the assembly, and
    /// apply the appropriate Aspects to them. 
    /// </summary>
    static class AttributeEnumerator
    {
        /// <summary>
        /// Enumerates all functions in the module, passing forth a list of custom attributes that match the predicate. Skipping the method, if there
        /// are no attributes that apply. Then apply each attribute to the method.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="enumFn"></param>
        /// <param name="pred"></param>
        public static void EnumerateMethodAttributes(AssemblyDefinition assembly, Action<TypeDefinition, MethodTarget, CustomAttribute> enumFn, Predicate<CustomAttribute> pred)
        {
            if (pred == null)
                throw new ArgumentNullException("predicate");

            var assemblyAttr = new List<CustomAttribute>();
            assemblyAttr.AddRange(assembly.CustomAttributes.Where(a => pred(a)));

            foreach (var module in assembly.Modules)
            {
                var moduleAttrs = new List<CustomAttribute>();
                // find the attributes at the module
                moduleAttrs.AddRange(assemblyAttr);
                moduleAttrs.AddRange(module.CustomAttributes.Where(a => pred(a)));

                foreach (var t in module.Types)
                {
                    // we don't want to instrument aspects, with aspects.
                    if (t.BaseType != null && t.BaseType.FullName == typeof(Aspect).FullName)
                        continue;

                    var classAttrs = new List<CustomAttribute>();
                    classAttrs.AddRange(moduleAttrs);
                    classAttrs.AddRange(t.CustomAttributes.Where(a => pred(a)));

                    foreach (var meth in t.Methods)
                    {
                        var methAttrs = new List<CustomAttribute>();
                        methAttrs.AddRange(classAttrs);
                        methAttrs.AddRange(meth.CustomAttributes.Where(a => pred(a)));
                        if (methAttrs.Count == 0)
                            continue;

                        var target = new MethodTarget(meth);
                        foreach (var attr in methAttrs)
                        {
                            if (AppliesTo(attr, meth))
                                enumFn(t, target, attr);
                        }
                    }
                }
            }
            
        }

        /// <summary>
        /// Defines whether or not the attribute applies to the given method.
        /// </summary>
        /// <param name="attr"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static bool AppliesTo(CustomAttribute attr, MethodDefinition method)
        {
            return true;
        }

    }
}
