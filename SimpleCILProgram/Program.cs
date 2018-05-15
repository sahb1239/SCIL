using System;
using System.Threading.Tasks;

namespace SimpleCILProgram
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var kk = await System.IO.File.ReadAllTextAsync("path", System.Text.Encoding.ASCII);

            Console.WriteLine(kk);
        }
    }
}