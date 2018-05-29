using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace SCIL.Processor.Extentions
{
    public static class MethodReferenceExtentions
    {
        public static string NameOnly(this MethodReference methodReference)
        {
            if (methodReference == null) throw new ArgumentNullException(nameof(methodReference));

            var splitted = methodReference.FullName.Split(' ');
            return splitted[splitted.Length - 1];
        }

        public static bool IsVoid(this MethodReference methodReference)
        {
            if (methodReference == null) throw new ArgumentNullException(nameof(methodReference));
            return methodReference.ReturnType.IsVoid();
        }

        public static IEnumerable<MethodReference> GetAllOverridesIncludingSelf(this MethodReference methodReference)
        {
            if (methodReference == null) throw new ArgumentNullException(nameof(methodReference));

            // Return own method
            yield return methodReference;

            // Get method definition
            var methodDefinition = methodReference.Resolve();

            // Get all overrides
            if (methodDefinition.HasOverrides)
            {
                foreach (var overridedMethodReference in methodDefinition.Overrides.SelectMany(
                    GetAllOverridesIncludingSelf))
                {
                    yield return overridedMethodReference;
                }
            }
        }
    }
}
