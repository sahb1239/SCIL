using System;
using System.Diagnostics;
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
                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                    node.OverrideOpCode = OpCodes.Ldc_I8;

                    // If I don't do this it does not unbox the variable correctly and therefore comes with invalid cast exceptions...
                    if (node.Operand is sbyte operandSbyte)
                    {
                        node.OverrideOperand = (long)operandSbyte;
                    }
                    else if (node.Operand is int operandInt)
                    {
                        node.OverrideOperand = (long)operandInt;
                    }
                    else if (node.Operand is long operandLong)
                    {
                        node.OverrideOperand = (long)operandLong;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    break;
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

                case Code.Ldc_R4:
                    node.OverrideOpCode = OpCodes.Ldc_R8;

                    if (node.Operand is Single operandSingle)
                    {
                        node.OverrideOperand = (double)operandSingle;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }

                    break;
            }

            base.Visit(node);
        }
    }
}