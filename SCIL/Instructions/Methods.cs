using System;
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
                        // Pop the number of arguments
                        for (int i = 0; i < callRef.Parameters.Count; i++)
                            programState.PopStack();

                        if (callRef.ReturnType.FullName == "System.Void")
                            programState.PushStack(); // Push a value - which will be popped
                        return call(callRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Callvirt:
                    if (instruction.Operand is MethodReference callVirtRef)
                    {
                        // Pop the number of arguments
                        for (int i = 0; i < callVirtRef.Parameters.Count; i++)
                            programState.PopStack();

                        if (callVirtRef.ReturnType.FullName == "System.Void")
                            programState.PushStack(); // Push a value - which will be popped

                        return callvirt(callVirtRef.FullName, programState);
                    }
                    throw new ArgumentOutOfRangeException(nameof(instruction.Operand));
                case Code.Ret:
                    if (methodBody.Method.ReturnType.FullName == "System.Void")
                        programState.PushStack(); // Push a value - which will be popped

                    if (instruction.Operand != null)
                    {
                        throw new ArgumentException(nameof(instruction.Operand));
                    }
                    return "retStm().";
            }

            return null;
        }
        private string call(string method, IFlixInstructionProgramState programState) => $"callStm({method}, {programState.PushStack()}).";
        private string callvirt(string method, IFlixInstructionProgramState programState) => $"callvirtStm({method}, {programState.PushStack()}).";
    }
}