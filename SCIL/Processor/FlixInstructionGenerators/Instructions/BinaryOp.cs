using System.Linq;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class BinaryOp : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            // Unsigned and overflow abstracted away
            switch (node.OpCode.Code)
            {
                case Code.Add:
                case Code.Add_Ovf:
                case Code.Add_Ovf_Un:
                    outputFlixCode = BinOp("Add", node);
                    return true;
                case Code.Sub:
                case Code.Sub_Ovf:
                case Code.Sub_Ovf_Un:
                    outputFlixCode = BinOp("Sub", node);
                    return true;
                case Code.Mul:
                case Code.Mul_Ovf:
                case Code.Mul_Ovf_Un:
                    outputFlixCode = BinOp("Mul", node);
                    return true;
                case Code.Div:
                case Code.Div_Un:
                    outputFlixCode = BinOp("Div", node);
                    return true;
                case Code.Rem:
                case Code.Rem_Un:
                    outputFlixCode = BinOp("Rem", node);
                    return true;
                case Code.Clt:
                case Code.Clt_Un:
                    outputFlixCode = BinOp("Clt", node);
                    return true;
                case Code.Cgt:
                case Code.Cgt_Un:
                    outputFlixCode = BinOp("Cgt", node);
                    return true;
                case Code.Ceq:
                    outputFlixCode = BinOp("Ceq", node);
                    return true;

                case Code.And:
                    outputFlixCode = BinOp("And", node);
                    return true;
                case Code.Or:
                    outputFlixCode = BinOp("Or", node);
                    return true;
                case Code.Xor:
                    outputFlixCode = BinOp("Xor", node);
                    return true;
            }

            outputFlixCode = null;
            return false;
        }

        private string BinOp(string op, Node node) => $"{op}Stm({node.PushStackNames.First()}, {node.PopStackNames.Last()}, {node.PopStackNames.First()}).";
    }
}
