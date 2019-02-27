using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aspekt.Bootstrap
{
    /// <summary>
    /// A static helper class for capturing and writing appropriate IL instructions
    /// </summary>
    internal static class IlGenerator
    {
        private static readonly MethodBase OnExitMethod = typeof(Aspect).GetMethod(
            nameof(Aspect.OnExit),
            new [] {typeof(MethodArguments)});

        private static readonly MethodInfo OnExitResult = typeof(Aspect).GetMethods().First(p => p.Name == nameof(Aspect.OnExit) && p.GetParameters().Length == 2);

        public static VariableDefinition CaptureMethodArguments(InstructionHelper ic, MethodDefinition md)
        {
            if (md.Parameters.Count == 0)
            {
                return null;
            }

            // first we need something to store the parameters in
            var argList = ic.NewVariable(typeof(Arguments));

            ic.Next(OpCodes.Ldc_I4, md.Parameters.Count);
            ic.NewObj<Arguments>(typeof(int)); // this will create the object we want on the stack
            ic.Next(OpCodes.Stloc, argList);


            for (int i = 0; i < md.Parameters.Count; ++i)
            {
                var p = md.Parameters[i];
                var pType = p.ParameterType;

                var arg = new VariableDefinition(md.Module.ImportReference(typeof(Argument)));
                md.Body.Variables.Add(arg);

                ic.Next(OpCodes.Ldstr, p.Name);
                ic.Next(OpCodes.Ldarg, p);

                if (pType.IsValueType || pType.IsGenericParameter)
                {
                    ic.Next(OpCodes.Box, pType);
                }

                ic.NewObj<Argument>(typeof(string), typeof(object));
                ic.Next(OpCodes.Stloc, arg);

                ic.Next(OpCodes.Ldloc, argList);
                ic.Next(OpCodes.Ldloc, arg);
                ic.CallVirt<Arguments>(nameof(Arguments.Add), typeof(Argument));

            }

            return argList;
        }

        public static string GenerateMethodNameFormat(MethodDefinition md)
        {
            var name = md.FullName;
            int i = 0;
            foreach (var p in md.Parameters)
            {
                name = name.Replace(p.ParameterType.FullName, $"{{{i++}}}");
            }
            return name;
        }

        public static VariableDefinition GenerateMethodArgs(InstructionHelper ic, VariableDefinition argList, MethodDefinition md)
        {
            var methArgs = ic.NewVariable<MethodArguments>();
            ic.Next(OpCodes.Ldstr, md.Name);
            ic.Next(OpCodes.Ldstr, md.FullName);
            ic.Next(OpCodes.Ldstr, GenerateMethodNameFormat(md));
            if (argList == null)
            {
                ic.Next(OpCodes.Ldnull);
            }
            else
            {
                ic.Next(OpCodes.Ldloc, argList);
            }

            if (md.IsStatic)
            {
                ic.Next(OpCodes.Ldnull);
            }
            else
            {
                ic.Next(OpCodes.Ldarg_0);
            }

            ic.NewObj<MethodArguments>(typeof(string), typeof(string), typeof(string), typeof(Arguments), typeof(object));
            ic.Next(OpCodes.Stloc, methArgs);
            return methArgs;
        }

        public static void LoadArg(InstructionHelper ic, CustomAttributeArgument arg)
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
                    {
                        throw new Exception("unknown type");
                    }

                case MetadataType.Object:
                    // The object comes in as a CustomAttributeArgument.
                    LoadArg(ic, (CustomAttributeArgument)arg.Value);
                    return;
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

        public static void InsertOnExitCalls(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            // adjust all the return instructions
            for (var i = 0; i < targetMethod.Body.Instructions.Count; ++i)
            {
                var inst = targetMethod.Body.Instructions[i];
                // Find each return code, and add our three instructions.
                if (inst.OpCode == OpCodes.Ret)
                {
                    var rep = inst;
                    if (targetMethod.ReturnType.MetadataType != MetadataType.Void)
                    {
                        rep = inst.Previous; // get the previous instruction, because this SHOULD be the retval
                    }
                    // Replace it with three new instructions, plus itself. We use ReplaceInstructionAndLeaveTarget
                    // to reuse the logic to adjust all the branch and exception handling adjustments.
                    // This would be low performance (during bootstrap phase) if there are lots of Ret instructions,
                    // but generally there is only one Ret per method.
                    ReplaceInstructionAndLeaveTarget(
                        il,
                        targetMethod,
                        rep,
                        il.Create(OpCodes.Ldloc, attrVar),
                        il.Create(OpCodes.Ldloc, methArgs),
                        il.Create(OpCodes.Callvirt, targetMethod.Module.ImportReference(OnExitMethod)));

                    i = i + 3;  // We just added 3 instructions
                }
            }
        }

        public static void InsertOnExitResultCalls(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            // adjust all the return instructions
            for (var i = 0; i < targetMethod.Body.Instructions.Count; ++i)
            {
                var inst = targetMethod.Body.Instructions[i];
                // Find each return code, and add our three instructions.
                if (inst.OpCode == OpCodes.Ret)
                {
                    var rep = inst;
                    rep = inst.Previous; // get the previous instruction, because this SHOULD be the retval

                    if (IsLoadInstruction(rep.OpCode))
                    {
                        ReplaceInstructionAndLeaveTarget(il, targetMethod, rep, il.Create(OpCodes.Ldloc, attrVar), il.Create(OpCodes.Ldloc, methArgs));
                        i = i + 2;
                    }
                    else
                    {
                        // Create a variable, store it, load the
                        var ih = new InstructionHelper(targetMethod.Module, il, rep, InstructionHelper.Insert.After);
                        var retVar = ih.NewVariable(targetMethod.ReturnType);
                        ih.Next(il.Create(OpCodes.Stloc_S, retVar));
                        ih.Next(il.Create(OpCodes.Ldloc, attrVar));
                        ih.Next(il.Create(OpCodes.Ldloc, methArgs));
                        ih.Next(il.Create(OpCodes.Ldloc, retVar));
                        i = i + 3;
                    }

                    // If it's a load instruction, I can just go one up, and put the   

                    var git = new GenericInstanceMethod(targetMethod.Module.ImportReference(OnExitResult));
                    git.GenericArguments.Add(targetMethod.ReturnType);

                    // Replace it with three new instructions, plus itself. We use ReplaceInstructionAndLeaveTarget
                    // to reuse the logic to adjust all the branch and exception handling adjustments.
                    // This would be low performance (during bootstrap phase) if there are lots of Ret instructions,
                    // but generally there is only one Ret per method.
                    ReplaceInstructionAndLeaveTarget(
                        il,
                        targetMethod,
                        inst,                        
                        il.Create(OpCodes.Callvirt, git));

                    i = i + 2;  // We just added 3 instructions
                }
            }
        }

        /// <summary>
        /// Replace an instruction with a new set of instructions, redirecting all branches to
        /// the new instruction and adjusting exception handlers.
        /// </summary>
        /// <param name="il"><see cref="ILProcessor"/> to use.</param>
        /// <param name="method"><see cref="MethodDefinition"/> where the replacement is taking place.</param>
        /// <param name="targetInstruction">Instruction to be replaced.</param>
        /// <param name="instructions">New instructions to insert.</param>
        public static void ReplaceInstruction(ILProcessor il, MethodDefinition method,
            Instruction targetInstruction, params Instruction[] instructions)
        {
            ReplaceInstructionAndLeaveTarget(il, method, targetInstruction, instructions);

            il.Remove(targetInstruction);
        }

        /// <summary>
        /// Replace an instruction with a new set of instructions, redirecting all branches to
        /// the new instruction and adjusting exception handlers. The target instruction, however,
        /// is left in place after the new instructions.
        /// </summary>
        /// <param name="il"><see cref="ILProcessor"/> to use.</param>
        /// <param name="method"><see cref="MethodDefinition"/> where the replacement is taking place.</param>
        /// <param name="targetInstruction">Instruction where the new instructions are inserted.</param>
        /// <param name="instructions">New instructions to insert.</param>
        public static void ReplaceInstructionAndLeaveTarget(ILProcessor il, MethodDefinition method,
            Instruction targetInstruction, params Instruction[] instructions)
        {
            var ih = new InstructionHelper(method.Module, il, targetInstruction, InstructionHelper.Insert.Before);
            for (var i = 0; i < instructions.Length; ++i)
            {
                ih.Next(instructions[i]);
            }

            // go through and replace the branch instructions.
            for (var i = 0; i < method.Body.Instructions.Count; ++i)
            {
                var inst = method.Body.Instructions[i];
                if (IsBranchInstruction(inst.OpCode) && inst.Operand == targetInstruction)
                {
                    ReplaceInstruction(
                        il,
                        method,
                        inst,
                        il.Create(inst.OpCode, ih.FirstInstruction));
                }
            }

            // go through and replace exception handler ends
            for (var i = 0; i < method.Body.ExceptionHandlers.Count; ++i)
            {
                var handler = method.Body.ExceptionHandlers[i];

                if (targetInstruction == handler.TryStart)
                {
                    handler.TryStart = ih.FirstInstruction;
                }

                if (targetInstruction == handler.TryEnd)
                {
                    handler.TryEnd = ih.FirstInstruction;
                }

                if (targetInstruction == handler.HandlerEnd)
                {
                    handler.HandlerEnd = ih.FirstInstruction;
                }

                if (targetInstruction == handler.HandlerStart)
                {
                    handler.HandlerStart = ih.FirstInstruction;
                }
            }
        }

        private static bool IsBranchInstruction(OpCode code)
        {
            return code == OpCodes.Br_S ||
                   code == OpCodes.Br ||
                   code == OpCodes.Brfalse ||
                   code == OpCodes.Brfalse_S ||
                   code == OpCodes.Brtrue ||
                   code == OpCodes.Brtrue_S ||
                   code == OpCodes.Leave ||
                   code == OpCodes.Leave_S;
        }

        private static bool IsLoadInstruction(OpCode code)
        {
            return code == OpCodes.Ldloc ||
                   code == OpCodes.Ldloca ||
                   code == OpCodes.Ldloca_S ||
                   code == OpCodes.Ldloc_0 ||
                   code == OpCodes.Ldloc_1 ||
                   code == OpCodes.Ldloc_2 ||
                   code == OpCodes.Ldloc_3 ||
                   code == OpCodes.Ldnull ||
                   code == OpCodes.Ldobj ||
                   code == OpCodes.Ldsfld ||
                   code == OpCodes.Ldsflda ||
                   code == OpCodes.Ldstr ||
                   code == OpCodes.Ldtoken;
        }

        public static void InsertOnEntryCalls(InstructionHelper ih, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            // load the attribute on the stack
            ih.Next(OpCodes.Ldloc, attrVar);
            // load the method args on the stck
            ih.Next(OpCodes.Ldloc, methArgs);
            // make the call
            ih.CallVirt<Aspect>("OnEntry", typeof(MethodArguments));
        }

        public static VariableDefinition CreateAttribute(InstructionHelper ic, CustomAttribute attr)
        {
            var attrVar = ic.NewVariable(attr.AttributeType);
            // put the arguments on the stack. What about calling convention???
            foreach (var arg in attr.ConstructorArguments)
            {
                LoadArg(ic, arg);
            }

            ic.NewObj(attr.Constructor);
            ic.Next(OpCodes.Stloc, attrVar);
            return attrVar;
        }


    }
}
