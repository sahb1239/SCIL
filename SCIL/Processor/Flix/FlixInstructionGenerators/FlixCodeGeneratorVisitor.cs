using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;
using Type = SCIL.Processor.Nodes.Type;

namespace SCIL.Processor.FlixInstructionGenerators
{
    public class FlixCodeGeneratorVisitor : BaseVisitor
    {
        private readonly IEnumerable<IFlixInstructionGenerator> _instructionGenerators;

        public FlixCodeGeneratorVisitor(IEnumerable<IFlixInstructionGenerator> instructionGenerators)
        {
            _instructionGenerators = instructionGenerators;
        }

        private StringBuilder Builder { get; } = new StringBuilder();

        public override void Visit(Module module)
        {
            // Add type name
            Builder.AppendLine(
                $"// module_{module.Definition.Name}");

            base.Visit(module);
        }

        public override void Visit(Type type)
        {
            // Add type name
            Builder.AppendLine(
                $"// type_{type.Definition.Name}<{String.Join(",", type.Definition.GenericParameters.Select(e => e.DeclaringType.FullName))}>");

            base.Visit(type);

            Builder.AppendLine();
        }

        public override void Visit(Method method)
        {
            // Add method name
            Builder.AppendLine(
                $"// method_{method.Definition.Name}<{String.Join(",", method.Definition.GenericParameters.Select(e => e.Name))}>({String.Join(", ", method.Definition.Parameters.Select(e => $"{e.ParameterType.FullName} {e.Name}"))})");

            base.Visit(method);
        }

        public override void Visit(Block block)
        {
            // Add block info
            Builder.AppendLine($"// Begin block (first offset: {block.Nodes.First(node => node.Instruction != null).Instruction.Offset})");
            foreach (var sources in block.Sources)
            {
                Builder.AppendLine($"// Source (offset: {sources.Nodes.Last(node => node.Instruction != null).Instruction.Offset})");
            }

            base.Visit(block);

            // Add block info
            Builder.AppendLine($"// End block (last offset: {block.Nodes.Last().Instruction.Offset})");
            foreach (var target in block.Targets)
            {
                Builder.AppendLine($"// Target (offset: {target.Nodes.First(node => node.Instruction != null).Instruction.Offset})");
            }

            Builder.AppendLine();
        }

        public override void Visit(Node node)
        {
            // Get node info
            var stackBehaviorInfo = node.GetRequiredNames();
            Builder.AppendLine(
                $"// Node {node.OpCode}, offset: {node.Instruction?.Offset}, pop: {stackBehaviorInfo.popNames}, push: {stackBehaviorInfo.pushNames}");

            // Generate code (first match)
            foreach (var generator in _instructionGenerators)
            {
                string flixCode;
                if (generator.GenerateCode(node, out flixCode))
                {
                    Builder.AppendLine(flixCode);
                    break;
                }
            }

            base.Visit(node);
        }

        public void Clear()
        {
            Builder.Clear();
        }

        public override string ToString()
        {
            return Builder.ToString().TrimEnd();
        }
    }
}
