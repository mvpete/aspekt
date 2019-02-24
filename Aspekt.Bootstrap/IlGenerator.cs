using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aspekt.Bootstrap
{
    /// <summary>
    /// A static helper class for capturing and writing appropriate IL instructions
    /// </summary>
    static class IlGenerator
    {
        public static VariableDefinition CaptureMethodArguments(InstructionHelper ic, MethodDefinition md)
        {
            if (md.Parameters.Count == 0)
                return null;

            // first we need something to store the parameters in
            var argList = new VariableDefinition(md.Module.ImportReference(typeof(Arguments)));
            md.Body.Variables.Add(argList);


            ic.Next(OpCodes.Ldc_I4, md.Parameters.Count);
            ic.NewObj<Arguments>(typeof(Int32)); // this will create the object we want on the stack
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
                    ic.Next(OpCodes.Box, pType);
                ic.NewObj<Argument>(typeof(string), typeof(object));
                ic.Next(OpCodes.Stloc, arg);

                ic.Next(OpCodes.Ldloc, argList);
                ic.Next(OpCodes.Ldloc, arg);
                ic.CallVirt<Arguments>(nameof(Arguments.Add), typeof(Argument));

            }

            return argList;
        }

        public static String GenerateMethodNameFormat(MethodDefinition md)
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
                ic.Next(OpCodes.Ldnull);
            else
                ic.Next(OpCodes.Ldloc, argList);

            if (md.IsStatic)
            {
                ic.Next(OpCodes.Ldnull);
            }
            else
                ic.Next(OpCodes.Ldarg_0);

            ic.NewObj<MethodArguments>(typeof(String), typeof(String), typeof(String), typeof(Arguments), typeof(object));
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
                        throw new Exception("unknown type");
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

        public static void InsertOnExitCalls(ILProcessor il, ModuleDefinition module, MethodDefinition md, VariableDefinition attrVar, VariableDefinition methArgs)
        {

            var returnInst = new List<Tuple<Instruction, Instruction>>();
            // adjust all the return instructions
            for (int i = 0; i < md.Body.Instructions.Count; ++i)
            {
                Instruction inst = md.Body.Instructions[i];
                // Find each return code, and add our three instructions.
                if (inst.OpCode == OpCodes.Ret)
                {
                    Instruction rep = inst;
                    if (md.ReturnType.FullName != typeof(void).FullName)
                        rep = inst.Previous; // get the previous instruction, because this SHOULD be the retval


                    var ih = new InstructionHelper(module, il, rep, InstructionHelper.Insert.Before);
                    ih.Next(OpCodes.Ldloc, attrVar);
                    ih.Next(OpCodes.Ldloc, methArgs);
                    ih.CallVirt<Aspect>(nameof(Aspect.OnExit), typeof(MethodArguments));

                    // This is kind of dumb. I need to store the return instruction, and the first instruction to 
                    // what I added. So that I can go through later, and find instructions that branch to 
                    // the old instruction, and replace that with instruction to branch
                    // to this new instruction.
                    returnInst.Add(new Tuple<Instruction, Instruction>(rep, ih.FirstInstruction));

                    i = i + 3;  // We just added 3 instructions
                }

            }

            // go through and replace the branch instructions.
            for (int i = 0; i < md.Body.Instructions.Count; ++i)
            {
                Instruction inst = md.Body.Instructions[i];
                var jp = returnInst.SingleOrDefault(t => t.Item1 == inst.Operand);
                if (IsBranchInstruction(inst.OpCode) && jp!=null)
                {
                    // create a new branch op to the instruction that I put before.
                    var brto = md.Body.GetILProcessor().Create(inst.OpCode, jp.Item2);
                    md.Body.GetILProcessor().InsertBefore(inst, brto);
                    md.Body.GetILProcessor().Remove(inst); // remove the existing branch.
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
                   code == OpCodes.Brtrue_S;
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
