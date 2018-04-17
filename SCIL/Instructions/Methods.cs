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
                        return call("Call", programState, callRef);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Callvirt:
                    if (instruction.Operand is MethodReference callVirtRef)
                    {
                        return call("Callvirt", programState, callVirtRef);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ret:
                    if (methodBody.Method.ReturnType.FullName == "System.Void")
                        programState.PushStack(); // Push a value - which will be popped

                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return $"RetStm(\"RET_{methodBody.Method.FullName}\").";
            }

            return null;
        }
        private string call(string callType, IFlixInstructionProgramState programState, MethodReference method)
        {
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
                    $"StargStm({programState.GetStoreArg(method, parameterIndex)}, {programState.PopStack()}, {parameterIndex}).");
            }

            // Add this to arguments
            if (method.HasThis)
            {
                output.AppendLine(
                    $"StargStm({programState.GetStoreArg(method, 0)}, {programState.PopStack()}, 0).");
            }

            // Add call statement
            output.AppendLine($"{callType}Stm({programState.PushStack()}, \"RET_{method.FullName}\", \"{method.FullName}\").");

            // Add return value
            if (method.ReturnType.FullName == "System.Void")
            {
                output.AppendLine("// System.Void: Pop stack");
                programState.PopStack();
                //programState.PushStack(); // Push a value - which will be popped
            }
            else
            {
                //output.AppendLine($"DupStm({programState.PushStack()}, \"RET_{method.FullName}\").");
            }

            return output.ToString().TrimEnd();
        }
    }
}