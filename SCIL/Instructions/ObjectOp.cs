using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    public class ObjectOp:IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Newobj:
                    if (instruction.Operand is MethodReference callRef)
                    {
                        return newobj(callRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
            }

            return null;
        }

        private string newobj(string method) => "newobj " + method;
    }
}