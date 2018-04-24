using System;
using System.Linq;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class UnaryOp : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            // Unsigned and overflow abstracted away
            switch (node.OpCode.Code)
            {
                case Code.Neg:
                    outputFlixCode = $"NegStm({node.PushStackNames.First()}, {node.PopStackNames.First()}).";
                    return true;

                case Code.Not:
                    outputFlixCode = $"NotStm({node.PushStackNames.First()}, {node.PopStackNames.First()}).";
                    return true;
            }

            outputFlixCode = null;
            return false;
        }
    }
}
