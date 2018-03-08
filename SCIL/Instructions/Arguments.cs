using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Arguments : IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarg_S:
                    return ldarg(GetOperandIndex(methodBody, instruction));

                case Code.Ldarga:
                case Code.Ldarga_S:
                    return ldarga(GetOperandIndex(methodBody, instruction));

                case Code.Starg:
                case Code.Starg_S:
                    return starg(GetOperandIndex(methodBody, instruction));

                case Code.Ldarg_0:
                    return ldarg(0);
                case Code.Ldarg_1:
                    return ldarg(1);
                case Code.Ldarg_2:
                    return ldarg(2);
                case Code.Ldarg_3:
                    return ldarg(3);
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

        private string ldarg(uint argNo) => "ldarg " + argNo;
        private string ldarga(uint argNo) => "ldarga " + argNo;
        private string starg(uint argNo) => "starg " + argNo;
    }
}