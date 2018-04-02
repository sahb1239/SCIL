using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class BinaryOp : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            // Unsigned and overflow abstracted away
            switch (instruction.OpCode.Code)
            {
                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                    return binOp("add", programState);
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    return binOp("sub", programState);
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    return binOp("mul", programState);
                case Code.Div:
                case Code.Div_Un:
                    return binOp("div", programState);
                case Code.Rem:
                case Code.Rem_Un:
                    return binOp("rem", programState);
                case Code.Ceq:
                    return binOp("ceq", programState);

                case Code.And:
                    return binOp("and", programState);
                case Code.Or:
                    return binOp("or", programState);
                case Code.Xor:
                    return binOp("xor", programState);
            }

            return null;
        }

        private string binOp(string op, IFlixInstructionProgramState programState)
        {
            string pop2 = programState.PopStack(),
                pop1 = programState.PopStack();

            return $"{op}Stm({programState.PushStack()}, {pop2}, {pop1}).";
        }
    }
}
