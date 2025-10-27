using Mono.Cecil;
using Mono.Cecil.Cil;
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


            for (var i = 0; i < md.Parameters.Count; ++i)
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
            var i = 0;
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
                    InsertInstructionsAt(
                        il,
                        targetMethod,
                        rep,
                        il.Create(OpCodes.Ldloc, attrVar),
                        il.Create(OpCodes.Ldloc, methArgs),
                        il.Create(OpCodes.Callvirt, attrVar.VariableType.Module.ImportReference(OnExitMethod)));

                    i = i + 3;  // We just added 3 instructions
                }
            }
        }

        /// <summary>
        /// Inserts an appropriate call to OnExit<T> where return values are present. If applied to void function, does nothing.
        /// </summary>
        /// <param name="il"></param>
        /// <param name="targetMethod"></param>
        /// <param name="attrVar"></param>
        /// <param name="methArgs"></param>
        public static void InsertOnExitResultCalls(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            if (targetMethod.ReturnType.MetadataType == MetadataType.Void)
            {
                WeaverLog.LogMethodWarning(targetMethod, 1, "return value handler on void function");
                return;
            }

            // Is derived from IAspectExitHandler<T>
            var iExitHandler = methArgs.VariableType.Module.ImportReference(typeof(IAspectExitHandler<>));
            var gii = new GenericInstanceType(iExitHandler);
            gii.GenericArguments.Add(targetMethod.ReturnType);

            var fullName = gii.ElementType.Resolve().FullName;

            var attrInst = attrVar.VariableType.Resolve();

            // If it implements the right interface
            if (!attrInst.Interfaces.Any(i => i.InterfaceType.FullName == gii.FullName))
            {
                WeaverLog.LogMethodWarning(targetMethod, 2, $"return value handler implements invalid handler type; requires {gii.FullName}");
                return;
            }

            // I just needed a type to resolve the name.
            var methodName = nameof(IAspectExitHandler<int>.OnExit);
            var method = attrInst.Methods.First(md => md.Name == methodName && md.Parameters.Count == 2 && md.Parameters[1].ParameterType.MetadataType == targetMethod.ReturnType.MetadataType);

            // adjust all the return instructions
            for (var i = 0; i < targetMethod.Body.Instructions.Count; ++i)
            {
                var inst = targetMethod.Body.Instructions[i];
                // Find each return code, and add our instructions
                // We need to either, a) insert our attr and method args, before the load op, so the stack lines up
                // b) store the value, load our attr and method args, then load what we stored.
                // then make the call, directly on the attribute.
                if (inst.OpCode == OpCodes.Ret)
                {
                    var rep = inst;
                    rep = inst.Previous; // get the previous instruction, because this SHOULD be the retval

                    if (IsLoadInstruction(rep.OpCode))
                    {
                        InsertInstructionsAt(il, targetMethod, rep,
                            il.Create(OpCodes.Ldloc, attrVar),
                            il.Create(OpCodes.Ldloc, methArgs));
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

                    InsertInstructionsAt(
                        il,
                        targetMethod,
                        inst,
                        il.Create(OpCodes.Call, method));

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
            InsertInstructionsAt(il, method, targetInstruction, instructions);

            il.Remove(targetInstruction);
        }

        /// <summary>
        /// Inserts new instructions before an existing instruction, redirecting all branches to the new instruction and
        /// adjusting exception handlers. The new instructions will always be executed before the target, regardless of
        /// branch statements, and will be including within any exception handler blocks with the target.
        /// </summary>
        /// <param name="il"><see cref="ILProcessor"/> to use.</param>
        /// <param name="method"><see cref="MethodDefinition"/> where the replacement is taking place.</param>
        /// <param name="targetInstruction">Instruction where the new instructions are inserted.</param>
        /// <param name="instructions">New instructions to insert.</param>
        public static void InsertInstructionsAt(ILProcessor il, MethodDefinition method,
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
                    // Fix: Directly update the operand instead of calling ReplaceInstruction
                    // to avoid infinite recursion
                    inst.Operand = ih.FirstInstruction;
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

        /// <summary>
        /// Adds OnExitAsync continuation to async methods
        /// </summary>
        public static void InsertOnExitAsyncContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            var returnTypeName = targetMethod.ReturnType.FullName;
            
            if (returnTypeName == "System.Threading.Tasks.Task")
            {
                // Task (no return value)
                WrapTaskWithContinuation(il, targetMethod, attrVar, methArgs, false, false);
            }
            else if (returnTypeName.StartsWith("System.Threading.Tasks.Task`1"))
            {
                // Task<T> (with return value)
                WrapTaskWithContinuation(il, targetMethod, attrVar, methArgs, true, false);
            }
            else if (returnTypeName == "System.Threading.Tasks.ValueTask")
            {
                // ValueTask (no return value)
                WrapValueTaskWithContinuation(il, targetMethod, attrVar, methArgs, false, false);
            }
            else if (returnTypeName.StartsWith("System.Threading.Tasks.ValueTask`1"))
            {
                // ValueTask<T> (with return value)
                WrapValueTaskWithContinuation(il, targetMethod, attrVar, methArgs, true, false);
            }
        }

        /// <summary>
        /// Adds OnExit sync continuation to async methods
        /// </summary>
        public static void InsertOnExitSyncContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            var returnTypeName = targetMethod.ReturnType.FullName;
            
            if (returnTypeName == "System.Threading.Tasks.Task")
            {
                WrapTaskWithContinuation(il, targetMethod, attrVar, methArgs, false, true);
            }
            else if (returnTypeName.StartsWith("System.Threading.Tasks.Task`1"))
            {
                WrapTaskWithContinuation(il, targetMethod, attrVar, methArgs, true, true);
            }
            else if (returnTypeName == "System.Threading.Tasks.ValueTask")
            {
                WrapValueTaskWithContinuation(il, targetMethod, attrVar, methArgs, false, true);
            }
            else if (returnTypeName.StartsWith("System.Threading.Tasks.ValueTask`1"))
            {
                WrapValueTaskWithContinuation(il, targetMethod, attrVar, methArgs, true, true);
            }
        }

        /// <summary>
        /// Adds OnExit result continuation to async methods (IAspectExitHandler<T>)
        /// </summary>
        public static void InsertOnExitResultContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            var returnTypeName = targetMethod.ReturnType.FullName;
            
            if (returnTypeName.StartsWith("System.Threading.Tasks.Task`1"))
            {
                WrapTaskWithResultContinuation(il, targetMethod, attrVar, methArgs);
            }
            else if (returnTypeName.StartsWith("System.Threading.Tasks.ValueTask`1"))
            {
                WrapValueTaskWithResultContinuation(il, targetMethod, attrVar, methArgs);
            }
        }

        /// <summary>
        /// Adds OnExceptionAsync continuation to async methods
        /// </summary>
        public static void InsertOnExceptionAsyncContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            var returnTypeName = targetMethod.ReturnType.FullName;
            
            if (returnTypeName == "System.Threading.Tasks.Task")
            {
                WrapTaskWithExceptionContinuation(il, targetMethod, attrVar, methArgs, false);
            }
            else if (returnTypeName.StartsWith("System.Threading.Tasks.Task`1"))
            {
                WrapTaskWithExceptionContinuation(il, targetMethod, attrVar, methArgs, true);
            }
            else if (returnTypeName == "System.Threading.Tasks.ValueTask")
            {
                WrapValueTaskWithExceptionContinuation(il, targetMethod, attrVar, methArgs, false);
            }
            else if (returnTypeName.StartsWith("System.Threading.Tasks.ValueTask`1"))
            {
                WrapValueTaskWithExceptionContinuation(il, targetMethod, attrVar, methArgs, true);
            }
        }

        private static void WrapTaskWithContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs, bool hasReturnValue, bool useSync)
        {
            // Generate wrapper method calls that will be injected as continuations
            var wrapperMethodName = hasReturnValue ? 
                (useSync ? "WrapTaskWithOnExit" : "WrapTaskWithOnExitAsync") : 
                (useSync ? "WrapTaskWithOnExitVoid" : "WrapTaskWithOnExitAsyncVoid");
            
            AddTaskContinuationWrapper(il, targetMethod, attrVar, methArgs, wrapperMethodName, hasReturnValue);
        }

        private static void WrapValueTaskWithContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs, bool hasReturnValue, bool useSync)
        {
            var wrapperMethodName = hasReturnValue ? 
                (useSync ? "WrapValueTaskWithOnExit" : "WrapValueTaskWithOnExitAsync") : 
                (useSync ? "WrapValueTaskWithOnExitVoid" : "WrapValueTaskWithOnExitAsyncVoid");
            
            AddValueTaskContinuationWrapper(il, targetMethod, attrVar, methArgs, wrapperMethodName, hasReturnValue);
        }

        private static void WrapTaskWithResultContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            AddTaskContinuationWrapper(il, targetMethod, attrVar, methArgs, "WrapTaskWithOnExitResult", true);
        }

        private static void WrapValueTaskWithResultContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs)
        {
            AddValueTaskContinuationWrapper(il, targetMethod, attrVar, methArgs, "WrapValueTaskWithOnExitResult", true);
        }

        private static void AddTaskContinuationWrapper(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs, string wrapperMethodName, bool hasReturnValue)
        {
            // Replace all return instructions with wrapper calls
            for (var i = 0; i < targetMethod.Body.Instructions.Count; ++i)
            {
                var inst = targetMethod.Body.Instructions[i];
                if (inst.OpCode == OpCodes.Ret)
                {
                    var rep = inst.Previous;
                    if (IsLoadInstruction(rep.OpCode))
                    {
                        // Stack has: [Task<T>] or [Task]
                        // We need: [Task<T>, aspect, methodArgs] -> [Task<T>]
                        InsertInstructionsAt(il, targetMethod, rep,
                            il.Create(OpCodes.Ldloc, attrVar),
                            il.Create(OpCodes.Ldloc, methArgs));
                        i += 2;
                    }
                    else
                    {
                        // Store task, then load everything
                        var taskVar = new VariableDefinition(targetMethod.ReturnType);
                        targetMethod.Body.Variables.Add(taskVar);
                        
                        InsertInstructionsAt(il, targetMethod, inst,
                            il.Create(OpCodes.Stloc, taskVar),
                            il.Create(OpCodes.Ldloc, taskVar),
                            il.Create(OpCodes.Ldloc, attrVar),
                            il.Create(OpCodes.Ldloc, methArgs));
                        i += 4;
                    }

                    // Call the wrapper method
                    var wrapperMethod = typeof(TaskContinuationHelpers).GetMethod(wrapperMethodName);
                    if (hasReturnValue && targetMethod.ReturnType is GenericInstanceType git)
                    {
                        var returnType = Type.GetType(git.GenericArguments[0].FullName) ?? typeof(object);
                        wrapperMethod = wrapperMethod.MakeGenericMethod(returnType);
                    }
                    var wrapperMethodRef = targetMethod.Module.ImportReference(wrapperMethod);

                    InsertInstructionsAt(il, targetMethod, inst,
                        il.Create(OpCodes.Call, wrapperMethodRef));
                    i += 1;
                }
            }
        }

        private static void AddValueTaskContinuationWrapper(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs, string wrapperMethodName, bool hasReturnValue)
        {
            // Similar to Task but for ValueTask
            for (var i = 0; i < targetMethod.Body.Instructions.Count; ++i)
            {
                var inst = targetMethod.Body.Instructions[i];
                if (inst.OpCode == OpCodes.Ret)
                {
                    var rep = inst.Previous;
                    if (IsLoadInstruction(rep.OpCode))
                    {
                        InsertInstructionsAt(il, targetMethod, rep,
                            il.Create(OpCodes.Ldloc, attrVar),
                            il.Create(OpCodes.Ldloc, methArgs));
                        i += 2;
                    }
                    else
                    {
                        var taskVar = new VariableDefinition(targetMethod.ReturnType);
                        targetMethod.Body.Variables.Add(taskVar);
                        
                        InsertInstructionsAt(il, targetMethod, inst,
                            il.Create(OpCodes.Stloc, taskVar),
                            il.Create(OpCodes.Ldloc, taskVar),
                            il.Create(OpCodes.Ldloc, attrVar),
                            il.Create(OpCodes.Ldloc, methArgs));
                        i += 4;
                    }

                    var wrapperMethod = typeof(TaskContinuationHelpers).GetMethod(wrapperMethodName);
                    if (hasReturnValue && targetMethod.ReturnType is GenericInstanceType git)
                    {
                        var returnType = Type.GetType(git.GenericArguments[0].FullName) ?? typeof(object);
                        wrapperMethod = wrapperMethod.MakeGenericMethod(returnType);
                    }
                    var wrapperMethodRef = targetMethod.Module.ImportReference(wrapperMethod);

                    InsertInstructionsAt(il, targetMethod, inst,
                        il.Create(OpCodes.Call, wrapperMethodRef));
                    i += 1;
                }
            }
        }

        private static void WrapTaskWithExceptionContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs, bool hasReturnValue)
        {
            var wrapperMethodName = hasReturnValue ? "WrapTaskWithOnExceptionAsync" : "WrapTaskWithOnExceptionAsyncVoid";
            AddTaskContinuationWrapper(il, targetMethod, attrVar, methArgs, wrapperMethodName, hasReturnValue);
        }

        private static void WrapValueTaskWithExceptionContinuation(ILProcessor il, MethodDefinition targetMethod, VariableDefinition attrVar, VariableDefinition methArgs, bool hasReturnValue)
        {
            var wrapperMethodName = hasReturnValue ? "WrapValueTaskWithOnExceptionAsync" : "WrapValueTaskWithOnExceptionAsyncVoid";
            AddValueTaskContinuationWrapper(il, targetMethod, attrVar, methArgs, wrapperMethodName, hasReturnValue);
        }

    }

    /// <summary>
    /// Helper class containing task continuation wrapper methods
    /// </summary>
    public static class TaskContinuationHelpers
    {
        // Task (void) wrappers
        public static Task WrapTaskWithOnExitVoid(Task task, Aspect aspect, MethodArguments methodArgs)
        {
            return task.ContinueWith(_ => aspect.OnExit(methodArgs), TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task WrapTaskWithOnExitAsyncVoid(Task task, Aspect aspect, MethodArguments methodArgs)
        {
            return task.ContinueWith(async _ => await aspect.OnExitAsync(methodArgs), TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        // Task<T> wrappers
        public static Task<T> WrapTaskWithOnExit<T>(Task<T> task, Aspect aspect, MethodArguments methodArgs)
        {
            return task.ContinueWith(t => { aspect.OnExit(methodArgs); return t.Result; }, TaskContinuationOptions.ExecuteSynchronously);
        }

        public static Task<T> WrapTaskWithOnExitAsync<T>(Task<T> task, Aspect aspect, MethodArguments methodArgs)
        {
            return task.ContinueWith(async t => { await aspect.OnExitAsync(methodArgs); return t.Result; }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        public static Task<T> WrapTaskWithOnExitResult<T>(Task<T> task, IAspectExitHandler<T> exitHandler, MethodArguments methodArgs)
        {
            return task.ContinueWith(t => exitHandler.OnExit(methodArgs, t.Result), TaskContinuationOptions.ExecuteSynchronously);
        }

        // ValueTask (void) wrappers
        public static ValueTask WrapValueTaskWithOnExitVoid(ValueTask task, Aspect aspect, MethodArguments methodArgs)
        {
            return new ValueTask(task.AsTask().ContinueWith(_ => aspect.OnExit(methodArgs), TaskContinuationOptions.ExecuteSynchronously));
        }

        public static ValueTask WrapValueTaskWithOnExitAsyncVoid(ValueTask task, Aspect aspect, MethodArguments methodArgs)
        {
            return new ValueTask(task.AsTask().ContinueWith(async _ => await aspect.OnExitAsync(methodArgs), TaskContinuationOptions.ExecuteSynchronously).Unwrap());
        }

        // ValueTask<T> wrappers
        public static ValueTask<T> WrapValueTaskWithOnExit<T>(ValueTask<T> task, Aspect aspect, MethodArguments methodArgs)
        {
            return new ValueTask<T>(task.AsTask().ContinueWith(t => { aspect.OnExit(methodArgs); return t.Result; }, TaskContinuationOptions.ExecuteSynchronously));
        }

        public static ValueTask<T> WrapValueTaskWithOnExitAsync<T>(ValueTask<T> task, Aspect aspect, MethodArguments methodArgs)
        {
            return new ValueTask<T>(task.AsTask().ContinueWith(async t => { await aspect.OnExitAsync(methodArgs); return t.Result; }, TaskContinuationOptions.ExecuteSynchronously).Unwrap());
        }

        public static ValueTask<T> WrapValueTaskWithOnExitResult<T>(ValueTask<T> task, IAspectExitHandler<T> exitHandler, MethodArguments methodArgs)
        {
            return new ValueTask<T>(task.AsTask().ContinueWith(t => exitHandler.OnExit(methodArgs, t.Result), TaskContinuationOptions.ExecuteSynchronously));
        }

        // Exception handling wrappers
        public static Task WrapTaskWithOnExceptionAsyncVoid(Task task, Aspect aspect, MethodArguments methodArgs)
        {
            return task.ContinueWith(async t => 
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    // Get the inner exception (the actual exception thrown)
                    var innerException = t.Exception.GetBaseException();
                    await aspect.OnExceptionAsync(methodArgs, innerException);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap().ContinueWith(_ => task).Unwrap();
        }

        public static Task<T> WrapTaskWithOnExceptionAsync<T>(Task<T> task, Aspect aspect, MethodArguments methodArgs)
        {
            return task.ContinueWith(async t => 
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    // Get the inner exception (the actual exception thrown)
                    var innerException = t.Exception.GetBaseException();
                    await aspect.OnExceptionAsync(methodArgs, innerException);
                    // Re-throw to preserve the original exception
                    throw innerException;
                }
                return t.Result; // Return the result if successful
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }

        public static ValueTask WrapValueTaskWithOnExceptionAsyncVoid(ValueTask task, Aspect aspect, MethodArguments methodArgs)
        {
            return new ValueTask(task.AsTask().ContinueWith(async t => 
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    var innerException = t.Exception.GetBaseException();
                    await aspect.OnExceptionAsync(methodArgs, innerException);
                }
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap().ContinueWith(_ => task.AsTask()).Unwrap());
        }

        public static ValueTask<T> WrapValueTaskWithOnExceptionAsync<T>(ValueTask<T> task, Aspect aspect, MethodArguments methodArgs)
        {
            return new ValueTask<T>(task.AsTask().ContinueWith(async t => 
            {
                if (t.IsFaulted && t.Exception != null)
                {
                    var innerException = t.Exception.GetBaseException();
                    await aspect.OnExceptionAsync(methodArgs, innerException);
                    throw innerException; // Re-throw to preserve the original exception
                }
                return t.Result; // Return the result if successful
            }, TaskContinuationOptions.ExecuteSynchronously).Unwrap());
        }
    }
}
