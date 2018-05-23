using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Mono.Cecil;
using SCIL.Decompressor;
using SCIL.Logger;

namespace SCIL.Processor
{
    public class FileProcessor
    {
        public ILogger Logger { get; }

        public ModuleProcessor ModuleProcessor { get; }

        public Configuration Configuration { get; }

        public FileProcessor(ILogger logger, ModuleProcessor moduleProcessor, Configuration configuration)
        {
            ModuleProcessor = moduleProcessor ?? throw new ArgumentNullException(nameof(moduleProcessor));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<string>> ProcessFile(FileInfo fileInfo)
        {
            // Detect if file is zip
            if (await ZipHelper.CheckSignature(fileInfo.FullName))
            {
                return await ProcessZip(fileInfo);
            }
            else
            {
                // TODO : Detect dll and exe
                // Just jump out into the water and see if we survive (no exceptions)
                using (var module = ModuleDefinition.ReadModule(fileInfo.OpenRead()))
                {
                    var createdFile = await ProcessAssembly(module);
                    if (createdFile != null)
                    {
                        return new[] {createdFile};
                    }
                    else
                    {
                        return new string[] { };
                    }
                }
            }
        }

        private async Task<IEnumerable<string>> ProcessZip(FileInfo fileInfo)
        {
            List<string> createdFiles = new List<string>();

            // Open zip file
            using (var zipFile = ZipFile.OpenRead(fileInfo.FullName))
            {
                // Create list of module definitions
                List<ModuleDefinition> moduleDefinitions = new List<ModuleDefinition>();
                var moduleResolver = new ModuleDefinitionsResolver(moduleDefinitions);

                try
                {
                    // Read all bundled assemblies
                    moduleDefinitions.AddRange(await ReadBundledAssemblies(zipFile, moduleResolver));

                    // Process assemblies
                    var assemblies = zipFile.Entries.Where(entry => FilterXamarinAssembliesDlls(entry) || FilterUnityAssembliesDlls(entry)).ToList();
                    foreach (var assembly in assemblies)
                    {
                        if (FilterXamarinAssembliesDlls(assembly))
                        {
                            Logger.Log("[Load]: Xamarin dll " + assembly.FullName);
                        }
                        else if (FilterUnityAssembliesDlls(assembly))
                        {
                            Logger.Log("[Load]: Unity dll: " + assembly.FullName);
                        }
                        else
                        {
                            Logger.Log("[Load]: Unknown dll: " + assembly.FullName);
                        }

                        // Copy file over to stream
                        var stream = new MemoryStream();
                        await assembly.Open().CopyToAsync(stream);

                        // Set position 0
                        stream.Position = 0;

                        moduleDefinitions.Add(ModuleDefinition.ReadModule(stream,
                            new ReaderParameters { AssemblyResolver = moduleResolver }));
                    }

                    // Process all modules
                    if (Configuration.Async)
                    {
                        createdFiles.AddRange(
                            (await Task.WhenAll(moduleDefinitions.Select(AsyncProcessAssembly))).Where(createdFile =>
                                createdFile != null));
                    }
                    else
                    {
                        foreach (var module in moduleDefinitions)
                        {
                            var createdFile = await ProcessAssembly(module);
                            if (createdFile != null)
                            {
                                createdFiles.Add(createdFile);
                            }
                        }
                    }
                }
                finally
                {
                    foreach (var module in moduleDefinitions)
                    {
                        try
                        {
                            module.Dispose();
                        }
                        catch (Exception)
                        {
                            // Ignored
                        }
                    }
                }

            }

            return createdFiles;
        }

        private async Task<IEnumerable<ModuleDefinition>> ReadBundledAssemblies(ZipArchive zipFile, IAssemblyResolver resolver)
        {
            List<ModuleDefinition> definitions = new List<ModuleDefinition>();

            try
            {
                // Detect if file is containing a bundle so file
                var libmonodroidbundle = zipFile.Entries.Where(entry =>
                        entry.FullName.StartsWith("lib", StringComparison.OrdinalIgnoreCase) &&
                        entry.FullName.EndsWith("libmonodroid_bundle_app.so", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // Process bundle
                if (libmonodroidbundle.Any())
                {
                    Logger.Log("libmonodroid_bundle_app.so found - starting processing");

                    ZipArchiveEntry selectedLibMonoDroidBundle;
                    if (libmonodroidbundle.Count() == 1)
                    {
                        selectedLibMonoDroidBundle = libmonodroidbundle.First();
                    }
                    else
                    {
                        selectedLibMonoDroidBundle =
                            libmonodroidbundle.FirstOrDefault(e => e.FullName.Contains("armeabi-v7a")) ?? libmonodroidbundle.First();
                    }

                    // Read bundle
                    Logger.Log("[Load]: Bundle " + selectedLibMonoDroidBundle.FullName);
                    using (var stream = new MemoryStream())
                    {
                        await selectedLibMonoDroidBundle.Open().CopyToAsync(stream);

                        // Set position 0
                        stream.Position = 0;

                        var files = await XamarinBundleUnpack.GetGzippedAssemblies(stream.ToArray());
                        foreach (var file in files)
                        {
                            // Load all files
                            definitions.Add(ModuleDefinition.ReadModule(new MemoryStream(file),
                                new ReaderParameters { AssemblyResolver = resolver }));
                        }
                    }
                }

                return definitions;
            }
            catch (Exception)
            {
                // Dispose all initilized definitions
                foreach (var module in definitions)
                {
                    try
                    {
                        module.Dispose();
                    }
                    catch (Exception)
                    {
                        // Ignored
                    }
                }

                // Rethrow exception
                throw;
            }
        }

        private bool FilterXamarinAssembliesDlls(ZipArchiveEntry entry)
        {
            return entry.FullName.StartsWith("assemblies", StringComparison.OrdinalIgnoreCase) &&
                   entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        }

        private bool FilterUnityAssembliesDlls(ZipArchiveEntry entry)
        {
            return entry.FullName.StartsWith("assets/bin/Data/Managed/", StringComparison.OrdinalIgnoreCase) &&
                   entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        }

        private Task<string> AsyncProcessAssembly(ModuleDefinition module)
        {
            return Task.Run(() => ProcessAssembly(module));
        }

        private Task<string> ProcessAssembly(ModuleDefinition module)
        {
            return ModuleProcessor.ReadModule(module);
        }

        private class ModuleDefinitionsResolver : BaseAssemblyResolver
        {
            private readonly IEnumerable<ModuleDefinition> _moduleDefinitions;
            private readonly DefaultAssemblyResolver _defaultResolver;

            public ModuleDefinitionsResolver(List<ModuleDefinition> moduleDefinitions)
            {
                _defaultResolver = new DefaultAssemblyResolver();
                _moduleDefinitions = moduleDefinitions;
            }

            public override AssemblyDefinition Resolve(AssemblyNameReference name)
            {
                // Try first to match from the list of assemblies
                // Get all the assemblies which are matching
                var matchingAssemblies = _moduleDefinitions.Select(module => module.Assembly)
                    .Where(assembly => name.FullName == assembly.FullName).ToList();

                if (matchingAssemblies.Any())
                {
                    // Just take the first assembly
                    Debug.Assert(matchingAssemblies.Count == 1);
                    return matchingAssemblies.First();
                }

                // Use default resolver
                return _defaultResolver.Resolve(name);
            }
        }
    }
}
