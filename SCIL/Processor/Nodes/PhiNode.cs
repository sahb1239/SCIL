using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace SCIL.Processor.Nodes
{
    public class PhiNode : Node
    {
        public PhiNode(Block block, List<Node> parents, int stackIndex) : base(block)
        {
            Parents = parents;
            StackIndex = stackIndex;
            OverrideOpCode = OpCodes.Nop;
        }

        public List<Node> Parents { get; }

        public int StackIndex { get; }

        public override string ToString()
        {
            return "PhiNode: " + StackIndex;
        }

        public override int PopCountFromStack => 0;
        public override int PushCountFromStack => 1;
    }
}
