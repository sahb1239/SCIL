using System;

namespace MethodCalls
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            var userInputWithLol = AddLOL(userInput);

            Console.WriteLine(userInputWithLol);
        }

        static string AddLOL(string s)
        {
            return s + " LOL";
        }
    }
}
