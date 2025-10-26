using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace Aspekt.Bootstrap.Test
{
    [TestClass]
    public class IlGeneratorTests
    {
        #region Data Sets

        public static IEnumerable<object[]> InstructionSets()
        {
            yield return new object[]
            {
                Instruction.Create(OpCodes.Ldc_I4, 0)
            };

            yield return new object[]
            {
                Instruction.Create(OpCodes.Ldc_I4, 1),
                Instruction.Create(OpCodes.Ldc_I4, 2),
                Instruction.Create(OpCodes.Add)
            };
        }

        public static IEnumerable<object[]> BranchInstructions()
        {
            yield return new object[] {OpCodes.Br};
            yield return new object[] {OpCodes.Br_S};
            yield return new object[] {OpCodes.Brfalse};
            yield return new object[] {OpCodes.Brfalse_S};
            yield return new object[] {OpCodes.Brtrue};
            yield return new object[] {OpCodes.Brtrue_S};
            yield return new object[] {OpCodes.Leave};
            yield return new object[] {OpCodes.Leave_S};
        }

        #endregion

        #region ReplaceInstruction

        [TestMethod]

        public void ReplaceInstruction_BasicInstruction_Replaces(params Instruction[] instructions)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            ilProcessor.Emit(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(OpCodes.Ret);

            // Act

            IlGenerator.ReplaceInstruction(ilProcessor, testMethod, testMethod.Body.Instructions[0],
                instructions);

            // Assert

            Assert.AreEqual(instructions.Length + 1, testMethod.Body.Instructions.Count);
            for (var i = 0; i < instructions.Length; i++)
            {
                Assert.AreEqual(instructions[i], testMethod.Body.Instructions[i]);
            }
            Assert.AreEqual(OpCodes.Ret, testMethod.Body.Instructions[testMethod.Body.Instructions.Count-1].OpCode);
        }

        [TestMethod]

        public void ReplaceInstruction_BranchedTo_ReplacesBranch(OpCode branchCode)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var target = ilProcessor.Create(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(branchCode, target);
            ilProcessor.Append(target);
            ilProcessor.Emit(OpCodes.Ret);

            // Act

            IlGenerator.ReplaceInstruction(ilProcessor, testMethod, target,
                ilProcessor.Create(OpCodes.Ldc_I4, 1),
                ilProcessor.Create(OpCodes.Ldc_I4, 2),
                ilProcessor.Create(OpCodes.Add));

            // Assert

            Assert.AreEqual(branchCode, testMethod.Body.Instructions[0].OpCode);
            Assert.AreEqual(1, ((Instruction) testMethod.Body.Instructions[0].Operand).Operand);
        }

        [TestMethod]

        public void ReplaceInstruction_BranchedTo_Recursive(OpCode branchCode)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var target2 = ilProcessor.Create(OpCodes.Ldc_I4, 0);
            var target1 = ilProcessor.Create(branchCode, target2);
            ilProcessor.Emit(branchCode, target1);
            ilProcessor.Append(target1);
            ilProcessor.Append(target2);
            ilProcessor.Emit(OpCodes.Ret);

            // Act

            IlGenerator.ReplaceInstruction(ilProcessor, testMethod, target2,
                ilProcessor.Create(OpCodes.Ldc_I4, 1),
                ilProcessor.Create(OpCodes.Ldc_I4, 2),
                ilProcessor.Create(OpCodes.Add));

            // Assert

            Assert.AreEqual(branchCode, testMethod.Body.Instructions[0].OpCode);
            Assert.AreEqual(testMethod.Body.Instructions[1], (Instruction) testMethod.Body.Instructions[0].Operand);
            Assert.AreEqual(branchCode, testMethod.Body.Instructions[1].OpCode);
            Assert.AreEqual(testMethod.Body.Instructions[2], (Instruction) testMethod.Body.Instructions[1].Operand);
        }

        [TestMethod]

        public void ReplaceInstruction_BeginTryBlock_AdjustsToFirstInstruction(params Instruction[] instructions)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var ret = ilProcessor.Create(OpCodes.Ret);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(OpCodes.Leave, ret);
            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Append(ret);

            testMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = testMethod.Body.Instructions[0],
                TryEnd = testMethod.Body.Instructions[2],
                HandlerStart = testMethod.Body.Instructions[2],
                HandlerEnd = testMethod.Body.Instructions[3]
            });

            // Act

            IlGenerator.ReplaceInstruction(ilProcessor, testMethod, testMethod.Body.Instructions[0],
                instructions);

            // Assert

            Assert.AreEqual(instructions[0], testMethod.Body.ExceptionHandlers[0].TryStart);
        }

        [TestMethod]

        public void ReplaceInstruction_BeginHandlerBlock_AdjustsToFirstInstruction(params Instruction[] instructions)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var ret = ilProcessor.Create(OpCodes.Ret);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(OpCodes.Leave, ret);
            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Append(ret);

            testMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = testMethod.Body.Instructions[0],
                TryEnd = testMethod.Body.Instructions[2],
                HandlerStart = testMethod.Body.Instructions[2],
                HandlerEnd = testMethod.Body.Instructions[3]
            });

            // Act

            IlGenerator.ReplaceInstruction(ilProcessor, testMethod, testMethod.Body.Instructions[2],
                instructions);

            // Assert

            Assert.AreEqual(instructions[0], testMethod.Body.ExceptionHandlers[0].TryEnd);
            Assert.AreEqual(instructions[0], testMethod.Body.ExceptionHandlers[0].HandlerStart);
        }

        [TestMethod]

        public void ReplaceInstruction_EndHandlerBlock_AdjustsToFirstInstruction(params Instruction[] instructions)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var ret = ilProcessor.Create(OpCodes.Ret);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(OpCodes.Leave, ret);
            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Append(ret);

            testMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = testMethod.Body.Instructions[0],
                TryEnd = testMethod.Body.Instructions[2],
                HandlerStart = testMethod.Body.Instructions[2],
                HandlerEnd = testMethod.Body.Instructions[3]
            });

            // Act

            IlGenerator.ReplaceInstruction(ilProcessor, testMethod, testMethod.Body.Instructions[3],
                instructions);

            // Assert

            Assert.AreEqual(instructions[0], testMethod.Body.ExceptionHandlers[0].HandlerEnd);
        }

        #endregion

        #region InsertInstructionsAt

        [TestMethod]

        public void InsertInstructionsAt_BasicInstruction_Replaces(params Instruction[] instructions)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            ilProcessor.Emit(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(OpCodes.Ret);

            // Act

            IlGenerator.InsertInstructionsAt(ilProcessor, testMethod, testMethod.Body.Instructions[0],
                instructions);

            // Assert

            Assert.AreEqual(instructions.Length + 2, testMethod.Body.Instructions.Count);
            for (var i = 0; i < instructions.Length; i++)
            {
                Assert.AreEqual(instructions[i], testMethod.Body.Instructions[i]);
            }
            Assert.AreEqual(OpCodes.Ldc_I4, testMethod.Body.Instructions[testMethod.Body.Instructions.Count-2].OpCode);
            Assert.AreEqual(0, testMethod.Body.Instructions[testMethod.Body.Instructions.Count-2].Operand);
            Assert.AreEqual(OpCodes.Ret, testMethod.Body.Instructions[testMethod.Body.Instructions.Count-1].OpCode);
        }

        [TestMethod]

        public void InsertInstructionsAt_BranchedTo_ReplacesBranch(OpCode branchCode)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var target = ilProcessor.Create(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(branchCode, target);
            ilProcessor.Append(target);
            ilProcessor.Emit(OpCodes.Ret);

            // Act

            IlGenerator.InsertInstructionsAt(ilProcessor, testMethod, target,
                ilProcessor.Create(OpCodes.Ldc_I4, 1),
                ilProcessor.Create(OpCodes.Ldc_I4, 2),
                ilProcessor.Create(OpCodes.Add));

            // Assert

            Assert.AreEqual(branchCode, testMethod.Body.Instructions[0].OpCode);
            Assert.AreEqual(1, ((Instruction) testMethod.Body.Instructions[0].Operand).Operand);
        }

        [TestMethod]

        public void InsertInstructionsAt_BranchedTo_Recursive(OpCode branchCode)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var target2 = ilProcessor.Create(OpCodes.Ldc_I4, 0);
            var target1 = ilProcessor.Create(branchCode, target2);
            ilProcessor.Emit(branchCode, target1);
            ilProcessor.Append(target1);
            ilProcessor.Append(target2);
            ilProcessor.Emit(OpCodes.Ret);

            // Act

            IlGenerator.InsertInstructionsAt(ilProcessor, testMethod, target2,
                ilProcessor.Create(OpCodes.Ldc_I4, 1),
                ilProcessor.Create(OpCodes.Ldc_I4, 2),
                ilProcessor.Create(OpCodes.Add));

            // Assert

            Assert.AreEqual(branchCode, testMethod.Body.Instructions[0].OpCode);
            Assert.AreEqual(testMethod.Body.Instructions[1], (Instruction) testMethod.Body.Instructions[0].Operand);
            Assert.AreEqual(branchCode, testMethod.Body.Instructions[1].OpCode);
            Assert.AreEqual(testMethod.Body.Instructions[2], (Instruction) testMethod.Body.Instructions[1].Operand);
        }

        [TestMethod]

        public void InsertInstructionsAt_BeginTryBlock_AdjustsToFirstInstruction(params Instruction[] instructions)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var ret = ilProcessor.Create(OpCodes.Ret);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(OpCodes.Leave, ret);
            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Append(ret);

            testMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = testMethod.Body.Instructions[0],
                TryEnd = testMethod.Body.Instructions[2],
                HandlerStart = testMethod.Body.Instructions[2],
                HandlerEnd = testMethod.Body.Instructions[3]
            });

            // Act

            IlGenerator.InsertInstructionsAt(ilProcessor, testMethod, testMethod.Body.Instructions[0],
                instructions);

            // Assert

            Assert.AreEqual(instructions[0], testMethod.Body.ExceptionHandlers[0].TryStart);
        }

        [TestMethod]

        public void InsertInstructionsAt_BeginHandlerBlock_AdjustsToFirstInstruction(params Instruction[] instructions)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var ret = ilProcessor.Create(OpCodes.Ret);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(OpCodes.Leave, ret);
            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Append(ret);

            testMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = testMethod.Body.Instructions[0],
                TryEnd = testMethod.Body.Instructions[2],
                HandlerStart = testMethod.Body.Instructions[2],
                HandlerEnd = testMethod.Body.Instructions[3]
            });

            // Act

            IlGenerator.InsertInstructionsAt(ilProcessor, testMethod, testMethod.Body.Instructions[2],
                instructions);

            // Assert

            Assert.AreEqual(instructions[0], testMethod.Body.ExceptionHandlers[0].TryEnd);
            Assert.AreEqual(instructions[0], testMethod.Body.ExceptionHandlers[0].HandlerStart);
        }

        [TestMethod]

        public void InsertInstructionsAt_EndHandlerBlock_AdjustsToFirstInstruction(params Instruction[] instructions)
        {
            // Arrange

            var testClass = CreateEmptyClass();

            var testMethod = new MethodDefinition("TestMethod", MethodAttributes.Public,
                testClass.Module.ImportReference(typeof(int)));

            var ilProcessor = testMethod.Body.GetILProcessor();

            var ret = ilProcessor.Create(OpCodes.Ret);
            ilProcessor.Emit(OpCodes.Ldc_I4, 0);
            ilProcessor.Emit(OpCodes.Leave, ret);
            ilProcessor.Emit(OpCodes.Nop);
            ilProcessor.Append(ret);

            testMethod.Body.ExceptionHandlers.Add(new ExceptionHandler(ExceptionHandlerType.Finally)
            {
                TryStart = testMethod.Body.Instructions[0],
                TryEnd = testMethod.Body.Instructions[2],
                HandlerStart = testMethod.Body.Instructions[2],
                HandlerEnd = testMethod.Body.Instructions[3]
            });

            // Act

            IlGenerator.InsertInstructionsAt(ilProcessor, testMethod, testMethod.Body.Instructions[3],
                instructions);

            // Assert

            Assert.AreEqual(instructions[0], testMethod.Body.ExceptionHandlers[0].HandlerEnd);
        }

        #endregion

        #region Helpers

        private TypeDefinition CreateEmptyClass()
        {
            var assembly = AssemblyDefinition.CreateAssembly(
                new AssemblyNameDefinition("Test", new Version(1, 0, 0, 0)),
                "Test.dll",
                ModuleKind.Dll);

            var testClass = new TypeDefinition("Test", "TestClass", TypeAttributes.Public,
                assembly.MainModule.ImportReference(typeof(object)));
            assembly.MainModule.Types.Add(testClass);

            return testClass;
        }

        #endregion
    }
}
