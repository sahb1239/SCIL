using System;
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
        public static async Task ProcessFile(FileInfo fileInfo, IServiceProvider services)
        {
            // Detect if file is zip
            if (await ZipHelper.CheckSignature(fileInfo.FullName))
            {
                await ProcessZip(fileInfo, services);
            }
            else
            {
                // TODO : Detect dll and exe
                // Just jump out into the water and see if we survive (no exceptions)
                await ProcessAssembly(fileInfo.OpenRead(), services);
            }
        }

        private static async Task ProcessZip(FileInfo fileInfo, IServiceProvider services)
        { 
            // Get logger
            var logger = services.GetRequiredService<ILogger>();
            
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
                    logger.Log("libmonodroid_bundle_app.so found - starting processing");

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
                    logger.Log("Loading " + selectedLibMonoDroidBundle.FullName);
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
                                await ProcessAssembly(memStream, services);
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
                            logger.Log("Loading Xamarin dll: " + assembly.FullName);
                        }
                        else if (FilterUnityAssembliesDlls(assembly))
                        {
                            logger.Log("Loading Unity dll: " + assembly.FullName);
                        }
                        else
                        {
                            logger.Log("Loading Unknown dll: " + assembly.FullName);
                        }

                        using (var stream = new MemoryStream())
                        {
                            await assembly.Open().CopyToAsync(stream);

                            // Set position 0
                            stream.Position = 0;

                            await ProcessAssembly(stream, services);
                        }
                    }
                }

            }

        }

        private static bool FilterXamarinAssembliesDlls(ZipArchiveEntry entry)
        {
            return entry.FullName.StartsWith("assemblies", StringComparison.OrdinalIgnoreCase) &&
                   entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        }

        private static bool FilterUnityAssembliesDlls(ZipArchiveEntry entry)
        {
            return entry.FullName.StartsWith("assets/bin/Data/Managed/", StringComparison.OrdinalIgnoreCase) &&
                   entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase);
        }

        private static Task ProcessAssembly(Stream stream, IServiceProvider services)
        {
            var moduleProcessor = services.Resolve<ModuleProcessor>();
            return moduleProcessor.ProcessAssembly(stream);
        }
    }
}
