using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace SCIL.Processor.Nodes
{
    public abstract class PhiNode : Node
    {
        protected PhiNode(Block block, List<Node> parents) : base(block)
        {
            Parents = parents;
            OverrideOpCode = OpCodes.Nop;
        }

        public List<Node> Parents { get; }
    }

    public class PhiStackNode : PhiNode
    {
        public PhiStackNode(Block block, List<Node> parents, int stackIndex) : base(block, parents)
        {
            StackIndex = stackIndex;
            OverrideOpCode = OpCodes.Nop;
        }

        public int StackIndex { get; }

        public override string ToString()
        {
            return "PhiNode Stack: " + StackIndex;
        }

        public override int PopCountFromStack => 0;
        public override int PushCountFromStack => 1;
    }

    public class PhiVariableNode : PhiNode
    {
        public PhiVariableNode(Block block, List<Node> parents, int variableIndex) : base(block, parents)
        {
            VariableIndex = variableIndex;
            OverrideOpCode = OpCodes.Nop;
        }

        public int VariableIndex { get; }

        public override string ToString()
        {
            return "PhiNode Variable: " + VariableIndex;
        }

        public override int PopCountFromStack => 0;
        public override int PushCountFromStack => 0;
    }
}
