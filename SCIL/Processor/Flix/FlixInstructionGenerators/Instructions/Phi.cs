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
            if (node is PhiNode phiNode)
            {
                outputFlixCode =
                    $"PhiStm{phiNode.Parents.Count}({string.Join(", ", phiNode.PushStackNames.First(), string.Join(", ", phiNode.Parents.Select(e => e.PushStackNames.Last())))}).";
                return true;
            }

            outputFlixCode = null;
            return false;
        }
    }
}
