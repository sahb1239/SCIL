using System;
using System.Linq;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class Branch : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            switch (node.OpCode.Code)
            {
                case Code.Brtrue: // Branch to target if value is non-zero (true). (https://en.wikipedia.org/wiki/List_of_CIL_instructions)
                case Code.Brtrue_S:
                    outputFlixCode = BrTrue(node);
                    return true;
            }

            outputFlixCode = null;
            return false;
        }

        private string BrTrue(Node node)
        {
            if (node.Operand is Instruction branchToInstruction)
            {
                return $"BrtrueStm({node.PopStackNames.First()}, {branchToInstruction.Offset}).";
            }
            throw new NotSupportedException();
        }
    }
}
