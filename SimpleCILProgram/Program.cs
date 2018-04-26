using System;

namespace SimpleCILProgram
{
    class Program
    {
        static void Main(string[] args)
        {
            var read = Console.ReadLine();

            Console.WriteLine(AddLOL(read));
        }

        static string AddLOL(string s)
        {
            return s + " LOL";
        }
    }

}
