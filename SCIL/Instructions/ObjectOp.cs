using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class ObjectOp: IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Newobj:
                    if (instruction.Operand is MethodReference callRef)
                    {
                        return newobj(callRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
            }

            return null;
        }

        private string newobj(string method, IFlixInstructionProgramState programState) => $"newobjStm({programState.PushStack()}, \"{method}\").";
    }
}