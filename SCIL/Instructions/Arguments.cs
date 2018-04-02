using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Arguments : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarg_S:
                    return ldarg(GetOperandIndex(methodBody, instruction), programState);

                case Code.Ldarga:
                case Code.Ldarga_S:
                    return ldarga(GetOperandIndex(methodBody, instruction), programState);

                case Code.Starg:
                case Code.Starg_S:
                    return starg(GetOperandIndex(methodBody, instruction), programState);

                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    return ldarg((uint) (instruction.OpCode.Code - Code.Ldarg_0), programState);
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

        private string ldarg(uint argNo, IFlixInstructionProgramState programState) => $"ldargStm({programState.PushStack()}, {programState.GetArg(argNo)}).";
        private string ldarga(uint argNo, IFlixInstructionProgramState programState) => $"ldargaStm({programState.PushStack()}, {programState.GetArg(argNo)}).";

        private string starg(uint argNo, IFlixInstructionProgramState programState) =>
            $"stargStm({programState.StoreArg(argNo)}, {programState.PopStack()}).";
    }
}