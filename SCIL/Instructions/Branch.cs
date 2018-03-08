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
                case Code.Brtrue:
                case Code.Brtrue_S:
                case Code.Brfalse:
                case Code.Brfalse_S:
                case Code.Br_S:
                    return "br";
            }

            return null;
        }

    }
}
