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


        public static void Apply(String targetFileName)
        {
            var rp = new ReaderParameters { ReadSymbols = true };
            var assembly = AssemblyDefinition.ReadAssembly(targetFileName, rp);

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
                     ih = new InstructionHelper(module, il, fi, InstructionHelper.Insert.Before);
                 else
                     ih = new InstructionHelper(module, il, target.StartInstruction, InstructionHelper.Insert.After);

                 if (target.MethodArguments == null)
                 {
                     var args = IlGenerator.CaptureMethodArguments(ih, meth);
                     target.MethodArguments = IlGenerator.GenerateMethodArgs(ih, args, meth);
                 }


                 // if the attribute overrides the method, we will put the call in.
                 // Otherwise, we will not.
                 var attrVar = IlGenerator.CreateAttribute(ih, attr);

                 if (MethodTraits.HasMethod(attr.AttributeType.Resolve(), nameof(Aspect.OnEntry), typeof(MethodArguments)))
                     IlGenerator.InsertOnEntryCalls(ih, attrVar, target.MethodArguments);

                 target.StartInstruction = ih.LastInstruction; // so that we will create the next aspects AFTER

                 // walk the instructions looking for returns, based on what teh function is returning 
                 // is where we inject the OnExit instructions.
                 // so how do I tell what the function returns?
                 if (MethodTraits.HasMethod(attr.AttributeType.Resolve(), nameof(Aspect.OnExit), typeof(MethodArguments)))
                     IlGenerator.InsertOnExitCalls(il, module, meth, attrVar, target.MethodArguments);


                 if (MethodTraits.HasMethod(attr.AttributeType.Resolve(), nameof(Aspect.OnException), typeof(MethodArguments), typeof(Exception)))
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
                            .CallVirt<Aspect>(nameof(Aspect.OnException), typeof(MethodArguments), typeof(Exception))
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
                             .CallVirt<Aspect>(nameof(Aspect.OnException), typeof(MethodArguments), typeof(Exception));
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

            var pdbName = Path.ChangeExtension(targetFileName, "pdb");
            var wp = new WriterParameters();
            if (File.Exists(pdbName))
            {
                wp.WriteSymbols = true;
            }
            assembly.Write(targetFileName, wp);
        }
    }


}
