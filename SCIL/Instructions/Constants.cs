using System;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Constants : IInstructionEmitter
    {
        public InstructionEmitterOutput GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
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
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldc((long) (instruction.OpCode.Value - Code.Ldc_I4_0)), true, 0);
                case Code.Ldc_I4_M1:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldc(-1), true, 0);
                case Code.Ldc_I4:
                case Code.Ldc_I4_S:
                case Code.Ldc_I8:
                    if (instruction.Operand is Int32 dec)
                    {
                        return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldc(dec), true, 0);
                    }
                    else if (instruction.Operand is SByte tmpbyte)
                    {
                        return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldc(tmpbyte), true, 0);
                    }
                    else if (instruction.Operand is long lbyte)
                    {
                        return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldc(lbyte), true, 0);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldc_R4: //R4 and R8 are floats!
                case Code.Ldc_R8:
                    if (instruction.Operand is double doub)
                    {
                        return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldc(doub), true, 0);
                    }
                    else if (instruction.Operand is float flo)
                    {
                        return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldc(flo), true, 0);
                    }
                    throw new ArgumentException(nameof(instruction.Operand));
                case Code.Ldnull:
                    return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldc("null"), true, 0);
                case Code.Ldstr:
                    if (instruction.Operand is string str)
                    {
                        return new InstructionEmitterOutput(typeDefinition, methodBody, instruction, ldstr(str), true, 0);
                    }
                    throw new ArgumentException(nameof(instruction.Operand));
            }

            return null;
        }
        private string ldc(long op) => "ldcStm({0}, " + op + ")";
        private string ldc(double op) => "ldcStm({0}, " + op + ")";
        private string ldc(string op) => "ldcStm({0}, " + op + ")";
        private string ldstr(string str) => "ldstrStm({0}, " + str + ")";
    }
}