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
                    return binOp("Add", programState);
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    return binOp("Sub", programState);
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    return binOp("Mul", programState);
                case Code.Div:
                case Code.Div_Un:
                    return binOp("Div", programState);
                case Code.Rem:
                case Code.Rem_Un:
                    return binOp("Rem", programState);
                case Code.Clt:
                case Code.Clt_Un:
                    return binOp("Clt", programState);
                case Code.Cgt:
                case Code.Cgt_Un:
                    return binOp("Cgt", programState);
                case Code.Ceq:
                    return binOp("Ceq", programState);
                    
                case Code.And:
                    return binOp("And", programState);
                case Code.Or:
                    return binOp("Or", programState);
                case Code.Xor:
                    return binOp("Xor", programState);
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
