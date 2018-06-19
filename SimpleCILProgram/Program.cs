using System;
using System.Threading.Tasks;

namespace SimpleCILProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            // This is tained from the user input
            LoLoLoLo(userInput);
        }

        public static void LoLoLoLo(string s)
        {
            Console.WriteLine(s);
        }
    }
}