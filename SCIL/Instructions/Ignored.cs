using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Ignored : IOldFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Nop:
                case Code.Box:
                case Code.Unbox:
                case Code.Unbox_Any:
                    return "// " + instruction.OpCode.Name;
            }
            return null;
        }
    }
}
