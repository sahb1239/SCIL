using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes.Visitor;
using SCIL;

namespace SCIL.Processor.Nodes
{
    class PhiNode
    {
        private readonly List<Node> _parents = new List<Node>();
        private readonly Block _block;
        private readonly string name;

        public PhiNode(Block block, Node node, Node[] parents)
        {
            this._block = block;
            this.name = node.VariableName;
            this._parents = new List<Node>(parents);
        }


        
    }
}
