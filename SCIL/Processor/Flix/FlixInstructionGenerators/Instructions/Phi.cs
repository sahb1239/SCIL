using System;
using System.Linq;
using SCIL.Processor.FlixInstructionGenerators;
using SCIL.Processor.Nodes;

namespace SCIL.Processor.Flix.FlixInstructionGenerators.Instructions
{
    public class Phi : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            if (node is PhiStackNode phiNode)
            {
                outputFlixCode =
                    $"PhiStm({string.Join(", ", phiNode.PushStackNames.First(), string.Join(", ", phiNode.Parents.Select(e => e.PushStackNames.Last())))}).";
                return true;
            }
            else if (node is PhiVariableNode variableNode)
            {
                outputFlixCode =
                    $"PhiLocStm({string.Join(", ", variableNode.VariableName, string.Join(", ", variableNode.Parents.Select(e => e.VariableName)))}.";
            }

            outputFlixCode = null;
            return false;
        }
    }
}
