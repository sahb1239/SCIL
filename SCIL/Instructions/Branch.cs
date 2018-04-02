using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Branch : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Br:
                case Code.Br_S:
                    // Load constant 1 (non zero)
                    return $"ldcStm({programState.PushStack()}, 1)." + Environment.NewLine + BrTrue(instruction, programState);
                case Code.Brtrue: // Branch to target if value is non-zero (true). (https://en.wikipedia.org/wiki/List_of_CIL_instructions)
                case Code.Brtrue_S:
                    return BrTrue(instruction, programState);
                case Code.Brfalse:
                case Code.Brfalse_S:
                    var popNeg = programState.PopStack();
                    return $"negStm({programState.PushStack()}, {popNeg})." + Environment.NewLine + BrTrue(instruction, programState);
                case Code.Beq:
                case Code.Beq_S:
                    string pop1Beq = programState.PopStack(), 
                        pop2Beq = programState.PopStack();
                    return $"ceqStm({programState.PushStack()}, {pop2Beq}, {pop1Beq})." + Environment.NewLine + BrTrue(instruction, programState);
                case Code.Bne_Un:
                case Code.Bne_Un_S:
                    string pop1Ceq = programState.PopStack(),
                        pop2Ceq = programState.PopStack(),
                        push1Ceq = programState.PushStack(),
                        popNegUn = programState.PopStack(),
                        pushNeg = programState.PushStack();
                    return
                        $"ceqStm({push1Ceq}, {pop1Ceq}, {pop2Ceq}).{Environment.NewLine}negStm({pushNeg}, {popNegUn}).{Environment.NewLine}{BrTrue(instruction, programState)}";
            }

            return null;
        }

        private string BrTrue(Instruction instruction, IFlixInstructionProgramState programState)
        {
            if (instruction.Operand is Instruction branchToInstruction)
            {
                return $"brtrueStm({programState.PopStack()}, {branchToInstruction.Offset}).";
            }
            throw new NotSupportedException();
        }
    }
}
