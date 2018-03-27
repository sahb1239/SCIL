using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class BinaryOp : IInstructionEmitter
    {
        public InstructionEmitterOutput GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            // Unsigned and overflow abstracted away
            switch (instruction.OpCode.Code)
            {
                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                    return binOp("add", typeDefinition, methodBody, instruction);
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    return binOp("sub", typeDefinition, methodBody, instruction);
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    return binOp("mul", typeDefinition, methodBody, instruction);
                case Code.Div:
                case Code.Div_Un:
                    return binOp("div", typeDefinition, methodBody, instruction);
                case Code.Rem:
                case Code.Rem_Un:
                    return binOp("rem", typeDefinition, methodBody, instruction);
                case Code.Ceq:
                    return binOp("ceq", typeDefinition, methodBody, instruction);

                case Code.And:
                    return binOp("and", typeDefinition, methodBody, instruction);
                case Code.Or:
                    return binOp("or", typeDefinition, methodBody, instruction);
                case Code.Xor:
                    return binOp("xor", typeDefinition, methodBody, instruction);
            }

            return null;
        }

        private InstructionEmitterOutput binOp(string op, TypeDefinition typeDefinition, MethodBody methodBody,
            Instruction instruction) => new InstructionEmitterOutput(typeDefinition, methodBody, instruction,
            op + "Stm({2}, {1}, {0}).", true, 2);
    }
}
