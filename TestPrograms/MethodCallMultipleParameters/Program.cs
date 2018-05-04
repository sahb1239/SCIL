using System;

namespace MethodCallMultipleParameters
{
    class Program
    {
        static void Main(string[] args)
        {
            var userInput = Console.ReadLine();

            var userInputWithLol = AddLOL(99, "something", userInput);

            Console.WriteLine(userInputWithLol);
        }

        static string AddLOL(int randomInt, string randomString, string s)
        {
            return s + " LOL";
        }
    }
}
