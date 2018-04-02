using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Methods : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Call:
                    if (instruction.Operand is MethodReference callRef)
                    {
                        return call(callRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Callvirt:
                    if (instruction.Operand is MethodReference callVirtRef)
                    {
                        return callvirt(callVirtRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ret:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return "retStm().";
            }

            return null;
        }
        private string call(string method) => $"callStm({method}).";
        private string callvirt(string method) => $"callvirtStm({method}).";
    }
}