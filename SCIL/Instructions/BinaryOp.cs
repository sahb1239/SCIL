using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class BinaryOp : IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            // Unsigned and overflow abstracted away
            switch (instruction.OpCode.Code)
            {
                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                    return binOp("add");
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    return binOp("sub");
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    return binOp("mul");
                case Code.Div:
                case Code.Div_Un:
                    return binOp("div");
                case Code.Rem:
                case Code.Rem_Un:
                    return binOp("rem");
                case Code.Ceq:
                    return binOp("ceq");

                case Code.And:
                    return binOp("and");
                case Code.Or:
                    return binOp("or");
                case Code.Xor:
                    return binOp("xor");
            }

            return null;
        }

        private string binOp(string op) => op;
    }
}
