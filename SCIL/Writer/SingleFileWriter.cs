using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Mono.Cecil;
using Mono.Collections.Generic;

namespace SCIL.Writer
{
    class SingleFileWriter : IModuleWriter, IDisposable
    {
        private readonly SingleFileWriter _parent;
        public StreamWriter FileStream { get; }
        private string OutputPath { get; }
        private bool ShouldDispose { get; } = true;
        private List<string> Files { get; } = new List<string>();

        public SingleFileWriter(string path)
        {
            OutputPath = path;
        }

        private SingleFileWriter(string path, string assembly, SingleFileWriter parent)
        {
            var file = new FileInfo(Path.Combine(path, assembly + ".flix"));
            FileStream = File.CreateText(file.FullName);
            OutputPath = path;

            // Add fileName to parent
            _parent = parent;
            var current = this;
            do
            {
                current.Files.Add(file.FullName);
            } while ((current = current._parent) != null);
        }

        private SingleFileWriter(SingleFileWriter fileWriter)
        {
            ShouldDispose = false;
            FileStream = fileWriter.FileStream;
            OutputPath = fileWriter.OutputPath;
        }

        public IEnumerable<string> GetCreatedFilesAndReset()
        {
            var files = new ReadOnlyCollection<string>(Files.ToArray());
            Files.Clear();
            return files;
        }

        public Task<IModuleWriter> GetAssemblyModuleWriter(string name)
        {
            return Task.FromResult((IModuleWriter) new SingleFileWriter(OutputPath, GetSafePath(name), this));
        }

        public async Task<IModuleWriter> GetTypeModuleWriter(TypeDefinition typeDefinition)
        {
            await FileStream.WriteLineAsync("// type_" + typeDefinition.FullName).ConfigureAwait(false);
            return new SingleFileWriter(this);
        }

        public Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition) =>
            WriteMethod(typeDefinition, methodDefinition, "");

        public async Task WriteMethod(TypeDefinition typeDefinition, MethodDefinition methodDefinition, string methodBody)
        {
            await FileStream.WriteLineAsync("// method_" + methodDefinition.FullName).ConfigureAwait(false);
            await FileStream.WriteAsync(methodBody.TrimEnd() + "\n").ConfigureAwait(false);
        }

        private static string GetSafePath(string input)
        {
            return new string(input.Where(c => Char.IsLetterOrDigit(c) || c == '.').ToArray());
        }

        public void Dispose()
        {
            if (ShouldDispose)
                FileStream?.Dispose();
        }
    }
}
