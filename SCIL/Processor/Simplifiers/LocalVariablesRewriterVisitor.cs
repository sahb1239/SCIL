using Mono.Cecil.Cil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Simplifiers
{
    [RegistrerRewriter]
    public class LocalVariablesRewriterVisitor : BaseVisitor
    {
        public override void Visit(Node node)
        {
            switch (node.OpCode.Code)
            {
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                    node.OverrideOperand = (sbyte)(node.Instruction.OpCode.Code - Code.Stloc_0);
                    node.OverrideOpCode = OpCodes.Stloc;
                    break;
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    node.OverrideOperand = (sbyte)(node.Instruction.OpCode.Code - Code.Ldloc_0);
                    node.OverrideOpCode = OpCodes.Ldloc;
                    break;
                case Code.Ldloc_S:
                    node.OverrideOpCode = OpCodes.Ldloc;
                    break;
                case Code.Ldloca_S:
                    node.OverrideOpCode = OpCodes.Ldloca;
                    break;

            }

            base.Visit(node);
        }
    }
}
