using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.Cecil;
using SCIL.Flix;

namespace SCIL.Writer
{
    interface IModuleWriter : IDisposable
    {
        IEnumerable<string> GetCreatedFilesAndReset();
        Task<IModuleWriter> GetAssemblyModuleWriter(string name);
        Task<IModuleWriter> GetTypeModuleWriter(TypeDefinition typeDefinition);
        Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition);
        Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition, string methodBody);
    }
}