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

                    if (node.Block.Method.Definition.ReturnType.FullName == "System.Void")
                    {
                        // Nothing to handle here
                        outputFlixCode = "// Return from void method";
                    }
                    else
                    {
                        outputFlixCode = $"RetStm(\"RET_{methodBody.Method.NameOnly()}\", {node.Block.Nodes.Reverse().ElementAt(1).PushStackNames.First()}).";
                    }
                    
                    return true;
            }

            outputFlixCode = null;
            return false;
        }
        
        private string call(string callType, Node node, MethodReference method)
        {
            StringBuilder output = new StringBuilder();

            // Pop the number of arguments
            for (int i = 0; i < method.Parameters.Count; i++)
            {
                uint parameterIndex;
                if (method.HasThis)
                {
                    parameterIndex = (uint)i + 1;
                }
                else
                {
                    parameterIndex = (uint)i;
                }

                output.AppendLine(
                    $"StargStm(\"{method.NameOnly()}\", {node.PopStackNames.Reverse().ElementAt(i)}, {parameterIndex}, \"{method.Parameters[i].ParameterType}\").");
            }

            // Add this to arguments
            if (method.HasThis)
            {
                output.AppendLine(
                    $"StargStm(\"{method.NameOnly()}\", {node.PopStackNames.Reverse().ElementAt(0)}, 0, \"{method.ReturnType}\").");
            }

            // Add call statement
            // Detect void stm
            if (method.ReturnType.FullName == "System.Void")
            {
                output.AppendLine($"{callType}Stm(\"NIL\", \"\", \"{method.NameOnly()}\", \"{method.ReturnType}\").");
            }
            else
            {
                output.AppendLine($"{callType}Stm({node.PushStackNames.First()}, \"RET_{method.NameOnly()}\", \"{method.NameOnly()}\", \"{method.ReturnType}\").");
            }

            //output.AppendLine($"{callType}Stm({node.PushStackNames.First()}, \"RET_{method.FullName}\", \"{method.FullName}\").");

            return output.ToString().TrimEnd();
        }
    }
}