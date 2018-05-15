using System;
using System.Threading.Tasks;

namespace AsyncFileRead
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var kk = await System.IO.File.ReadAllTextAsync("path");

            Console.WriteLine(kk);
        }
    }
}