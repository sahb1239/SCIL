using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace SCIL.Processor.Nodes
{
    public class PhiNode : Node
    {
        public PhiNode(Block block, Node node, Node[] parents) : base(block)
        {
            Name = node.VariableName;
            Parents = new List<Node>(parents);
        }
        
        public List<Node> Parents { get; }

        public string Name { get; }
    }
}
