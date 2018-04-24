using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class ObjectOp : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            switch (node.OpCode.Code)
            {
                case Code.Newobj:
                    if (node.Operand is MethodReference callRef)
                    {
                        outputFlixCode = newobj(callRef.FullName, node);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
            }

            outputFlixCode = null;
            return false;
        }

        private string newobj(string method, Node node) => $"NewobjStm({node.PushStackNames.First()}, \"{method}\").";
    }
}