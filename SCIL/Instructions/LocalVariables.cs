using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class LocalVariables : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return stloc(instruction.OpCode.Value - (int)Code.Stloc_0, programState);
                case Code.Stloc:
                case Code.Stloc_S:
                    if (instruction.Operand is sbyte stByt)
                    {
                        return stloc(stByt, programState);
                    }
                    else if (instruction.Operand is VariableDefinition stVar)
                    {
                        return stloc(stVar.Index, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return ldloc(instruction.OpCode.Value - (int)Code.Ldloc_0, programState);
                case Code.Ldloc:
                case Code.Ldloc_S:
                    if (instruction.Operand is sbyte ldByt)
                    {
                        return ldloc(ldByt, programState);
                    }
                    else if (instruction.Operand is VariableDefinition ldVar)
                    {
                        return ldloc(ldVar.Index, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldloca:
                case Code.Ldloca_S:
                    if (instruction.Operand is VariableDefinition ldlocaVar)
                    {
                        return ldloca(ldlocaVar.Index, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
            }

            return null;
        }

        private string stloc(int var, IFlixInstructionProgramState programState) => $"stlocStm({programState.StoreVar((uint) var)}, {programState.PopStack()}).";
        private string ldloc(int var, IFlixInstructionProgramState programState) => $"ldlocStm({programState.PushStack()}, {programState.GetVar((uint)var)}).";
        private string ldloca(int var, IFlixInstructionProgramState programState) => $"ldlocaStm({programState.PushStack()}, {programState.GetVar((uint)var)}).";
    }
}