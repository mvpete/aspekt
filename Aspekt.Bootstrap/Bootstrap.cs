using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aspekt.Bootstrap
{

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

    public static class Bootstrap
    {
        // EnumerateMethods
        private static void EnumerateMethodAttributes(ModuleDefinition module, Action<TypeDefinition, MethodTarget, CustomAttribute> enumFn, Predicate<CustomAttribute> pred)
        {
            foreach (var t in module.Types)
                foreach (var meth in t.Methods)
                {
                    var target = new MethodTarget(meth);
                    foreach (var attr in meth.CustomAttributes)
                    {
                        if (pred != null && pred(attr))
                            enumFn(t, target, attr);
                    }
                }
        }

        // Enhancement here, use IEnumerable since it's a more generic collection type. (Collection<T> impllements IEnumerable)
        private static bool HasCustomAttributeType(IEnumerable<CustomAttribute> attributes, Type t)
        {
            return attributes.Any(attr => attr.AttributeType.FullName == t.FullName);
        }

        private static bool CompareParameters(Collection<ParameterDefinition> pars, params Type[] types)
        {
            if (pars.Count != types.Length)
                return false;
            for(int i=0; i<pars.Count; ++i)
            {
                if (pars[i].ParameterType.FullName != types[i].FullName)
                    return false;
            }
            return true;
        }

        private static bool HasMethod(TypeDefinition td, String methodName, params Type[] types)
        {
            return td.Methods.Any((mth) => { return mth.Name == methodName && CompareParameters(mth.Parameters, types); });
        }

        private static PropertyDefinition GetAttributeProperty(CustomAttribute attr, Predicate<PropertyDefinition> pred)
        {
            // FirstOrDefault extension method will return null if no item matches predicate.
            return attr.AttributeType.Resolve().Properties.FirstOrDefault(p => pred(p));
        }


        // Generating IL

        private static VariableDefinition CaptureMethodArguments(InstructionHelper ic, MethodDefinition md)
        {
            if (md.Parameters.Count == 0)
                return null;
            // first we need something to store the parameters in
            var argList = new VariableDefinition(md.Module.Import(typeof(Arguments)));
            md.Body.Variables.Add(argList);

            ic.Next(OpCodes.Ldc_I4, md.Parameters.Count);
            ic.NewObj<Arguments>(typeof(Int32)); // this will create the object we want on the stack
            ic.Next(OpCodes.Stloc, argList);
              

            for (int i = 0; i < md.Parameters.Count; ++i)
            {
                var p = md.Parameters[i];
                var pType = p.ParameterType;
                ic.Next(OpCodes.Ldloc, argList);
                ic.Next(OpCodes.Ldarg, p);
                // I don't think I care if it

                if (pType.IsValueType || pType.IsGenericParameter)
                    ic.Next(OpCodes.Box, pType);

                ic.CallVirt<Arguments>("Add", typeof(object));

            }

            return argList;
        }

        private static VariableDefinition GenerateMethodArgs(InstructionHelper ic, VariableDefinition argList, MethodDefinition md)
        {
            var methArgs = ic.NewVariable<MethodArguments>();
            ic.Next(OpCodes.Ldstr, md.Name);
            ic.Next(OpCodes.Ldstr, md.FullName);
            if (argList == null)
                ic.Next(OpCodes.Ldnull);
            else
                ic.Next(OpCodes.Ldloc, argList);
            ic.Next(OpCodes.Ldarg_0);
            ic.NewObj<MethodArguments>(typeof(String), typeof(String), typeof(Arguments), typeof(object));
            ic.Next(OpCodes.Stloc, methArgs);
            return methArgs;
        }

        private static void LoadArg(InstructionHelper ic, CustomAttributeArgument arg)
        {
            var type = arg.Type.MetadataType;
            switch (type)
            {
                case MetadataType.Void:
                    throw new Exception("no support for void ctor");
                case MetadataType.Boolean:
                    ic.Next(OpCodes.Ldc_I4, (bool)arg.Value);
                    return;
                case MetadataType.ValueType:
                case MetadataType.SByte:
                case MetadataType.Byte:
                case MetadataType.Char:
                case MetadataType.Int16:
                case MetadataType.Int32:
                case MetadataType.UInt16:
                case MetadataType.UInt32:
                    ic.Next(OpCodes.Ldc_I4, (int)arg.Value);
                    return;
		        case MetadataType.Int64:
                case MetadataType.UInt64:
                    ic.Next(OpCodes.Ldc_I8, (long)arg.Value);
                    return;
                case MetadataType.Double:
                    ic.Next(OpCodes.Ldc_R8, (double)arg.Value);
                    return;
                case MetadataType.String:
                    ic.Next(OpCodes.Ldstr, (string)arg.Value);
                    return;
                case MetadataType.Class:
                    // If it's a class type AND System.Type then I need to load the type. Otherwise not sure.
                    if (arg.Type.FullName == typeof(Type).FullName)
                    {
                        ic.Next(OpCodes.Ldtoken, (TypeReference)arg.Value);
                        ic.Call<Type>("GetTypeFromHandle", typeof(RuntimeTypeHandle));
                        return;
                    }
                    else
                        throw new Exception("unknown type");
                // Charly -- Removed some redundant enum types, since default will catch the ones not stated in the switch statement. 
                case MetadataType.Single:
                case MetadataType.Pointer:
                case MetadataType.ByReference:
                case MetadataType.Var:
                case MetadataType.Array:
                case MetadataType.GenericInstance:
                case MetadataType.TypedByReference:
                case MetadataType.IntPtr:
                case MetadataType.UIntPtr:
                case MetadataType.FunctionPointer:
                case MetadataType.Object:
                case MetadataType.MVar:
                case MetadataType.RequiredModifier:
                case MetadataType.OptionalModifier:
                case MetadataType.Sentinel:
                case MetadataType.Pinned:
                    throw new NotImplementedException($"Type {type} is not implemented");
                default:
                    throw new Exception("unknown type");
            }
        }

        private static void PlaceOnExitCalls(ILProcessor il, ModuleDefinition module, MethodDefinition md, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            for(int i=0; i<md.Body.Instructions.Count; ++i)
            {
                Instruction inst = md.Body.Instructions[i];
                if (inst.OpCode == OpCodes.Ret)
                {
                    Instruction rep = inst;
                    if (md.ReturnType.FullName != typeof(void).FullName)
                        rep = inst.Previous; // get the previous instruction, because this SHOULD be the retval

                    var ih = new InstructionHelper(module, il, rep, InstructionHelper.Insert.Before);
                    ih.Next(OpCodes.Ldloc, attrVar);
                    ih.Next(OpCodes.Ldloc, methArgs);
                    ih.CallVirt<Aspect>("OnExit", typeof(MethodArguments));
                    i = i + 3;  // We just added 3 instructions
                }

            }
        }

        private static void InsertOnEntryCalls(InstructionHelper ih, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            ih.Next(OpCodes.Ldloc, attrVar);
            ih.Next(OpCodes.Ldloc, methArgs);
            ih.CallVirt<Aspect>("OnEntry", typeof(MethodArguments));
        }

        private static VariableDefinition CreateAttribute(InstructionHelper ic, VariableDefinition methodArgs, MethodDefinition md, CustomAttribute attr)
        {
            // charly some arguments on this method are not used. (methodArgs and md)
            
            var attrVar = ic.NewVariable(attr.AttributeType);
            // put the arguments on the stack. What about calling convention???
            foreach(var arg in attr.ConstructorArguments)
            {
                LoadArg(ic, arg);
            }
            ic.NewObj(attr.Constructor);
            ic.Next(OpCodes.Stloc, attrVar);
            return attrVar;
        }

        public static void Apply(String targetFileName)
        {
            var module = ModuleDefinition.ReadModule(targetFileName);

            // I know that right now, if we have multiple attributes, we're going to generate multiple method arguments.
            // I know how to deal with this.
            EnumerateMethodAttributes(module, (classType, target, attr) =>
             {
                 var meth = target.Method;
                 meth.Body.SimplifyMacros();
                 var il = meth.Body.GetILProcessor();

                 var fi = meth.Body.Instructions.First();
                 InstructionHelper ih;
                 if(target.StartInstruction == null)
                    ih = new InstructionHelper(module, il, fi, InstructionHelper.Insert.Before);
                 else
                    ih = new InstructionHelper(module, il, target.StartInstruction, InstructionHelper.Insert.After);
                 
                 if (target.MethodArguments == null)
                 {
                    var args = CaptureMethodArguments(ih, meth);
                    target.MethodArguments = GenerateMethodArgs(ih, args, meth);
                 }

                 
                 // if the attribute overrides the method, we will put the call in.
                 // Otherwise, we will not.
                 var attrVar = CreateAttribute(ih, target.MethodArguments, meth, attr);

                 if(HasMethod(attr.AttributeType.Resolve(), "OnEntry", typeof(MethodArguments)))
                    InsertOnEntryCalls(ih, attrVar, target.MethodArguments);

                 target.StartInstruction = ih.LastInstruction; // so that we will create the next aspects AFTER

                 // walk the instructions looking for returns, based on what teh function is returning 
                 // is where we inject the OnExit instructions.
                 // so how do I tell what the function returns?
                 if (HasMethod(attr.AttributeType.Resolve(), "OnExit", typeof(MethodArguments)))
                     PlaceOnExitCalls(il,module,meth,attrVar,target.MethodArguments);


                 if (HasMethod(attr.AttributeType.Resolve(), "OnException", typeof(MethodArguments), typeof(Exception)))
                 {
                     if (target.ExceptionHandler == null)
                     {
                         var exception = new VariableDefinition("ex", meth.Module.Import(typeof(Exception)));
                         meth.Body.Variables.Add(exception);

                         var c = new InstructionHelper(module, il, meth.Body.Instructions.Last());
                         c.Next(il.Create(OpCodes.Stloc_S, exception))
                            .Next(OpCodes.Ldloc, attrVar)
                            .Next(OpCodes.Ldloc, target.MethodArguments)
                            .Next(OpCodes.Ldloc_S, exception)
                            .CallVirt<Aspect>("OnException", typeof(MethodArguments), typeof(Exception))
                            .Next(OpCodes.Rethrow)
                            .Next(OpCodes.Ret);

                         var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
                         {
                             TryStart = target.StartInstruction.Next,
                             TryEnd = c.FirstInstruction,
                             HandlerStart = c.FirstInstruction,
                             HandlerEnd = c.LastInstruction,
                             CatchType = module.Import(typeof(Exception)),
                         };

                         target.Exception = exception;
                         target.ExceptionHandler = handler;
                         meth.Body.ExceptionHandlers.Add(handler);
                     }
                     else
                     {
                         var c = new InstructionHelper(module, il, target.ExceptionHandler.HandlerStart);
                         c.Next(OpCodes.Ldloc, attrVar)
                             .Next(OpCodes.Ldloc, target.MethodArguments)
                             .Next(OpCodes.Ldloc_S, target.Exception)
                             .CallVirt<Aspect>("OnException", typeof(MethodArguments), typeof(Exception));
                     }
                     
                 }

                 

                 meth.Body.OptimizeMacros();
             }, (attr) =>
             {
                 try
                 {
                     return attr.AttributeType.Resolve().BaseType.FullName == (typeof(Aspect).FullName);
                 }
                 catch (Exception)
                 {
                     return false; // if we can't resolve, then we just skip it.
                 }
             });


            module.Write(targetFileName);
        }
    }


}
