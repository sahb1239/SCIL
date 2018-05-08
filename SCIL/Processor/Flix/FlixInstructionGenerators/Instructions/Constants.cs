using System;
using System.Globalization;
using System.Linq;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class Constants : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            switch (node.OpCode.Code)
            {
                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                case Code.Ldc_I8:
                    if (node.Operand is Int32 dec)
                    {
                        outputFlixCode = ldc(dec, node);
                        return true;
                    }
                    else if (node.Operand is SByte tmpbyte)
                    {
                        outputFlixCode = ldc(tmpbyte, node);
                        return true;
                    }
                    else if (node.Operand is long lbyte)
                    {
                        outputFlixCode = ldc(lbyte, node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Ldc_R4: //R4 and R8 are floats!
                case Code.Ldc_R8:
                    if (node.Operand is double doub)
                    {
                        outputFlixCode = ldc(doub, node);
                        return true;
                    }
                    else if (node.Operand is float flo)
                    {
                        outputFlixCode = ldc(flo, node);
                        return true;
                    }
                    throw new ArgumentException(nameof(node.Operand));
                case Code.Ldnull:
                    outputFlixCode = "";//ldc("null", node);
                    return true;
                case Code.Ldstr:
                    if (node.Operand is string str)
                    {
                        outputFlixCode = ldstr(str, node);
                        return true;
                    }
                    throw new ArgumentException(nameof(node.Operand));
            }

            outputFlixCode = null;
            return false;
        }

        private string ldc(long op, Node node) => $"LdcStm({node.PushStackNames.First()}, {op.ToString("D", new CultureInfo("en-US"))}i32).";
        private string ldc(double op, Node node) => $"LdcFStm({node.PushStackNames.First()}, {op.ToString("F", new CultureInfo("en-US"))}f32).";
        private string ldc(string op, Node node) => $"LdcStm({node.PushStackNames.First()}, {op}).";
        private string ldstr(string str, Node node) => $"LdstrStm({node.PushStackNames.First()}, \"{EscapeStr(str)}\").";
        private string EscapeStr(string str) => str.Replace("\\", "").Replace("\"", ""); // TODO: Find other special chars, newlines etc and actually handle them
        
    }
}