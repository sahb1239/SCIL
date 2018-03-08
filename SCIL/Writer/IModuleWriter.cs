using System.Threading.Tasks;
using Mono.Cecil;

namespace SCIL.Writer
{
    interface IModuleWriter
    {
        Task WriteType(TypeDefinition typeDefinition);
        Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition);
        Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition, string methodBody);
    }
}