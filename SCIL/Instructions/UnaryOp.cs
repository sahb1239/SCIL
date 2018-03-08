using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class UnaryOp : IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            // Unsigned and overflow abstracted away
            switch (instruction.OpCode.Code)
            {
                case Code.Neg:
                    return uOp("neg");

                case Code.Not:
                    return uOp("not");
            }

            return null;
        }
        private string uOp(string op) => op;
    }
}
