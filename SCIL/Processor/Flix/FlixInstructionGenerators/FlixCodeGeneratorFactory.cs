using System.Collections.Generic;

namespace SCIL.Processor.FlixInstructionGenerators
{
    public class FlixCodeGeneratorFactory
    {
        private readonly IEnumerable<IFlixInstructionGenerator> _instructionGenerators;

        public FlixCodeGeneratorFactory(IEnumerable<IFlixInstructionGenerator> instructionGenerators)
        {
            _instructionGenerators = instructionGenerators;
        }

        public FlixCodeGeneratorVisitor Generate()
        {
            return new FlixCodeGeneratorVisitor(_instructionGenerators);
        }
    }
}