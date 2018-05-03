using System.Collections.Generic;
using Mono.Cecil.Cil;

namespace SCIL.Processor.Nodes
{
    public class PhiStackNode : Node
    {
        public PhiStackNode(Block block, List<Node> parents, int stackIndex) : base(block)
        {
            Parents = parents;
            StackIndex = stackIndex;
            OverrideOpCode = OpCodes.Nop;
        }

        public List<Node> Parents { get; }

        public int StackIndex { get; }

        public override string ToString()
        {
            return "PhiNode Stack: " + StackIndex;
        }

        public override int PopCountFromStack => 0;
        public override int PushCountFromStack => 1;
    }

    public class PhiVariableNode : Node
    {
        public PhiVariableNode(Block block, List<Node> parents, int variableIndex) : base(block)
        {
            Parents = parents;
            VariableIndex = variableIndex;
            OverrideOpCode = OpCodes.Nop;
        }

        public List<Node> Parents { get; }

        public int VariableIndex { get; }

        public override string ToString()
        {
            return "PhiNode Variable: " + VariableIndex;
        }

        public override int PopCountFromStack => 0;
        public override int PushCountFromStack => 0;
    }
}
