using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

namespace SCIL.Decompressor
{
    // https://reverseengineering.stackexchange.com/questions/16508/unpacking-xamarin-mono-dll-from-libmonodroid-bundle-app-so/16512
    class XamarinBundleUnpack
    {
        public static async Task<IEnumerable<byte[]>> GetGzippedAssemblies(byte[] bytes)
        {
            // Get tmp file
            var file = Path.GetTempFileName();

            // Write to that file
            try
            {
                await File.WriteAllBytesAsync(file, bytes).ConfigureAwait(false);
                return await GetGzippedAssemblies(file).ConfigureAwait(false);
            }
            finally
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    // Ignore - at some point it might be deleted
                }
            }
        }

        public static Task<IEnumerable<byte[]>> GetGzippedAssemblies(string libmonodroid_bundle_app_file)
        {
            List<byte[]> output = new List<byte[]>();

            // Load ELF
            using (var elf = ELFReader.Load(libmonodroid_bundle_app_file))
            {
                // Get data section
                var rodata = (Section<UInt32>) elf.Sections.FirstOrDefault(x => x.Name == ".rodata");
                if (rodata == null)
                {
                    throw new XamarinBundleUnpackException("Could not find .rodata section in file");
                }

                // Load data
                Memory<byte> dataSpan = new Memory<byte>(rodata.GetContents());

                // Read files from data section
                int lastGzipEntry = 0;
                int nextGzipEntry = 0;

                // Load next gzip until we are starting from 0 again
                while ((nextGzipEntry = FindNextGzipIndex(dataSpan, nextGzipEntry)) > lastGzipEntry && nextGzipEntry >= 0)
                {
                    // Decompress gzip
                    Memory<byte> compressedbytes = dataSpan.Slice(nextGzipEntry);
                    try
                    {
                        var decompbytes = Decompress(compressedbytes);
                        output.Add(decompbytes);
                    }
                    catch (Exception)
                    {
                        // It might not be a Gzip and therefore we just go to next part
                    }

                    // Set last and ensure we go to next Gzip
                    lastGzipEntry = nextGzipEntry;
                    nextGzipEntry += 0x2; // Skip Gzip header
                }
            }

            return Task.FromResult((IEnumerable<byte[]>) output);
        }

        private static byte[] Decompress(Memory<byte> data)
        {
            using (var compressedStream = new MemoryStream(data.ToArray()))
            using (var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                zipStream.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }

        private static int FindNextGzipIndex(Memory<byte> bytes, int nextGzipEntry)
        {
            var byteSpan = bytes.Span;
            for (int j = nextGzipEntry; j < bytes.Length; j++)
            {
                if (byteSpan[j] == 0x1f && byteSpan[j + 1] == 0x8b)
                {
                    nextGzipEntry = j;
                    return nextGzipEntry;
                }
            }

            return -1;
        }
    }

    public class XamarinBundleUnpackException : Exception
    {
        public XamarinBundleUnpackException(string message) : base(message)
        {
        }
    }
}
