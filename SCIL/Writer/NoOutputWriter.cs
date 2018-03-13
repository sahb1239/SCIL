using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace SCIL.Writer
{
    class NoOutputWriter : IModuleWriter
    {
        public Task<IModuleWriter> GetAssemblyModuleWriter(string name)
        {
            return Task.FromResult((IModuleWriter)this);
        }

        public Task<IModuleWriter> GetTypeModuleWriter(TypeDefinition typeDefinition)
        {
            return Task.FromResult((IModuleWriter)this);
        }

        public Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition)
        {
            return Task.CompletedTask;
        }

        public Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition, string methodBody)
        {
            return Task.CompletedTask;
        }
    }
}
