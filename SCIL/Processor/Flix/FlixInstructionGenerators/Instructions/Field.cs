using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class Field : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            switch (node.OpCode.Code)
            {
                case Code.Ldfld:
                    if (node.Operand is FieldReference)
                    {
                        outputFlixCode = ldfld(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Ldflda:
                    if (node.Operand is FieldReference)
                    {
                        outputFlixCode = ldflda(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Ldsfld:
                    if (node.Operand is FieldReference)
                    {
                        outputFlixCode = ldsfld(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Ldsflda:
                    if (node.Operand is FieldReference)
                    {
                        outputFlixCode = ldsflda(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Ldftn:
                    if (node.Operand is MethodReference methodRef)
                    {
                        outputFlixCode = ldftn(methodRef.FullName, node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Stfld:
                    if (node.Operand is FieldReference)
                    {
                        outputFlixCode = stfld(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Stsfld:
                    if (node.Operand is FieldReference)
                    {
                        outputFlixCode = stsfld(node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
            }

            outputFlixCode = null;
            return false;
        }
        

        private string ldfld(Node node) => $"LdfldStm({node.PushStackNames.First()}, \"{node.FieldName}\").";
        private string ldflda(Node node) => $"LdfldaStm({node.PushStackNames.First()}, \"{node.FieldName}\")."; //ldfld and ldflda seems to look a lot alike.
        private string ldftn(string method, Node node) => $"LdftnStm({node.PushStackNames.First()}, \"{method}\").";
        private string ldsfld(Node node) => $"LdsfldStm({node.PushStackNames.First()}, \"{node.FieldName}\").";
        private string ldsflda(Node node) => $"LdsfldaStm({node.PushStackNames.First()}, \"{node.FieldName}\").";
        private string stfld(Node node) => $"StfldStm(\"{node.FieldName}\", {node.PopStackNames.First()}).";
        private string stsfld(Node node) => $"StsfldStm(\"{node.FieldName}\", {node.PopStackNames.First()}).";
    }
}