using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Nodes
{
    public class Type : Element
    {
        private readonly List<Method> _methods = new List<Method>();
        private readonly List<Type> _nestedTypes = new List<Type>();
        private readonly List<Node> _initilizationPoints = new List<Node>();

        public Type(TypeDefinition type, IEnumerable<Method> methods) : this(type, methods, null)
        {
        }

        public Type(TypeDefinition type, IEnumerable<Method> methods, IEnumerable<Type> nestedTypes)
        {
            Definition = type ?? throw new ArgumentNullException(nameof(type));

            if (methods == null) throw new ArgumentNullException(nameof(methods));
            _methods.AddRange(methods);

            if (nestedTypes != null) _nestedTypes.AddRange(nestedTypes);

            // Update type for each method
            _methods.ForEach(method => method.Type = this);
        }

        public TypeDefinition Definition { get; }
        public IEnumerable<Method> Methods => _methods.AsReadOnly();
        public Module Module { get; set; }
        public IEnumerable<Type> NestedTypes => _nestedTypes.AsReadOnly();

        public bool IsGeneratedTaskType { get; set; }

        public IEnumerable<Node> InitilizationPoints => _initilizationPoints.AsReadOnly();

        public void SetInitilizationPoints(IEnumerable<Node> nodes)
        {
            _initilizationPoints.Clear();
            _initilizationPoints.AddRange(nodes.Distinct());
        }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}