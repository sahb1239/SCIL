using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Arguments : IInstructionEmitter
    {
        public InstructionEmitterOutput GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarg_S:
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldarg(GetOperandIndex(methodBody, instruction)), true, 0);

                case Code.Ldarga:
                case Code.Ldarga_S:
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldarga(GetOperandIndex(methodBody, instruction)), true, 0);

                case Code.Starg:
                case Code.Starg_S:
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, starg(GetOperandIndex(methodBody, instruction)), false, 1);

                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldarg((uint) (instruction.OpCode.Code - Code.Ldarg_0)), true, 0);
            }

            return null;
        }

        private uint GetOperandIndex(MethodBody methodBody, Instruction instruction)
        {
            if (instruction.Operand is ParameterDefinition parameterDefinition)
            {
                // Get index in parameters
                var parameterIndex = methodBody.Method.Parameters.IndexOf(parameterDefinition);
                if (parameterIndex == -1)
                    throw new ArgumentOutOfRangeException(nameof(parameterIndex), "Could not find parameter matching Ldarg");

                // Detect if it's a static method ("this" is argument 0 on non static methods)
                var staticMethod = methodBody.Method.IsStatic;

                // Get final index
                var finalIndex = staticMethod ? parameterIndex : parameterIndex + 1;

                // Check if index is less than 0
                if (finalIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(finalIndex), "Final index was less than 0");

                return (uint) finalIndex;
            }

            throw new NotImplementedException("Could not find operand index");
        }

        private string ldarg(uint argNo) => "ldargStm({0}, " + argNo + ").";
        private string ldarga(uint argNo) => "ldargaStm({0}, " + argNo + ").";
        private string starg(uint argNo) => "stargStm({0}, " + argNo + ").";
    }
}