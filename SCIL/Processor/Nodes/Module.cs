using System;
using System.Collections.Generic;
using Mono.Cecil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Nodes
{
    public class Module : Element
    {
        private readonly List<Type> _types = new List<Type>();

        public Module(ModuleDefinition module, IEnumerable<Type> types)
        {
            Definition = module ?? throw new ArgumentNullException(nameof(module));

            if (types == null) throw new ArgumentNullException(nameof(types));
            _types.AddRange(types);

            // Update type for each method
            _types.ForEach(method => method.Module = this);
        }

        public ModuleDefinition Definition { get; }
        public IReadOnlyCollection<Type> Types => _types.AsReadOnly();

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}