using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SCIL.Processor.Nodes;
using SCIL.Processor.Nodes.Visitor;
using Type = SCIL.Processor.Nodes.Type;

namespace SCIL.Processor.TypeAnalyzer
{
    [RegistrerVisitor(RegistrerVisitorAttribute.RewriterOrder + 1)]
    public class InitilizationAnalyzerVisitor : BaseVisitor
    {
        private readonly InitilizationPopulateVisitor _visitor = new InitilizationPopulateVisitor();
        // Get all initilization points
        private class InitilizationPopulateVisitor : BaseVisitor
        {
            public IDictionary<TypeDefinition, List<Node>> InitilizationPoints { get; } = new Dictionary<TypeDefinition, List<Node>>();

            public override void Visit(Node node)
            {
                if (node.OpCode.Code == Code.Newobj)
                {
                    // Get type
                    if (node.Operand is MethodReference methodReference)
                    {
                        var type = methodReference.DeclaringType.Resolve();

                        // Add to the list
                        if (!InitilizationPoints.ContainsKey(type))
                        {
                            InitilizationPoints.Add(type, new List<Node>());
                        }

                        InitilizationPoints[type].Add(node);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        public override void Visit(Module module)
        {
            // Populate list
            _visitor.Visit(module);

            base.Visit(module);
        }

        public override void Visit(Type type)
        {
            if (_visitor.InitilizationPoints.ContainsKey(type.Definition))
            {
                // Get initilization points
                type.InitilizationPoints = _visitor.InitilizationPoints[type.Definition].Distinct().ToList();
            }

            // Also visit subtypes
            base.Visit(type);
        }
    }
}
