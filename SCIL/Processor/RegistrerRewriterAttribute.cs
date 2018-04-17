using System;

namespace SCIL.Processor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegistrerRewriterAttribute : RegistrerVisitorAttribute
    {
        public RegistrerRewriterAttribute() : base(RewriterOrder)
        {
        }

        public RegistrerRewriterAttribute(bool ignored) : base(ignored)
        {
        }

        public RegistrerRewriterAttribute(string name) : base(RewriterOrder, name)
        {
        }
    }
}