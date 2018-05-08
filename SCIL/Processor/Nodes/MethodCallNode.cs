using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;

namespace SCIL.Processor.Nodes
{
    public class MethodCallNode : Node
    {
        public Node Node { get; }
        public Method Method { get; set; }

        public MethodCallNode(Node node, Method method) : base(node.Instruction, node.Block)
        {
            Node = node ?? throw new ArgumentNullException(nameof(node));
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }
    }
}
