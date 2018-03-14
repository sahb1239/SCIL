using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    public class Misc:IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Pop:
                    return "pop";
                case Code.Dup:
                    return "dup";
            }
            return null;
        }
    }
}