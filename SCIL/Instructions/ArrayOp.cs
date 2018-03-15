using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    public class ArrayOp:IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Stelem_I:
                case Code.Stelem_I2:
                case Code.Stelem_I4:
                case Code.Stelem_I8:
                case Code.Stelem_R4:
                case Code.Stelem_R8: //R4 and R8 are floats!
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return "stelem";
                case Code.Stelem_Any:
                    if (instruction.Operand is GenericInstanceType genTyp)
                    {
                        return stelem(genTyp.FullName);
                    }
                    else if (instruction.Operand is TypeDefinition stelemTypeDef)
                    {
                        return stelem(stelemTypeDef.FullName);
                    }
                    else if (instruction.Operand is GenericParameter genParm)
                    {
                        return stelem(genParm.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Stelem_Ref:
                    return "stelem.ref";
                case Code.Ldelem_I:
                case Code.Ldelem_I1:
                case Code.Ldelem_I2:
                case Code.Ldelem_I4:
                case Code.Ldelem_I8:
                case Code.Ldelem_R4:
                case Code.Ldelem_R8:
                case Code.Ldelem_U1:
                case Code.Ldelem_U2:
                case Code.Ldelem_U4:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return "ldelem";
                case Code.Ldelem_Any:
                    if (instruction.Operand is TypeDefinition ldelemTypeDef)
                    {
                        return ldelem(ldelemTypeDef.FullName);
                    }
                    else if (instruction.Operand is TypeReference ldelemTypeRef)
                    {
                        return ldelem(ldelemTypeRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldelem_Ref:
                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return "ldelem.ref";
                case Code.Ldelema:
                    if (instruction.Operand is TypeReference ldelemaTypeRef)
                    {
                        return ldelema(ldelemaTypeRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
            }

            return null;
        }

        private string stelem(string tok) => "stelem " + tok;
        private string ldelem(string tok) => "ldelem " + tok;
        private string ldelema(string tok) => "ldelema " + tok;
    }
}