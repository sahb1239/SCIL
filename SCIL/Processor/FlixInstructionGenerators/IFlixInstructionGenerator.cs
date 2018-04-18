namespace SCIL.Processor.FlixInstructionGenerators
{
    public interface IFlixInstructionGenerator
    {
        bool GenerateCode(Node node, out string outputFlixCode);
    }
}
