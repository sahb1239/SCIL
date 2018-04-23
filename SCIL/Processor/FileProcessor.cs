using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SCIL.Decompressor;
using SCIL.Logger;

namespace SCIL.Processor
{
    public class FileProcessor
    {
        public ILogger Logger { get; }

        public ModuleProcessor ModuleProcessor { get; }

        public FileProcessor(ILogger logger, ModuleProcessor moduleProcessor)
        {
            ModuleProcessor = moduleProcessor;
            Logger = logger;
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
                return new[] {await ProcessAssembly(fileInfo.OpenRead())};
            }
        }

        private async Task<IEnumerable<string>> ProcessZip(FileInfo fileInfo)
        {
            List<string> createdFiles = new List<string>();

            // Open zip file
            using (var zipFile = ZipFile.OpenRead(fileInfo.FullName))
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
                    Logger.Log("Loading " + selectedLibMonoDroidBundle.FullName);
                    using (var stream = new MemoryStream())
                    {
                        await selectedLibMonoDroidBundle.Open().CopyToAsync(stream);

                        // Set position 0
                        stream.Position = 0;

                        var files = await XamarinBundleUnpack.GetGzippedAssemblies(stream.ToArray());
                        foreach (var file in files)
                        {
                            using (var memStream = new MemoryStream(file))
                            {
                                createdFiles.Add(await ProcessAssembly(memStream));
                            }
                        }
                    }
                }

                // Process assemblies
                var assemblies = zipFile.Entries.Where(entry => FilterXamarinAssembliesDlls(entry) || FilterUnityAssembliesDlls(entry)).ToList();
                foreach (var assembly in assemblies)
                {
                    {
                        if (FilterXamarinAssembliesDlls(assembly))
                        {
                            Logger.Log("Loading Xamarin dll: " + assembly.FullName);
                        }
                        else if (FilterUnityAssembliesDlls(assembly))
                        {
                            Logger.Log("Loading Unity dll: " + assembly.FullName);
                        }
                        else
                        {
                            Logger.Log("Loading Unknown dll: " + assembly.FullName);
                        }

                        using (var stream = new MemoryStream())
                        {
                            await assembly.Open().CopyToAsync(stream);

                            // Set position 0
                            stream.Position = 0;

                            createdFiles.Add(await ProcessAssembly(stream));
                        }
                    }
                }

            }

            return createdFiles;
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

        private Task<string> ProcessAssembly(Stream stream)
        {
            return ModuleProcessor.ProcessAssembly(stream);
        }
    }
}
