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
                    node.OverrideOpCode = OpCodes.Stloc;
                    node.OverrideOperand = (sbyte) (node.OpCode.Value - (int) Code.Stloc_0);
                    break;
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    node.OverrideOpCode = OpCodes.Ldloc;
                    node.OverrideOperand = (sbyte)(node.OpCode.Value - (int)Code.Ldloc_0);
                    break;
            }

            base.Visit(node);
        }
    }
}
