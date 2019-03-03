using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using System;
using System.IO;
using System.Linq;

namespace Aspekt.Bootstrap
{

    public static class Bootstrap
    {
        // Generating IL


        public static void Apply(string targetFileName)
        {
            var targetOutputName = Path.ChangeExtension(targetFileName, "tmp");
            var rp = new ReaderParameters { ReadSymbols = true, ReadWrite = true };
            using (var assembly = AssemblyDefinition.ReadAssembly(targetFileName, rp))
            {

                // I know that right now, if we have multiple attributes, we're going to generate multiple method arguments.
                // I know how to deal with this.
                AttributeEnumerator.EnumerateMethodAttributes(assembly, (classType, target, attr) =>
                 {
                     var meth = target.Method;
                     var module = meth.Module;

                     // In order to work with the parameters call simplifymacros / optimize macros
                     meth.Body.SimplifyMacros();
                     var il = meth.Body.GetILProcessor();

                     var fi = meth.Body.Instructions.First();
                     InstructionHelper ih;

                     // This is really hacky. This is used to signal whether it's our first aspect, or
                     // the next one.
                     if (target.StartInstruction == null)
                     {
                         ih = new InstructionHelper(module, il, fi, InstructionHelper.Insert.Before);
                     }
                     else
                     {
                         ih = new InstructionHelper(module, il, target.StartInstruction,
                             InstructionHelper.Insert.After);
                     }

                     if (target.MethodArguments == null)
                     {
                         var args = IlGenerator.CaptureMethodArguments(ih, meth);
                         target.MethodArguments = IlGenerator.GenerateMethodArgs(ih, args, meth);
                     }


                     // if the attribute overrides the method, we will put the call in.
                     // Otherwise, we will not.
                     var attrVar = IlGenerator.CreateAttribute(ih, attr);

                     if (MethodTraits.HasMethod(attr.AttributeType.Resolve(), nameof(Aspect.OnEntry),
                         typeof(MethodArguments)))
                     {
                         IlGenerator.InsertOnEntryCalls(ih, attrVar, target.MethodArguments);
                     }

                     target.StartInstruction = ih.LastInstruction; // so that we will create the next aspects AFTER

                     if (MethodTraits.HasMethod(attr.AttributeType.Resolve(), nameof(Aspect.OnException), typeof(MethodArguments), typeof(Exception)))
                     {
                         if (target.ExceptionHandler == null)
                         {
                             var c = new InstructionHelper(module, il, meth.Body.Instructions.Last());

                             var exception = c.NewVariable(typeof(Exception));

                             c.Next(il.Create(OpCodes.Stloc_S, exception))
                                .Next(OpCodes.Ldloc, attrVar)
                                .Next(OpCodes.Ldloc, target.MethodArguments)
                                .Next(OpCodes.Ldloc_S, exception)
                                .CallVirt<Aspect>(nameof(Aspect.OnException), typeof(MethodArguments), typeof(Exception))
                                .Next(OpCodes.Rethrow);

                             var ret = il.Create(OpCodes.Ret);
                             VariableDefinition returnVariable = null;
                             Instruction returnBlockStart;

                             if (meth.ReturnType.MetadataType != MetadataType.Void)
                             {
                                 // Create a variable to store/retrieve the return value from within the try/catch block
                                 returnVariable = ih.NewVariable(meth.ReturnType);

                                 // Load the variable before the Ret operation
                                 c.Next(OpCodes.Ldloc, returnVariable);
                                 returnBlockStart = c.LastInstruction;
                                 c.Next(ret);
                             }
                             else
                             {
                                 // No return type, so just return
                                 c.Next(ret);
                                 returnBlockStart = c.LastInstruction;
                             }

                             // Find all Ret operations (except the new one), and replace them with leave operations,
                             // branching to the new returnBlockStart. For methods which don't return void,
                             // store the return value first since the leave operation clears the stack.
                             for (var i = 0; i<meth.Body.Instructions.Count; ++i)
                             {
                                 var instruction = meth.Body.Instructions[i];
                                 if (instruction.OpCode == OpCodes.Ret && instruction != ret)
                                 {
                                     if (returnVariable != null)
                                     {
                                         IlGenerator.ReplaceInstruction(
                                             il,
                                             meth,
                                             instruction,
                                             il.Create(OpCodes.Stloc, returnVariable),
                                             il.Create(OpCodes.Leave, returnBlockStart));

                                         i++; // We added an extra instruction
                                     }
                                     else
                                     {
                                         IlGenerator.ReplaceInstruction(
                                             il,
                                             meth,
                                             instruction,
                                             il.Create(OpCodes.Leave, returnBlockStart));
                                     }
                                 }
                             }

                             var handler = new ExceptionHandler(ExceptionHandlerType.Catch)
                             {
                                 TryStart = target.StartInstruction.Next,
                                 TryEnd = c.FirstInstruction,
                                 HandlerStart = c.FirstInstruction,
                                 HandlerEnd = returnBlockStart,
                                 CatchType = module.ImportReference(typeof(Exception)),
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
                                 .CallVirt<Aspect>(nameof(Aspect.OnException), typeof(MethodArguments), typeof(Exception));
                         }

                     }

                     // walk the instructions looking for returns, based on what teh function is returning
                     // is where we inject the OnExit instructions.
                     // so how do I tell what the function returns?
                     var hasOnExit = MethodTraits.HasMethod(attr.AttributeType.Resolve(), nameof(Aspect.OnExit), typeof(MethodArguments));
                     var hasOnExitVal = MethodTraits.HasGenericMethod(attr.AttributeType.Resolve(), nameof(Aspect.OnExit), 2);
                     if ( hasOnExit && hasOnExitVal )
                     {
                         WeaverLog.LogMethodWarning(meth, 3, "multiple OnExit found; override void used");
                     }

                     if (hasOnExit)
                     {
                         IlGenerator.InsertOnExitCalls(il, meth, attrVar, target.MethodArguments);
                     }
                     else if (hasOnExitVal)
                     {
                         IlGenerator.InsertOnExitResultCalls(il, meth, attrVar, target.MethodArguments);
                     }

                     meth.Body.OptimizeMacros();
                 }, (attr) =>
                 {
                     try
                     {
                         var type = attr.AttributeType.Resolve();
                         return type.IsSubclassOf(typeof(Aspect));
                     }
                     catch (Exception)
                     {
                         return false; // if we can't resolve, then we just skip it.
                     }
                 });
                var pdbName = Path.ChangeExtension(targetFileName, "pdb");
                var wp = new WriterParameters();
                if (File.Exists(pdbName))
                {
                    wp.WriteSymbols = true;
                }

                assembly.Write(wp);
            }

        }
    }


}
