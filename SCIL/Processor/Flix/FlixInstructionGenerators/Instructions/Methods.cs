using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Extentions;
using SCIL.Processor.Nodes;

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
                        // Handle results
                        if (IsResultCall(node, callRef, out outputFlixCode))
                            return true;

                        outputFlixCode = call("Call", node, callRef);
                        return true;
                    }
                    throw new ArgumentOutOfRangeException(nameof(node.Operand));
                case Code.Callvirt:
                    if (node.Operand is MethodReference callVirtRef)
                    {
                        // Handle results
                        if (IsResultCall(node, callVirtRef, out outputFlixCode))
                            return true;

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
                        outputFlixCode = $"RetStm(\"RET_{methodBody.Method.NameOnly()}\", {node.PopStackNames.First()}).";
                    }
                    
                    return true;
            }

            outputFlixCode = null;
            return false;
        }

        private bool IsResultCall(Node node, MethodReference method, out string flixCode)
        {
            // Non generic (no result)
            // For now handle as normal method call
            if (method.DeclaringType.FullName == "System.Runtime.CompilerServices.AsyncTaskMethodBuilder" && 
                method.FullName == "System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder::SetResult()")
            {
                flixCode = "";
                return false;
            }
            else if (method.DeclaringType.FullName == "System.Runtime.CompilerServices.TaskAwaiter" &&
                     method.FullName == "System.Void System.Runtime.CompilerServices.TaskAwaiter::GetResult()")
            {
                flixCode = "";
                return false;
            }

            // Generic
            if (method.DeclaringType.FullName.StartsWith("System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1") &&
                     method.FullName.StartsWith("System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder`1") &&
                     method.FullName.Contains("SetResult"))
            {
                // Assert 2 arguments
                Debug.Assert(node.PopStack.Count == 2);

                // Find type of method
                var type = node.Block.Method.Type;

                // This should be generated task type
                Debug.Assert(type.IsGeneratedTaskType);

                // Debug assert that there are any initilization points
                Debug.Assert(type.InitilizationPoints.Any());

                if (!type.InitilizationPoints.Any())
                {
                    flixCode = "";
                    return false;
                }

                // Check we only have one initilization point
                Debug.Assert(type.InitilizationPoints.Count() == 1);

                // Get inititilization method
                var initilizationPoint = type.InitilizationPoints.First().Block.Method;

                // Get instance type
                var instanceType = (GenericInstanceType) method.DeclaringType;
                flixCode = $"SetResultStm(\"{initilizationPoint.Definition.NameOnly()}\", {node.PopStackNames.First()},\"{instanceType.GenericArguments.Single().FullName}\").";
                return true;
            }
            else if (method.DeclaringType.FullName.StartsWith("System.Runtime.CompilerServices.TaskAwaiter`1") &&
                     method.FullName.StartsWith("!0 System.Runtime.CompilerServices.TaskAwaiter`1<") &&
                     method.FullName.EndsWith(">::GetResult()"))
            {
                Debug.Assert(node.TaskMethod != null);
                if (node.TaskMethod == null)
                {
                    flixCode = "";
                    return false;
                }

                flixCode = $"GetResultStm({node.PushStackNames.First()}, \"{node.TaskMethod.NameOnly()}\").";
                return true;
            }

            flixCode = "";
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
                    $"StargStm(\"{method.NameOnly()}\", {node.PopStackNames.Reverse().ElementAt((int)parameterIndex)}, {parameterIndex}, \"{method.Parameters[i].ParameterType}\").");
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
                output.AppendLine($"{callType}Stm(\"NIL\", \"\", \"{method.NameOnly()}\", \"{method.ReturnType}\", 0).");
            }
            else if (method.ReturnType.Namespace == "System.Threading.Tasks" && method.ReturnType.Name.StartsWith("Task"))
            {
                // Detect tasks
                output.AppendLine($"{callType}Stm({node.PushStackNames.First()}, \"RET_{method.NameOnly()}\", \"{method.NameOnly()}\", \"{method.ReturnType}\", 1).");
            }
            else
            {
                output.AppendLine($"{callType}Stm({node.PushStackNames.First()}, \"RET_{method.NameOnly()}\", \"{method.NameOnly()}\", \"{method.ReturnType}\", 0).");
            }

            //output.AppendLine($"{callType}Stm({node.PushStackNames.First()}, \"RET_{method.FullName}\", \"{method.FullName}\").");

            return output.ToString().TrimEnd();
        }
    }
}