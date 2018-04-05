using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;

namespace SCIL.Writer
{
    class SingleFileWriter : IModuleWriter
    {
        public FileInfo File { get; }
        public SingleFileWriter(string path)
        {
            File = new FileInfo(Path.Combine(path, "output.txt"));
            File.Create();
        }

        public async Task<IModuleWriter> GetAssemblyModuleWriter(string name)
        {
            using (StreamWriter stream =
            new StreamWriter(File.FullName, true))
            {
                await stream.WriteLineAsync("assembly_" + name);
            }

            return await Task.FromResult((IModuleWriter)this);
        }

        public async Task<IModuleWriter> GetTypeModuleWriter(TypeDefinition typeDefinition)
        {
            using (StreamWriter stream =
            new StreamWriter(File.FullName, true))
            {
                await stream.WriteLineAsync("type_" + typeDefinition.FullName);
            }

            return await Task.FromResult((IModuleWriter)this);
        }

        public Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition) =>
            WriteMethod(typeDefinition, methodDefinition, "");

        public async Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition, string methodBody)
        {
            using (StreamWriter stream =
            new StreamWriter(File.FullName, true))
            {
                await stream.WriteLineAsync("method_" + methodDefinition.FullName).ConfigureAwait(false);
                await stream.WriteAsync(methodBody.TrimEnd() + "\n").ConfigureAwait(false);
            }
        }
    }
}
