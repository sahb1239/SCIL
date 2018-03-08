using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Branch : IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Br:
                case Code.Br_S:
                    return "br";
            }

            return null;
        }

    }
}
