using System;

namespace SCIL.Processor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegistrerAnalyzerAttribute : RegistrerVisitorAttribute
    {
        public RegistrerAnalyzerAttribute() : base(AnalyzerOrder)
        {
        }

        public RegistrerAnalyzerAttribute(bool ignored) : base(ignored)
        {
        }

        public RegistrerAnalyzerAttribute(string name) : base(AnalyzerOrder, name)
        {
        }
    }
}