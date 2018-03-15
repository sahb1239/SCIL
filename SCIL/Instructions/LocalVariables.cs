using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    public class LocalVariables:IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
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
                    return stloc(instruction.OpCode.Value - 10);
                case Code.Stloc:
                case Code.Stloc_S:
                    if (instruction.Operand is sbyte stByt)
                    {
                        return stloc(stByt);
                    }
                    else if (instruction.Operand is VariableDefinition stVar)
                    {
                        return stloc(stVar.Index);
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
                    return ldloc(instruction.OpCode.Value - 6);
                case Code.Ldloc:
                case Code.Ldloc_S:
                    if (instruction.Operand is sbyte ldByt)
                    {
                        return ldloc(ldByt);
                    }
                    else if (instruction.Operand is VariableDefinition ldVar)
                    {
                        return ldloc(ldVar.Index);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldloca:
                case Code.Ldloca_S:
                    if (instruction.Operand is VariableDefinition ldlocaVar)
                    {
                        return ldloca(ldlocaVar.Index);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
            }

            return null;
        }

        private string stloc(int var) => "stloc " + var;
        private string ldloc(int var) => "ldloc " + var;
        private string ldloca(int var) => "ldloca " + var;
    }
}