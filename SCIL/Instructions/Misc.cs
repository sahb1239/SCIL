using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Misc : IInstructionEmitter
    {
        public InstructionEmitterOutput GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Pop:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }

                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, "popStm({0})", false, 1);
                case Code.Dup:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, "dupStm({1}, {0})", true, 0, true);
            }
            return null;
        }
    }
}