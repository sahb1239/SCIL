using System;

namespace SCIL.Processor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegistrerVisitorAttribute : Attribute
    {
        public const uint RewriterOrder = 10;
        public const uint SSAOrder = 20;
        public const uint AnalyzerOrder = 30;

        public RegistrerVisitorAttribute()
        {
        }

        public RegistrerVisitorAttribute(uint order)
        {
            Order = order;
        }

        public RegistrerVisitorAttribute(bool ignored)
        {
            Ignored = ignored;
        }

        public RegistrerVisitorAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public RegistrerVisitorAttribute(uint order, string name)
        {
            Order = order;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public uint Order { get; } = 100;
        public bool Ignored { get; } = false;
        public string Name { get; }
    }
}
