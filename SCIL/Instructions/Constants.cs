using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Constants : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
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
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return ldc((long) (instruction.OpCode.Value - Code.Ldc_I4_0), programState);
                case Code.Ldc_I4_M1:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return ldc(-1, programState);
                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                case Code.Ldc_I8:
                    if (instruction.Operand is Int32 dec)
                    {
                        return ldc(dec, programState);
                    }
                    else if (instruction.Operand is SByte tmpbyte)
                    {
                        return ldc(tmpbyte, programState);
                    }
                    else if (instruction.Operand is long lbyte)
                    {
                        return ldc(lbyte, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldc_R4: //R4 and R8 are floats!
                case Code.Ldc_R8:
                    if (instruction.Operand is double doub)
                    {
                        return ldc(doub, programState);
                    }
                    else if (instruction.Operand is float flo)
                    {
                        return  ldc(flo, programState);
                    }
                    throw new ArgumentException(nameof(instruction.Operand));
                case Code.Ldnull:
                    return ldc("null", programState);
                case Code.Ldstr:
                    if (instruction.Operand is string str)
                    {
                        return  ldstr(str, programState);
                    }
                    throw new ArgumentException(nameof(instruction.Operand));
            }

            return null;
        }
        private string ldc(long op, IFlixInstructionProgramState state) => $"LdcStm({state.PushStack()}, {op}).";
        private string ldc(double op, IFlixInstructionProgramState state) => $"LdcStm({state.PushStack()}, {op}).";
        private string ldc(string op, IFlixInstructionProgramState state) => $"LdcStm({state.PushStack()}, {op}).";
        private string ldstr(string str, IFlixInstructionProgramState state) => $"LdstrStm({state.PushStack()}, {str}).";
    }
}