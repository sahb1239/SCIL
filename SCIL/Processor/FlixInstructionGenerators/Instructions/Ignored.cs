using Mono.Cecil.Cil;

namespace SCIL.Processor.FlixInstructionGenerators.Instructions
{
    public class Ignored : IFlixInstructionGenerator
    {
        public bool GenerateCode(Node node, out string outputFlixCode)
        {
            switch (node.OpCode.Code)
            {
                case Code.Nop:
                case Code.Box:
                case Code.Unbox:
                case Code.Unbox_Any:
                    outputFlixCode = "// " + node.OpCode.Name;
                    return true;
            }

            outputFlixCode = null;
            return false;
        }
    }
}
