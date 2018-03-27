using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Branch : IInstructionEmitter
    {
        public InstructionEmitterOutput GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Br:
                case Code.Br_S:
                    // Load constant 1 (non zero)
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, "ldcStm({0}, 1)." + Environment.NewLine + BrTrue(instruction), true, 1);
                case Code.Brtrue: // Branch to target if value is non-zero (true). (https://en.wikipedia.org/wiki/List_of_CIL_instructions)
                case Code.Brtrue_S:
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, BrTrue(instruction), false, 1);
                case Code.Brfalse:
                case Code.Brfalse_S:
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, "negStm({0})." + Environment.NewLine + BrTrue(instruction), true, 2);
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
                return "brtrueStm(" + branchToInstruction.Offset + ").";
            }
            throw new NotSupportedException();
        }

    }
}
