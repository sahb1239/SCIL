using Mono.Cecil.Cil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Simplifiers
{
    [RegistrerRewriter]
    public class ConstantRewriterVisitor : BaseVisitor
    {
        public override void Visit(Node node)
        {
            switch (node.OpCode.Code)
            {
                case Code.Ldc_I4_0:
                case Code.Ldc_I4_1:
                case Code.Ldc_I4_2:
                case Code.Ldc_I4_3:
                case Code.Ldc_I4_4:
                case Code.Ldc_I4_5:
                case Code.Ldc_I4_6:
                case Code.Ldc_I4_7:
                case Code.Ldc_I4_8:
                    node.OverrideOpCode = OpCodes.Ldc_I8;
                    node.OverrideOperand = (long)(node.OpCode.Value - Code.Ldc_I4_0);
                    break;
                case Code.Ldc_I4_M1:
                    node.OverrideOpCode = OpCodes.Ldc_I8;
                    node.OverrideOperand = (long)-1;
                    break;

            }

            base.Visit(node);
        }
    }
}