using System;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class Methods : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            var methodBody = node.Block.Method.Definition.Body;

            switch (node.OpCode.Code)
            {
                case Code.Call:
                    if (node.Operand is MethodReference callRef)
                    {
                        outputFlixCode = call("Call", node, callRef);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Callvirt:
                    if (node.Operand is MethodReference callVirtRef)
                    {
                        outputFlixCode = call("Callvirt", node, callVirtRef);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Ret:
                    if (node.Operand != null)
                    {
                        throw new ArgumentException(nameof(node.Operand));
                    }
                    outputFlixCode = $"RetStm(\"RET_{methodBody.Method.FullName}\").";
                    return true;
            }

            outputFlixCode = null;
            return false;
        }
        
        private string call(string callType, Node node, MethodReference method)
        {
            StringBuilder output = new StringBuilder();
            
            // Add call statement
            output.AppendLine($"{callType}Stm({node.PushStackNames.First()}, \"RET_{method.FullName}\", \"{method.FullName}\").");

            return output.ToString().TrimEnd();
        }
    }
}