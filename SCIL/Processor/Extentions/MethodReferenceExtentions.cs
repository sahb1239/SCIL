using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace SCIL.Processor.Extentions
{
    public static class MethodReferenceExtentions
    {
        public static bool IsVoid(this MethodReference methodReference)
        {
            if (methodReference == null) throw new ArgumentNullException(nameof(methodReference));
            return methodReference.ReturnType.IsVoid();
        }
    }
}
