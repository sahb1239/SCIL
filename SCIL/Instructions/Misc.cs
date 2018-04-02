using System;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Misc : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Pop:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    
                    return $"popStm({programState.PopStack()}).";
                case Code.Dup:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }

                    var peek = programState.PeekStack();
                    return $"dupStm({programState.PushStack()}, {peek}).";
            }
            return null;
        }
    }
}