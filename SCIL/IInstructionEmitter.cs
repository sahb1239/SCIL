using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    interface IInstructionEmitter
    {
        string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction);
    }
}