using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Branch : IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Br:
                case Code.Br_S:
                    // Load constant 1 (non zero)
                    return "ldc 1" + Environment.NewLine + BrTrue(instruction);
                case Code.Brtrue: // Branch to target if value is non-zero (true). (https://en.wikipedia.org/wiki/List_of_CIL_instructions)
                case Code.Brtrue_S:
                    return BrTrue(instruction); 
                case Code.Brfalse:
                case Code.Brfalse_S:
                    return "neg" + Environment.NewLine + BrTrue(instruction);
                case Code.Beq:
                case Code.Beq_S:
                    return "ceq" + Environment.NewLine + BrTrue(instruction);
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    return "ceq" + Environment.NewLine + "neg" + Environment.NewLine + BrTrue(instruction);
            }

            return null;
        }

        private string BrTrue(Instruction instruction)
        {
            if (instruction.Operand is Instruction branchToInstruction)
            {
                return "brtrue " + branchToInstruction.Offset;
            }
            throw new NotSupportedException();
        }

    }
}
