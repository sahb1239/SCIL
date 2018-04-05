using System;
using System.Threading.Tasks;
using Mono.Cecil;

namespace SCIL.Writer
{
    interface IModuleWriter : IDisposable
    {
        Task<IModuleWriter> GetAssemblyModuleWriter(string name);
        Task<IModuleWriter> GetTypeModuleWriter(TypeDefinition typeDefinition);
        Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition);
        Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition, string methodBody);
    }
}