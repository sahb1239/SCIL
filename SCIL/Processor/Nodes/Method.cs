﻿using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;
using SCIL.Processor.Nodes.Visitor;

namespace SCIL.Processor.Nodes
{
    public class Method : Element
    {
        private readonly List<Block> _blocks = new List<Block>();

        public Method(MethodDefinition method, Block startBlock, IEnumerable<Block> blocks)
        {
            Definition = method ?? throw new ArgumentNullException(nameof(method));
            StartBlock = startBlock ?? throw new ArgumentNullException(nameof(startBlock));

            if (blocks == null) throw new ArgumentNullException(nameof(blocks));
            _blocks.AddRange(blocks);

            // Update method for each block
            _blocks.ForEach(block => block.Method = this);
        }

        public IReadOnlyCollection<Block> Blocks => _blocks.AsReadOnly();
        public MethodDefinition Definition { get; }
        public Block StartBlock { get; }
        public Type Type { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Type : Element
    {
        private readonly List<Method> _methods = new List<Method>();

        public Type(TypeDefinition type, IEnumerable<Method> methods)
        {
            Definition = type ?? throw new ArgumentNullException(nameof(type));

            if (methods == null) throw new ArgumentNullException(nameof(methods));
            _methods.AddRange(methods);

            // Update type for each method
            _methods.ForEach(method => method.Type = this);
        }

        public TypeDefinition Definition { get; }
        public IReadOnlyCollection<Method> Methods => _methods.AsReadOnly();
        public Module Module { get; set; }

        public override void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

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