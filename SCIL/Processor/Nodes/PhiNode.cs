using System.Collections.Generic;

namespace SCIL.Processor.Nodes
{
    class PhiNode
    {
        private readonly List<Node> _parents = new List<Node>();
        private readonly Block _block;
        private readonly string _name;

        public PhiNode(Block block, Node node, Node[] parents)
        {
            _block = block;
            _name = node.VariableName;
            _parents = new List<Node>(parents);
        }


        
    }
}
