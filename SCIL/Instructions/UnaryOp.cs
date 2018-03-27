using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class UnaryOp : IInstructionEmitter
    {
        public InstructionEmitterOutput GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            // Unsigned and overflow abstracted away
            switch (instruction.OpCode.Code)
            {
                case Code.Neg:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, uOp("negStm({1}, {0})."), true, 1);

                case Code.Not:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, uOp("notStm({1}, {0})."), true, 1);
            }

            return null;
        }
        private string uOp(string op) => op;
    }
}
