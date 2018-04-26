using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class Arguments : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            switch (node.OpCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarg_S:
                    outputFlixCode = ldarg(node);
                    return true;
                case Code.Ldarga:
                case Code.Ldarga_S:
                    outputFlixCode = ldarga(node);
                    return true;

                case Code.Starg:
                case Code.Starg_S:
                    outputFlixCode = starg(node, GetOperandIndex(node));
                    return true;

                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    outputFlixCode = ldarg(node);
                    return true;
            }

            outputFlixCode = null;
            return false;
        }

        public string GetCode(Node node)
        {
            switch (node.OpCode.Code)
            {
                case Code.Ldarg:
                case Code.Ldarg_S:
                    return ldarg(node);

                case Code.Ldarga:
                case Code.Ldarga_S:
                    return ldarga(node);

                case Code.Starg:
                case Code.Starg_S:
                    return starg(node, GetOperandIndex(node));

                case Code.Ldarg_0:
                case Code.Ldarg_1:
                case Code.Ldarg_2:
                case Code.Ldarg_3:
                    return ldarg(node);
            }

            return null;
        }

        private uint GetOperandIndex(Node node)
        {
            // Get method body
            var methodBody = node.Block.Method.Definition.Body;

            if (node.Operand is ParameterDefinition parameterDefinition)
            {
                // Get index in parameters
                var parameterIndex = methodBody.Method.Parameters.IndexOf(parameterDefinition);
                if (parameterIndex == -1)
                    throw new ArgumentOutOfRangeException(nameof(parameterIndex), "Could not find parameter matching Ldarg");

                // Detect if it's a static method ("this" is argument 0 on non static methods)
                var staticMethod = methodBody.Method.IsStatic;

                // Get final index
                var finalIndex = staticMethod ? parameterIndex : parameterIndex + 1;

                // Check if index is less than 0
                if (finalIndex < 0)
                    throw new ArgumentOutOfRangeException(nameof(finalIndex), "Final index was less than 0");

                return (uint) finalIndex;
            }
            else if (node.Operand is sbyte index)
            {
                return (uint) index;
            }

            throw new NotImplementedException("Could not find operand index");
        }

        private string ldarg(Node node) => $"LdargStm({node.PushStackNames.First()}, \"{node.Block.Method.Definition.NameOnly()}_{GetOperandIndex(node)}\").";
        private string ldarga(Node node) => $"LdargaStm({node.PushStackNames.First()}, \"{node.Block.Method.Definition.NameOnly()}_{GetOperandIndex(node)}\").";

        private string starg(Node node, uint argNo) =>
            $"StargStm({node.ArgumentName}, {node.PopStackNames.First()}, {argNo}, \"{GetVariableDefinition(node).VariableType.FullName}\").";

        private VariableDefinition GetVariableDefinition(Node node)
        {
            if (node.Operand is VariableDefinition variableDefinition)
                return variableDefinition;

            var index = GetVariableIndex(node.Operand);
            return node.Block.Method.Definition.Body.Variables.First(e => e.Index == index);
        }
        private int GetVariableIndex(object operand)
        {
            if (operand is sbyte index)
            {
                return index;
            }
            else if (operand is VariableDefinition variableDefinition)
            {
                return variableDefinition.Index;
            }

            throw new NotImplementedException();
        }
    }
}