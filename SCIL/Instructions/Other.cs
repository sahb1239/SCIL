using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    [EmitterOrder(150)]
    class Other : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.StackBehaviourPush)
            {
                case StackBehaviour.Push1:
                case StackBehaviour.Pushi:
                case StackBehaviour.Pushi8:
                case StackBehaviour.Pushr4:
                case StackBehaviour.Pushr8:
                case StackBehaviour.Pushref:
                    programState.PushStack();
                    break;
                case StackBehaviour.Push1_push1:
                    break;
            }

            return null;
        }
    }
}
