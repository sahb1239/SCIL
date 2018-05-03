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

            if (node is PhiNode phiNode && phiNode.Parents.Count > 2)
            {
                for (int i = 0; i < phiNode.Parents.Count - 1; i++)
                {
                    newNodes.Add(new PhiNode(phiNode.Block, new List<Node> { i == 0 ? phiNode.Parents[i] : newNodes.Last(), phiNode.Parents[i + 1] }, phiNode.StackIndex));
                }
            }

            if (newNodes.Any())
                node.Replace(newNodes.ToArray());

            base.Visit(node);
        }
    }
}
