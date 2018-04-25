using Mono.Cecil.Cil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Simplifiers
{
    [RegistrerRewriter]
    public class ArgumentRewriterVisitor : BaseVisitor
    {
        public override void Visit(Node node)
        {
            switch (node.OpCode.Code)
            {
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    node.OverrideOperand = (sbyte)(node.Instruction.OpCode.Code - Code.Ldarg_0);
                    node.OverrideOpCode = OpCodes.Ldarg;
                    break;
            }

            base.Visit(node);
        }
    }
}
