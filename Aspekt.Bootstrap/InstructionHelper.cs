using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

namespace Aspekt.Bootstrap
{
    public class InstructionHelper
    {
        ILProcessor il_;
        ModuleDefinition module_;
        Instruction last_;
        Instruction first_;
        Insert i_;

        public enum Insert {  Before, After };

        public InstructionHelper(ModuleDefinition md, ILProcessor il, Instruction last, Insert i)
        {
            i_ = i;
            module_ = md;
            il_ = il;
            last_ = last;
        }

        public InstructionHelper(ModuleDefinition md, ILProcessor il, Instruction last)
        {
            i_ = Insert.After;
            module_ = md;
            il_ = il;
            last_ = last;
        }

        public InstructionHelper NewObj<T>(params Type[] args)
        {
            // get the constructor ref
            var ctor = typeof(T).GetConstructor(args);
            var ctorRef = module_.ImportReference(ctor);
            return NewObj(ctorRef);
        }
        public InstructionHelper NewObj(MethodReference t)
        {
            return Next(il_.Create(OpCodes.Newobj, t));
        }

        public VariableDefinition NewVariable<T>()
        {
            return NewVariable(typeof(T));
        }

        public VariableDefinition NewVariable(TypeReference tr)
        {
            var vd = new VariableDefinition(tr);
            il_.Body.Variables.Add(vd);

            // Ensure that the method is initializing locals
            il_.Body.InitLocals = true;

            return vd;
        }

        public VariableDefinition NewVariable(Type t)
        {
            return NewVariable(module_.ImportReference(t));
        }

        public InstructionHelper Call(TypeDefinition type, string callName, params Type[] args)
        {
            var mth = type.Methods.SingleOrDefault(m => m.Name == callName && m.Parameters.Select(p => p.ParameterType).SequenceEqual(args.Select(a => module_.ImportReference(a))));
            return Call(mth);
        }

        public InstructionHelper Call(Type type, string callName, params Type[] args)
        {
            var mth = type.GetMethod(callName, args);
            return Call(module_.ImportReference(mth));
        }

        public InstructionHelper Call<T>(String callName, params Type[] args)
        {
            return Call(typeof(T), callName, args);
        }

        public InstructionHelper Call(MethodDefinition md)
        {
            return Call(module_.ImportReference(md));
        }

        public InstructionHelper Call(MethodReference mr)
        {
            return Next(OpCodes.Call, mr);
        }

        public InstructionHelper CallVirt(Type declaringType, string methodName, params Type[] args)
        {
            MethodBase method = declaringType.GetMethod(methodName, args);
            if (method == null)
            {
                throw new MissingMemberException(declaringType.FullName, methodName);
            }

            // Workaround for .NET Standard 1.3 flavor of Mono.Cecil
            if (method.DeclaringType.IsGenericType && !method.DeclaringType.IsGenericTypeDefinition)
            {
                method = method.Module.ResolveMethod(method.MetadataToken);
            }

            return Next(OpCodes.Callvirt, module_.ImportReference(method));
        }

        public InstructionHelper CallVirt<T>(string methodName, params Type[] args)
        {
            return CallVirt(typeof(T), methodName, args);
        }

        public InstructionHelper CallVirt(TypeReference tr, string methodName, params Type[] args)
        {
            var t = Type.GetType(tr.FullName + ", " + tr.Module.Assembly.FullName);

            return CallVirt(t, methodName, args);
        }

        public InstructionHelper Next(Instruction i)
        {
            if (first_ == null)
                first_ = i;
            if (i_ == Insert.Before)
            {
                i_ = Insert.After;
                il_.InsertBefore(last_, i);
            }
            else
                il_.InsertAfter(last_, i);
            last_ = i;
            return this;
        }


        public InstructionHelper Next(OpCode op, long i)
        {
            return Next(il_.Create(op, i));
        }

        public InstructionHelper Next(OpCode op, bool b)
        {
            return Next(il_.Create(op, boolToInt(b)));
        }

        public InstructionHelper Next(OpCode op, double d)
        {
            return Next(il_.Create(op, d));
        }

        public InstructionHelper Next(OpCode op, String s)
        {
            return Next(il_.Create(op, s));
        }

        public InstructionHelper Next(OpCode op, MethodReference mr)
        {
            return Next(il_.Create(op, mr));
        }
        public InstructionHelper Next(OpCode op, int val)
        {
            return Next(il_.Create(op, val));
        }

        public InstructionHelper Next(OpCode op, VariableDefinition vd)
        {
            return Next(il_.Create(op, vd));
        }

        public InstructionHelper Next(OpCode op, ParameterDefinition pd)
        {
            return Next(il_.Create(op, pd));
        }

        public InstructionHelper Next(OpCode op, TypeReference tr)
        {
            return Next(il_.Create(op, tr));
        }

        public InstructionHelper Next(OpCode op)
        {
            return Next(il_.Create(op));
        }

        private int boolToInt(bool b)
        {
            return b ? 1 : 0;
        }

        public Instruction FirstInstruction { get { return first_; } }
        public Instruction LastInstruction { get { return last_; } }
    }
}
