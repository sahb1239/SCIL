using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class UnaryOp : IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            // Unsigned and overflow abstracted away
            switch (instruction.OpCode.Code)
            {
                case Code.Neg:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return uOp("neg");

                case Code.Not:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return uOp("not");
            }

            return null;
        }
        private string uOp(string op) => op;
    }
}
