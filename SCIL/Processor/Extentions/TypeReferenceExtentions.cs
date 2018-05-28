using System;
using Mono.Cecil;

namespace SCIL.Processor.Extentions
{
    public static class TypeReferenceExtentions
    {
        public static bool IsVoid(this TypeReference typeReference)
        {
            if (typeReference == null) throw new ArgumentNullException(nameof(typeReference));
            return typeReference.FullName == "System.Void";
        }
    }
}