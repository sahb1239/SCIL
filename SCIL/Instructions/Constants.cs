using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    public class Constants : IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
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
                    return ldc(instruction.OpCode.Value - 22);
                case Code.Ldc_I4_M1:
                    return ldc(-1);
                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                case Code.Ldc_I8:
                    if (instruction.Operand is Int32 dec)
                    {
                        return ldc(dec);
                    }
                    else if (instruction.Operand is SByte tmpbyte)
                    {
                        return ldc(tmpbyte);
                    }
                    else if (instruction.Operand is long lbyte)
                    {
                        return ldc(lbyte);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldc_R4: //R4 and R8 are floats!
                case Code.Ldc_R8:
                    if (instruction.Operand is double doub)
                    {
                        return ldc(doub);
                    }
                    else if (instruction.Operand is float flo)
                    {
                        return ldc(flo);
                    }
                    throw new ArgumentException(nameof(instruction.Operand));
                case Code.Ldnull:
                    return ldc("null");
                case Code.Ldstr:
                    if (instruction.Operand is string str)
                    {
                        return ldstr(str);
                    }
                    throw new ArgumentException(nameof(instruction.Operand));
            }

            return null;
        }
        private string ldc(long op) => "ldc " + op;
        private string ldc(double op) => "ldc " + op;
        private string ldc(string op) => "ldc " + op;
        private string ldstr(string str) => "ldstr " + str;
    }
}