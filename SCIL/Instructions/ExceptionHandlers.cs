using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;

namespace SCIL.Instructions
{
    [EmitterOrder(5)]
    class ExceptionHandlers : IOldFlixInstructionGenerator
    {
        public string GetCode(MethodBody methodBody, Instruction instruction, IFlixInstructionProgramState programState)
        {
            foreach (var exceptionHandler in methodBody.ExceptionHandlers)
            {
                if (exceptionHandler.HandlerStart.Offset == instruction.Offset)
                {
                    switch (exceptionHandler.HandlerType)
                    {
                        case ExceptionHandlerType.Catch:
                            // Push exception on stack (ugly hack)
                            programState.PushStack();
                            //Console.WriteLine();

                            break;
                        /*case ExceptionHandlerType.Filter:
                            break;
                        case ExceptionHandlerType.Fault:
                            break;*/
                    }
            }
        }

            return null;
        }
    }
}
