using System;

namespace SCIL.Processor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegistrerVisitor : Attribute
    {
        public const uint RewriterOrder = 10;

        public RegistrerVisitor()
        {
        }

        public RegistrerVisitor(uint order)
        {
            Order = order;
        }

        public RegistrerVisitor(bool ignored)
        {
            Ignored = ignored;
        }

        public RegistrerVisitor(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public RegistrerVisitor(uint order, string name)
        {
            Order = order;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public uint Order { get; } = 100;
        public bool Ignored { get; } = false;
        public string Name { get; }
    }
}
