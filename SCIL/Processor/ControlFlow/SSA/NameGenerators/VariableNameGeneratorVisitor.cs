using System;
using System.Collections.Generic;
using System.Linq;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.ControlFlow.SSA.NameGenerators
{
    public class VariableNameGeneratorVisitor : BaseVisitor
    {
        public override void Visit(Method method)
        {
            var visitor = new MethodVisitor(method);
            visitor.Visit(method);
        }


        private class MethodVisitor : BaseVisitor
        {
            private readonly Method _method;
            private readonly IDictionary<Block, Variables> _variables = new Dictionary<Block, Variables>();


            public MethodVisitor(Method method)
            {
                this._method = method;
            }

            public override void Visit(Block block)
            {
                // Get variables
                Variables currentVariables;
                if (block.Sources.Any())
                {
                    var variables = _variables[block.Sources.First()];
                    currentVariables = variables.Copy();
                }
                else
                {
                    currentVariables = new Variables(_method);
                }

                // Add to stack list
                _variables[block] = currentVariables;

                // Run visitor
                var visitor = new BlockVisitor(currentVariables);
                visitor.Visit(block);
            }

            private class BlockVisitor : BaseVisitor
            {
                private readonly Variables _variables;

                public BlockVisitor(Variables variables)
                {
                    _variables = variables ?? throw new ArgumentNullException(nameof(variables));
                }

                public override void Visit(Node node)
                {
                    // Set argument names
                    node.ArgumentName = $"\"{node.GetRequiredArgumentIndex()}\"";

                    // Set variable names
                    var variableIndex = node.GetRequiredVariableIndex();
                    if (variableIndex.variableInstruction)
                    {
                        // Get variable name
                        string variableName;

                        if (variableIndex.set)
                        {
                            variableName = _variables.SetIndex(variableIndex.index);
                        }
                        else
                        {
                            variableName = _variables.GetIndex(variableIndex.index);
                        }

                        node.VariableName = $"{variableName}";
                    }

                    base.Visit(node);
                }
            }
        }
    }
}
