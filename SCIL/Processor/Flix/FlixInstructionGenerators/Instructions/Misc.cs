using System;
using System.Linq;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class Misc : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            switch (node.OpCode.Code)
            {
                case Code.Pop:
                    if (node.Operand != null)
                    {
                        throw new ArgumentException(nameof(node.Operand));
                    }

                    outputFlixCode = $"PopStm({node.PopStackNames.First()}).";
                    return true;
                case Code.Box:
                case Code.Dup:
                    outputFlixCode = $"DupStm({node.PushStackNames.First()}, {node.PopStackNames.First()}).";
                    return true;
            }

            outputFlixCode = null;
            return false;
        }
    }
}