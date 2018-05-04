using System.Collections.Generic;
using System.Linq;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.ControlFlow.SSA.Simplifiers
{
    public class PhiNodeRewriterVisitor : BaseVisitor
    {
        public override void Visit(Node node)
        {
            List<Node> newNodes = new List<Node>();

            if (node is PhiStackNode phiNode && phiNode.Parents.Count > 2)
            {
                for (int i = 0; i < phiNode.Parents.Count - 1; i++)
                {
                    newNodes.Add(new PhiStackNode(phiNode.Block, new List<Node> { i == 0 ? phiNode.Parents[i] : newNodes.Last(), phiNode.Parents[i + 1] }, phiNode.StackIndex));
                }
            }
            else if (node is PhiVariableNode variableNode && variableNode.Parents.Count > 2)
            {
                for (int i = 0; i < variableNode.Parents.Count - 1; i++)
                {
                    newNodes.Add(new PhiVariableNode(variableNode.Block, new List<Node> { i == 0 ? variableNode.Parents[i] : newNodes.Last(), variableNode.Parents[i + 1] }, variableNode.VariableIndex));
                }
            }

            if (newNodes.Any())
                node.Replace(newNodes.ToArray());

            base.Visit(node);
        }
    }
}
