using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Rocks;

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

            // Get base method
            // Possible not correct with generic parameters due to https://github.com/jbevain/cecil/issues/180
            var originalBaseMethod = methodDefinition.GetOriginalBaseMethod();

            // Check if base method is not equal to currentMethodDefinition
            if (originalBaseMethod != methodDefinition)
            {
                // Get overrides from the base method
                foreach (var overridedMethodReference in GetAllOverridesIncludingSelf(originalBaseMethod))
                {
                    yield return overridedMethodReference;
                }

                // We don't need to use the overrides field since we already used Mono.Cecil.Rocks to find overrided method
                yield break;
            }

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
