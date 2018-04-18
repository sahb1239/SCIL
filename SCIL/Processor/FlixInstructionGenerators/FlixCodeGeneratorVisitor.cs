using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.FlixInstructionGenerators
{
    class FlixCodeGeneratorVisitor : BaseVisitor
    {
        private readonly IEnumerable<IFlixInstructionGenerator> _instructionGenerators;
        private readonly StringBuilder _stringBuilder = new StringBuilder();

        public FlixCodeGeneratorVisitor(IEnumerable<IFlixInstructionGenerator> instructionGenerators)
        {
            _instructionGenerators = instructionGenerators;
        }

        public override void Visit(Method method)
        {
            // Add some newlines
            if (_stringBuilder.Length > 0)
            {
                _stringBuilder.AppendLine().AppendLine();
            }

            // Add method name
            _stringBuilder.AppendLine(
                $"// method_{method.Definition.Name}<{String.Join(",", method.Definition.GenericParameters.Select(e => e.DeclaringType.FullName))}>({String.Join(", ", method.Definition.Parameters.Select(e => $"{e.ParameterType.FullName} {e.Name}"))})");

            _stringBuilder.AppendLine();

            base.Visit(method);
        }

        public override void Visit(Block block)
        {
            // Add block info
            _stringBuilder.AppendLine($"// Begin block (first offset: {block.Nodes.First().Instruction.Offset})");

            base.Visit(block);

            // Add block info
            _stringBuilder.AppendLine($"// End block (last offset: {block.Nodes.Last().Instruction.Offset})");
        }

        public override void Visit(Node node)
        {
            // Get node info
            var stackBehaviorInfo = node.GetRequiredNames();
            _stringBuilder.AppendLine(
                $"// Node {node.OpCode}, offset: {node.Instruction.Offset}, pop: {stackBehaviorInfo.popNames}, push: {stackBehaviorInfo.pushNames}");

            base.Visit(node);
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
}
