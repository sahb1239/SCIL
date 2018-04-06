using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class UnaryOp : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            // Unsigned and overflow abstracted away
            switch (instruction.OpCode.Code)
            {
                case Code.Neg:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }

                    var popNeg = programState.PopStack();
                    return $"NegStm({programState.PushStack()}, {popNeg}).";

                case Code.Not:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }

                    var popNot = programState.PopStack();
                    return $"NotStm({programState.PushStack()}, {popNot}).";
            }

            return null;
        }
    }
}
