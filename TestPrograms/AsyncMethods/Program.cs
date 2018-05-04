using System;
using System.Threading.Tasks;

namespace AsyncMethods
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var userInput = Console.ReadLine();

            var withLOL = await AddLOL(userInput);

            Console.WriteLine(withLOL);
        }

        static async Task<string> AddLOL(string s)
        {
            return s.Insert(s.Length, " LOL");
        }
    }
}
