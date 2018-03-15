using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    public class Field:IInstructionEmitter
    {
        public string GetCode(TypeDefinition typeDefinition, MethodBody methodBody, Instruction instruction)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldfld:
                    if (instruction.Operand is FieldDefinition fieldDef)
                    {
                        return ldfld(fieldDef.FullName);
                    }
                    else if (instruction.Operand is FieldReference fieldRef)
                    {
                        return ldfld(fieldRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldflda:
                    if (instruction.Operand is FieldDefinition fieldADef)
                    {
                        return ldflda(fieldADef.FullName);
                    }
                    else if (instruction.Operand is FieldReference fieldRef)
                    {
                        return ldflda(fieldRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldftn:
                    if (instruction.Operand is MethodReference methodRef)
                    {
                        return ldftn(methodRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldsfld:
                    if (instruction.Operand is FieldDefinition staticFieldDef)
                    {
                        return ldfld(staticFieldDef.FullName);
                    }
                    else if (instruction.Operand is FieldReference staticFieldRef)
                    {
                        return ldfld(staticFieldRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldsflda:
                    if (instruction.Operand is FieldDefinition staticFieldADef)
                    {
                        return ldsfld(staticFieldADef.FullName);
                    }
                    else if (instruction.Operand is FieldReference staticFieldARef)
                    {
                        return ldsflda(staticFieldARef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Stfld:
                    if (instruction.Operand is FieldDefinition stFldDef)
                    {
                        return stfld(stFldDef.FullName);
                    }
                    else if (instruction.Operand is FieldReference stFldRef)
                    {
                        return stfld(stFldRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Stsfld:
                    if (instruction.Operand is FieldDefinition stsFldDef)
                    {
                        return stsfld(stsFldDef.FullName);
                    }
                    else if (instruction.Operand is FieldReference stsFldRef)
                    {
                        return stsfld(stsFldRef.FullName);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
            }

            return null;
        }

        private string ldfld(string field) => "ldfld " + field;
        private string ldflda(string field) => "ldflda " + field; //ldfld and ldflda seems to look a lot alike.
        private string ldftn(string method) => "ldftn " + method;
        private string ldsfld(string field) => "ldsfld " + field;
        private string ldsflda(string field) => "ldsflda " + field;
        private string stfld(string field) => "stfld " + field;
        private string stsfld(string field) => "stsfld " + field;
    }
}