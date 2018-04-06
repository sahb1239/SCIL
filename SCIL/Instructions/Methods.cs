using System;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    class Methods : IFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            switch (instruction.OpCode.Code)
            {
                case Code.Call:
                    if (instruction.Operand is MethodReference callRef)
                    {
                        return call("call", programState, callRef);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Callvirt:
                    if (instruction.Operand is MethodReference callVirtRef)
                    {
                        return call("callvirt", programState, callVirtRef);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ret:
                    if (methodBody.Method.ReturnType.FullName == "System.Void")
                        programState.PushStack(); // Push a value - which will be popped

                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return $"retStm(\"RET_{methodBody.Method.FullName}\").";
            }

            return null;
        }
        private string call(string callType, IFlixInstructionProgramState programState, MethodReference method)
        {
            MethodDefinition methodDefinition = method.Resolve();

            StringBuilder output = new StringBuilder();

            // Pop the number of arguments
            for (int i = method.Parameters.Count - 1; i >= 0 ; i--)
            {
                uint parameterIndex;
                if (method.HasThis)
                {
                    parameterIndex = (uint) i + 1;
                }
                else
                {
                    parameterIndex = (uint) i;
                }

                output.AppendLine(
                    $"stargStm({programState.GetStoreArg(methodDefinition, parameterIndex)}, {programState.PopStack()}).");
            }

            // Add this to arguments
            if (method.HasThis)
            {
                output.AppendLine(
                    $"stargStm({programState.GetStoreArg(methodDefinition, 0)}, {programState.PopStack()}).");
            }

            // Add call statement
            output.AppendLine($"{callType}Stm(\"{method.FullName}\", {programState.PushStack()}).");

            // Add return value
            if (method.ReturnType.FullName == "System.Void")
            {
                programState.PushStack(); // Push a value - which will be popped
            }
            else
            {
                output.AppendLine($"dupStm({programState.PushStack()}, \"RET_{methodDefinition.FullName}\").");
            }

            return output.ToString();
        }
    }
}