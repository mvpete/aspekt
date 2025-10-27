using Mono.Cecil;

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
                        methAttrs.AddRange(meth.CustomAttributes.Where(a => pred(a)));
                        methAttrs.AddRange(classAttrs);

                        // Check for parameter-level contract attributes and convert them to method-level attributes
                        var parameterAttrs = GetParameterContractAttributes(meth);
                        methAttrs.AddRange(parameterAttrs);

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
        /// Scans method parameters for contract attributes and converts them to method-level attributes.
        /// </summary>
        private static List<CustomAttribute> GetParameterContractAttributes(MethodDefinition method)
        {
            var methodLevelAttrs = new List<CustomAttribute>();
            var module = method.Module;

            for (int i = 0; i < method.Parameters.Count; i++)
            {
                var parameter = method.Parameters[i];
                var parameterName = parameter.Name;

                foreach (var paramAttr in parameter.CustomAttributes)
                {
                    // Convert parameter-level attribute to method-level attribute
                    var convertedAttr = ConvertParameterAttributeToMethodLevel(paramAttr, parameterName, module);
                    if (convertedAttr != null)
                    {
                        methodLevelAttrs.Add(convertedAttr);
                    }
                }
            }

            return methodLevelAttrs;
        }

        /// <summary>
        /// Finds a type by name in the loaded assemblies.
        /// </summary>
        private static TypeDefinition? FindTypeInAssemblies(ModuleDefinition module, string typeName)
        {
            // Search in the current module first
            var type = module.Types.FirstOrDefault(t => t.FullName == typeName);
            if (type != null) return type;

            // Search in referenced assemblies
            foreach (var assemblyRef in module.AssemblyReferences)
            {
                try
                {
                    var assembly = module.AssemblyResolver.Resolve(assemblyRef);
                    if (assembly != null)
                    {
                        foreach (var mod in assembly.Modules)
                        {
                            type = mod.Types.FirstOrDefault(t => t.FullName == typeName);
                            if (type != null) return type;
                        }
                    }
                }
                catch
                {
                    // Ignore resolution errors
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a parameter-level attribute to a method-level attribute by injecting the parameter name.
        /// </summary>
        private static CustomAttribute? ConvertParameterAttributeToMethodLevel(CustomAttribute paramAttr, string parameterName, ModuleDefinition module)
        {
            var attrType = paramAttr.AttributeType.Resolve();
            if (attrType == null) return null;

            // Find a constructor that:
            // 1. Takes a string as the first parameter (for parameter name)
            // 2. Has exactly one more parameter than the parameter attribute constructor (for the parameter name)
            var paramCtorArgCount = paramAttr.ConstructorArguments.Count;
            var targetCtor = attrType.Methods.FirstOrDefault(m => 
                m.IsConstructor && 
                m.Parameters.Count == paramCtorArgCount + 1 && 
                m.Parameters[0].ParameterType.FullName == "System.String");

            if (targetCtor == null)
            {
                // No matching constructor found - this attribute doesn't support method-level usage with parameter name
                throw new InvalidOperationException($"Parameter attribute {attrType.FullName} does not have a constructor with 'parameterName' as the first argument. All parameter-level contract attributes must have a constructor that takes the parameter name as the first string argument.");
            }

            // Create the method-level attribute with parameter name as first argument
            var methodLevelAttr = new CustomAttribute(module.ImportReference(targetCtor));
            
            // Add parameter name as first argument
            methodLevelAttr.ConstructorArguments.Add(new CustomAttributeArgument(module.TypeSystem.String, parameterName));
            
            // Copy all constructor arguments from the parameter attribute, ensuring proper types for the target constructor
            for (int i = 0; i < paramAttr.ConstructorArguments.Count; i++)
            {
                var sourceArg = paramAttr.ConstructorArguments[i];
                var targetParamType = targetCtor.Parameters[i + 1].ParameterType; // +1 to skip the string parameter
                
                // Create properly typed argument for the target constructor
                var targetArg = new CustomAttributeArgument(targetParamType, sourceArg.Value);
                methodLevelAttr.ConstructorArguments.Add(targetArg);
            }

            return methodLevelAttr;
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
