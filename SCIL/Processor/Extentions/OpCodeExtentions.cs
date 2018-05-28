using System;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Processor.Extentions
{
    public static class OpCodeExtentions
    {
        public static bool IsLdarg(this OpCode opCode)
        {
            switch (opCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarg_S:
                case Code.Ldarga:
                case Code.Ldarga_S:
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    return true;
            }

            return false;
        }

        public static bool IsStarg(this OpCode opCode)
        {
            switch (opCode.Code)
            {
                case Code.Starg:
                case Code.Starg_S:
                    return true;
            }

            return false;
        }

        public static int GetArgumentIndex(this OpCode opCode, object operand)
        {
            switch (opCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarg_S:
                case Code.Ldarga:
                case Code.Ldarga_S:
                case Code.Starg:
                case Code.Starg_S:
                    if (operand is sbyte index)
                    {
                        return index;
                    }
                    else if (operand is ParameterDefinition parameterDefinition)
                    {
                        return parameterDefinition.Index;
                    }

                    throw new NotSupportedException();
                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    return opCode.Code - Code.Ldarg_0;
            }

            throw new ArgumentOutOfRangeException(nameof(opCode), "Opcode is not a argument opcode");
        }

        public static int GetVariableIndex(this OpCode opCode, object operand)
        {
            switch (opCode.Code)
            {
                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloca:
                case Code.Ldloca_S:
                    if (operand is VariableDefinition variableDefinition)
                        return variableDefinition.Index;
                    if (operand is sbyte index)
                        return index;

                    throw new NotSupportedException();
                case Code.Ldloc_0:
                case Code.Ldloc_1:
                case Code.Ldloc_2:
                case Code.Ldloc_3:
                    return opCode.Code - Code.Ldloc_0;
            }

            throw new ArgumentOutOfRangeException(nameof(opCode), "Opcode is not a argument opcode");
        }

        public static FieldReference GetField(this OpCode opCode, object operand)
        {
            switch (opCode.Code)
            {
                // Load field
                case Code.Ldfld:
                case Code.Ldflda: 
                
                // Load static field
                case Code.Ldsfld:
                case Code.Ldsflda:
                    if (operand is FieldReference fieldReference)
                    {
                        return fieldReference;
                    }

                    throw new NotSupportedException();
            }

            throw new ArgumentOutOfRangeException(nameof(opCode), "Opcode is not a field opcode");
        }
    }
}