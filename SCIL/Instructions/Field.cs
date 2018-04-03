using System;
using System.Collections;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Field : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Ldfld:
                    if (instruction.Operand is FieldDefinition fieldDef)
                    {
                        return ldfld(fieldDef.FullName, programState);
                    }
                    else if (instruction.Operand is FieldReference fieldRef)
                    {
                        return ldfld(fieldRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldflda:
                    if (instruction.Operand is FieldDefinition fieldADef)
                    {
                        return ldflda(fieldADef.FullName, programState);
                    }
                    else if (instruction.Operand is FieldReference fieldRef)
                    {
                        return ldflda(fieldRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldftn:
                    if (instruction.Operand is MethodReference methodRef)
                    {
                        return ldftn(methodRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldsfld:
                    if (instruction.Operand is FieldDefinition staticFieldDef)
                    {
                        return ldfld(staticFieldDef.FullName, programState);
                    }
                    else if (instruction.Operand is FieldReference staticFieldRef)
                    {
                        return ldfld(staticFieldRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ldsflda:
                    if (instruction.Operand is FieldDefinition staticFieldADef)
                    {
                        return ldsfld(staticFieldADef.FullName, programState);
                    }
                    else if (instruction.Operand is FieldReference staticFieldARef)
                    {
                        return ldsflda(staticFieldARef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Stfld:
                    if (instruction.Operand is FieldDefinition stFldDef)
                    {
                        return stfld(stFldDef.FullName, programState);
                    }
                    else if (instruction.Operand is FieldReference stFldRef)
                    {
                        return stfld(stFldRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Stsfld:
                    if (instruction.Operand is FieldDefinition stsFldDef)
                    {
                        return stsfld(stsFldDef.FullName, programState);
                    }
                    else if (instruction.Operand is FieldReference stsFldRef)
                    {
                        return stsfld(stsFldRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
            }

            return null;
        }

        private string ldfld(string field, IFlixInstructionProgramState programState) => $"ldfldStm({programState.PushStack()}, {programState.GetField(field)}).";
        private string ldflda(string field, IFlixInstructionProgramState programState) => $"ldfldaStm({programState.PushStack()}, {programState.GetField(field)})."; //ldfld and ldflda seems to look a lot alike.
        private string ldftn(string method, IFlixInstructionProgramState programState) => null; //$"ldftnStm({method}";
        private string ldsfld(string field, IFlixInstructionProgramState programState) => $"ldsfldStm({programState.PushStack()}, {programState.GetField(field)}).";
        private string ldsflda(string field, IFlixInstructionProgramState programState) => $"ldsfldaStm({programState.PushStack()}, {programState.GetField(field)}).";
        private string stfld(string field, IFlixInstructionProgramState programState) => $"stfldStm({programState.GetField(field)}, {programState.PopStack()}).";
        private string stsfld(string field, IFlixInstructionProgramState programState) => $"stsfldStm({programState.GetField(field)}, {programState.PopStack()}).";
    }
}