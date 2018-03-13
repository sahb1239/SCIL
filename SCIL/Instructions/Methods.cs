using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    public class Methods:IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
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
                    return "ret";
            }

            return null;
        }
        private string call(string method) => "call " + method;
        private string callvirt(string method) => "call " + method;
    }
}