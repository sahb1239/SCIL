using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    public class Misc:IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Pop:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return "pop";
                case Code.Dup:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return "dup";
            }
            return null;
        }
    }
}